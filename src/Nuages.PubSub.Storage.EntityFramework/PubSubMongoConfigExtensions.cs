using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework;


[ExcludeFromCodeCoverage]
public static class PubSubMemoryConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubInMemoryStorage(this IPubSubBuilder builder)
    {
        builder.Services.AddDbContext<PubSubDbContext>(options =>
        {
            options.UseInMemoryDatabase("PubSubDbContext");
        });
        
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework>();
    }
}
