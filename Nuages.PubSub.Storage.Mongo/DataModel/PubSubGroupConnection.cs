
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nuages.PubSub.Storage.Mongo.DataModel;

public class PubSubGroupConnection 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
}