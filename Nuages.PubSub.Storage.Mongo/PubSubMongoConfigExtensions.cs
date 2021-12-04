using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.Storage.Mongo;

[ExcludeFromCodeCoverage]
public static class PubSubMongoConfigExtensions
{
    // ReSharper disable once UnusedMember.Global
    public static void AddPubSubMongoStorage(this IPubSubBuilder builder)
    {
    builder.Services.AddScoped<IPubSubStorage, MongoPubSubStorage>();
    }
}