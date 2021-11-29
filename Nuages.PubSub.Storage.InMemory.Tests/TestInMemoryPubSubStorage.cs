
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
    }
}