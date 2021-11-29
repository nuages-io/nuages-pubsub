
namespace Nuages.PubSub.Storage.InMemory.DataModel;

public class WebSocketGroupConnection
{
    public string Id { get; set; } = "";
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
}