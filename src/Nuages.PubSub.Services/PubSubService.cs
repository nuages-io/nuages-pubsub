using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Services.Storage;

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
        
        _pubSubStorage.Initialize();
    }
    
    private IAmazonApiGatewayManagementApi CreateApiGateway(string url)
    {
        return _apiClientientProvider.Create(url, _pubSubOptions.Region);
    }

    public string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, int? expiresAfterSeconds = null)
    {
        expiresAfterSeconds ??= 60 * 60 * 24;
        
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("sub", userId)
            }),
            Expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(expiresAfterSeconds.Value )),
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

    public async Task ConnectAsync(string hub, string connectionid, string userId, int? expiresAfterSeconds = null)
    {
        await _pubSubStorage.CreateConnectionAsync(hub, connectionid, userId, expiresAfterSeconds);

        var groups =  _pubSubStorage.GetGroupsForUser(hub, userId);
        await foreach (var g in groups)
        {
            await  _pubSubStorage.AddConnectionToGroupAsync(hub,g, connectionid);
        }
        
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

    protected virtual async Task SendMessageAsync(string hub, IAsyncEnumerable<string> connectionIds,  PubSubMessage message)
    {
        var text = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        
        Console.WriteLine($"text: {text}");
        
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

        Console.WriteLine($"_pubSubOptions.Uri {_pubSubOptions.Uri}");
        
        using var apiGateway = CreateApiGateway(_pubSubOptions.Uri!);
        
        await foreach (var connectionId in connectionIds)
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
            catch (GoneException)
            {
                await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
                break;
            }
            catch (AmazonServiceException e)
            {
                Console.WriteLine(e.Message);
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our collection.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
                }
            }
        }
    }

    private async Task CloseConnectionsAsync(string hub, IAsyncEnumerable<string> connectionIds)
    {
        var api = CreateApiGateway(_pubSubOptions.Uri!);

        await foreach (var connectionId in connectionIds)
        {
            try
            {
                await api.DeleteConnectionAsync(new DeleteConnectionRequest
                {
                    ConnectionId = connectionId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
        }
    }
    
    public async Task<bool> CreateAckAsync(string hub, string connectionId, string? ackId)
    {
        if (string.IsNullOrEmpty(ackId) || ackId == "$")
            return true;

        if (await _pubSubStorage.ExistAckAsync(hub, connectionId, ackId))
            return false;

        await _pubSubStorage.InsertAckAsync(hub, connectionId, ackId);
        
        return true;
    }

    public async Task SendAckToConnectionAsync(string hub, string connectionId, string ackId, bool success, PubSubAckResult? result = null)
    {
        var message = new PubSubMessage
        {
            type = "ack",
            ackId = ackId,
            error = success ? null : result.ToString(),
            success = success
        };

        await SendToConnectionAsync(hub, connectionId, message);
    }
}