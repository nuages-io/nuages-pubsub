using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Lambda;

using Routes.Authorize;
using Routes.Broadcast;
using Routes.Connect;
using Routes.Disconnect;
using Routes.Echo;

public static class PubSubLambdaConfigExtension
{
    public static IPubSubBuilder AddPubSubLambda(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        serviceCollection.AddScoped<IConnectRoute, ConnectRoute>();
        serviceCollection.AddScoped<IDisconnectRoute, DisconnectRoute>();
        serviceCollection.AddScoped<IEchoRoute, EchoRoute>();
        serviceCollection.AddScoped<IBroadcastMessageRoute, BroadcastMessageRoute>();
        
        return new PubSubBuilder(serviceCollection);
    }
}