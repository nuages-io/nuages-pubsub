#region

using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;


// ReSharper disable once UnusedType.Global
public class WebSocketConnectionRepository : MongoRepository<WebSocketConnection>, IWebSocketConnectionRepository
{
    [ExcludeFromCodeCoverage]
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

    public IEnumerable<IWebSocketConnection> GetAllConnectionForHub(string hub)
    {
        return AsQueryable().Where(h => h.Hub == hub);
    }
    public IEnumerable<IWebSocketConnection> GetConnectionsForUser(string hub, string userId)
    {
        return AsQueryable().Where(h => h.Hub == hub && h.Sub == userId);
    }

    public bool UserHasConnections(string hub, string userId)
    {
        return AsQueryable().Any(h => h.Hub == hub && h.Sub == userId);
    }

    public bool ConnectionExists(string hub, string connectionId)
    {
        return AsQueryable()
            .Any(c => c.Hub == hub && c.ConnectionId == connectionId);
    }

    public IWebSocketConnection? GetConnectionByConnectionId(string hub, string connectionId)
    {
        return FindOneAsync(c => c.Hub == hub && c.ConnectionId == connectionId).Result;
    }

    public async Task DeleteByConnectionIdAsync(string hub, string connectionId)
    {
        await DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == hub);
    }
}

public interface IWebSocketConnectionRepository : IMongoRepository<WebSocketConnection>
{
    void InitializeIndexes();
    IEnumerable<IWebSocketConnection> GetAllConnectionForHub(string hub);
    
    IEnumerable<IWebSocketConnection> GetConnectionsForUser(string hub, string userId);
    bool UserHasConnections(string hub, string userId);
    bool ConnectionExists(string hub, string connectionId);

    IWebSocketConnection? GetConnectionByConnectionId(string hub, string connectionId);

    Task DeleteByConnectionIdAsync(string hub, string connectionId);
}