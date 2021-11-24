using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Service;

public static class PubSubServiceConfigExtension
{
    public static IPubSubBuilder AddPubSubService(this IPubSubBuilder builder)
    {
        builder.Services.AddScoped<IPubSubService, PubSubService>();

        return builder;
    }
    
    public static IPubSubBuilder AddPubSubService(this IServiceCollection services)
    {
        services.AddScoped<IPubSubService, PubSubService>();

        return new PubSubBuilder(services);
    }
}