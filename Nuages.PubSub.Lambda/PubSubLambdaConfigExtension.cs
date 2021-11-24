using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Lambda.Routes.Send;
using Nuages.PubSub.Service;

namespace Nuages.PubSub.Lambda;

using Routes.Authorize;
using Routes.Connect;
using Routes.Disconnect;
using Routes.Echo;

public static class PubSubLambdaConfigExtension
{
    public static IPubSubBuilder AddPubSubLambdaRoutes(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        serviceCollection.AddScoped<IConnectRoute, ConnectRoute>();
        serviceCollection.AddScoped<IDisconnectRoute, DisconnectRoute>();
        serviceCollection.AddScoped<IEchoRoute, EchoRoute>();
        serviceCollection.AddScoped<ISendRoute, SendRoute>();
        
        return new PubSubBuilder(serviceCollection);
    }
}