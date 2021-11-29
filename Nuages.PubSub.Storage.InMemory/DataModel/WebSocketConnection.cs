

namespace Nuages.PubSub.Storage.InMemory.DataModel;

public class WebSocketConnection : IWebSocketConnection
{
    public string Id { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = "";

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

    public void AddPermission(string permissionString)
    {
        Permissions ??= new List<string>();
            
        Permissions.Add(permissionString);

    }
}