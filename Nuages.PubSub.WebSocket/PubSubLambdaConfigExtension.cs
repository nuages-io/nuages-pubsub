using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Model;
using Nuages.PubSub.WebSocket.Routes.Send;

namespace Nuages.PubSub.WebSocket;

using Routes.Authorize;
using Routes.Connect;
using Routes.Disconnect;
using Routes.Echo;

public static class PubSubLambdaConfigExtension
{
    public static IPubSubBuilder AddPubSubLambdaRoutes(this ServiceCollection serviceCollection, IConfiguration configuration, Action<PubSubAuthOptions>? configureOptions = null)
    {
        serviceCollection.Configure<PubSubAuthOptions>(configuration.GetSection("Nuages:Auth"));

        if (configureOptions != null)
            serviceCollection.Configure(configureOptions);
        
        serviceCollection.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        serviceCollection.AddScoped<IConnectRoute, ConnectRoute>();
        serviceCollection.AddScoped<IDisconnectRoute, DisconnectRoute>();
        serviceCollection.AddScoped<IEchoRoute, EchoRoute>();
        serviceCollection.AddScoped<ISendRoute, SendRoute>();
        
        return new PubSubBuilder(serviceCollection, configuration);
    }
}