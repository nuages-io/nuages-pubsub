using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

[ExcludeFromCodeCoverage]
public class PubSubRouteBuilder : IPubSubRouteBuilder
{
    public PubSubRouteBuilder(IServiceCollection services, IConfiguration? configuration = null)
    {
        Services = services;
        Configuration = configuration;
    }

    public IConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}

public interface IPubSubRouteBuilder
{
    IServiceCollection Services { get; }
    // ReSharper disable once UnusedMemberInSuper.Global
    IConfiguration? Configuration { get; }
}