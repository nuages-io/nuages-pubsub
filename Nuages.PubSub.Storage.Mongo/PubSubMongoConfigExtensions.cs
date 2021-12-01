using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Nuages.MongoDB;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

[ExcludeFromCodeCoverage]
public static class PubSubMongoConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubMongoStorage(this IPubSubBuilder builder, Action<MongoOptions>? configure = null)
    {
        builder.Services.AddScoped<IPubSubConnectionRepository, PubSubConnectionRepository>();
        builder.Services.AddScoped<IPubSubGroupConnectionRepository, PubSubGroupConnectionRepository>();
        builder.Services.AddScoped<IPubSubGroupUserRepository, PubSubGroupUserRepository>();
        builder.Services.AddScoped<IPubSubAckRepository, PubSubAckRepository>();
        
        builder.Services.AddScoped<IPubSubStorage, MongoPubSubStorage>();
        builder.Services.AddNuagesMongoDb(builder.Configuration, configure);
    }
}