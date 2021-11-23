using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services.Authorize;
using Nuages.PubSub.Services.Broadcast;
using Nuages.PubSub.Services.Connect;
using Nuages.PubSub.Services.Disconnect;
using Nuages.PubSub.Services.Echo;

namespace Nuages.PubSub;

public static class PubSubConfigExtension
{
    public static IPubSubBuilder AddPubSub(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAuthorizeService, AuthorizeService>();
        serviceCollection.AddScoped<IConnectService, ConnectService>();
        serviceCollection.AddScoped<IDisconnectService, DisconnectService>();
        serviceCollection.AddScoped<IEchoService, EchoService>();
        serviceCollection.AddScoped<IBroadcastMessageService, BroadcastMessageService>();
        
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