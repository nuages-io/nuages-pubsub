using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Routes.Authorize;
using Nuages.PubSub.Routes.Broadcast;
using Nuages.PubSub.Routes.Connect;
using Nuages.PubSub.Routes.Disconnect;
using Nuages.PubSub.Routes.Echo;

namespace Nuages.PubSub;

public static class PubSubConfigExtension
{
    public static IPubSubBuilder AddPubSub(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        serviceCollection.AddScoped<IConnectRoute, ConnectRoute>();
        serviceCollection.AddScoped<IDisconnectRoute, DisconnectRoute>();
        serviceCollection.AddScoped<IEchoRoute, EchoRoute>();
        serviceCollection.AddScoped<IBroadcastMessageRoute, BroadcastMessageRoute>();
        
        return new PubSubBuilder(serviceCollection);
    }
}

public class PubSubBuilder : IPubSubBuilder
{
    public PubSubBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}
    
public interface IPubSubBuilder
{
    IServiceCollection Services { get; }
}