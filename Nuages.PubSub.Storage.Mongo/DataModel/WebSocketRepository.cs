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
}

public interface IWebSocketRepository : IMongoRepository<WebSocketConnection>
{
}