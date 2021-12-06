
using MongoDB.Bson;

namespace Nuages.PubSub.Storage.Mongo.DataModel;

public class PubSubGroupConnection 
{
    public ObjectId Id { get; set; } 
    
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
}