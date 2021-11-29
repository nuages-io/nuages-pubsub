namespace Nuages.PubSub.Storage;

public abstract class PubSubStorgeBase<T> where T : IWebSocketConnection, new()
{
    public abstract Task DeleteConnectionFromGroups(string hub, string connectionId);
    public abstract Task DeleteConnection(string hub, string connectionId);
    
    protected abstract string GetNewId();
    
    public async Task<T> CreateConnectionAsync(string hub, string connectionid, string sub, TimeSpan? expireDelay) 
    {
        var conn = new T
        {
            Id = GetNewId(),
            ConnectionId = connectionid,
            Sub = sub,
            Hub = hub,
            CreatedOn = DateTime.UtcNow
        };

        if (expireDelay.HasValue)
        {
            conn.ExpireOn = conn.CreatedOn.Add(expireDelay.Value);
        }
        
        return await Task.FromResult(conn);
    }

}