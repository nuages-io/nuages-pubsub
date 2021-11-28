// ReSharper disable UnusedMemberInSuper.Global
namespace Nuages.PubSub.Storage;

public interface IWebSocketConnection
{
    public string ConnectionId { get;  } 
    public string Sub { get;  } 
    public DateTime CreatedOn { get;  }
    public DateTime? ExpireOn { get;  }
    public string Hub { get; } 

    public List<string>? Permissions { get; }
    void AddPermission(string permissionString);
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