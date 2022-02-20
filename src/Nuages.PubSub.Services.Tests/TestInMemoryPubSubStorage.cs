using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Services.Storage;

using Nuages.PubSub.Services.Storage.InMemory;

using Xunit;

namespace Nuages.PubSub.Services.Tests;

public class TestInMemoryPubSubStorage
{
    private readonly IPubSubStorage _pubSubStorage;
    private readonly string _hub;
    private readonly string _sub;

    public TestInMemoryPubSubStorage()
    {
        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        _pubSubStorage = new PubSubStorageInMemory(new PubSubDbContext(contextOptions));
        _hub = "Hub";
        _sub = "sub-test";
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
        
        var coll =  _pubSubStorage.GetAllConnectionAsync(_hub);
        Assert.Single(coll.ToEnumerable());
        
        var collEmpty =  _pubSubStorage.GetAllConnectionAsync("Bad_Hub");
        Assert.Empty(collEmpty.ToEnumerable());

        var userConnections =  _pubSubStorage.GetConnectionsForUserAsync(_hub, _sub);
        Assert.Single(userConnections.ToEnumerable());

        var userConnectionsEmpty =  _pubSubStorage.GetConnectionsForUserAsync("Bad_Hub", _sub);
        Assert.Empty(userConnectionsEmpty.ToEnumerable());
        
        Assert.True(await _pubSubStorage.UserHasConnectionsAsync(_hub, _sub));

        Assert.False(await _pubSubStorage.UserHasConnectionsAsync("Bad_Hub", _sub));
    }
    
    [Fact]
    public async Task ShouldRemoveConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

       await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        var existing = await _pubSubStorage.GetConnectionAsync(_hub, connectionId);
        Assert.NotNull(existing);

        var existingNull = await _pubSubStorage.GetConnectionAsync("Bad_Hub", connectionId);
        Assert.Null(existingNull);
        
        await _pubSubStorage.DeleteConnectionAsync(_hub, connectionId);
        
        existing = await _pubSubStorage.GetConnectionAsync(_hub, connectionId);
        Assert.Null(existing);
        
        
    }

    [Fact]
    public async Task ShouldAddConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, 60);

        Assert.False(connection.IsExpired());
        
        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId);

        Assert.True(await _pubSubStorage.IsConnectionInGroup(_hub, group, connectionId));
        
        Assert.False(await _pubSubStorage.GroupHasConnectionsAsync("Bad_hub", group));
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));
        
        await _pubSubStorage.RemoveConnectionFromGroupAsync("Bad_hub", group, connectionId);
        await _pubSubStorage.RemoveConnectionFromGroupAsync(_hub, group, connectionId);
        
        Assert.Empty( _pubSubStorage.GetGroupsForUser(_hub, _sub).ToEnumerable());
        
    }

    [Fact] 
    public async Task ShouldDeleteConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, 60);

        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId);

        var collOne =  _pubSubStorage.GetConnectionsIdsForGroupAsync(_hub, group);
        Assert.Single(collOne.ToEnumerable());
        
        var collEmpty =  _pubSubStorage.GetConnectionsIdsForGroupAsync("Bad_Hub", group);
        Assert.Empty(collEmpty.ToEnumerable());
        
        await _pubSubStorage.DeleteConnectionAsync("Bad_Hub", connectionId);
        await _pubSubStorage.DeleteConnectionAsync(_hub, connectionId);

        var coll =  _pubSubStorage.GetConnectionsIdsForGroupAsync(_hub, group);
        Assert.Empty(coll.ToEnumerable());

        
     
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _sub);

        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        var groups =  _pubSubStorage.GetGroupsForUser(_hub, _sub);
        Assert.Equal(group, groups.ToEnumerable().First());

        await _pubSubStorage.RemoveUserFromGroupAsync(_hub, group, _sub);
        
        Assert.Empty( _pubSubStorage.GetGroupsForUser(_hub, _sub).ToEnumerable());
        
        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _sub);

        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        await _pubSubStorage.RemoveUserFromAllGroupsAsync(_hub, _sub);
        
        Assert.Empty( _pubSubStorage.GetGroupsForUser(_hub, _sub).ToEnumerable());
    }

    [Fact]
    public async Task ShouldAddPermission()
    {
        var connectionId = Guid.NewGuid().ToString();
        const string permissionId = "Permission";
        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.AddPermissionAsync(_hub, connectionId, permissionId);
        
        Assert.True(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId));
        
        await _pubSubStorage.AddPermissionAsync(_hub, "Bad_Connecrtion", permissionId);
        
        Assert.True(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId + ".Target"));

        await _pubSubStorage.RemovePermissionAsync(_hub, connectionId, permissionId);
        
        Assert.False(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId));
    }
    
    [Fact]
    public async Task ShouldAddAck()
    {
        var connectionId = Guid.NewGuid().ToString();
        var ackId = Guid.NewGuid().ToString();
        
        Assert.False(await _pubSubStorage.ExistAckAsync(_hub, connectionId, ackId));
        
        await _pubSubStorage.InsertAckAsync(_hub, connectionId, ackId);
        
        Assert.True(await _pubSubStorage.ExistAckAsync(_hub, connectionId, ackId));
    }
}