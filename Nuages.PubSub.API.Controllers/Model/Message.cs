// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.API.Controllers.Model;

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