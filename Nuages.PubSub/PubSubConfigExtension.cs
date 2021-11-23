using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.DataModel;

namespace Nuages.PubSub;

public static class PubSubConfigExtension
{
    public static void AddPubSub(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IWebSocketRepository, WebSocketRepository>();
        serviceCollection.AddScoped<IAuthorizeService, AuthorizeService>();
        serviceCollection.AddScoped<IConnectService, ConnectService>();
        serviceCollection.AddScoped<IDisconnectService, DisconnectService>();
        serviceCollection.AddScoped<IEchoService, EchoService>();
        serviceCollection.AddScoped<IBroadcastMessageService, BroadcastMessageService>();
    }
}