
using Amazon.DynamoDBv2.DataModel;
using Nuages.PubSub.Services.Storage;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Storage.DynamoDb.DataModel;

[DynamoDBTable("pub_sub_group_connection")]
public class PubSubGroupConnection  : IPubSubGroupConnection
{
    [DynamoDBHashKey]
    public string Hub { get; set; }= null!;
    
    [DynamoDBRangeKey]
    public string GroupAndConnectionId { get; set; } = null!;
    
    public string UserId { get; set; } = null!; 
    public string GroupAndUserId { get; set; } = null!; 
    public string ConnectionId { get; set; } = null!; 
     
    public DateTime? ExpireOn { get; set; }
    public DateTime CreatedOn { get; set; }

    public string Group { get; set; } = null!; 
    
    public void Initialize()
    {
        GroupAndConnectionId = $"{Group}-{ConnectionId}";
        GroupAndUserId = $"{Group}-{UserId}";
    }
}