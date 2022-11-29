using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Services.Tests;
using Nuages.PubSub.Storage.DynamoDb;

namespace Nuages.PubSub.Services.DynamoDb.Tests;

// ReSharper disable once UnusedType.Global
public class TestsPubSubServiceDynamoDb : TestsPubSubServiceBase
{
   
    public TestsPubSubServiceDynamoDb()
    {
        Hub = "Hub";
        Group = "Groupe1";
        ConnectionId = Guid.NewGuid().ToString();
        UserId = "user";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName!)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubDynamoDbStorage();

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        var context = ServiceProvider.GetRequiredService<IPubSubStorage>();
        context.TruncateAllData();
        
        PubSubService = ServiceProvider.GetRequiredService<IPubSubService>();
        
        Task.Run(() => PubSubService.ConnectAsync(Hub, ConnectionId, UserId)).Wait();
    }

}
