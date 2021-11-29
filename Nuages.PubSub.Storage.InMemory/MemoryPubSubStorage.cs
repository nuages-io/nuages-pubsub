using System.Diagnostics.CodeAnalysis;
using Nuages.PubSub.Storage.InMemory.DataModel;

namespace Nuages.PubSub.Storage.InMemory;

public class MemoryPubSubStorage : PubSubStorgeBase<WebSocketConnection>, IPubSubStorage
{
    private Dictionary<string, List<IWebSocketConnection>> HubConnections { get; } = new();

    private Dictionary<string, List<WebSocketGroupConnection>> HubConnectionsAndGroups { get;  } = new();

    private Dictionary<string, List<WebSocketGroupUser>> HubUsersAndGroups { get;  } = new();

    [ExcludeFromCodeCoverage]
    protected override async Task UpdateAsync(IWebSocketConnection connection)
    {
        await Task.CompletedTask;
    }

    private List<IWebSocketConnection> GetHubConnections(string hub)
    {
        if (!HubConnections.ContainsKey(hub))
            HubConnections[hub] = new List<IWebSocketConnection>();

        return HubConnections[hub];
    }

    private List<WebSocketGroupConnection> GetHubConnectionsAndGroups(string hub)
    {
        if (!HubConnectionsAndGroups.ContainsKey(hub))
            HubConnectionsAndGroups[hub] = new List<WebSocketGroupConnection>();

        return HubConnectionsAndGroups[hub];
    }

    private List<WebSocketGroupUser> GetHubUsersAndGroups(string hub)
    {
        if (!HubUsersAndGroups.ContainsKey(hub))
            HubUsersAndGroups[hub] = new List<WebSocketGroupUser>();

        return HubUsersAndGroups[hub];
    }
    
    protected override string GetNewId()
    {
        return Guid.NewGuid().ToString();
    }
    
    public async Task DeleteConnectionFromGroupsAsync(string hub, string connectionId)
    {

        GetHubConnectionsAndGroups(hub).RemoveAll(c => c.Hub == hub && c.ConnectionId == connectionId);

        await Task.CompletedTask;
    }

    public async  Task DeleteConnectionAsync(string hub, string connectionId)
    {
        GetHubConnections(hub).RemoveAll(c => c.Hub == hub && c.ConnectionId == connectionId);

        await DeleteConnectionFromGroupsAsync(hub, connectionId);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(GetHubConnections(hub));
    }

    public override async Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        var connection = GetHubConnections(hub).SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);
        return await Task.FromResult(connection);
    }

    public async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var coll = GetHubConnectionsAndGroups(hub).Where(c => c.Group == group).Select(c => c.ConnectionId);
        return await Task.FromResult(GetHubConnections(hub).Where(c => coll.Contains(c.ConnectionId)));
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
       
        return await Task.FromResult(GetHubConnectionsAndGroups(hub).Any(c => c.Group == group));
    }

    public override async Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var conn = GetHubConnections(hub).Where(c => c.Sub == userId);

        return await Task.FromResult(conn);
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        
        return await Task.FromResult(GetHubConnections(hub).Any(c => c.Sub == userId));
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        return await Task.FromResult(GetHubConnections(hub).Any(c => c.ConnectionId == connectionid));
    }

    [ExcludeFromCodeCoverage]
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

        await Task.Run(() =>
        {
            GetHubConnectionsAndGroups(hub).Add(connection);
        });
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
       
        var exising = GetHubConnectionsAndGroups(hub)
            .SingleOrDefault(c => c.Group == group && c.ConnectionId == connectionId);

        if (exising != null)
            GetHubConnectionsAndGroups(hub).Remove(exising);

        await Task.CompletedTask;
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = GetHubUsersAndGroups(hub).AsQueryable()
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

            GetHubUsersAndGroups(hub).Add(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    
    }
    
    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        
        GetHubUsersAndGroups(hub).RemoveAll( c => c.Group == group && c.Sub == userId);
        
        GetHubConnectionsAndGroups(hub).RemoveAll( c => c.Group == group && c.Sub == userId);

        await Task.CompletedTask;
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        GetHubUsersAndGroups(hub).RemoveAll( c => c.Sub == userId);

        GetHubConnectionsAndGroups(hub).RemoveAll( c => c.Sub == userId);
        
        await Task.CompletedTask;
    }

    protected override async Task InsertAsync(IWebSocketConnection connection)
    {
        await Task.Run(() =>
        {
            GetHubConnections(connection.Hub).Add(connection);
        });
    }

    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub)
    {
        var ids = GetHubUsersAndGroups(hub).Where(c => c.Sub == sub).Select(c => c.Group);

        return await Task.FromResult(ids);
    }

}