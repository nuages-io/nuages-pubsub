using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services.Tests;
using Nuages.PubSub.Storage;
using Nuages.PubSub.Storage.EntityFramework;
using Nuages.PubSub.Storage.EntityFramework.SqlServer;
using Xunit;

namespace Nuages.PubSub.Services.EntityFramework.SqlServer.Tests;

[Collection("SqlServer")]
// ReSharper disable once UnusedType.Global
public class TestsPubSubServiceSqlServer : TestsPubSubServiceBase
{
    public TestsPubSubServiceSqlServer()
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
            
        serviceCollection.AddDbContext<SqlServerPubSubDbContext>(builder =>
        {
            var connectionString =  configuration["ConnectionStrings:SqlServer"];

            builder
                .UseSqlServer(connectionString);
        });
        
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubEntityFrameworkStorage<SqlServerPubSubDbContext>();

        serviceCollection.AddScoped<IAmazonApiGatewayManagementApi, FakeApiGateway>();
        serviceCollection.AddScoped<IAmazonApiGatewayManagementApiClientProvider, FakeApiGatewayProvider>();
        
        ServiceProvider = serviceCollection.BuildServiceProvider();
        
        var context = ServiceProvider.GetRequiredService<IPubSubStorage>();
        context.TruncateAllData();
        
        PubSubService = ServiceProvider.GetRequiredService<IPubSubService>();
        
        Task.Run(() => PubSubService.ConnectAsync(Hub, ConnectionId, UserId)).Wait();
    }
}