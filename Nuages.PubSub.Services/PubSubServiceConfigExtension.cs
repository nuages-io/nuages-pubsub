using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

public static class PubSubServiceConfigExtension
{
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IPubSubBuilder builder)
    {
        return AddPubSubService(builder.Services, builder.Configuration);
    }
    
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPubSubService, PubSubService>();

        return new PubSubBuilder(services, configuration);
    }
}