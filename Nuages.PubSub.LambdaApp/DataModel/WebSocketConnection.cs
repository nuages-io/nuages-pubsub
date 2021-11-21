#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.LambdaApp.DataModel
{
    [MongoCollection("web_socket_connexion", "nuages_system", true)]
    public class WebSocketConnection : MongoDocument
    {
        public string ConnectionId { get; set; } = "";
        public string Sub { get; set; } = "";
    }
}