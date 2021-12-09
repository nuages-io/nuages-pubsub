
using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Services.Storage.InMemory.DataModel;

#if DEBUG
[ExcludeFromCodeCoverage]
public class PubSubGroupUser
{
    public string Group { get; set; } = "";
    public string Sub { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
}
#endif