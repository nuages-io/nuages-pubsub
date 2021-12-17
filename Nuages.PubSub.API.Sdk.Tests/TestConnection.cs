using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable InconsistentNaming

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestConnection : BaseTest
{
    public TestConnection(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

  


    [Fact]
    public async Task ShouldConnectionExists()
    {
        using var client = await CreateWebsocketClient();

        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        client.MessageReceived
            .Subscribe(response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        receivedEvent.Set();
                        break;
                    }
                }
        });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.NotNull(connectionId);
        Assert.True(await PubSubClient.ConnectionExistsAsync(connectionId!));
        
    }
    
    [Fact]
    public async Task ShouldSendMessageToConnection()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);

        client.MessageReceived
            .Subscribe( response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        var connectionId = msg.data!.connectionId;
                        
                        TestOutputHelper.WriteLine(connectionId);

                        PubSubClient.SendToConnectionAsync(connectionId, new Message
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
    }
    
    [Fact]
    public async Task ShouldCloseConnection()
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
            .Subscribe(response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;

                switch (@msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        receivedEvent.Set();
                        
                        break;
                    }
                }

                
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.True(await PubSubClient.ConnectionExistsAsync(connectionId!));
        await PubSubClient.CloseConnectionAsync(connectionId!);
        
        //receivedEvent.WaitOne(TimeSpan.FromSeconds(10));
        
        Assert.True(disconnected);
        Assert.False(await PubSubClient.ConnectionExistsAsync(connectionId!));
        
    }

    [Fact]
    public async Task ShouldCheckPermission()
    {
        var receivedEvent = new ManualResetEvent(false);
        using var client = await CreateWebsocketClient();

        string? connectionId = null;
        
        client.MessageReceived
            .Subscribe(response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;

                switch (@msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        receivedEvent.Set();
                        
                        break;
                    }
                }
                
            });
        
        await client.Start();


        SendEcho(client);
        
        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));
        
        Assert.NotNull(connectionId);
        Assert.True(await PubSubClient.CheckPermissionAsync(PubSubPermission.JoinOrLeaveGroup, connectionId!));
        
        await PubSubClient.RevokePermissionAsync(PubSubPermission.JoinOrLeaveGroup, connectionId!);
        
        Assert.False(await PubSubClient.CheckPermissionAsync(PubSubPermission.JoinOrLeaveGroup, connectionId!));

        await PubSubClient.GrantPermissionAsync(PubSubPermission.JoinOrLeaveGroup, connectionId!);

        Assert.True(await PubSubClient.CheckPermissionAsync(PubSubPermission.JoinOrLeaveGroup, connectionId!));

    }
}