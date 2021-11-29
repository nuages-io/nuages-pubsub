using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Storage;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Services;

public static class PubSubServiceConfigExtension
{
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService<T>(this IPubSubBuilder builder, Action<PubSubOptions>? configureOptions = null)  where T : class, IWebSocketConnection, new()
    {
        return AddPubSubService<T>(builder.Services, builder.Configuration, configureOptions);
    }
    
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService<T>(this IServiceCollection services, IConfiguration configuration, Action<PubSubOptions>? configureOptions = null) where T : class, IWebSocketConnection, new()
    {
        services.Configure<PubSubOptions>(configuration.GetSection("Nuages:PubSub"));

        if (configureOptions != null)
            services.Configure(configureOptions);
        
        services.AddScoped<IPubSubService, PubSubService<T>>();

        return new PubSubBuilder(services, configuration);
    }
}