using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

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
