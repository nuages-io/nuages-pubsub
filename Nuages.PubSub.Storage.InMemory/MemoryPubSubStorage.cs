using Nuages.PubSub.Storage.InMemory.DataModel;

namespace Nuages.PubSub.Storage.InMemory;

public class MemoryPubSubStorage : PubSubStorgeBase<WebSocketConnection>, IPubSubStorage<WebSocketConnection>
{
    public List<IWebSocketConnection> Connections { get; set; } = new();
    public List<WebSocketGroupConnection> ConnectionAndGroups { get; set; } = new();
    

    public Task Connect(string hub, string connectionid, string sub, TimeSpan? expireDelay = default)
    {
        throw new NotImplementedException();
    }

    protected override string GetNewId()
    {
        return Guid.NewGuid().ToString();
    }
    
    public override async Task DeleteConnectionFromGroups(string hub, string connectionId)
    {
        var connectionGroup = await GetConnectionGroupAsync(hub, connectionId);

        if (connectionGroup != null)
            ConnectionAndGroups.Remove(connectionGroup);
    }

    private async Task<WebSocketGroupConnection?> GetConnectionGroupAsync(string hub, string connectionId)
    {
        var connection = ConnectionAndGroups.SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);

        return await Task.FromResult(connection);
    }

    public override async  Task DeleteConnection(string hub, string connectionId)
    {
        var connection = await GetConnectionAsync(hub, connectionId);

        if (connection != null)
            Connections.Remove(connection);
    }

    public Task<IEnumerable<IWebSocketConnection>> GetAllConnectionAsync(string hub)
    {
        throw new NotImplementedException();
    }

    public async Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        var connection = Connections.SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);

        return await Task.FromResult(connection);
    }

    public Task<IEnumerable<IWebSocketConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UserHasConnectionsAsync(string hub, string group)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        throw new NotImplementedException();
    }

    public Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddPermissionAsync(string hub, string connectionId, string permissionString)
    {
        throw new NotImplementedException();
    }

    public Task RemovePermissionAsync(string hub, string connectionId, string permissionString)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString)
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

    public async Task Insert(WebSocketConnection connection)
    {

        await Task.Run(() =>
        {
            Connections.Add(connection);
        });

    }

    public Task<IEnumerable<string>> GetUserGroupIdsForUser(string hub, string sub)
    {
        throw new NotImplementedException();
    }

    public Task AddConnetionToGroupAsync(string hub, string connectionid, string group)
    {
        throw new NotImplementedException();
    }
}