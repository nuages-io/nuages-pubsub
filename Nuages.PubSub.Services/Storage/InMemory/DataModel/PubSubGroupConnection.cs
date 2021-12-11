
using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services.Storage.InMemory.DataModel;

#if DEBUG
[ExcludeFromCodeCoverage]
public class PubSubGroupConnection : IPubSubGroupConnection
{
    public string Id { get; set; } = "";
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime? ExpireOn { get; set; }
}
#endif