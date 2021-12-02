using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly IPubSubConnectionRepository _pubSubConnectionRepository;
    private readonly IPubSubGroupConnectionRepository _pubSubGroupConnectionRepository;
    private readonly IPubSubGroupUserRepository _pubSubGroupUserRepository;
    private readonly IPubSubAckRepository _pubSubAckRepository;

    public MongoPubSubStorage(IPubSubConnectionRepository pubSubConnectionRepository, 
        IPubSubGroupConnectionRepository pubSubGroupConnectionRepository, IPubSubGroupUserRepository pubSubGroupUserRepository,
        IPubSubAckRepository pubSubAckRepository)
    {
        _pubSubConnectionRepository = pubSubConnectionRepository;
        _pubSubGroupConnectionRepository = pubSubGroupConnectionRepository;
        _pubSubGroupUserRepository = pubSubGroupUserRepository;
        _pubSubAckRepository = pubSubAckRepository;
    }
    
    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string sub)
    {
        var list = await _pubSubGroupUserRepository.GetGroupsForUserAsync(hub, sub);
        
        return list.Select(c => c.Group);
    }

    protected override string GetNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }

    private async Task DeleteConnectionFromAllGroupsAsync(string hub, string connectionId)
    {
        await _pubSubGroupConnectionRepository.DeleteManyAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _pubSubConnectionRepository.DeleteByConnectionIdAsync(hub, connectionId);
        
        await DeleteConnectionFromAllGroupsAsync(hub, connectionId);
    }

    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(_pubSubConnectionRepository.GetAllConnectionForHub(hub));
    }

    public async Task<IEnumerable<IPubSubConnection>> GetConnectionsForGroupAsync(string hub, string group)
    {
        var query = _pubSubGroupConnectionRepository.GetConnectionsForGroup(hub, group).ToList();
        var connections = _pubSubConnectionRepository.AsQueryable().Where(c => query.Contains(c.ConnectionId) && c.Hub == hub);
        
        return await Task.FromResult(connections);
    }
    
    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_pubSubGroupConnectionRepository.GroupHasConnections(hub, group));
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        return await Task.FromResult( _pubSubConnectionRepository.GetConnectionsForUser(hub, userId));
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        return await Task.FromResult(_pubSubConnectionRepository.UserHasConnections(hub, userId));
    }
    
    public async Task<bool> ConnectionExistsAsync(string hub, string connectionId)
    {
        return await Task.FromResult(
            _pubSubConnectionRepository.ConnectionExists(hub, connectionId)
        );
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _pubSubConnectionRepository.InitializeIndexes();
            _pubSubGroupConnectionRepository.InitializeIndexes();
            _pubSubGroupUserRepository.InitializeIndexes();
            _pubSubAckRepository.InitializeIndexes();
        });
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _pubSubConnectionRepository.ReplaceOneAsync((PubSubConnection) connection);
    }
    
    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await Task.FromResult(_pubSubConnectionRepository.GetConnectionByConnectionId(hub, connectionId));
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId)
    {
        var existing = _pubSubGroupConnectionRepository.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId);

        if (existing == null)
        {
            var groupConnection = new PubSubGroupConnection
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ConnectionId = connectionId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.UtcNow,
                Sub = userId
            };

            await _pubSubGroupConnectionRepository.InsertOneAsync(groupConnection);
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _pubSubGroupConnectionRepository.DeleteConnectionFromGroupAsync(hub, group, connectionId);
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = _pubSubGroupUserRepository.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.Sub == userId);

        if (existing == null)
        {
            var userConnection = new PubSubGroupUser
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Sub = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            await _pubSubGroupUserRepository.InsertOneAsync(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _pubSubGroupUserRepository.DeleteUserFromGroupAsync(hub, group, userId);
        await _pubSubGroupConnectionRepository.DeleteUserConnectionFromGroupAsync(hub, group, userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _pubSubGroupUserRepository.DeleteUserFromAllGroupsAsync(hub, userId);
        await _pubSubGroupConnectionRepository.DeleteAllUserConnectionsFromGroupAsync(hub, userId);
    }

    protected override async Task InsertAsync(IPubSubConnection connection)
    {
        await _pubSubConnectionRepository.InsertOneAsync((PubSubConnection) connection);
    }
    
    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        return await _pubSubAckRepository.ExistsAsync(hub, connectionId, ackId);
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        var pubSubAck = new PubSubAck
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AckId = ackId,
            ConnectionId = connectionId,
            Hub = hub
        };

        await _pubSubAckRepository.InsertOneAsync(pubSubAck);
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        return await Task.FromResult(_pubSubGroupConnectionRepository.AsQueryable()
            .Any(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId));
    }
}