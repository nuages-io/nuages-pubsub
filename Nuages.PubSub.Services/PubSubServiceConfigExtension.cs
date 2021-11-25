using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

public static class PubSubServiceConfigExtension
{
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IPubSubBuilder builder, Action<PubSubAuthOptions>? configureOptions = null)
    {
        return AddPubSubService(builder.Services, builder.Configuration, configureOptions);
    }
    
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IServiceCollection services, IConfiguration configuration, Action<PubSubAuthOptions>? configureOptions = null)
    {
        services.Configure<PubSubAuthOptions>(configuration.GetSection("Nuages:Auth"));

        if (configureOptions != null)
            services.Configure(configureOptions);
        
        services.AddScoped<IPubSubService, PubSubService>();

        return new PubSubBuilder(services, configuration);
    }
}