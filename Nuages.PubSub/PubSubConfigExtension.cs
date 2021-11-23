using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.DataModel;
using Nuages.PubSub.Services.Authorize;
using Nuages.PubSub.Services.Broadcast;
using Nuages.PubSub.Services.Connect;
using Nuages.PubSub.Services.Disconnect;
using Nuages.PubSub.Services.Echo;

namespace Nuages.PubSub;

public static class PubSubConfigExtension
{
    public static void AddPubSub(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWebSocketRepository, WebSocketRepository>();
        serviceCollection.AddScoped<IAuthorizeService, AuthorizeService>();
        serviceCollection.AddScoped<IConnectService, ConnectService>();
        serviceCollection.AddScoped<IDisconnectService, DisconnectService>();
        serviceCollection.AddScoped<IEchoService, Services.Echo.EchoService>();
        serviceCollection.AddScoped<IBroadcastMessageService, BroadcastMessageService>();
    }
}