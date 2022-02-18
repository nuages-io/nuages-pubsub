

namespace Nuages.PubSub.Services.Storage;

public abstract class PubSubStorgeBase<T> where T : IPubSubConnection, new()
{
    public abstract Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId);
    protected abstract Task UpdateAsync(IPubSubConnection connection);
    public abstract Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId);
    public abstract Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId);
    protected abstract Task InsertAsync(IPubSubConnection conn);
    
    protected abstract string GetNewId();
    
    public async Task<IPubSubConnection> CreateConnectionAsync(string hub, string connectionid, string userId, TimeSpan? expireDelay) 
    {
        var conn = new T
        {
           // Id = GetNewId(),
            ConnectionId = connectionid,
            UserId = userId,
            Hub = hub,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        await  InsertAsync(conn);
        
        return await Task.FromResult(conn);
    }
    
     
    public async Task AddPermissionAsync(string hub,  string connectionId, string permissionString)
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
            
            if (!connection.Permissions.Any())
                connection.Permissions = null;
            
            await UpdateAsync(connection);
        }
    }

    public async Task<bool> HasPermissionAsync(string hub, string connectionId, string permissionString)
    {
        var connection = await GetConnectionAsync(hub, connectionId);

        return await HasPermissionAsync(connection, permissionString);
    }

    private static async Task<bool> HasPermissionAsync(IPubSubConnection? connection, string permissionString)
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