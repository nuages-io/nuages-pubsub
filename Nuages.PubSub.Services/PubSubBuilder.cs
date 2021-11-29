using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubBuilder : IPubSubBuilder
{
    public PubSubBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; set; }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}