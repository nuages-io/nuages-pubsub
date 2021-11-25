using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services;

public interface IPubSubBuilder
{
    IServiceCollection Services { get; }
}