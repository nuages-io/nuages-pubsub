using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
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
    private readonly IAmazonApiGatewayManagementApiClientProvider _apiClientientProvider;
    private readonly PubSubOptions _pubSubOptions;

    public PubSubService(IPubSubStorage pubSubStorage, IOptions<PubSubOptions> pubSubOptions, IAmazonApiGatewayManagementApiClientProvider apiClientientProvider)
    {
        _pubSubStorage = pubSubStorage;
        _apiClientientProvider = apiClientientProvider;
        _pubSubOptions = pubSubOptions.Value;
    }
    
    private IAmazonApiGatewayManagementApi CreateApiGateway(string url)
    {
        return _apiClientientProvider.Create(url, RegionEndpoint.CACentral1.SystemName);
    }

    public string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default)
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
            Audience = audience,
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

    public async Task ConnectAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay = default)
    {
        await _pubSubStorage.CreateConnectionAsync(hub, connectionid, sub, expireDelay);

        var groups = await  _pubSubStorage.GetGroupsForUser(hub, sub);
        foreach (var g in groups)
        {
            await  _pubSubStorage.AddConnectionToGroupAsync(hub,g, connectionid, sub);
        }
    }

    public async Task DisconnectAsync(string hub, string connectionId)
    {
        await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
    }
    
    public async Task GrantPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        var permissionString = GetPermissionString(permission, target);

        await _pubSubStorage.AddPermissionAsync(hub, connectionId, permissionString);
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

        await _pubSubStorage.RemovePermissionAsync(hub, connectionId, permissionString);
    }

    public async Task<bool> CheckPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        var permissionString = GetPermissionString(permission, target);

        return await _pubSubStorage.HasPermissionAsync(hub ,connectionId, permissionString);
    }

    protected virtual async Task SendMessageAsync(string hub, IEnumerable<string> connectionIds,  PubSubMessage message)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));

        Console.WriteLine($"_pubSubOptions.Uri {_pubSubOptions.Uri}");
        
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
                    
                Console.WriteLine($"PostToConnectionAsync : {connectionId}");
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
                    await DisconnectAsync(hub, postConnectionRequest.ConnectionId);
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

            await DisconnectAsync(hub, connectionId);
        }
   
    }
}