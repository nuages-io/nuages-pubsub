namespace Nuages.PubSub.Storage;

public interface IPubSubStorage 
{
    Task<IPubSubConnection> CreateConnectionAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay);

    Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub);
    Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId);
    
    Task<IEnumerable<IPubSubConnection>> GetConnectionsForGroupAsync(string hub, string group);
    Task<bool> GroupHasConnectionsAsync(string hub, string group);
    
    Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId);
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
    
    Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub);
    Task DeleteConnectionAsync(string hub, string connectionId);

    Task<bool> ExistAckAsync(string hub, string connectionId, string ackId);
    Task InsertAckAsync(string hub, string connectionId, string ackId);


    Task<bool> IsConnectionInGroup(string hub, string @group, string connectionId);
}