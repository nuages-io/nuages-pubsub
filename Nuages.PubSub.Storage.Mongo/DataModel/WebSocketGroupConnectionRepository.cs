#region

using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class WebSocketConnectionGroupConnectionRepository : MongoRepository<WebSocketGroupConnection>, IWebSocketGroupConnectionRepository
{
    protected WebSocketConnectionGroupConnectionRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public WebSocketConnectionGroupConnectionRepository(IMongoDatabaseProvider provider) : base(provider)
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
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupConnection>(
                Builders<WebSocketGroupConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Sub)
                , new CreateIndexOptions
                {
                    Name = "IX_HubSub",
                    Unique = false
                })
        );
    }
}

public interface IWebSocketGroupConnectionRepository : IMongoRepository<WebSocketGroupConnection>
{
    void InitializeIndexes();
}