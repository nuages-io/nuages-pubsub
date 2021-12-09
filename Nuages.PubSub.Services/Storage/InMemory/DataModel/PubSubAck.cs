
namespace Nuages.PubSub.Services.Storage.InMemory.DataModel;

#if DEBUG
public class PubSubAck 
{
    public string ConnectionId { get; set; } = "";
    public string AckId { get; set; } = "";

}
#endif