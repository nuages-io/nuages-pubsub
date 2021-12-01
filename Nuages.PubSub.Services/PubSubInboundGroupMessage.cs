// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.Services;

public class PubSubInboundGroupMessage
{
    public string group { get; set; } = null!;
    public string datatype { get; set; } = "json";
    public object? data { get; set; }
    public bool noEcho { get; set; }
    public string? ackId { get; set; }
}