using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Storage.InMemory;
using Xunit;

namespace Nuages.PubSub.Services.Tests;

public class TestsPubSubServiceInMemory
{
    private readonly IPubSubService _pubSubService;
    private readonly string _hub;
    private readonly string _group;
    private readonly string _connectionId;
    private readonly string _userId;
    
    public TestsPubSubServiceInMemory()
    {
        _hub = "Hub";
        _group = "Groupe1";
        _connectionId = Guid.NewGuid().ToString();
        _userId = "user";
        
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
        
        _pubSubService.ConnectAsync(_hub, _connectionId, _userId);
    }
    
    
    [Fact]
    public async Task ShouldAddConnectionToGroup()
    {
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _userId);

        await _pubSubService.GrantPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.True(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));

    }
}