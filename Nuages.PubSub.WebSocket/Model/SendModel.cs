// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.WebSocket.Model;

public class SendModel
{
    public string type { get; set; } = null!;
    public string group { get; set; } = null!;
    public string datatype { get; set; } = "json";
    public object? data { get; set; }
    public bool noEcho { get; set; } = false;
}