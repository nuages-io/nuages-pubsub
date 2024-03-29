// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace Nuages.PubSub.API.Endpoints.Model;

// ReSharper disable once ClassNeverInstantiated.Global
public class Message
{
    public string type { get; set; } = "message";
    
    public MessageDataType dataType { get; set; } = MessageDataType.json;
    
    public object? data { get; set; }
    
}

public enum MessageDataType
{
    json,
    text
}