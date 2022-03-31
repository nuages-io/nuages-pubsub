
using MongoDB.Bson;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.Mongo.DataModel;

public class PubSubGroupConnection : IPubSubGroupConnection
{
    public ObjectId Id { get; set; } 
    
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime? ExpireOn { get; set; }
}