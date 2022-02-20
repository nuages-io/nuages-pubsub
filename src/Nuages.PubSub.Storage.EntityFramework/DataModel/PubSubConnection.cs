using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubConnection : IPubSubConnection
{
    
    public string Id { get; set; } = "";
    
    public string ConnectionId { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public DateTime? ExpireOn { get; set; }
    public string Hub { get; set; } = "";

    // ReSharper disable once MemberCanBePrivate.Global
    [NotMapped]
    public List<string>? Permissions { get; set; }


}