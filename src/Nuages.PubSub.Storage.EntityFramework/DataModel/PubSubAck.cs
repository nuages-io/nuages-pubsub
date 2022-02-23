using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

public class PubSubAck
{
    public string Hub { [ExcludeFromCodeCoverage] get; set; } = "";

    public string ConnectionId { [ExcludeFromCodeCoverage] get; set; } = "";

    public string AckId { [ExcludeFromCodeCoverage] get; set; } = "";
}