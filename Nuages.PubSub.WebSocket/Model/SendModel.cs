// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.WebSocket.Model;

public class SendModel
{
    public string type { get; set; } = null!;
    public string? hub { get; set; }
    public object? data { get; set; } 

}