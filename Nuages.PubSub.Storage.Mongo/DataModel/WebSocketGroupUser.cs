#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("web_socket_group_user", "Nuages:DbName", true)]
public class WebSocketGroupUser : MongoDocument
{
    public string Group { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
}