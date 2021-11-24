namespace Nuages.PubSub.Lambda.Model;

public class SendModel
{
    public string type { get; set; } = default!;
    public object data { get; set; } = default!;

}