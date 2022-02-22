using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Tests;
using Xunit;

namespace Nuages.PubSub.Storage.DynamoDb.Tests;

// ReSharper disable once InconsistentNaming
public class TestDynamoDbPubSubStorage_Integration : TestPubSubStorageBase
{
    
    public TestDynamoDbPubSubStorage_Integration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubDynamoDbStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        Hub = "Hub";
        Sub = "sub-test";
        
        PubSubStorage = serviceProvider.GetRequiredService<IPubSubStorage>();

        PubSubStorage.TruncateAllData();
    }
    
}