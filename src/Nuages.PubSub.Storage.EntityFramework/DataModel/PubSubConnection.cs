using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubConnection : IPubSubConnection
{
    
    public string Id { get; set; } = "";
    public string Hub { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public string UserId { get; set; } = "";
    
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    
    public List<string>? Permissions { get; set; }


}