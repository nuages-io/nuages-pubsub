namespace Nuages.PubSub.Storage;

public interface IPubSubStorage 
{
    Task<IWebSocketConnection> CreateConnectionAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay);

    Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub);
    Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId);
    
    Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group);
    Task<bool> GroupHasConnectionsAsync(string hub, string group);
    
    Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId);
    Task<bool> UserHasConnectionsAsync(string hub, string userId);
    
    Task<bool> ConnectionExistsAsync(string hub, string connectionid);
    
    Task InitializeAsync();

    Task AddPermissionAsync(string hub, string connectionId, string permissionString);
    Task RemovePermissionAsync(string hub, string connectionId, string permissionString);
    
    Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString);
    
    Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId);
    Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId);
    Task AddUserToGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromAllGroupsAsync(string hub, string userId);
    
    Task InsertAsync(IWebSocketConnection connection);
    
    Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub);
    Task DeleteConnectionAsync(string hub, string connectionId);
    Task DeleteConnectionFromGroupsAsync(string hub, string connectionId);
}