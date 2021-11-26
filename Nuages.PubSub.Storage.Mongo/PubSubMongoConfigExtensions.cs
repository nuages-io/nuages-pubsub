using Microsoft.Extensions.DependencyInjection;
using Nuages.MongoDB;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo.DataModel;

namespace Nuages.PubSub.Storage.Mongo;

public static class PubSubMongoConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubMongoStorage(this IPubSubBuilder builder, Action<MongoOptions>? configure = null)
    {
        builder.Services.AddScoped<IWebSocketConnectionRepository, WebSocketConnectionRepository>();
        builder.Services.AddScoped<IWebSocketGroupConnectionRepository, WebSocketGroupConnectionRepository>();
        builder.Services.AddScoped<IWebSocketGroupUserRepository, WebSocketGroupUserRepository>();
        
        builder.Services.AddScoped<IPubSubStorage, MongoPubSubStorage>();
        builder.Services.AddNuagesMongoDb(builder.Configuration, configure);
    }
}