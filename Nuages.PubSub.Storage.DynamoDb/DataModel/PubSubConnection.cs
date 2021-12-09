using Amazon.DynamoDBv2.DataModel;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_connection")]
public class PubSubConnection : IPubSubConnection
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!; 
    
    public string ConnectionId { get; set; } = null!;
    public string Sub { get; set; }= null!;
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = null!;

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

}