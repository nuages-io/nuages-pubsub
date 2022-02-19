using System.Diagnostics.CodeAnalysis;
using Nuages.PubSub.Services.Storage.InMemory.DataModel;

namespace Nuages.PubSub.Services.Storage.InMemory;


public class MemoryPubSubStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private Dictionary<string, List<IPubSubConnection>> HubConnections { get; } = new();

    private Dictionary<string, List<PubSubGroupConnection>> HubConnectionsAndGroups { get;  } = new();

    private Dictionary<string, List<PubSubGroupUser>> HubUsersAndGroups { get;  } = new();

    private Dictionary<string, List<PubSubAck>> HubAck { get;  } = new();
    
    [ExcludeFromCodeCoverage]
    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await Task.CompletedTask;
    }

    private List<IPubSubConnection> GetHubConnections(string hub)
    {
        if (!HubConnections.ContainsKey(hub))
            HubConnections[hub] = new List<IPubSubConnection>();

        return HubConnections[hub];
    }

    private List<PubSubGroupConnection> GetHubConnectionsAndGroups(string hub)
    {
        if (!HubConnectionsAndGroups.ContainsKey(hub))
            HubConnectionsAndGroups[hub] = new List<PubSubGroupConnection>();

        return HubConnectionsAndGroups[hub];
    }

    private List<PubSubGroupUser> GetHubUsersAndGroups(string hub)
    {
        if (!HubUsersAndGroups.ContainsKey(hub))
            HubUsersAndGroups[hub] = new List<PubSubGroupUser>();

        return HubUsersAndGroups[hub];
    }
    
    private List<PubSubAck> GetHubPubSubAck(string hub)
    {
        if (!HubAck.ContainsKey(hub))
            HubAck[hub] = new List<PubSubAck>();

        return HubAck[hub];
    }

    private async Task DeleteConnectionFromAllGroupsAsync(string hub, string connectionId)
    {

        GetHubConnectionsAndGroups(hub).RemoveAll(c => c.Hub == hub && c.ConnectionId == connectionId);

        await Task.CompletedTask;
    }

    public async  Task DeleteConnectionAsync(string hub, string connectionId)
    {
        GetHubConnections(hub).RemoveAll(c => c.Hub == hub && c.ConnectionId == connectionId);

        await DeleteConnectionFromAllGroupsAsync(hub, connectionId);
    }


    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(GetHubConnections(hub));
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        var connection = GetHubConnections(hub).SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId);
        return await Task.FromResult(connection);
    }

    public async Task<IEnumerable<string>> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        return await Task.FromResult(GetHubConnectionsAndGroups(hub).Where(c => c.Group == group && !c.IsExpired())
            .Select(c => c.ConnectionId));
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
       
        return await Task.FromResult(GetHubConnectionsAndGroups(hub).Any(c => c.Group == group));
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        var conn = GetHubConnections(hub).Where(c => c.UserId == userId);

        return await Task.FromResult(conn);
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        
        return await Task.FromResult(GetHubConnections(hub).Any(c => c.UserId == userId));
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        return await Task.FromResult(GetHubConnections(hub).Any(c => c.ConnectionId == connectionid));
    }


    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        var conn = await GetConnectionAsync(hub, connectionId);

        if (conn != null)
        {
            var connection = new PubSubGroupConnection
            {
                Id = Guid.NewGuid().ToString(),
                Group = group,
                Hub = hub,
                ConnectionId = connectionId,
                CreatedOn = DateTime.UtcNow,
                UserId = conn.UserId,
                ExpireOn = conn.ExpireOn
            };
            
            GetHubConnectionsAndGroups(hub).Add(connection);
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
       
        var exising = GetHubConnectionsAndGroups(hub)
            .SingleOrDefault(c => c.Group == group && c.ConnectionId == connectionId);

        if (exising != null)
            GetHubConnectionsAndGroups(hub).Remove(exising);

        await Task.CompletedTask;
    }

    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        return await Task.FromResult(GetHubUsersAndGroups(hub).Any(u => u.Group == group && u.UserId == userId));
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = GetHubUsersAndGroups(hub).AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.UserId == userId);

        if (existing == null)
        {
            var userConnection = new PubSubGroupUser
            {
                UserId = userId,
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
        
        GetHubUsersAndGroups(hub).RemoveAll( c => c.Group == group && c.UserId == userId);
        
        GetHubConnectionsAndGroups(hub).RemoveAll( c => c.Group == group && c.UserId == userId);

        await Task.CompletedTask;
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        GetHubUsersAndGroups(hub).RemoveAll( c => c.UserId == userId);

        GetHubConnectionsAndGroups(hub).RemoveAll( c => c.UserId == userId);
        
        await Task.CompletedTask;
    }

    protected override async Task InsertAsync(IPubSubConnection connection)
    {
        await Task.Run(() =>
        {
            GetHubConnections(connection.Hub).Add(connection);
        });
    }

    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string userId)
    {
        var ids = GetHubUsersAndGroups(hub).Where(c => c.UserId == userId).Select(c => c.Group);

        return await Task.FromResult(ids);
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        return await Task.Run(() =>
        {
            return GetHubPubSubAck(hub).Any(c => c.ConnectionId == connectionId && c.AckId == ackId);
        });
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        await Task.Run(() =>
        {
            var pubSubAck = new PubSubAck
            {
                ConnectionId = connectionId,
                AckId = ackId
            };
            
            GetHubPubSubAck(hub).Add(pubSubAck);
        }); 
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        return await Task.FromResult(GetHubConnectionsAndGroups(hub).Any(c => c.Group == group && c.ConnectionId == connectionId));
    }

    [ExcludeFromCodeCoverage]
    public void DeleteAll()
    {
        HubAck.Clear();
        HubConnections.Clear();
        HubConnectionsAndGroups.Clear();
        HubUsersAndGroups.Clear();
    }
}
