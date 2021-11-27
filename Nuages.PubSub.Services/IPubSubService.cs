using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public interface IPubSubService
{
    //All
    Task<APIGatewayProxyResponse> SendToAllAsync(string url, string audience, string content);
    Task CloseAllConnectionsAsync(string url, string audience);
    
    //Connection
    Task<APIGatewayProxyResponse> SendToConnectionAsync(string url, string audience, string connectionId, string content);
    Task CloseConnectionAsync(string url, string audience, string connectionId);
    Task<bool> ConnectionExistsAsync(string audience, string connectionId);
    
    //Group
    Task<APIGatewayProxyResponse> SendToGroupAsync(string url, string audience, string group, string content);
    Task CloseGroupConnectionsAsync(string url, string audience, string group);
    Task<bool> GroupExistsAsync(string audience, string group);
    
    //User
    Task<APIGatewayProxyResponse> SendToUserAsync(string url, string audience, string userId, string content);
    Task CloseUserConnectionsAsync(string url, string audience, string userId);
    Task<bool> UserExistsAsync(string audience, string userId);
    
    //https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
    string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default);
}


