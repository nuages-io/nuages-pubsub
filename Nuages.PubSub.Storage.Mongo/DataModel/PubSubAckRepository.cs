#region

using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using Nuages.MongoDB.DatabaseProvider;
using Nuages.MongoDB.Repository;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;


// ReSharper disable once UnusedType.Global
public class PubSubAckRepository : MongoRepository<PubSubAck>, IPubSubAckRepository
{
    [ExcludeFromCodeCoverage]
    protected PubSubAckRepository(IMongoDatabase db) : base(db)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public PubSubAckRepository(IMongoDatabaseProvider provider) : base(provider)
    {
    }

    public void InitializeIndexes()
    {
        Collection?.Indexes.CreateOne(
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
        
    }

    public async Task<bool> ExistsAsync(string hub, string connectionId, string ackId)
    {
        return await Task.FromResult(AsQueryable().Any(c => c.Hub == hub && c.ConnectionId == connectionId && c.AckId == ackId));
    }
}

public interface IPubSubAckRepository : IMongoRepository<PubSubAck>
{
    void InitializeIndexes();

    Task<bool> ExistsAsync(string hub, string connectionId, string ackId);
}