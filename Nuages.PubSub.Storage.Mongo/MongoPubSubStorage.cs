using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private readonly IMongoCollection<PubSubConnection> _pubSubConnectionCollection;
    private readonly IMongoCollection<PubSubGroupConnection> _pubSubGroupConnectionCollection;
    private readonly IMongoCollection<PubSubGroupUser> _pubSubGroupUserCollection;
    private readonly IMongoCollection<PubSubAck> _pubSubAckCollection;
    
    public MongoPubSubStorage(IOptions<PubSubMongoOptions> options, IOptions<PubSubOptions> pubSubOptions)
    {
        var mongoOptions = options.Value;
        
        var connectionString =  mongoOptions.ConnectionString;
        var dbName = mongoOptions.DatabaseName;
        
        var mongoCLient = new MongoClient(connectionString);
        var database = mongoCLient.GetDatabase(dbName);

        var prefix = pubSubOptions.Value.TableNamePrefix ?? "";
        _pubSubConnectionCollection = database.GetCollection<PubSubConnection>(prefix + "pub_sub_connection");
        _pubSubGroupConnectionCollection = database.GetCollection<PubSubGroupConnection>(prefix + "pub_sub_group_connection");
        _pubSubGroupUserCollection = database.GetCollection<PubSubGroupUser>(prefix + "pub_sub_group_user");
        _pubSubAckCollection = database.GetCollection<PubSubAck>(prefix + "pub_sub_ack");

        Initialize();
    }
    
    
    public async Task<IEnumerable<IPubSubConnection>> GetAllConnectionAsync(string hub)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable().Where(c => c.Hub == hub));
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable().SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId));
    }

    public async Task<IEnumerable<string>> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        return await Task.FromResult(_pubSubGroupConnectionCollection.AsQueryable()
            .Where(c => c.Hub == hub && c.Group == group).ToList().Where(c => !c.IsExpired()).Select(c => c.ConnectionId));
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_pubSubGroupConnectionCollection.AsQueryable().Any(c => c.Hub == hub && c.Group == group));
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        await _pubSubConnectionCollection.ReplaceOneAsync(doc => doc.Id ==connection.Id, (PubSubConnection) connection);
    }

    public override async Task<IEnumerable<IPubSubConnection>> GetConnectionsForUserAsync(string hub, string userId)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable().Where(c => c.Hub == hub && c.UserId == userId));
    }

    public async Task<bool> UserHasConnectionsAsync(string hub, string userId)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable().Any(c => c.Hub == hub && c.UserId == userId));
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionid)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable()
            .Any(c => c.Hub == hub && c.ConnectionId == connectionid));
    }

    private void Initialize()
    {
         _pubSubAckCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubAck>(
                Builders<PubSubAck>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.ConnectionId)
                    .Ascending(p => p.AckId)
                , new CreateIndexOptions
                {
                    Name = "UK_Id",
                    Unique = true
                })
        );
        
         _pubSubConnectionCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubConnection>(
                Builders<PubSubConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.ConnectionId)
                , new CreateIndexOptions
                {
                    Name = "UK_Id",
                    Unique = true
                })
        );
        
         _pubSubConnectionCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubConnection>(
                Builders<PubSubConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                , new CreateIndexOptions
                {
                    Name = "IX_Hub",
                    Unique = false
                })
        );
        
         _pubSubConnectionCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubConnection>(
                Builders<PubSubConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_Sub",
                    Unique = false
                })
        );

        
         _pubSubGroupConnectionCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupConnection>(
                Builders<PubSubGroupConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                    .Ascending(p => p.ConnectionId)
                , new CreateIndexOptions
                {
                    Name = "UK_HubGroupId",
                    Unique = true
                })
        );
        
         _pubSubGroupConnectionCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupConnection>(
                Builders<PubSubGroupConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                , new CreateIndexOptions
                {
                    Name = "IX_HubGroup",
                    Unique = false
                })
        );
        
         _pubSubGroupUserCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupUser>(
                Builders<PubSubGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "UK_HubGroupId",
                    Unique = true
                })
        );
        
         _pubSubGroupUserCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupUser>(
                Builders<PubSubGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                , new CreateIndexOptions
                {
                    Name = "IX_HubGroup",
                    Unique = false
                })
        );
        
         _pubSubGroupUserCollection.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupUser>(
                Builders<PubSubGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_HubUser",
                    Unique = false
                })
        );
    }

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId, string userId)
    {
        var existing = _pubSubGroupConnectionCollection.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId);

        if (existing == null)
        {
            var conn = await GetConnectionAsync(hub, connectionId);
            if (conn != null)
            {
                var groupConnection = new PubSubGroupConnection
                {
                    Id = ObjectId.GenerateNewId(),
                    ConnectionId = connectionId,
                    Group = group,
                    Hub = hub,
                    CreatedOn = DateTime.UtcNow,
                    UserId = userId,
                    ExpireOn = conn.ExpireOn
                };

                await _pubSubGroupConnectionCollection.InsertOneAsync(groupConnection);
            }
           
        }
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _pubSubGroupConnectionCollection.DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId);
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        var existing = _pubSubGroupUserCollection.AsQueryable()
            .SingleOrDefault(c => c.Hub == hub && c.Group == group && c.UserId == userId);

        if (existing == null)
        {
            var userConnection = new PubSubGroupUser
            {
                Id = ObjectId.GenerateNewId(),
                UserId = userId,
                Group = group,
                Hub = hub,
                CreatedOn = DateTime.Now
            };

            await _pubSubGroupUserCollection.InsertOneAsync(userConnection);
        }

        await AddConnectionToGroupFromUserGroups(hub, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _pubSubGroupUserCollection.DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.UserId == userId);
        await _pubSubGroupConnectionCollection.DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.UserId == userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _pubSubGroupUserCollection.DeleteManyAsync(c => c.Hub == hub &&c.UserId == userId);
        await _pubSubGroupConnectionCollection.DeleteManyAsync(c => c.Hub == hub && c.UserId == userId);
    }

    public async Task<IEnumerable<string>> GetGroupsForUser(string hub, string userId)
    {
        return await Task.FromResult(_pubSubGroupConnectionCollection.AsQueryable().Where(c => c.Hub == hub && c.UserId == userId)
            .Select(c => c.Group));
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _pubSubConnectionCollection.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == hub);
        
        await _pubSubGroupConnectionCollection.DeleteManyAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
    }

    public async Task<bool> ExistAckAsync(string hub, string connectionId, string ackId)
    {
        return await Task.FromResult(_pubSubAckCollection.AsQueryable()
            .Any(c => c.Hub == hub && c.ConnectionId == connectionId && c.AckId == ackId));
    }

    public async Task InsertAckAsync(string hub, string connectionId, string ackId)
    {
        var pubSubAck = new PubSubAck
        {
            Id = ObjectId.GenerateNewId(),
            AckId = ackId,
            ConnectionId = connectionId,
            Hub = hub
        };

        await _pubSubAckCollection.InsertOneAsync(pubSubAck);
    }

    public async Task<bool> IsConnectionInGroup(string hub, string group, string connectionId)
    {
        return await Task.FromResult(_pubSubGroupConnectionCollection.AsQueryable()
            .Any(c => c.Hub == hub && c.Group == group && c.ConnectionId == connectionId));
    }

    public void DeleteAll()
    {
        throw new NotImplementedException();
    }

    protected override async Task InsertAsync(IPubSubConnection conn)
    {
        await _pubSubConnectionCollection.InsertOneAsync((PubSubConnection) conn);
    }

    protected override string GetNewId()
    {
        return ObjectId.GenerateNewId().ToString();
    }
}