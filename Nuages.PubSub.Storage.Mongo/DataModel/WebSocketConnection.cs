#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("web_socket_connection", "Nuages:DbName", true)]
public class WebSocketConnection : MongoDocument, IWebSocketConnection
{
    public string ConnectionId { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = "";

    public List<string>? Permissions { get; set; } 
}