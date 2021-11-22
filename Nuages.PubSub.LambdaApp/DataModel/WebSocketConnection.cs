#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.LambdaApp.DataModel
{
    [MongoCollection("web_socket_connexion", "Nuages:DbName", true)]
    public class WebSocketConnection : MongoDocument
    {
        public string ConnectionId { get; set; } = "";
        public string Sub { get; set; } = "";
    }
}