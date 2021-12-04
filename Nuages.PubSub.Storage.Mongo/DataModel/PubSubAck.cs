

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nuages.PubSub.Storage.Mongo.DataModel;


public class PubSubAck 
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    public string ConnectionId { get; set; } = "";
    public string Hub { get; set; } = "";
    public string AckId { get; set; } = "";

}