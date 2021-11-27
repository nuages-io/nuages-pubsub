using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Amazon;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using Microsoft.IdentityModel.Tokens;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService : IPubSubService
{
    private readonly IPubSubStorage _pubSubStorage;

    public PubSubService(IPubSubStorage pubSubStorage)
    {
        _pubSubStorage = pubSubStorage;
    }
    
    private static AmazonApiGatewayManagementApiClient CreateApiGateway(string url)
    {
        return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            AuthenticationRegion = RegionEndpoint.CACentral1.SystemName,
            ServiceURL = url
        });
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

    protected virtual async Task SendMessageAsync(string url, string audience, IEnumerable<string> connectionIds,  string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        using var apiGateway = CreateApiGateway(url);
        
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
                    await _pubSubStorage.DeleteAsync(audience, postConnectionRequest.ConnectionId);
                }
            }
        }
    }
    
    private async Task CloseConnectionsAsync(string url, string audience, IEnumerable<string> connectionIds)
    {
        var api = CreateApiGateway(url);

        foreach (var connectionId in connectionIds)
        {
            await api.DeleteConnectionAsync(new DeleteConnectionRequest
            {
                ConnectionId = connectionId
            });

            await _pubSubStorage.DeleteAsync(audience, connectionId);
        }
   
    }
}