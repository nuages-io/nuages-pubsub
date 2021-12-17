
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
    public string UserId { get; set; } = null!;
    public DateTime? ExpireOn { get; set; }

    public string HubAndGroup { get; set; } = null!;
    public string HubAndGroupAndConnectionId { get; set; } = null!;
    public string HubAndConnectionId { get; set; } = null!;
    public string HubAndUserId { get; set; } = null!;
    public string HubAndGroupAndUserId { get; set; } = null!;

    public void Initialize()
    {
        HubAndGroup = $"{Hub}-{Group}";
        HubAndGroupAndConnectionId = $"{Hub}-{Group}-{ConnectionId}";
        HubAndConnectionId = $"{Hub}-{ConnectionId}";
        HubAndUserId = $"{Hub}-{UserId}";
        HubAndGroupAndUserId = $"{Hub}-{Group}-{UserId}";
    }
}