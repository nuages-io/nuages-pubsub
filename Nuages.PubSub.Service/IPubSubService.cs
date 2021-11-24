namespace Nuages.PubSub.Service;

public interface IPubSubService
{
    Task SendToAllAsync(string content);
}