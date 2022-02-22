using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Services.Tests;
using Nuages.PubSub.Storage.EntityFramework;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NUages.PubSub.Storage.EntityFramework.Tests.MySql;

[Collection("MySql")]
// ReSharper disable once UnusedType.Global
public class TestsPubSubServiceMySql : TestsPubSubServiceBase
{
  
    public TestsPubSubServiceMySql()
    {
        Hub = "Hub";
        Group = "Groupe1";
        ConnectionId = Guid.NewGuid().ToString();
        UserId = "user";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection.AddDbContext<MySqlPubSubContext>(builder =>
        {
            var connectionString =  configuration["ConnectionStrings:MySql"];

            var serverVersion = ServerVersion.AutoDetect(connectionString);

            builder
                .UseMySql(connectionString, serverVersion);

        });
        
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubEntityFrameworkStorage<MySqlPubSubContext>();

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        var context = ServiceProvider.GetRequiredService<IPubSubStorage>();
        context.TruncateAllData();
        
        PubSubService = ServiceProvider.GetRequiredService<IPubSubService>();
        
        Task.Run(() => PubSubService.ConnectAsync(Hub, ConnectionId, UserId, 60)).Wait();
    }

}