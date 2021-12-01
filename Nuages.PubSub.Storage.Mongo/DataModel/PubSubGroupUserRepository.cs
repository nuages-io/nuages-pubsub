#region

using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

// ReSharper disable once UnusedType.Global
public class PubSubGroupUserRepository : MongoRepository<PubSubGroupUser>, IPubSubGroupUserRepository
{
    [ExcludeFromCodeCoverage]
    protected PubSubGroupUserRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public PubSubGroupUserRepository(IMongoDatabaseProvider provider) : base(provider)
    {
    }

    public void InitializeIndexes()
    {
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupUser>(
                Builders<PubSubGroupUser>.IndexKeys
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
        
        Collection?.Indexes.CreateOne(
            new CreateIndexModel<PubSubGroupUser>(
                Builders<PubSubGroupUser>.IndexKeys
                    .Ascending(p => p.Hub)
                    .Ascending(p => p.Sub)
                , new CreateIndexOptions
                {
                    Name = "IX_HubUser",
                    Unique = false
                })
        );
    }

    public async Task<IEnumerable<PubSubGroupUser>> GetGroupsForUserAsync(string hub, string sub)
    {
        var groups = AsQueryable().Where(c => c.Hub == hub && c.Sub == sub);

        return await Task.FromResult(groups);
    }

    public async Task DeleteUserFromGroupAsync(string hub, string group, string sub)
    {
        await DeleteOneAsync(c => c.Hub == hub && c.Group == group && c.Sub == sub);
    }
    
    public async Task DeleteUserFromAllGroupsAsync(string hub, string sub)
    {
        await DeleteManyAsync(c => c.Hub == hub &&c.Sub == sub);
    }
}

public interface IPubSubGroupUserRepository : IMongoRepository<PubSubGroupUser>
{
    void InitializeIndexes();

    Task<IEnumerable<PubSubGroupUser>> GetGroupsForUserAsync(string hub, string sub);

    Task DeleteUserFromGroupAsync(string hub, string group, string sub);
    
    Task DeleteUserFromAllGroupsAsync(string hub,  string sub);
}