using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nuages.PubSub.Services.Storage.InMemory;


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
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageInMemory>();
    }
}
