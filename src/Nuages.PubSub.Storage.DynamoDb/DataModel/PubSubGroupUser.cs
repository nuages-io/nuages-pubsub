
using Amazon.DynamoDBv2.DataModel;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_group_user")]
public class PubSubGroupUser 
{
    [DynamoDBHashKey]
    public string Id { get; set; }  = null!;
    
    public string Group { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime CreatedOn { get; set; } 
    public string Hub { get; set; } = null!;

    public string HubAndGroupAndUserId { get; set; } = null!;
    public string HubAndUserId { get; set; } = null!;

    public void Initialize()
    {
        HubAndUserId = $"{Hub}-{UserId}";
        HubAndGroupAndUserId = $"{Hub}-{Group}-{UserId}";
    }
}