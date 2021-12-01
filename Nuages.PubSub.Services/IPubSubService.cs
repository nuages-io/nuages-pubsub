using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public interface IPubSubService
{
    //All
    Task<APIGatewayProxyResponse> SendToAllAsync(string hub, PubSubMessage message, List<string>? excludedIds = null);
    Task CloseAllConnectionsAsync(string hub);

    Task ConnectAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay = default);
    
    //Connection
    Task<APIGatewayProxyResponse> SendToConnectionAsync(string hub, string connectionId, PubSubMessage message);
    Task CloseConnectionAsync(string hub, string connectionId);
    Task<bool> ConnectionExistsAsync(string hub, string connectionId);
    
    //Group
    Task<APIGatewayProxyResponse> SendToGroupAsync(string hub, string group, PubSubMessage message, List<string>? excludedIds = null);
    Task CloseGroupConnectionsAsync(string hub, string group);
    Task<bool> GroupExistsAsync(string hub, string group);

    Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId);
    Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId);
    
    Task AddUserToGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromAllGroupsAsync(string hub, string userId);
    
    //User
    Task<APIGatewayProxyResponse> SendToUserAsync( string hub, string userId, PubSubMessage message, List<string>? excludedIds = null);
    Task CloseUserConnectionsAsync(string hub, string userId);
    Task<bool> UserExistsAsync(string hub, string userId);
    
    //https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
    string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = default);

    Task GrantPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null);
    Task RevokePermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null);
    Task<bool> CheckPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null);
    
    Task DisconnectAsync(string hub, string connectionId);
    
    Task<bool> CreateAckAsync(string hub, string connectionId, string? inMessageAckId);
    Task SendAckToConnectionAsync(string hub, string connectionId, string ackId, bool success, PubSubAckResult? result = null);
}


