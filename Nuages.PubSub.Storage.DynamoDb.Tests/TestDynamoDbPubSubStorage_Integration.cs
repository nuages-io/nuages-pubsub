using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;
using Nuages.PubSub.Storage.Mongo;
using Xunit;

namespace Nuages.PubSub.Storage.DynamoDb.Tests;

public class TestDynamoDbPubSubStorage_Integration
{
    private readonly string _sub;
    private readonly string _hub;
    
    private readonly IPubSubStorage _pubSubStorage;

    public TestDynamoDbPubSubStorage_Integration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubDynamoDbStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        _hub = "Hub";
        _sub = "sub-test";
        
        _pubSubStorage = serviceProvider.GetRequiredService<IPubSubStorage>();
        
        
        _pubSubStorage.DeleteAll();
    }
    
    [Fact]
    public async Task ShouldAddConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        Assert.False(connection.IsExpired());
        
        var exists = await _pubSubStorage.ConnectionExistsAsync(_hub, connectionId);
        Assert.True(exists);
        
        var existsNull = await _pubSubStorage.ConnectionExistsAsync("Bad_Hub", connectionId);
        Assert.False(existsNull);
        
        var coll = await _pubSubStorage.GetAllConnectionAsync(_hub);
        Assert.Single(coll);
        
        var collEmpty = await _pubSubStorage.GetAllConnectionAsync("Bad_Hub");
        Assert.Empty(collEmpty);
        
        var userConnections = await _pubSubStorage.GetConnectionsForUserAsync(_hub, _sub);
        Assert.Single(userConnections);
        
        var userConnectionsEmpty = await _pubSubStorage.GetConnectionsForUserAsync("Bad_Hub", _sub);
        Assert.Empty(userConnectionsEmpty);
        
        Assert.True(await _pubSubStorage.UserHasConnectionsAsync(_hub, _sub));
        
        Assert.False(await _pubSubStorage.UserHasConnectionsAsync("Bad_Hub", _sub));
    }
    
    
}