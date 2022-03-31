using System.Diagnostics.CodeAnalysis;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubGroupConnection : IPubSubGroupConnection
{
    public string GroupName { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime? ExpireOn { get; set; }
}