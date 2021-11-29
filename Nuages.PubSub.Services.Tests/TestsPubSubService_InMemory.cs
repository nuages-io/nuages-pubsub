using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Storage.InMemory;
using Xunit;

namespace Nuages.PubSub.Services.Tests;

public class TestsPubSubServiceInMemory
{
    private readonly IPubSubService _pubSubService;

    public TestsPubSubServiceInMemory()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubInMemoryStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        _pubSubService = serviceProvider.GetRequiredService<IPubSubService>();
    }
    
    
    [Fact]
    public void Test1()
    {
    }
}