using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Tests;

namespace Nuages.PubSub.Storage.Mongo.Tests;

// ReSharper disable once UnusedType.Global
public class TestMongoPubSubStorage : TestPubSubStorageBase
{

    public TestMongoPubSubStorage()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubMongoStorage(config =>
            {
                config.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
            });
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PubSubMongoOptions>>().Value;
        
        Hub = "Hub";
        Sub = "sub-test";

        var connectionString = options.ConnectionString;

        var url = new MongoUrl(options.ConnectionString);
        
        var mongoCLient = new MongoClient(connectionString);
        mongoCLient.DropDatabase(url.DatabaseName);
        
        PubSubStorage = serviceProvider.GetRequiredService<IPubSubStorage>();
        PubSubStorage.Initialize();
    }
    
}