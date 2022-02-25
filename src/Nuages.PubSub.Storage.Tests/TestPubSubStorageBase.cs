using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nuages.PubSub.Storage.Tests;

public abstract class TestPubSubStorageBase
{
    protected IPubSubStorage PubSubStorage = null!;
    protected string Hub = string.Empty;
    protected string Sub = string.Empty;
    
    [Fact]
    public async Task ShouldAddConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, null);

        Assert.False(connection.IsExpired());
        
        var exists = await PubSubStorage.ConnectionExistsAsync(Hub, connectionId);
        Assert.True(exists);

        var existsNull = await PubSubStorage.ConnectionExistsAsync("Bad_Hub", connectionId);
        Assert.False(existsNull);
        
        var coll =  PubSubStorage.GetAllConnectionAsync(Hub);
        Assert.Single(coll.ToEnumerable());
        
        var collEmpty =  PubSubStorage.GetAllConnectionAsync("Bad_Hub");
        Assert.Empty(collEmpty.ToEnumerable());

        var userConnections =  PubSubStorage.GetConnectionsForUserAsync(Hub, Sub);
        Assert.Single(userConnections.ToEnumerable());

        var userConnectionsEmpty =  PubSubStorage.GetConnectionsForUserAsync("Bad_Hub", Sub);
        Assert.Empty(userConnectionsEmpty.ToEnumerable());
        
        Assert.True(await PubSubStorage.UserHasConnectionsAsync(Hub, Sub));

        Assert.False(await PubSubStorage.UserHasConnectionsAsync("Bad_Hub", Sub));
    }
    
    [Fact]
    public async Task ShouldRemoveConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

       await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, null);

        var existing = await PubSubStorage.GetConnectionAsync(Hub, connectionId);
        Assert.NotNull(existing);

        var existingNull = await PubSubStorage.GetConnectionAsync("Bad_Hub", connectionId);
        Assert.Null(existingNull);
        
        await PubSubStorage.DeleteConnectionAsync(Hub, connectionId);
        
        existing = await PubSubStorage.GetConnectionAsync(Hub, connectionId);
        Assert.Null(existing);
        
        
    }

    [Fact]
    public async Task ShouldAddConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        var connection = await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, 60);

        Assert.False(connection.IsExpired());
        
        await PubSubStorage.AddConnectionToGroupAsync(Hub, group, connectionId);

        Assert.True(await PubSubStorage.IsConnectionInGroup(Hub, group, connectionId));
        
        Assert.False(await PubSubStorage.GroupHasConnectionsAsync("Bad_hub", group));
        Assert.True(await PubSubStorage.GroupHasConnectionsAsync(Hub, group));
        
        await PubSubStorage.RemoveConnectionFromGroupAsync("Bad_hub", group, connectionId);
        await PubSubStorage.RemoveConnectionFromGroupAsync(Hub, group, connectionId);
        
        Assert.Empty( PubSubStorage.GetGroupsForUser(Hub, Sub).ToEnumerable());
        
    }

    [Fact] 
    public async Task ShouldDeleteConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, 60);

        await PubSubStorage.AddConnectionToGroupAsync(Hub, group, connectionId);

        var collOne =  PubSubStorage.GetConnectionsIdsForGroupAsync(Hub, group);
        Assert.Single(collOne.ToEnumerable());
        
        var collEmpty =  PubSubStorage.GetConnectionsIdsForGroupAsync("Bad_Hub", group);
        Assert.Empty(collEmpty.ToEnumerable());
        
        await PubSubStorage.DeleteConnectionAsync("Bad_Hub", connectionId);
        await PubSubStorage.DeleteConnectionAsync(Hub, connectionId);

        var coll =  PubSubStorage.GetConnectionsIdsForGroupAsync(Hub, group);
        Assert.Empty(coll.ToEnumerable());

        
     
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, null);

        await PubSubStorage.AddUserToGroupAsync(Hub, group, Sub);

        Assert.True(await PubSubStorage.GroupHasConnectionsAsync(Hub, group));

        var groups =  PubSubStorage.GetGroupsForUser(Hub, Sub);
        Assert.Equal(group, groups.ToEnumerable().First());

        await PubSubStorage.RemoveUserFromGroupAsync(Hub, group, Sub);
        
        Assert.Empty( PubSubStorage.GetGroupsForUser(Hub, Sub).ToEnumerable());
        
        group = Guid.NewGuid().ToString();
        
        await PubSubStorage.AddUserToGroupAsync(Hub, group, Sub);

        Assert.True(await PubSubStorage.GroupHasConnectionsAsync(Hub, group));

        await PubSubStorage.RemoveUserFromAllGroupsAsync(Hub, Sub);
        
        Assert.Empty( PubSubStorage.GetGroupsForUser(Hub, Sub).ToEnumerable());
    }

    [Fact]
    public async Task ShouldAddPermission()
    {
        var connectionId = Guid.NewGuid().ToString();
        const string permissionId = "Permission";
        await PubSubStorage.CreateConnectionAsync(Hub, connectionId, Sub, null);

        await PubSubStorage.AddPermissionAsync(Hub, connectionId, permissionId);
        
        Assert.True(await PubSubStorage.HasPermissionAsync(Hub, connectionId, permissionId));
        
        await PubSubStorage.AddPermissionAsync(Hub, "Bad_Connecrtion", permissionId);
        
        Assert.True(await PubSubStorage.HasPermissionAsync(Hub, connectionId, permissionId + ".Target"));

        await PubSubStorage.RemovePermissionAsync(Hub, connectionId, permissionId);
        
        Assert.False(await PubSubStorage.HasPermissionAsync(Hub, connectionId, permissionId));
    }
    
    [Fact]
    public async Task ShouldAddAck()
    {
        var connectionId = Guid.NewGuid().ToString();
        var ackId = Guid.NewGuid().ToString();
        
        Assert.False(await PubSubStorage.ExistAckAsync(Hub, connectionId, ackId));
        
        await PubSubStorage.InsertAckAsync(Hub, connectionId, ackId);
        
        Assert.True(await PubSubStorage.ExistAckAsync(Hub, connectionId, ackId));
    }
}