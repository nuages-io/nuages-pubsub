using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Storage.EntityFramework.DataModel;

namespace Nuages.PubSub.Storage.EntityFramework;


public class PubSubStorageEntityFramework<T> : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
    where T : PubSubDbContext
{
    private readonly T _context;

    public PubSubStorageEntityFramework(T context)
    {
        _context = context;
    }

    public void Initialize()
    {
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            _context.Database.EnsureDeleted();
        
        _context.Database.EnsureCreated();
    }

    [ExcludeFromCodeCoverage]
    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _context.SaveChangesAsync();
    }

    public async  Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _context.DeleteConnectionAsync(hub, connectionId);
        
        await _context.DeleteConnectionFromAllGroupConnectionAsync(hub, connectionId);

        await _context.DeleteAckForConnectionAsync(hub, connectionId);
    }
    
    public async IAsyncEnumerable<IPubSubConnection> GetAllConnectionAsync(string hub)
    {
        var list = _context.Connections.Where(c => c.Hub == hub).ToAsyncEnumerable();

        await foreach (var item in list)
        {
            yield return item;
        }
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await _context.Connections.SingleOrDefaultAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
    }

    public async IAsyncEnumerable<string> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        var list =  _context.GroupConnections.Where(g => g.Hub == hub &&  g.GroupName == group).ToAsyncEnumerable();

        await foreach (var item in list)
        {
            if ( !item.IsExpired())
                yield return item.ConnectionId;
        }
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_context.GroupConnections.Any(c => c.Hub == hub && c.GroupName == group));
    }

    public override async IAsyncEnumerable<IPubSubConnection> GetConnectionsForUserAsync(string hub, string userId)
    {
        var coll = _context.Connections.Where(c => c.Hub == hub && c.UserId == userId).ToAsyncEnumerable();

        await foreach (var item in coll)
            yield return item;
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        return await Task.FromResult(_context.Connections.Any(c => c.Hub == hub && c.UserId == userId));
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        return await Task.FromResult(_context.Connections.Any(c => c.Hub == hub && c.ConnectionId == connectionid));
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        var conn = await GetConnectionAsync(hub, connectionId);

        if (conn != null)
        {
            var connection = new PubSubGroupConnection
            {
                GroupName = group,
                Hub = hub,
                ConnectionId = connectionId,
                CreatedOn = DateTime.UtcNow,
                UserId = conn.UserId,
                ExpireOn = conn.ExpireOn
            };
            
            _context.GroupConnections.Add(connection);
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _context.DeleteConnectionFromGroupConnectionAsync(hub, group, connectionId);
    }

    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        return await Task.FromResult(_context.GroupUsers.Any(u => u.Hub == hub && u.GroupName == group && u.UserId == userId));
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = _context.GroupUsers
            .SingleOrDefault(c => c.Hub == hub && c.GroupName == group && c.UserId == userId);

        if (existing == null)
        {
            var userConnection = new PubSubGroupUser
            {
                UserId = userId,
                GroupName = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            _context.GroupUsers.Add(userConnection);
            
            await _context.SaveChangesAsync();
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }
    
    
    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _context.DeleteUserFromGroupConnectionAsync(hub, group, userId);
        
        await _context.DeleteUserFromGroupUserAsync(hub, group, userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _context.DeleteUserFromAllGroupConnectionsAsync(hub, userId);

        await _context.DeleteUserFromAllGroupUsersAsync(hub, userId);
    }

    protected override async Task InsertAsync(IPubSubConnection connection)
    {
        await Task.Run(() =>
        {
            var conn = (PubSubConnection)connection;
            _context.Connections.Add(conn);

            _context.SaveChanges();
        });
    }

    public async IAsyncEnumerable<string> GetGroupsForUser(string hub, string userId)
    {
        var ids = _context.GroupUsers.Where(c => c.Hub == hub && c.UserId == userId).ToAsyncEnumerable();

        await foreach (var item in ids)
            yield return item.GroupName;
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        return await Task.Run(() =>
        {
            return _context.Acks.Any(c => c.Hub == hub && c.ConnectionId == connectionId && c.AckId == ackId);
        });
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        await Task.Run(() =>
        {
            var pubSubAck = new PubSubAck
            {
                Hub = hub,
                ConnectionId = connectionId,
                AckId = ackId
            };
            
            _context.Acks.Add(pubSubAck);
            _context.SaveChanges();
        }); 
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        return await Task.FromResult(_context.GroupConnections.Any(c => c.Hub == hub && c.GroupName == group && c.ConnectionId == connectionId));
    }

    [ExcludeFromCodeCoverage]
    public void TruncateAllData()
    {
        _context.Database.EnsureDeleted();
    }
}
