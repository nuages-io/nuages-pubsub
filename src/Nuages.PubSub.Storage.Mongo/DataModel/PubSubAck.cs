

using MongoDB.Bson;

namespace Nuages.PubSub.Storage.Mongo.DataModel;


public class PubSubAck 
{
    public ObjectId Id { get; set; } 
    
    public string ConnectionId { get; set; } = "";
    public string Hub { get; set; } = "";
    public string AckId { get; set; } = "";

}