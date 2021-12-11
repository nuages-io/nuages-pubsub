
using Amazon.DynamoDBv2.DataModel;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_group_connection")]
public class PubSubGroupConnection  : IPubSubGroupConnection
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;
    
    public string Group { get; set; } = null!;
    public string ConnectionId { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; }= null!;
    public string Sub { get; set; } = null!;
    public DateTime? ExpireOn { get; set; }
}