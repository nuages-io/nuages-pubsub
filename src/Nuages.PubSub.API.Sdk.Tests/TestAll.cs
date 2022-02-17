using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestAll : BaseTest
{
    public TestAll(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        
        PubSubClient = new PubSubServiceClient(TestUrl, TestApiKey, TestHub);
    }
    
    [Fact]
    public async Task ShouldSendMessageToAllConnection()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);
        string? connectionId ;

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

                        PubSubClient.SendToAllAsync(new Message
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
            .Subscribe(response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;

                connectionId = msg.type switch
                {
                    "echo" => msg.data!.connectionId,
                    _ => connectionId
                };

                receivedEvent.Set();
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.True(await PubSubClient.ConnectionExistsAsync(connectionId!));
        await PubSubClient.CloseAllConnectionsAsync();
        
        //receivedEvent.WaitOne(TimeSpan.FromSeconds(10));
        
        Assert.True(disconnected);
        Assert.False(await PubSubClient.ConnectionExistsAsync(connectionId!));
        
    }



}