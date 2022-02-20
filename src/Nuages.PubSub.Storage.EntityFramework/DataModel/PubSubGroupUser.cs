using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

[ExcludeFromCodeCoverage]
public class PubSubGroupUser
{
    [Key]
    public string Id { get; set; } = "";
    public string Group { get; set; } = "";
    public string UserId { get; set; } = "";
    public DateTime CreatedOn { get; set; }
    public string Hub { get; set; } = "";
}