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

   
}