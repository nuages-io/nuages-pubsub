using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.DynamoDb;

[ExcludeFromCodeCoverage]
public static class DynamoDbMongoConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubDynamoDbStorage(this IPubSubBuilder builder, Action<PubSubDynamoDbOptions>? options = null)
    {
        if (builder.Configuration != null)
        {
            builder.Services.Configure<PubSubDynamoDbOptions>(builder.Configuration.GetSection("Nuages:DynamoDb"));
        }
        
        if (options != null)
            builder.Services.Configure(options);
        
        builder.Services.AddScoped<IPubSubStorage, DynamoDbStorage>();
    }
}