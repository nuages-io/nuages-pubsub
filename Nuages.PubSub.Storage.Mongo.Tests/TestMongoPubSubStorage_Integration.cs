using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;
using Xunit;

namespace Nuages.PubSub.Storage.Mongo.Tests;

public class TestMongoPubSubStorage
{
    private readonly IPubSubStorage _pubSubStorage;
    private readonly string _hub;
    private readonly string _userId;

    public TestMongoPubSubStorage()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", false)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
            
        serviceCollection
            .AddPubSubService(configuration)
            .AddPubSubMongoStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<PubSubMongoOptions>>().Value;
        
        _hub = "Hub";
        _userId = "sub-test";

        var connectionString = options.ConnectionString;
        var dbName = options.DatabaseName;
        
        var mongoCLient = new MongoClient(connectionString);
        mongoCLient.DropDatabase(dbName);
        
        _pubSubStorage = serviceProvider.GetRequiredService<IPubSubStorage>();
    }
    
    [Fact]
    public async Task ShouldAddConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, null);

        Assert.False(connection.IsExpired());
        
        var exists = await _pubSubStorage.ConnectionExistsAsync(_hub, connectionId);
        Assert.True(exists);

        var existsNull = await _pubSubStorage.ConnectionExistsAsync("Bad_Hub", connectionId);
        Assert.False(existsNull);
        
        var coll = await _pubSubStorage.GetAllConnectionAsync(_hub);
        Assert.Single(coll);
        
        var collEmpty = await _pubSubStorage.GetAllConnectionAsync("Bad_Hub");
        Assert.Empty(collEmpty);

        var userConnections = await _pubSubStorage.GetConnectionsForUserAsync(_hub, _userId);
        Assert.Single(userConnections);

        var userConnectionsEmpty = await _pubSubStorage.GetConnectionsForUserAsync("Bad_Hub", _userId);
        Assert.Empty(userConnectionsEmpty);
        
        Assert.True(await _pubSubStorage.UserHasConnectionsAsync(_hub, _userId));

        Assert.False(await _pubSubStorage.UserHasConnectionsAsync("Bad_Hub", _userId));
    }
    
    [Fact]
    public async Task ShouldRemoveConnection()
    {
        var connectionId = Guid.NewGuid().ToString();

        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, null);

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

        var connection = await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, TimeSpan.FromDays(1));

        Assert.False(connection.IsExpired());
        
        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId, _userId);

        Assert.False(await _pubSubStorage.GroupHasConnectionsAsync("Bad_hub", group));
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));
        
        await _pubSubStorage.RemoveConnectionFromGroupAsync("Bad_hub", group, connectionId);
        await _pubSubStorage.RemoveConnectionFromGroupAsync(_hub, group, connectionId);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _userId));
        
    }

    [Fact] 
    public async Task ShouldDeleteConnectionToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

       await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, null);

        await _pubSubStorage.AddConnectionToGroupAsync(_hub, group, connectionId, _userId);

        var collOne = await _pubSubStorage.GetConnectionsIdsForGroupAsync(_hub, group);
        Assert.Single(collOne);
        
        var collEmpty = await _pubSubStorage.GetConnectionsIdsForGroupAsync("Bad_Hub", group);
        Assert.Empty(collEmpty);
        
        await _pubSubStorage.DeleteConnectionAsync("Bad_Hub", connectionId);
        await _pubSubStorage.DeleteConnectionAsync(_hub, connectionId);

        var coll = await _pubSubStorage.GetConnectionsIdsForGroupAsync(_hub, group);
        Assert.Empty(coll);

        
     
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroup()
    {
        var group = Guid.NewGuid().ToString();
        var connectionId = Guid.NewGuid().ToString();

        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, null);

        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _userId);

        Assert.True(await _pubSubStorage.IsConnectionInGroup(_hub, group, connectionId));
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        var groups = await _pubSubStorage.GetGroupsForUser(_hub, _userId);
        Assert.Equal(group, groups.First());

        await _pubSubStorage.RemoveUserFromGroupAsync(_hub, group, _userId);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _userId));
        
        await _pubSubStorage.AddUserToGroupAsync(_hub, group, _userId);
        Assert.True(await _pubSubStorage.IsUserInGroupAsync(_hub, group, _userId));
        
        Assert.True(await _pubSubStorage.GroupHasConnectionsAsync(_hub, group));

        await _pubSubStorage.RemoveUserFromAllGroupsAsync(_hub, _userId);
        
        Assert.Empty(await _pubSubStorage.GetGroupsForUser(_hub, _userId));
    }

    [Fact]
    public async Task ShouldAddPermission()
    {
        var connectionId = Guid.NewGuid().ToString();
        const string permissionId = "Permission";
        await _pubSubStorage.CreateConnectionAsync(_hub, connectionId, _userId, null);

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