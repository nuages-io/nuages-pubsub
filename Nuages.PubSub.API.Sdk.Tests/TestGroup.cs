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
            .Subscribe(  response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        
                        TestOutputHelper.WriteLine(connectionId);

                         PubSubClient.AddConnectionToGroupAsync(TestGroup, connectionId).Wait();
                         //await _pubSubClient.AddUserToGroupAsync(_userId, _group);
                         
                         
                         PubSubClient.SendToGroupAsync(TestGroup, new Message
                        {
                            Data = new
                            {
                                Message = "Hello"
                            }
                        }).Wait();

                        break;
                    }
                    default:
                    {
                        TestOutputHelper.WriteLine(response.Text);

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
        
        Assert.True(await PubSubClient.GroupExistsAsync(TestGroup));
        
        Assert.True(await PubSubClient.IsConnectionInGroupAsync(TestGroup, connectionId!));

        await PubSubClient.RemoveConnectionFromGroupAsync(TestGroup, connectionId!);
        
        Assert.False(await PubSubClient.IsConnectionInGroupAsync(TestGroup, connectionId!));
        
        Assert.False(await PubSubClient.GroupExistsAsync(TestGroup));
    }


    [Fact]
    public async Task ShouldSendMessageToGroupUser()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        client.MessageReceived
            .Subscribe( response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        
                        TestOutputHelper.WriteLine(connectionId);

                        PubSubClient.AddUserToGroupAsync(TestGroup, TestUserId).Wait();
                         
                        PubSubClient.SendToGroupAsync(TestGroup, new Message
                        {
                            Data = new
                            {
                                Message = "Hello"
                            }
                        }).Wait();

                        break;
                    }
                    default:
                    {
                        TestOutputHelper.WriteLine(response.Text);

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
        
        Assert.True(await PubSubClient.GroupExistsAsync(TestGroup));
        
        Assert.True(await PubSubClient.IsConnectionInGroupAsync(TestGroup, connectionId!));

        await PubSubClient.RemoveUserFromGroupAsync(TestGroup, TestUserId);
        
        Assert.False(await PubSubClient.IsConnectionInGroupAsync(TestGroup, connectionId!));
        
        Assert.False(await PubSubClient.GroupExistsAsync(TestGroup));
    }

    [Fact]
    public async Task ShouldRemoveUserFromGroup()
    {
        await PubSubClient.AddUserToGroupAsync(TestGroup, TestUserId);
        Assert.True(await PubSubClient.IsUserInGroupAsync(TestGroup, TestUserId));
        await PubSubClient.RemoveUserFromAllGroupsAsync(TestUserId);
        Assert.False(await PubSubClient.IsUserInGroupAsync(TestGroup, TestUserId));
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
            .Subscribe( response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;

                connectionId = msg.type switch
                {
                    "echo" => msg.data!.connectionId,
                    _ => connectionId
                };
                
                PubSubClient.AddConnectionToGroupAsync(TestGroup, connectionId!).Wait();
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.True(await PubSubClient.ConnectionExistsAsync(connectionId!));
        await PubSubClient.CloseGroupConnectionsAsync(TestGroup);
        
        receivedEvent.WaitOne(TimeSpan.FromSeconds(3));
        
        Assert.True(disconnected);
        Assert.False(await PubSubClient.ConnectionExistsAsync(connectionId!));
        
    }
    
    
}