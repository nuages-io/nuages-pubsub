namespace Nuages.PubSub.Services.Storage;

public interface IPubSubGroupConnection
{
    public DateTime? ExpireOn { get;  set;}
}

public static class WebSocketGroupConnectionExtensions
{
    public static bool IsExpired(this IPubSubGroupConnection pubSubGroupConnection)
    {
        if (!pubSubGroupConnection.ExpireOn.HasValue)
            return false;

        return pubSubGroupConnection.ExpireOn < DateTime.UtcNow;
    }
}