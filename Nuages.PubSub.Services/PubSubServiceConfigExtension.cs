using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

public static class PubSubServiceConfigExtension
{
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IPubSubBuilder builder)
    {
        builder.Services.AddScoped<IPubSubService, PubSubService>();

        return builder;
    }
    
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IServiceCollection services)
    {
        services.AddScoped<IPubSubService, PubSubService>();

        return new PubSubBuilder(services);
    }
}