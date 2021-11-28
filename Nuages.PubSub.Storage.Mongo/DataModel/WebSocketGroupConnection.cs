#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("web_socket_group_connection", "Nuages:DbName", true)]
public class WebSocketGroupConnection : MongoDocument
{
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public string Hub { get; set; } = "";
}