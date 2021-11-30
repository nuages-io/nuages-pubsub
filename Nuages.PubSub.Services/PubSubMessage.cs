// ReSharper disable InconsistentNaming

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nuages.PubSub.Services;

public class PubSubMessage
{
    public string type { get; set; } = "message";
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PubSubMessageSource from { get; set; } = PubSubMessageSource.server;
    
    public string dataType { get; set; } = "json";
    
    public object? data { get; set; }
}

public enum PubSubMessageSource
{
    server,
    group,
    self
}

public class PubSubGroupMessage : PubSubMessage
{
    public string? group { get; set; }
    public string? fromSub { get; set; }
}