#region

using Nuages.MongoDB.Model;

#endregion

namespace Nuages.PubSub.Storage.Mongo.DataModel;

[MongoCollection("pub_sub_connection", "Nuages:DbName", true)]
public class PubSubConnection : MongoDocument, IPubSubConnection
{
    public string ConnectionId { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = "";

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

}