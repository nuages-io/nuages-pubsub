using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : IPubSubStorage
{
    private readonly IWebSocketConnectionRepository _webSocketConnectionRepository;
    private readonly IWebSocketGroupConnectionRepository _webSocketGroupConnectionRepository;
    private readonly IWebSocketGroupUserRepository _webSocketGroupUserRepository;

    public MongoPubSubStorage(IWebSocketConnectionRepository webSocketConnectionRepository, 
        IWebSocketGroupConnectionRepository webSocketGroupConnectionRepository, IWebSocketGroupUserRepository webSocketGroupUserRepository)
    {
        _webSocketConnectionRepository = webSocketConnectionRepository;
        _webSocketGroupConnectionRepository = webSocketGroupConnectionRepository;
        _webSocketGroupUserRepository = webSocketGroupUserRepository;
    }

    public async Task InsertAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay = default)
    {
        var conn = new WebSocketConnection
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ConnectionId = connectionid,
            Sub = sub,
            Hub = hub,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        await _webSocketConnectionRepository.InsertOneAsync(conn);
    }

    public async Task DeleteAsync(string hub, string connectionId)
    {
        await _webSocketConnectionRepository.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == hub);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(_webSocketConnectionRepository.GetAllConnectionForHub(hub));
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var query = _webSocketGroupConnectionRepository.GetConnectionsForGroup(hub, group);
        var connections = _webSocketConnectionRepository.AsQueryable().Where(c => query.Contains(c.ConnectionId) && c.Hub == hub);
        
        return await Task.FromResult(connections);
    }
    
    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_webSocketGroupConnectionRepository.GroupHasConnections(hub, group));
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        return await Task.FromResult( _webSocketConnectionRepository.GetConnectionsForUser(hub, userId));
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        return await Task.FromResult(_webSocketConnectionRepository.UserHasConnections(hub, userId));
    }
    
    public async Task<bool> ConnectionExistsAsync(string hub, string connectionId)
    {
        return await Task.FromResult(
            _webSocketConnectionRepository.ConnectionExists(hub, connectionId)
        );
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _webSocketConnectionRepository.InitializeIndexes();
            _webSocketGroupConnectionRepository.InitializeIndexes();
            _webSocketGroupUserRepository.InitializeIndexes();
        });
    }

    public Task AddPermissionAsync(string hub, string permissionString, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task RemovePermissionAsync(string hub, string permissionString, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasPermissionAsync(string hub, string permissionString, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        throw new NotImplementedException();
    }
}