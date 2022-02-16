using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Send;

namespace Nuages.PubSub.WebSocket.Endpoints;

using Routes.Authorize;
using Routes.Connect;
using Routes.Disconnect;
using Routes.Echo;

public static class PubSubLambdaConfigExtension
{
   
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IPubSubBuilder AddPubSubLambdaRoutes(this IServiceCollection serviceCollection, IConfiguration? configuration = null)
    {
        serviceCollection.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        serviceCollection.AddScoped<IConnectRoute, ConnectRoute>();
        serviceCollection.AddScoped<IDisconnectRoute, DisconnectRoute>();
        serviceCollection.AddScoped<IEchoRoute, EchoRoute>();
        serviceCollection.AddScoped<ISendRoute, SendRoute>();
        serviceCollection.AddScoped<IJoinRoute, JoinRoute>();
        serviceCollection.AddScoped<ILeaveRoute, LeaveRoute>();
        
        return new PubSubBuilder(serviceCollection, configuration);
    }
}