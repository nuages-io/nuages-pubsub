namespace Nuages.PubSub.Storage;

public interface IPubSubStorage
{
    Task InsertAsync(string hub, string connectionid, string sub,  TimeSpan? expireDelay = default);
    Task DeleteAsync(string hub, string connectionId);

    Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub);
    
    Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group);
    Task<bool> GroupHasConnectionsAsync(string hub, string group);
    
    Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId);
    Task<bool> UserHasConnectionsAsync(string hub, string group);
    
    Task<bool> ConnectionExistsAsync(string hub, string connectionid);
    
    Task InitializeAsync();


    Task AddPermissionAsync(string hub,string permissionString, string connectionId);
    Task RemovePermissionAsync(string hub,string permissionString, string connectionId);
    Task<bool> HasPermissionAsync(string hub,string permissionString, string connectionId);
    Task AddConnectionToGroupAsync(string hub, string group, string connectionId);
    Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId);
    Task AddUserToGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromAllGroupsAsync(string hub, string userId);
}