using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services.Storage.InMemory;


[ExcludeFromCodeCoverage]
public static class PubSubMemoryConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubInMemoryStorage(this IPubSubBuilder builder)
    {
       
        builder.Services.AddScoped<IPubSubStorage, MemoryPubSubStorage>();
    }
}
