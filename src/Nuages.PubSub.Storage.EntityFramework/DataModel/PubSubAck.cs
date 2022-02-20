
using System.ComponentModel.DataAnnotations;

namespace Nuages.PubSub.Storage.EntityFramework.DataModel;

public class PubSubAck 
{
    [Key]
    public string Id { get; set; } = "";

    public string Hub { get; set; } = "";
    public string ConnectionId { get; set; } = "";
    public string AckId { get; set; } = "";

}