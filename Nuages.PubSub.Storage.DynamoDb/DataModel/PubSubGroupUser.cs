
using Amazon.DynamoDBv2.DataModel;

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_group_user")]
public class PubSubGroupUser 
{
    [DynamoDBHashKey]
    public string Id { get; set; }  = null!;
    
    public string Group { get; set; } = null!;
    public string Sub { get; set; }= null!;
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = null!;
}