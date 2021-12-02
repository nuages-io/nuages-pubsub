// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubInboundGroupMessage
{
    public string group { get; set; } = null!;
    public string datatype { get; set; } = "json";
    public object? data { get; set; }
    public bool noEcho { get; set; }
    public string? ackId { get; set; } 
}