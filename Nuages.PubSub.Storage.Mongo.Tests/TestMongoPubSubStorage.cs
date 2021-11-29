using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Nuages.PubSub.Storage.Mongo.DataModel;
using Xunit;

namespace Nuages.PubSub.Storage.Mongo.Tests;

public class TestInMemoryPubSubStorage
{
    private readonly IPubSubStorage _pubSubStorage;
    private readonly string _hub;
    private readonly string _sub;
    private readonly Mock<IWebSocketConnectionRepository> _webSocketConnectionRepository;
    private readonly Mock<IWebSocketGroupConnectionRepository> _webSocketGroupConnectionRepository;
    private readonly Mock<IWebSocketGroupUserRepository> _webSocketUserGroupRepository;

    public TestInMemoryPubSubStorage()
    {
        _webSocketConnectionRepository = new Mock<IWebSocketConnectionRepository>();
        _webSocketGroupConnectionRepository = new Mock<IWebSocketGroupConnectionRepository>();
        _webSocketUserGroupRepository = new Mock<IWebSocketGroupUserRepository>();
        
        _pubSubStorage = new MongoPubSubStorage(_webSocketConnectionRepository.Object, _webSocketGroupConnectionRepository.Object, _webSocketUserGroupRepository.Object);
        
        _hub = "Hub";
        _sub = "sub-test";
    }
    
    [Fact]
    public async Task ShouldAddConnection()
    {
        var connectionId = Guid.NewGuid().ToString();   
        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);
        
        
        _webSocketConnectionRepository.Setup(w => w.ConnectionExists(_hub, connectionId)).Returns(true);
        _webSocketConnectionRepository.Setup(w => w.GetAllConnectionForHub(_hub))
            .Returns(new List<IWebSocketConnection> {connection });

        _webSocketConnectionRepository.Setup(w => w.GetConnectionsForUser(_hub, _sub))
            .Returns(new List<IWebSocketConnection> { connection });
        _webSocketConnectionRepository.Setup(w => w.UserHasConnections(_hub, _sub)) .Returns(true);
            
        Assert.False(connection.IsExpired());
        
        await _pubSubStorage.InsertAsync(connection);

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
    
    [Fact]
    public async Task ShouldRemoveConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        _webSocketConnectionRepository.Setup(w => w.GetConnectionByConnectionId(_hub, connectionId)).Returns(connection);
        _webSocketConnectionRepository.Setup(w => w.DeleteByConnectionIdAsync(_hub, connectionId)).Callback(() =>
        {
            _webSocketConnectionRepository.Setup(w => w.GetConnectionByConnectionId(_hub, connectionId)).Returns((IWebSocketConnection?) null);
        });
        
        await _pubSubStorage.InsertAsync(connection);

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

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, TimeSpan.FromDays(1));

        _webSocketGroupConnectionRepository.Setup(c => c.GroupHasConnections(_hub, group)).Returns(true);
        _webSocketUserGroupRepository.Setup(c => c.GetUserGroupForUserAsync(_hub, _sub)).ReturnsAsync(
            new List<WebSocketGroupUser>
            {
                new()
                {
                    Group = group,
                    Sub = _sub,
                    Hub = _hub
                }
            });
        
        _webSocketGroupConnectionRepository.Setup(c => c.DeleteConnectionFromGroupAsync(_hub, group, connectionId)).Callback(() =>
        {
            _webSocketUserGroupRepository.Setup(c => c.GetUserGroupForUserAsync(_hub, _sub)).ReturnsAsync(new List<WebSocketGroupUser>());
        });
        
        Assert.False(connection.IsExpired());
        
        await _pubSubStorage.InsertAsync(connection);

        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId, _sub);

        Assert.False(await _pubSubStorage.GroupHasConnectionsAsync("Bad_hub", group));
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));
        
        Assert.Single(await _pubSubStorage.GetGroupsForUser(_hub, _sub));
        
        await _pubSubStorage.RemoveConnectionFromGroupAsync("Bad_hub", group, connectionId);
        await _pubSubStorage.RemoveConnectionFromGroupAsync(_hub, group, connectionId);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _sub));
        
    }

    [Fact] 
    public async Task ShouldDeleteConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.InsertAsync(connection);

        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId, _sub);

        var collOne = await _pubSubStorage.GetConnectionsForGroupAsync(_hub, group);
        Assert.Single(collOne);
        
        var collEmpty = await _pubSubStorage.GetConnectionsForGroupAsync("Bad_Hub", group);
        Assert.Empty(collEmpty);
        
        await _pubSubStorage.DeleteConnectionAsync("Bad_Hub", connectionId);
        await _pubSubStorage.DeleteConnectionAsync(_hub, connectionId);

        var coll = await _pubSubStorage.GetConnectionsForGroupAsync(_hub, group);
        Assert.Empty(coll);
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        
        await _pubSubStorage.InsertAsync(connection);

        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _sub);

        _webSocketGroupConnectionRepository.Setup(c => c.GroupHasConnections(_hub, group)).Returns(true);

        _webSocketUserGroupRepository.Setup(c => c.GetUserGroupForUserAsync(_hub, _sub)).ReturnsAsync(
            new List<WebSocketGroupUser>
            {
                new ()
                {
                    Group = group,
                    Sub = _sub,
                    Hub = _hub
                }
            });
            
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        var groups = await _pubSubStorage.GetGroupsForUser(_hub, _sub);
        Assert.Equal(group, groups.First());

        await _pubSubStorage.RemoveUserFromGroupAsync(_hub, group, _sub);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _sub));
        
        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _sub);

        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        await _pubSubStorage.RemoveUserFromAllGroupsAsync(_hub, _sub);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _sub));
    }

    [Fact]
    public async Task ShouldAddPermission()
    {
        var connectionId = Guid.NewGuid().ToString();
        const string permissionId = "Permission";
        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _sub, null);

        await _pubSubStorage.InsertAsync(connection);

        _webSocketConnectionRepository.Setup(w => w.GetConnectionByConnectionId(_hub, connectionId)).Returns(connection);
        
        await _pubSubStorage.AddPermissionAsync(_hub, connectionId, permissionId);
        
        Assert.True(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId));
        
        await _pubSubStorage.AddPermissionAsync(_hub, "Bad_Connecrtion", permissionId);
        
        Assert.True(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId + ".Target"));

        await _pubSubStorage.RemovePermissionAsync(_hub, connectionId, permissionId);
        
        Assert.False(await _pubSubStorage.HasPermissionAsync(_hub, connectionId, permissionId));
    }
}