using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.Mongo.DataModel;


public class PubSubConnection : IPubSubConnection
{
    public string Id { get; set; } = null!;
    
    public string ConnectionId { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = "";

    // ReSharper disable once MemberCanBePrivate.Global
    public List<string>? Permissions { get; set; }

}