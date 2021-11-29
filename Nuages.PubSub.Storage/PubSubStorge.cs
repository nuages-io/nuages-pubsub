namespace Nuages.PubSub.Storage;

public abstract class PubSubStorgeBase<T> where T : IWebSocketConnection, new()
{
    public abstract Task DeleteConnectionFromGroups(string hub, string connectionId);
    public abstract Task DeleteConnection(string hub, string connectionId);
    public abstract Task<IWebSocketConnection?> GetConnectionAsync(string hub, string connectionId);
    public abstract Task UpdateAsync(IWebSocketConnection connection);
    public abstract Task<IEnumerable<IWebSocketConnection>> GetConnectionsForUserAsync(string hub, string userId);
    public abstract Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId);
    
    protected abstract string GetNewId();
    
    public async Task<T> CreateConnectionAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay) 
    {
        var conn = new T
        {
            Id = GetNewId(),
            ConnectionId = connectionid,
            Sub = sub,
            Hub = hub,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        return await Task.FromResult(conn);
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
    
    protected async Task AddConnectionToGroupFromUserGroups(string hub, string group, string userId)
    {
        var connections = await GetConnectionsForUserAsync(hub, userId);
        foreach (var conn in connections)
        {
            await AddConnectionToGroupAsync(hub, group, conn.ConnectionId, userId);
        }
    }

}