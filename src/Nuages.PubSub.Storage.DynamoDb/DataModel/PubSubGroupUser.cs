
using Amazon.DynamoDBv2.DataModel;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_group_user")]
public class PubSubGroupUser 
{
    [DynamoDBHashKey]
    public string Hub { get; set; } = null!;
    
    [DynamoDBRangeKey]
    public string GroupAndUserId { get; set; } = null!;
    
    public string UserId { get; set; } = null!; //LSI
    
    public DateTime CreatedOn { get; set; } 
    
    public string Group { get; set; } = null!;

    public void Initialize()
    {
        GroupAndUserId = $"{Group}-{UserId}";
    }
}