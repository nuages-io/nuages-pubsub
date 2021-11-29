
using System;
using System.Threading.Tasks;
using Nuages.PubSub.Storage.InMemory.DataModel;
using Xunit;

namespace Nuages.PubSub.Storage.InMemory.Tests;

public class TestInMemoryPubSubStorage
{
    private readonly IPubSubStorage<WebSocketConnection> _pubSubStorage;
    private readonly string _hub;
    private readonly string _sub;

    public TestInMemoryPubSubStorage()
    {
        _pubSubStorage = new MemoryPubSubStorage();
        _hub = "Hub";
        _sub = "sub-test";
    }
    
    [Fact]
    public async Task ShouldAddConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.Insert(connection);

        var exists = await _pubSubStorage.ConnectionExistsAsync(_hub, connectionId);
        Assert.True(exists);

        var coll = await _pubSubStorage.GetAllConnectionAsync(_hub);
        Assert.Single(coll);

        var userConnections = await _pubSubStorage.GetConnectionsForUserAsync(_hub, _sub);
        Assert.Single(userConnections);

        Assert.True(await _pubSubStorage.UserHasConnectionsAsync(_hub, _sub));

    }
    
    [Fact]
    public async Task ShouldRemoveConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.Insert(connection);

        var existing = await _pubSubStorage.GetConnectionAsync(_hub, connectionId);
        Assert.NotNull(existing);

        await _pubSubStorage.DeleteConnection(_hub, connectionId);
        
        existing = await _pubSubStorage.GetConnectionAsync(_hub, connectionId);
        Assert.Null(existing);
        
    }

    [Fact]
    public async Task ShouldAddConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.Insert(connection);

        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId, _sub);

        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));
    }
    
   
}