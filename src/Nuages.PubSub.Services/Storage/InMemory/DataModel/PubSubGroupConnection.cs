
using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services.Storage.InMemory.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubGroupConnection : IPubSubGroupConnection
{
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime? ExpireOn { get; set; }
}