using Amazon.DynamoDBv2.DataModel;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_connection")]
public class PubSubConnection : IPubSubConnection
{
    [DynamoDBHashKey]
    public string Hub { get; set; } = null!;
    [DynamoDBRangeKey]
    public string ConnectionId { get; set; } = null!;

    public string UserId { get; set; }= null!;
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

}