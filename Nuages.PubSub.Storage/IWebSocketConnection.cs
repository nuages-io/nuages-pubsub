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