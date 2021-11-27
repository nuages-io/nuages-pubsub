using MongoDB.Bson;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public class MongoPubSubStorage : IPubSubStorage
{
    private readonly IWebSocketConnectionRepository _webSocketConnectionRepository;
    private readonly IWebSocketGroupConnectionRepository _webSocketGroupConnectionRepository;
    private readonly IWebSocketGroupUserRepository _webSocketGroupUserRepository;

    public MongoPubSubStorage(IWebSocketConnectionRepository webSocketConnectionRepository, 
        IWebSocketGroupConnectionRepository webSocketGroupConnectionRepository, IWebSocketGroupUserRepository webSocketGroupUserRepository)
    {
        _webSocketConnectionRepository = webSocketConnectionRepository;
        _webSocketGroupConnectionRepository = webSocketGroupConnectionRepository;
        _webSocketGroupUserRepository = webSocketGroupUserRepository;
    }

    public async Task InsertAsync(string audience, string connectionid, string sub, TimeSpan? expireDelay = default)
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
        
        await _webSocketConnectionRepository.InsertOneAsync(conn);
    }

    public async Task DeleteAsync(string audience, string connectionId)
    {
        await _webSocketConnectionRepository.DeleteOneAsync(c => c.ConnectionId == connectionId && c.Hub == audience);
    }

    public async Task<IEnumerable<string>> GetAllConnectionIdsAsync(string audience)
    {
        return await Task.FromResult(_webSocketConnectionRepository.GetAllConnectionForAudience(audience));
    }

    public async Task<IEnumerable<string>> GetConnectionIdsForGroupAsync(string audience, string group)
    {
        return await Task.FromResult(_webSocketGroupConnectionRepository.GetConnectionsForGroup(audience, group));
    }
    
    public async Task<bool> GroupHasConnectionsAsync(string audience, string group)
    {
        return await Task.FromResult(_webSocketGroupConnectionRepository.GroupHasConnections(audience, group));
    }

    public async Task<IEnumerable<string>> GetConnectionIdsForUserAsync(string audience, string userId)
    {
        return await Task.FromResult( _webSocketConnectionRepository.GetConnectionsForUser(audience, userId));
    }

    public async Task<bool> UserHasConnectionsAsync(string audience, string userId)
    {
        return await Task.FromResult(_webSocketConnectionRepository.UserHasConnections(audience, userId));
    }
    
    public async Task<bool> ConnectionExistsAsync(string audience, string connectionId)
    {
        return await Task.FromResult(
            _webSocketConnectionRepository.ConnectionExists(audience, connectionId)
        );
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _webSocketConnectionRepository.InitializeIndexes();
            _webSocketGroupConnectionRepository.InitializeIndexes();
            _webSocketGroupUserRepository.InitializeIndexes();
        });
    }
}