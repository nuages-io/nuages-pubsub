#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("pub_sub_group_connection", "Nuages:DbName", true)]
public class PubSubGroupConnection : MongoDocument
{
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
}