using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.Storage.EntityFramework;


[ExcludeFromCodeCoverage]
public static class PubSubEntityFrameworkConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubEntityFrameworkStorage<T>(this IPubSubBuilder builder) where T : PubSubDbContext
    {
      
        
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework<T>>();
    }
}
