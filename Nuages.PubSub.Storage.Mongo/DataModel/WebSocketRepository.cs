#region

using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class WebSocketRepository : MongoRepository<WebSocketConnection>, IWebSocketRepository
{
    protected WebSocketRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public WebSocketRepository(IMongoDatabaseProvider provider) : base(provider)
    {
    }

    public void InitializeIndexes()
    {
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketConnection>(
                Builders<WebSocketConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.ConnectionId)
                , new CreateIndexOptions
                {
                    Name = "UK_Id",
                    Unique = true
                })
        );
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketConnection>(
                Builders<WebSocketConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                , new CreateIndexOptions
                {
                    Name = "IX_Hub",
                    Unique = false
                })
        );
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketConnection>(
                Builders<WebSocketConnection>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Sub)
                , new CreateIndexOptions
                {
                    Name = "IX_Sub",
                    Unique = false
                })
        );
    }
}

public interface IWebSocketRepository : IMongoRepository<WebSocketConnection>
{
    void InitializeIndexes();
}