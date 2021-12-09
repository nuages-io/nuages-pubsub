
namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

public class PubSubGroupConnection 
{
    public string Id { get; set; } = null!;
    
    public string Group { get; set; } = null!;
    public string ConnectionId { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; }= null!;
    public string Sub { get; set; } = null!;
}