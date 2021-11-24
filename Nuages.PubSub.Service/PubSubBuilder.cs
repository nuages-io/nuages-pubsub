using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Service;


public class PubSubBuilder : IPubSubBuilder
{
    public PubSubBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}