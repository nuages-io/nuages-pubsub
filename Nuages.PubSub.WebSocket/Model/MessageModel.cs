// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.WebSocket.Model;

public class MessageModel
{
    public string type { get; set; } = null!;
    
    public string from { get; set; } = "";
    public string group { get; set; } = null!;
    
    public string datatype { get; set; } = "json";
    public object? data { get; set; }
    public string? fromSub { get; set; } = null;

}