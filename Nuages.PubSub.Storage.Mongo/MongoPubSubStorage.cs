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

    public async Task InsertAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay = null)
    {
        var conn = new WebSocketConnection
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ConnectionId = connectionid,
            Sub = sub,
            Hub = hub,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        await _webSocketRepository.InsertOneAsync(conn);
    }

    public async Task DeleteAsync(string hub, string connectionId)
    {
        await _webSocketRepository.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == hub);
    }

    public IEnumerable<string> GetAllConnectionIds(string hub)
    {
        return _webSocketRepository.AsQueryable().Where(c => c.Hub == hub).Select(c => c.ConnectionId);
    }

    public IEnumerable<string> GetAllConnectionForGroup(string hub, string @group)
    {
        throw new NotImplementedException();
    }
}