// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Nuages.AWS.Secrets;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubMessage
{
    public string type { get; set; } = "message";
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PubSubMessageSource from { get; set; } = PubSubMessageSource.server;
    
    public string dataType { get; set; } = "json";
    
    public object? data { get; set; }
    
    public string? group { get; set; }
    public string? fromSub { get; set; }
    
    public string? ackId { get; set; }
    public bool success { get; set; } = true;
    public string? error { get; set; }
}

public enum PubSubMessageSource
{
    server,
    group,
    self
}
