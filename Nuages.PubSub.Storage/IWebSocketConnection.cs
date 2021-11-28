namespace Nuages.PubSub.Storage;

public interface IWebSocketConnection
{
    public string ConnectionId { get; set; } 
    public string Sub { get; set; } 
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } 

    public List<string>? Permissions { get; set; }
}

public static class WebSocketConnectionExtensions
{
    public static bool IsExpired(this IWebSocketConnection webSocketConnection)
    {
        if (!webSocketConnection.ExpireOn.HasValue)
            return false;

        return webSocketConnection.ExpireOn < DateTime.UtcNow;
    }

}