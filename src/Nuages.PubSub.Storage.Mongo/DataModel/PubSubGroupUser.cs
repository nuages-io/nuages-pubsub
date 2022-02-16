
using MongoDB.Bson;

namespace Nuages.PubSub.Storage.Mongo.DataModel;

public class PubSubGroupUser 
{
    public ObjectId Id { get; set; } 
    
    public string Group { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
}