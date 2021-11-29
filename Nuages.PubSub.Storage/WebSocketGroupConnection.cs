
namespace Nuages.PubSub.Storage;

public interface IWebSocketGroupConnection 
{
    public string Id { get; set; }
    public string Group { get; set; } 
    public string ConnectionId { get; set; } 
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; }
}