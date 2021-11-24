using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : IPubSubStorage
{
    private readonly IWebSocketRepository _webSocketRepository;

    public MongoPubSubStorage(IWebSocketRepository webSocketRepository)
    {
        _webSocketRepository = webSocketRepository;
    }

    public async Task InsertAsync(string connectionid, string sub)
    {
        await _webSocketRepository.InsertOneAsync(new WebSocketConnection
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ConnectionId = connectionid,
            Sub = sub
        });
    }

    public async Task DeleteAsync(string connectionId)
    {
        await _webSocketRepository.DeleteOneAsync(c => c.ConnectionId == connectionId);
    }

    public IEnumerable<string> GetAllConnectionIds()
    {
        return _webSocketRepository.AsQueryable().Select(c => c.ConnectionId);
    }
}