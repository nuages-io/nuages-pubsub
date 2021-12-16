using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestGroup : BaseTest
{
    public TestGroup(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }
    
    [Fact]
    public async Task ShouldSendMessageToGroupConnection()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        client.MessageReceived
            .Subscribe( async response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        
                        _testOutputHelper.WriteLine(connectionId);

                        await _pubSubClient.AddConnectionToGroupAsync(_group, connectionId);
                         //await _pubSubClient.AddUserToGroupAsync(_userId, _group);
                         
                         
                         await _pubSubClient.SendToGroupAsync(_group, new Message
                        {
                            Data = new
                            {
                                Message = "Hello"
                            }
                        });

                        break;
                    }
                    default:
                    {
                        _testOutputHelper.WriteLine(response.Text);

                        received = response.Text;
                        
                        receivedEvent.Set();
                        break;
                    }
                }
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        var expected = JsonSerializer.Serialize(new
            { from = "server", dataType = "json", data = new { Message = "Hello" }, success = true });

        Assert.Equal(expected, received);
        
        Assert.True(await _pubSubClient.GroupExistsAsync(_group));
        
        Assert.True(await _pubSubClient.IsConnectionInGroupAsync(_group, connectionId!));

        await _pubSubClient.RemoveConnectionFromGroupAsync(_group, connectionId!);
        
        Assert.False(await _pubSubClient.IsConnectionInGroupAsync(_group, connectionId!));
        
        Assert.False(await _pubSubClient.GroupExistsAsync(_group));
    }


    [Fact]
    public async Task ShouldSendMessageToGroupUser()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        client.MessageReceived
            .Subscribe( async response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        
                        _testOutputHelper.WriteLine(connectionId);

                        await _pubSubClient.AddUserToGroupAsync(_group, _userId);
                         
                        await _pubSubClient.SendToGroupAsync(_group, new Message
                        {
                            Data = new
                            {
                                Message = "Hello"
                            }
                        });

                        break;
                    }
                    default:
                    {
                        _testOutputHelper.WriteLine(response.Text);

                        received = response.Text;
                        
                        receivedEvent.Set();
                        break;
                    }
                }
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(30));

        var expected = JsonSerializer.Serialize(new
            { from = "server", dataType = "json", data = new { Message = "Hello" }, success = true });

        Assert.Equal(expected, received);
        
        Assert.True(await _pubSubClient.GroupExistsAsync(_group));
        
        Assert.True(await _pubSubClient.IsConnectionInGroupAsync(_group, connectionId!));

        await _pubSubClient.RemoveUserFromGroupAsync(_group, _userId);
        
        Assert.False(await _pubSubClient.IsConnectionInGroupAsync(_group, connectionId!));
        
        Assert.False(await _pubSubClient.GroupExistsAsync(_group));
    }

    [Fact]
    public async Task ShouldRemoveUserFromGroup()
    {
        await _pubSubClient.AddUserToGroupAsync(_group, _userId);
        Assert.True(await _pubSubClient.IsUserInGroupAsync(_group, _userId));
        await _pubSubClient.RemoveUserFromAllGroupsAsync(_userId);
        Assert.False(await _pubSubClient.IsUserInGroupAsync(_group, _userId));
    }
    
    [Fact]
    public async Task ShouldCloseAllConnection()
    {
        using var client = await CreateWebsocketClient();

        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        var disconnected = false;
        
        client.DisconnectionHappened.Subscribe(_ =>
        {
            disconnected = true;
        });
        
        client.MessageReceived
            .Subscribe(async response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;

                connectionId = msg.type switch
                {
                    "echo" => msg.data!.connectionId,
                    _ => connectionId
                };
                
                await _pubSubClient.AddConnectionToGroupAsync(_group, connectionId!);
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.True(await _pubSubClient.ConnectionExistsAsync(connectionId!));
        await _pubSubClient.CloseGroupConnectionsAsync(_group);
        
        receivedEvent.WaitOne(TimeSpan.FromSeconds(3));
        
        Assert.True(disconnected);
        Assert.False(await _pubSubClient.ConnectionExistsAsync(connectionId!));
        
    }
    
    
}