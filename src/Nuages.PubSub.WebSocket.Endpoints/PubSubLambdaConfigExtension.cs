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
    public static IPubSubRouteBuilder AddPubSubLambdaRoutes(this IPubSubBuilder builder)
    {
        builder.Services.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        builder.Services.AddScoped<IConnectRoute, ConnectRoute>();
        builder.Services.AddScoped<IDisconnectRoute, DisconnectRoute>();
        builder.Services.AddScoped<IEchoRoute, EchoRoute>();
        builder.Services.AddScoped<ISendRoute, SendRoute>();
        builder.Services.AddScoped<IJoinRoute, JoinRoute>();
        builder.Services.AddScoped<ILeaveRoute, LeaveRoute>();
        
        return new PubSubRouteBuilder(builder.Services, builder.Configuration);
    }

    // ReSharper disable once UnusedMember.Global
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IPubSubRouteBuilder UseExternalAuthRoute(this IPubSubRouteBuilder builder)
    { 
        builder.Services.AddScoped<IAuthorizeRoute, AuthorizeRouteExternal>();

        return builder;
    }
}