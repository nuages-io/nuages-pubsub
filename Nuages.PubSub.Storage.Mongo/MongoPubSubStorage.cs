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

    public async Task InsertAsync(string audience, string connectionid, string sub, TimeSpan? expireDelay = null)
    {
        var conn = new WebSocketConnection
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ConnectionId = connectionid,
            Sub = sub,
            Hub = audience,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        await _webSocketRepository.InsertOneAsync(conn);
    }

    public async Task DeleteAsync(string audience, string connectionId)
    {
        await _webSocketRepository.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == audience);
    }

    public async Task<IEnumerable<string>> GetAllConnectionIdsAsync(string audience)
    {
        return await Task.FromResult(_webSocketRepository.AsQueryable().Where(h => h.Hub == audience)
            .Select(c => c.ConnectionId));
    }

    public async Task<IEnumerable<string>> GetConnectionIdsForGroupAsync(string audience, string @group)
    {
        throw new NotImplementedException();
    }
    
    public async Task<bool> GroupHasConnectionsAsync(string audience, string @group)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetConnectionIdsForUserAsync(string audience, string userId)
    {
        return await Task.FromResult(_webSocketRepository.AsQueryable().Where(h => h.Hub == audience && h.Sub == userId).Select(c => c.ConnectionId));
    }

    public async Task<bool> UserHasConnectionsAsync(string audience, string userId)
    {
        return await Task.FromResult(_webSocketRepository.AsQueryable().Any(h => h.Hub == audience && h.Sub == userId));
    }
    
    public async Task<bool> ConnectionExistsAsync(string connectionId, string audience)
    {
        return await Task.FromResult(
            _webSocketRepository.AsQueryable().Any(c => c.Hub == audience &&  c.ConnectionId == connectionId)
        );
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _webSocketRepository.InitializeIndexes();
        });
    }
}