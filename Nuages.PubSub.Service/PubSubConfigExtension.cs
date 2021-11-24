using Microsoft.Extensions.DependencyInjection;


namespace Nuages.PubSub.Lambda;

public static class PubSubConfigExtension
{
   
}

public class PubSubBuilder : IPubSubBuilder
{
    public PubSubBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }

}
    
public interface IPubSubBuilder
{
    IServiceCollection Services { get; }
}