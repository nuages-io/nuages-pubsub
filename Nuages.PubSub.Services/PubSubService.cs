using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Amazon;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService : IPubSubService
{
    private readonly IPubSubStorage _pubSubStorage;
    private readonly PubSubOptions _pubSubOptions;

    public PubSubService(IPubSubStorage pubSubStorage, IOptions<PubSubOptions> pubSubOptions)
    {
        _pubSubStorage = pubSubStorage;
        _pubSubOptions = pubSubOptions.Value;
    }
    
    private static AmazonApiGatewayManagementApiClient CreateApiGateway(string url)
    {
        return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            AuthenticationRegion = RegionEndpoint.CACentral1.SystemName,
            ServiceURL = url
        });
    }

    public string GenerateToken(string issuer, string hub, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("sub", userId)
            }),
            Expires = DateTime.UtcNow.Add(expireDelay ?? TimeSpan.FromDays(1)),
            Issuer = issuer,
            Audience = hub,
            SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var roleList = roles.ToList();
        
        if (roleList.Any())
        {
            tokenDescriptor.Claims ??= new Dictionary<string, object>();

            tokenDescriptor.Claims.Add(new KeyValuePair<string, object>("roles", roleList));
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task GrantPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        var permissionString = GetPermissionString(permission, target);

        await _pubSubStorage.AddPermissionAsync(hub, permissionString, connectionId);
    }

    private static string GetPermissionString(PubSubPermission permission, string? target)
    {
        var permissionString = permission.ToString();
        if (!string.IsNullOrEmpty(target))
            permissionString += $".{target}";
        return permissionString;
    }

    public async Task RevokePermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        var permissionString = GetPermissionString(permission, target);

        await _pubSubStorage.RemovePermissionAsync(hub, permissionString, connectionId);
    }

    public async Task<bool> CheckPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        var permissionString = GetPermissionString(permission, target);

        return await _pubSubStorage.HasPermissionAsync(hub ,permissionString, connectionId);
    }

    protected virtual async Task SendMessageAsync(string hub, IEnumerable<string> connectionIds,  string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        using var apiGateway = CreateApiGateway(_pubSubOptions.Uri!);
        
        foreach (var connectionId in connectionIds)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = stream
            };

            try
            {
                stream.Position = 0;
                    
                await apiGateway.PostToConnectionAsync(postConnectionRequest);
            }
            catch (AmazonServiceException e)
            {
                Console.WriteLine(e.Message);
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our collection.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    await _pubSubStorage.Disconnect(hub, postConnectionRequest.ConnectionId);
                }
            }
        }
    }
    
    private async Task CloseConnectionsAsync(string hub, IEnumerable<string> connectionIds)
    {
        var api = CreateApiGateway(_pubSubOptions.Uri!);

        foreach (var connectionId in connectionIds)
        {
            await api.DeleteConnectionAsync(new DeleteConnectionRequest
            {
                ConnectionId = connectionId
            });

            await _pubSubStorage.Disconnect(hub, connectionId);
        }
   
    }
}