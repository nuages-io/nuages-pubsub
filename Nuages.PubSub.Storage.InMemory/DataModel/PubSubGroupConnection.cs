
using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Storage.InMemory.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubGroupConnection
{
    public string Id { get; set; } = "";
    public string Group { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string Sub { get; set; } = "";
}