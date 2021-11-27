namespace Nuages.PubSub.Storage;

public interface IPubSubStorage
{
    Task InsertAsync(string audience, string connectionid, string sub,  TimeSpan? expireDelay = default);
    Task DeleteAsync(string audience, string connectionId);

    Task<IEnumerable<string>> GetAllConnectionIdsAsync(string audience);
    
    Task<IEnumerable<string>> GetConnectionIdsForGroupAsync(string audience, string group);
    Task<bool> GroupHasConnectionsAsync(string audience, string group);
    
    Task<IEnumerable<string>> GetConnectionIdsForUserAsync(string audience, string userId);
    Task<bool> UserHasConnectionsAsync(string audience, string group);
    
    Task<bool> ConnectionExistsAsync(string audience, string connectionid);
    
    Task InitializeAsync();


    Task AddPermissionAsync(string audience,string permissionString, string connectionId);
    Task RemovePermissionAsync(string audience,string permissionString, string connectionId);
    Task<bool> HasPermissionAsync(string audience,string permissionString, string connectionId);
    Task AddConnectionToGroupAsync(string audience, string group, string connectionId);
    Task RemoveConnectionFromGroupAsync(string audience, string group, string connectionId);
    Task AddUserToGroupAsync(string audience, string group, string userId);
    Task RemoveUserFromGroupAsync(string audience, string group, string userId);
    Task RemoveUserFromAllGroupsAsync(string audience, string userId);
}