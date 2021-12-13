// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.API.Controllers.Model;

public class Message
{
    public string type { get; set; } = "message";
    
    public string dataType { get; set; } = "json";
    
    public object? data { get; set; }
    
}