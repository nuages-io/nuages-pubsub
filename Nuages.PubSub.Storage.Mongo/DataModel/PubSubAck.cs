using Nuages.MongoDB.Model;

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("pub_sub_ack", "Nuages:DbName", true)]
public class PubSubAck : MongoDocument
{
    public string ConnectionId { get; set; } = "";
    public string Hub { get; set; } = "";
    public string AckId { get; set; } = "";

}