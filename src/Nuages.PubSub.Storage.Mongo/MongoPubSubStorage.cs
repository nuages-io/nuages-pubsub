using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo.DataModel;
#pragma warning disable CS8618

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : PubSubStorgeBase<PubSubConnection>, IPubSubStorage
{
    private IMongoCollection<PubSubConnection> _pubSubConnectionCollection;
    private IMongoCollection<PubSubGroupConnection> _pubSubGroupConnectionCollection;
    private IMongoCollection<PubSubGroupUser> _pubSubGroupUserCollection;
    private IMongoCollection<PubSubAck> _pubSubAckCollection;
    private readonly PubSubMongoOptions _mongoOptions;
    private readonly PubSubOptions _pubSubOptions;

    public MongoPubSubStorage(IOptions<PubSubMongoOptions> options, IOptions<PubSubOptions> pubSubOptions)
    {
        _mongoOptions = options.Value;
        _pubSubOptions = pubSubOptions.Value;
    }
    
    
    public async IAsyncEnumerable<IPubSubConnection> GetAllConnectionAsync(string hub)
    {
        var coll = _pubSubConnectionCollection.AsQueryable().Where(c => c.Hub == hub).ToAsyncEnumerable();

        await foreach (var item in coll)
            yield return item;
    }

    public override async Task<IPubSubConnection?> GetConnectionAsync(string hub, string connectionId)
    {
        return await Task.FromResult(_pubSubConnectionCollection.AsQueryable().SingleOrDefault(c => c.Hub == hub && c.ConnectionId == connectionId));
    }

    public async IAsyncEnumerable<string> GetConnectionsIdsForGroupAsync(string hub, string group)
    {
        var coll = _pubSubGroupConnectionCollection.AsQueryable()
            .Where(c => c.Hub == hub && c.Group == group).ToAsyncEnumerable();

        await foreach (var item in coll)
        {
            if (!item.IsExpired())
                yield return item.ConnectionId;
        }
            
    }

    public async Task<bool> GroupHasConnectionsAsync(string hub, string group)
    {
        return await Task.FromResult(_pubSubGroupConnectionCollection.AsQueryable().Any(c => c.Hub == hub && c.Group == group));
    }

    protected override async Task UpdateAsync(IPubSubConnection connection)
    {
        var conn = (PubSubConnection)connection;
        
        await _pubSubConnectionCollection.ReplaceOneAsync(doc => doc.Id ==conn.Id, conn);
    }

    public override async IAsyncEnumerable<IPubSubConnection> GetConnectionsForUserAsync(string hub, string userId)
    {
        var coll = _pubSubConnectionCollection.AsQueryable().Where(c => c.Hub == hub && c.UserId == userId).ToAsyncEnumerable();

        await foreach (var item in coll)
        {
            yield return item;
        }
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

    public void Initialize()
    {
        var connectionString =  _mongoOptions.ConnectionString;
        var dbName = _mongoOptions.DatabaseName;
        
        var mongoCLient = new MongoClient(connectionString);
        var database = mongoCLient.GetDatabase(dbName);

        var prefix = _pubSubOptions.StackName + "_";
        _pubSubConnectionCollection = database.GetCollection<PubSubConnection>(prefix + "pub_sub_connection");
        _pubSubGroupConnectionCollection = database.GetCollection<PubSubGroupConnection>(prefix + "pub_sub_group_connection");
        _pubSubGroupUserCollection = database.GetCollection<PubSubGroupUser>(prefix + "pub_sub_group_user");
        _pubSubAckCollection = database.GetCollection<PubSubAck>(prefix + "pub_sub_ack");

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

    public override async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
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
                    UserId = conn.UserId,
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

    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        return await Task.FromResult(_pubSubGroupUserCollection.AsQueryable()
            .Any(u => u.Hub == hub && u.Group == group && u.UserId == userId));
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

    public async IAsyncEnumerable<string> GetGroupsForUser(string hub, string userId)
    {
        var coll = _pubSubGroupConnectionCollection.AsQueryable().Where(c => c.Hub == hub && c.UserId == userId)
            .Select(c => c.Group).ToAsyncEnumerable();

        await foreach (var item in coll)
            yield return item;
    }

    public async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await _pubSubConnectionCollection.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == hub);
        
        await _pubSubGroupConnectionCollection.DeleteManyAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
        
        await _pubSubAckCollection.DeleteManyAsync(c => c.Hub == hub && c.ConnectionId == connectionId);
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
    
    protected override async Task InsertAsync(IPubSubConnection conn)
    {
        var connection = (PubSubConnection)conn;
        connection.Id = ObjectId.GenerateNewId().ToString();
        await _pubSubConnectionCollection.InsertOneAsync(connection);
    }
    
    [ExcludeFromCodeCoverage]
    public void TruncateAllData()
    {
        //Not Required
    }


}