


using Amazon.DynamoDBv2.DataModel;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_ack")]
public class PubSubAck 
{
    [DynamoDBHashKey]
    public string Hub { get; set; } = null!;
    [DynamoDBRangeKey]
    public string ConnectionIdAndAckId { get; set; } = null!;
    
    [DynamoDBIgnore]
    public string ConnectionId { get; set; } = null!;

    [DynamoDBIgnore]
    public string AckId { get; set; } = null!;

    public void Initialize()
    {
        ConnectionIdAndAckId = $"{ConnectionId}-{AckId}";
    }
}