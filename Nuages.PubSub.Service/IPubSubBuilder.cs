using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Service;

public interface IPubSubBuilder
{
    IServiceCollection Services { get; }
}