#region

using MongoDB.Bson;
using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class WebSocketGroupConnectionRepository : MongoRepository<WebSocketGroupConnection>, IWebSocketGroupConnectionRepository
{
    protected WebSocketGroupConnectionRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public WebSocketGroupConnectionRepository(IMongoDatabaseProvider provider) : base(provider)
    {
    }

    public void InitializeIndexes()
    {
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupConnection>(
                Builders<WebSocketGroupConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                    .Ascending(p => p.ConnectionId)
                , new CreateIndexOptions
                {
                    Name = "UK_HubGroupId",
                    Unique = true
                })
        );
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupConnection>(
                Builders<WebSocketGroupConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                , new CreateIndexOptions
                {
                    Name = "IX_HubGroup",
                    Unique = false
                })
        );
        
    }
    
    public IEnumerable<string> GetConnectionsForGroup(string hub, string group)
    {
        return AsQueryable()
            .Where(c => c.Hub == hub && c.Group == group).Select(c => c.ConnectionId);
    }

    public bool GroupHasConnections(string hub, string group)
    {
        return AsQueryable()
            .Any(c => c.Hub == hub && c.Group == group);
    }

    public async Task AddConnetionToGroupAsync(string hub, string group, string connectionid)
    {
        await InsertOneAsync(new WebSocketGroupConnection
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ConnectionId = connectionid,
            Group = group,
            CreatedOn = DateTime.UtcNow,
            
        });
    }
}

public interface IWebSocketGroupConnectionRepository : IMongoRepository<WebSocketGroupConnection>
{
    void InitializeIndexes();
    IEnumerable<string> GetConnectionsForGroup(string hub, string group);
    bool GroupHasConnections(string hub, string group);
    Task AddConnetionToGroupAsync(string hub, string group, string connectionid);
}