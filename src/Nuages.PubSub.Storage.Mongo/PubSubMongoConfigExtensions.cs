using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.Mongo;

[ExcludeFromCodeCoverage]
public static class PubSubMongoConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubMongoStorage(this IPubSubBuilder builder, Action<PubSubMongoOptions>? options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped )
    {
        if (options != null)
            builder.Services.Configure(options);

        switch (serviceLifetime)
        {
            case ServiceLifetime.Singleton:
            {
                builder.Services.AddSingleton<IMongoClientProvider>(new MongoClientProvider());
                break;
            }
            case ServiceLifetime.Scoped:
            {
                builder.Services.AddScoped<IMongoClientProvider, MongoClientProvider>();
                break;
            }
            case ServiceLifetime.Transient:
            {
                builder.Services.AddTransient<IMongoClientProvider, MongoClientProvider>();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }
        
        
        builder.Services.AddScoped<IPubSubStorage, MongoPubSubStorage>();
    }
}