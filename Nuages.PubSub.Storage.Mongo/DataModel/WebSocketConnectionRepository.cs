#region

using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class WebSocketConnectionRepository : MongoRepository<WebSocketConnection>, IWebSocketConnectionRepository
{
    protected WebSocketConnectionRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public WebSocketConnectionRepository(IMongoDatabaseProvider provider) : base(provider)
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

    public IEnumerable<string> GetAllConnectionForAudience(string audience)
    {
        return AsQueryable().Where(h => h.Hub == audience)
            .Select(c => c.ConnectionId);
    }
    public IEnumerable<string> GetConnectionsForUser(string audience, string userId)
    {
        return AsQueryable().Where(h => h.Hub == audience && h.Sub == userId)
            .Select(c => c.ConnectionId);
    }

    public bool UserHasConnections(string audience, string userId)
    {
        return AsQueryable().Any(h => h.Hub == audience && h.Sub == userId);
    }

    public bool ConnectionExists(string audience, string connectionId)
    {
        return AsQueryable()
            .Any(c => c.Hub == audience && c.ConnectionId == connectionId);
    }
}

public interface IWebSocketConnectionRepository : IMongoRepository<WebSocketConnection>
{
    void InitializeIndexes();
    IEnumerable<string> GetAllConnectionForAudience(string audience);
    
    IEnumerable<string> GetConnectionsForUser(string audience, string userId);
    bool UserHasConnections(string audience, string userId);
    bool ConnectionExists(string audience, string connectionId);

}