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
    public static void AddPubSubEntityFrameworkStorage(this IPubSubBuilder builder)
    {
      
        
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework>();
    }
}
