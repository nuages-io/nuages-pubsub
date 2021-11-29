using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : PubSubStorgeBase<WebSocketConnection>, IPubSubStorage<WebSocketConnection>
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
    
    public async Task<IEnumerable<string>> GetUserGroupIdsForUser(string hub, string sub)
    {
        var list = await _webSocketGroupUserRepository.GetUserGroupForUser(hub, sub);
        
        return list.Select(c => c.Group);
    }

    protected override string GetNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    public override async Task DeleteConnectionFromGroups(string hub, string connectionId)
    {
        await _webSocketGroupConnectionRepository.DeleteManyAsync(c => c.Hub== hub && c.ConnectionId == connectionId);
    }

    public override async Task DeleteConnection(string hub, string connectionId)
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

    public async Task UpdateAsync(IWebSocketConnection connection)
    {
        await _webSocketConnectionRepository.ReplaceOneAsync((WebSocketConnection) connection);
    }
    
    public async Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await _webSocketConnectionRepository.FindOneAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
    }
    
    public async Task AddPermissionAsync(string hub, string permissionString, string connectionId)
    {
        var connection = await GetConnectionAsync(hub, connectionId);
        if (connection == null)
            return;

        if (!await HasPermissionAsync(connection, permissionString))
        {
           
            connection.AddPermission(permissionString);
            
            await UpdateAsync(connection);
        }
    }

    public async Task RemovePermissionAsync(string hub, string connectionId, string permissionString)
    {
        var connection = await GetConnectionAsync(hub, connectionId);

        if (connection?.Permissions != null)
        {
            connection.Permissions.Remove(permissionString);
            
            await UpdateAsync(connection);
        }
    }

    public async Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString)
    {
        var connection = await GetConnectionAsync(hub, connectionId);

        return await HasPermissionAsync(connection, permissionString);
    }
    
    public async Task<bool> HasPermissionAsync(IWebSocketConnection? connection, string permissionString)
    {
        if (connection?.Permissions == null)
            return false;
        
        var list = new List<string> { permissionString };
        var parentPermission = permissionString.Split(".").First();
        
        if (parentPermission != permissionString)
            list.Add(parentPermission);

        return await Task.FromResult(connection.Permissions.Intersect(list).Any());
    }

    public async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
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
                CreatedOn = DateTime.UtcNow
            };

            await _webSocketGroupConnectionRepository.InsertOneAsync(groupConnection);
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _webSocketGroupConnectionRepository.DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId);
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

    private async Task AddConnectionToGroupFromUserGroups(string hub, string group, string userId)
    {
        var connections = await GetConnectionsForUserAsync(hub, userId);
        foreach (var conn in connections)
        {
            if (!_webSocketGroupConnectionRepository.AsQueryable().Any(c =>
                    c.Hub == hub && c.Group == group && c.ConnectionId == conn.ConnectionId))
            {
                await _webSocketGroupConnectionRepository.AddConnetionToGroupAsync(hub, group, conn.ConnectionId);
            }
        }
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _webSocketGroupUserRepository.DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.Id == userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _webSocketGroupUserRepository.DeleteManyAsync(c => c.Hub == hub && c.Id == userId);
    }

    public async Task Insert(WebSocketConnection connection)
    {
        await _webSocketConnectionRepository.InsertOneAsync(connection);
    }
}