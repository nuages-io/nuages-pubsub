
namespace Nuages.PubSub.Services.Storage;

public interface IPubSubStorage 
{
    Task<IPubSubConnection> CreateConnectionAsync(string hub, string connectionid, string userId, int? expiresAfterSeconds);

    //Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub);
    //IEnumerable<IPubSubConnection> GetAllConnections(string hub);
    IAsyncEnumerable<IPubSubConnection> GetAllConnectionAsync(string hub);
    
    Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId);
    
    IAsyncEnumerable<string> GetConnectionsIdsForGroupAsync(string hub, string group);
    Task<bool> GroupHasConnectionsAsync(string hub, string group);
    
    IAsyncEnumerable<IPubSubConnection> GetConnectionsForUserAsync(string hub, string userId);
    Task<bool> UserHasConnectionsAsync(string hub, string userId);
    Task<bool> ConnectionExistsAsync(string hub, string connectionid);
    
    Task AddPermissionAsync(string hub, string connectionId, string permissionString);
    Task RemovePermissionAsync(string hub, string connectionId, string permissionString);
    
    Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString);
    
    Task AddConnectionToGroupAsync(string hub, string group, string connectionId);
    Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId);
    
    Task<bool> IsUserInGroupAsync(string hub, string group, string userId);
    
    Task AddUserToGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromGroupAsync(string hub, string group, string userId);
    Task RemoveUserFromAllGroupsAsync(string hub, string userId);
    
    IAsyncEnumerable<string> GetGroupsForUser(string hub, string userId);
    Task DeleteConnectionAsync(string hub, string connectionId);

    Task<bool> ExistAckAsync(string hub, string connectionId, string ackId);
    Task InsertAckAsync(string hub, string connectionId, string ackId);


    Task<bool> IsConnectionInGroup(string hub, string group, string connectionId);

    void DeleteAll();
}