using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : PubSubStorgeBase<WebSocketConnection>, IPubSubStorage
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
    
    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub)
    {
        var list = await _webSocketGroupUserRepository.GetGroupsForUserAsync(hub, sub);
        
        return list.Select(c => c.Group);
    }

    protected override string GetNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    public async Task DeleteConnectionFromGroupsAsync(string hub, string connectionId)
    {
        await _webSocketGroupConnectionRepository.DeleteManyAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _webSocketConnectionRepository.DeleteByConnectionIdAsync(hub, connectionId);
        
        await DeleteConnectionFromGroupsAsync(hub, connectionId);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(_webSocketConnectionRepository.GetAllConnectionForHub(hub));
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var query = _webSocketGroupConnectionRepository.GetConnectionsForGroup(hub, group).ToList();
        var connections = _webSocketConnectionRepository.AsQueryable().Where(c => query.Contains(c.ConnectionId) && c.Hub == hub);
        
        return await Task.FromResult(connections);
    }
    
    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_webSocketGroupConnectionRepository.GroupHasConnections(hub, group));
    }

    public override async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId)
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

    protected override async Task UpdateAsync(IWebSocketConnection connection)
    {
        await _webSocketConnectionRepository.ReplaceOneAsync((WebSocketConnection) connection);
    }
    
    public override async Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await Task.FromResult(_webSocketConnectionRepository.GetConnectionByConnectionId(hub, connectionId));
    }
   

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId)
    {
        var existing = _webSocketGroupConnectionRepository.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId);

        if (existing == null)
        {
            var groupConnection = new WebSocketGroupConnection
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ConnectionId = connectionId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.UtcNow,
                Sub = userId
            };

            await _webSocketGroupConnectionRepository.InsertOneAsync(groupConnection);
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _webSocketGroupConnectionRepository.DeleteConnectionFromGroupAsync(hub, group, connectionId);
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = _webSocketGroupUserRepository.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.Sub == userId);

        if (existing == null)
        {
            var userConnection = new WebSocketGroupUser
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Sub = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            await _webSocketGroupUserRepository.InsertOneAsync(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }


    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _webSocketGroupUserRepository.DeleteUserFromGroupAsync(hub, group, userId);
        await _webSocketGroupConnectionRepository.DeleteUserConnectionFromGroupAsync(hub, group, userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _webSocketGroupUserRepository.DeleteUserFromAllGroupsAsync(hub, userId);
        await _webSocketGroupConnectionRepository.DeleteAllUserConnectionsFromGroupAsync(hub, userId);
    }

    public async Task InsertAsync(IWebSocketConnection connection)
    {
        await _webSocketConnectionRepository.InsertOneAsync((WebSocketConnection) connection);
    }
}