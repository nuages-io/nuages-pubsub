using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Amazon;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class PubSubService : IPubSubService
{
    private readonly IPubSubStorage _pubSubStorage;

    public PubSubService(IPubSubStorage pubSubStorage)
    {
        _pubSubStorage = pubSubStorage;
    }
    
    public virtual async Task<APIGatewayProxyResponse> SendToConnectionAsync(string url, string hub,  string connectionId, string content)
    {
        await SendMessageAsync(url, hub, new List<string>{ connectionId } , content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public Task<APIGatewayProxyResponse> SendToGroupAsync(string url, string hub, string @group, string content)
    {
        throw new NotImplementedException();
    }

    public Task<APIGatewayProxyResponse> SendToUserAsync(string url, string hub, string userId, string content)
    {
        throw new NotImplementedException();
    }

    public string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = null)
    {
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new []
            {
                new Claim("sub", userId)
            }),
            Expires = DateTime.UtcNow.Add(expireDelay ?? new TimeSpan(7,0,0)),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var roleList = roles.ToList();
        
        if (roleList.Any())
        {
            tokenDescriptor.Claims ??= new Dictionary<string, object>();

            foreach (var role in roleList)
            {
                tokenDescriptor.Claims.Add(new KeyValuePair<string, object>(role, "true"));
            }
        }
        
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string url, string hub, string content)
    {
        var ids = _pubSubStorage.GetAllConnectionIds(hub);
        
        await SendMessageAsync(url, hub, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    

    protected virtual async Task SendMessageAsync(string url, string hub, IEnumerable<string> connectionIds,  string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        using var apiGateway = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            AuthenticationRegion = RegionEndpoint.CACentral1.SystemName,
            ServiceURL = url
        });
        
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
                    await _pubSubStorage.DeleteAsync(hub, postConnectionRequest.ConnectionId);
                }
               
            }
        }
        
       
    }
}