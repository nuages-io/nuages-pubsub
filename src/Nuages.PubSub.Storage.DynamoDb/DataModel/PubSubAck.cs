


using Amazon.DynamoDBv2.DataModel;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_ack")]
public class PubSubAck 
{
    [DynamoDBHashKey]
    public string Id { get; set; } = null!;
    
    public string ConnectionId { get; set; } = null!;
    public string Hub { get; set; } = null!;
    public string AckId { get; set; } = null!;

    public string HubAndConnectionIdAndAckId { get; set; } = null!;

    public void Initialize()
    {
        HubAndConnectionIdAndAckId = $"{Hub}-{ConnectionId}-{AckId}";
    }
}