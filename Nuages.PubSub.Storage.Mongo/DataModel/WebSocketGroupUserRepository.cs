#region

using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class WebSocketGroupUserRepository : MongoRepository<WebSocketGroupUser>, IWebSocketGroupUserRepository
{
    protected WebSocketGroupUserRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public WebSocketGroupUserRepository(IMongoDatabaseProvider provider) : base(provider)
    {
    }

    public void InitializeIndexes()
    {
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupUser>(
                Builders<WebSocketGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                    .Ascending(p => p.Sub)
                , new CreateIndexOptions
                {
                    Name = "UK_HubGroupId",
                    Unique = true
                })
        );
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupUser>(
                Builders<WebSocketGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Group)
                , new CreateIndexOptions
                {
                    Name = "IX_HubGroup",
                    Unique = false
                })
        );
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<WebSocketGroupUser>(
                Builders<WebSocketGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Sub)
                , new CreateIndexOptions
                {
                    Name = "IX_HubUser",
                    Unique = false
                })
        );
        
    }

    public async Task<IEnumerable<WebSocketGroupUser>> GetUserGroupForUser(string hub, string sub)
    {
        var groups = AsQueryable().Where(c => c.Hub == hub && c.Sub == sub);

        return await Task.FromResult(groups);
    }
}

public interface IWebSocketGroupUserRepository : IMongoRepository<WebSocketGroupUser>
{
    void InitializeIndexes();

    Task<IEnumerable<WebSocketGroupUser>> GetUserGroupForUser(string hub, string sub);
}