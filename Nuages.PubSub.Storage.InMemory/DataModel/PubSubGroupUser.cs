
namespace Nuages.PubSub.Storage.InMemory.DataModel;

public class PubSubGroupUser
{
    public string Group { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
}