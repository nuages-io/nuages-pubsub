using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;


public class PubSubBuilder : IPubSubBuilder
{
    public PubSubBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}