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
    public static IPubSubRouteBuilder AddPubSubLambdaRoutes(this IPubSubBuilder builder, IConfiguration? configuration = null)
    {
        builder.Services.AddScoped<IAuthorizeRoute, AuthorizeRoute>();
        builder.Services.AddScoped<IConnectRoute, ConnectRoute>();
        builder.Services.AddScoped<IDisconnectRoute, DisconnectRoute>();
        builder.Services.AddScoped<IEchoRoute, EchoRoute>();
        builder.Services.AddScoped<ISendRoute, SendRoute>();
        builder.Services.AddScoped<IJoinRoute, JoinRoute>();
        builder.Services.AddScoped<ILeaveRoute, LeaveRoute>();
        
        return new PubSubRouteBuilder(builder.Services, configuration);
    }

    // ReSharper disable once UnusedMember.Global
    public static IPubSubRouteBuilder UseExternalAuthRoute(this IPubSubRouteBuilder builder,  Action<PubSubExternalAuthOption>? configureOptions = null)
    {
        if (builder.Configuration != null)
        {
            builder.Services.Configure<PubSubExternalAuthOption>(builder.Configuration.GetSection("Nuages:ExternalAuth"));
        }
        
        if (configureOptions != null)
            builder.Services.Configure(configureOptions);
        
        builder.Services.AddScoped<IAuthorizeRoute, AuthorizeRouteExternal>();

        return builder;
    }
}