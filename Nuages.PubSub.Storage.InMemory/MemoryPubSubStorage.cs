using Nuages.PubSub.Storage.InMemory.DataModel;

namespace Nuages.PubSub.Storage.InMemory;

public class MemoryPubSubStorage : PubSubStorgeBase<WebSocketConnection>, IPubSubStorage<WebSocketConnection>
{
    private Dictionary<string, List<IWebSocketConnection>> HubConnections { get; } = new();

    private Dictionary<string, List<WebSocketGroupConnection>> HubConnectionAndGroups { get;  } = new();

    private Dictionary<string, List<WebSocketGroupUser>> HubUsersAndGroups { get;  } = new();

    public override async Task UpdateAsync(IWebSocketConnection connection)
    {
        await Task.CompletedTask;
    }

    protected override string GetNewId()
    {
        return Guid.NewGuid().ToString();
    }
    
    public override async Task DeleteConnectionFromGroups(string hub, string connectionId)
    {
        var connectionGroup = await GetConnectionGroupAsync(hub, connectionId);
        if (connectionGroup != null)
            HubConnectionAndGroups[hub].Remove(connectionGroup);
    }

    private async Task<WebSocketGroupConnection?> GetConnectionGroupAsync(string hub, string connectionId)
    {
        var connection = HubConnectionAndGroups[hub].SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);
        return await Task.FromResult(connection);
    }

    public override async  Task DeleteConnection(string hub, string connectionId)
    {
        var connection = await GetConnectionAsync(hub, connectionId);

        if (connection != null)
            HubConnections[hub].Remove(connection);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(HubConnections[hub]);
    }

    public override async Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        var connection = HubConnections[hub].SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);
        return await Task.FromResult(connection);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var coll = HubConnectionAndGroups[hub].Where(c => c.Group == group).Select(c => c.ConnectionId);
        return await Task.FromResult(HubConnections[hub].Where(c => coll.Contains(c.ConnectionId)));
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(HubConnectionAndGroups[hub].Any(c => c.Group == group));
    }

    public override async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var conn = HubConnections[hub].Where(c => c.Sub == userId);

        return await Task.FromResult(conn);
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        return await Task.FromResult(HubConnections[hub].Any(c => c.Sub == userId));
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        return await Task.FromResult(HubConnections[hub].Any(c => c.ConnectionId == connectionid));
    }

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId)
    {
        var connection = new WebSocketGroupConnection
        {
            Id = GetNewId(),
            Group = group,
            Hub = hub,
            ConnectionId = connectionId,
            CreatedOn = DateTime.UtcNow,
            Sub = userId

        };

        if (!HubConnectionAndGroups.ContainsKey(hub))
        {
            HubConnectionAndGroups[hub] = new List<WebSocketGroupConnection>();
        }

        await Task.Run(() =>
        {
            HubConnectionAndGroups[hub].Add(connection);
        });
        
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        var exising = HubConnectionAndGroups[hub]
            .SingleOrDefault(c => c.Group == group && c.ConnectionId == connectionId);

        if (exising != null)
            HubConnectionAndGroups[hub].Remove(exising);

        await Task.CompletedTask;
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = HubUsersAndGroups[hub].AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.Sub == userId);

        if (existing == null)
        {
            var userConnection = new WebSocketGroupUser
            {
                Id = GetNewId(),
                Sub = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            HubUsersAndGroups[hub].Add(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    
    }
    
    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        HubUsersAndGroups[hub].RemoveAll( c => c.Group == group && c.Sub == userId);
        HubConnectionAndGroups[hub].RemoveAll( c => c.Group == group && c.Sub == userId);

        await Task.CompletedTask;
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        HubUsersAndGroups[hub].RemoveAll( c => c.Sub == userId);
        HubConnectionAndGroups[hub].RemoveAll( c => c.Sub == userId);
        
        await Task.CompletedTask;
    }

    public async Task Insert(WebSocketConnection connection)
    {
        await Task.Run(() =>
        {
            if (!HubConnections.ContainsKey(connection.Hub))
            {
                HubConnections[connection.Hub] = new List<IWebSocketConnection>();
            }
            
            HubConnections[connection.Hub].Add(connection);
        });
    }

    public async Task<IEnumerable<string>> GetGroupForUser(string hub, string sub)
    {
        var ids = HubUsersAndGroups[hub].Where(c => c.Sub == sub).Select(c => c.Group);

        return await Task.FromResult(ids);
    }

}