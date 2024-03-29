using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public static class PubSubServiceConfigExtension
{
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IPubSubBuilder builder, Action<PubSubOptions>? configureOptions = null) 
    {
        return AddPubSubService(builder.Services, builder.Configuration, configureOptions);
    }
    
    // ReSharper disable once UnusedMember.Global
    public static IPubSubBuilder AddPubSubService(this IServiceCollection services, IConfiguration? configuration = null, Action<PubSubOptions>? configureOptions = null) 
    {
        if (configuration != null)
        {
            services.Configure<PubSubOptions>(configuration.GetSection("Nuages:PubSub"));
        }
        
        if (configureOptions != null)
            services.Configure(configureOptions);
        
        services.AddScoped<IAmazonApiGatewayManagementApiClientProvider, AmazonApiGatewayManagementApiClientProvider>();
        services.AddScoped<IPubSubService, PubSubService>();
        
        return new PubSubBuilder(services, configuration);
    }
}