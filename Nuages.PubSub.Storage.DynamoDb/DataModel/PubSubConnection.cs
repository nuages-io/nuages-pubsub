using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;


public class PubSubConnection : IPubSubConnection
{
    public string Id { get; set; } = null!; 
    
    public string ConnectionId { get; set; } = null!;
    public string Sub { get; set; }= null!;
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

}