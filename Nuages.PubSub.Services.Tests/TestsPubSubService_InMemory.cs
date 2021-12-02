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
    private readonly string _sub;
    
    public TestsPubSubServiceInMemory()
    {
        _hub = "Hub";
        _group = "Groupe1";
        _connectionId = Guid.NewGuid().ToString();
        _sub = "user";
        
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
        
        _pubSubService.ConnectAsync(_hub, _connectionId, _sub);
    }
    
    
    [Fact]
    public async Task ShouldAddConnectionToGroup()
    {
        Assert.True(await _pubSubService.ConnectionExistsAsync(_hub, _connectionId));
        Assert.True(await _pubSubService.UserExistsAsync(_hub, _sub));
        
        await _pubSubService.AddConnectionToGroupAsync(_hub, _group, _connectionId, _sub);

        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.GrantPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.True(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));
        
        await _pubSubService.RevokePermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId);
        
        Assert.False(await _pubSubService.CheckPermissionAsync(_hub, PubSubPermission.SendMessageToGroup, _connectionId, _group));

        await _pubSubService.RemoveConnectionFromGroupAsync(_hub, _group, _connectionId);
        
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
    }
    
    
    [Fact]
    public async Task ShouldAddUserToGroup()
    {
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);

        await _pubSubService.GroupExistsAsync(_hub, _group);
        
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));

        await _pubSubService.RemoveUserFromGroupAsync(_hub, _group, _sub);
        
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.AddUserToGroupAsync(_hub, _group, _sub);
        Assert.True(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
        
        await _pubSubService.RemoveUserFromAllGroupsAsync(_hub,  _sub);
        Assert.False(await _pubSubService.IsConnectionInGroup(_hub, _group, _connectionId));
    }

    [Fact]
    public async Task ShouldCreateAck()
    {
        var ack = Guid.NewGuid().ToString();
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));

        Assert.False(await _pubSubService.CreateAckAsync(_hub, _connectionId, ack));
        
        Assert.True(await _pubSubService.CreateAckAsync(_hub, _connectionId, "$"));
    }
}