using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.PubSub.Services.Tests;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.Services.Mongo.Tests;

// ReSharper disable once UnusedType.Global
public class TestsPubSubServiceMongo : TestsPubSubServiceBase, IDisposable
{
   
    private readonly string _dbName;
    private readonly MongoClient _client;

    public TestsPubSubServiceMongo()
    {
        Hub = "Hub";
        Group = "Groupe1";
        ConnectionId = Guid.NewGuid().ToString();
        UserId = "user";
        
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
                config.DatabaseName = configuration["Nuages:Mongo:DatabaseName"];
            });

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var options = ServiceProvider.GetRequiredService<IOptions<PubSubMongoOptions>>().Value;
        
        var connectionString = options.ConnectionString;
        _dbName = options.DatabaseName;
        
        _client = new MongoClient(connectionString);
        
        _client.DropDatabase(_dbName);
        
        PubSubService = ServiceProvider.GetRequiredService<IPubSubService>();
        
        Task.Run(() => PubSubService.ConnectAsync(Hub, ConnectionId, UserId, 60)).Wait();
    }

    // private FakeApiGateway GetApiGateway()
    // {
    //     var gatewayProvider = ServiceProvider.GetRequiredService<IAmazonApiGatewayManagementApiClientProvider>();
    //     var apiGateWay = gatewayProvider.Create(string.Empty, string.Empty);
    //
    //     return (apiGateWay as FakeApiGateway)!;
    // }
    

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        _client.DropDatabase(_dbName);
        ServiceProvider.Dispose();
    }
}
