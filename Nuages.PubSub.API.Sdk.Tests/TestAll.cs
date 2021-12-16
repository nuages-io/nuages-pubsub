using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Xunit;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class TestAll : BaseTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    

    public TestAll(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        _pubSubClient = new PubSubServiceClient(_url, _apiKey, _hub);
    }
    
    [Fact]
    public async Task ShouldSendMessageToAllConnection()
    {
        using var client = await CreateWebsocketClient();

        string? received = null;
        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        client.MessageReceived
            .Subscribe(async response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        
                        _testOutputHelper.WriteLine(connectionId);

                        await _pubSubClient.SendToAllAsync(new Message
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
    }

    
    [Fact]
    public async Task ShouldCloseAllConnection()
    {
        using var client = await CreateWebsocketClient();

        var receivedEvent = new ManualResetEvent(false);
        string? connectionId = null;

        bool disconnected = false;
        
        client.DisconnectionHappened.Subscribe(response =>
        {
            disconnected = true;
        });
        
        client.MessageReceived
            .Subscribe(response =>
            {
                var msg = JsonSerializer.Deserialize<Response>(response.Text)!;
                
                switch (msg.type)
                {
                    case "echo":
                    {
                        connectionId = msg.data!.connectionId;
                        

                        break;
                    }
                }
            });

        await client.Start();

        SendEcho(client);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));

        Assert.True(await _pubSubClient.ConnectionExistsAsync(connectionId));
        await _pubSubClient.CloseAllConnectionsAsync();
        
        receivedEvent.WaitOne(TimeSpan.FromSeconds(10));
        
        Assert.True(disconnected);
        Assert.False(await _pubSubClient.ConnectionExistsAsync(connectionId));
        
    }



}