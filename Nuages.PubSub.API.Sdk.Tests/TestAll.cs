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
    }
    
    [Fact]
    public async Task ShouldSendToAllAsync()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);

        await client.SendToAllAsync(new Message
        {
            Data = new
            {
                Message = "Yo men!",

            }
        });
    }

    [Fact]
    public async Task ShouldCloseAllAsync()
    {
        var client = new PubSubServiceClient(_url, _apiKey, _hub);

        await client.CloseAllConnectionsAsync();
    }

    class ReponseData
    {
        public string connectionId { get; set; }
    }
    class Response
    {
        public string type { get; set; }
        public ReponseData data { get; set; }
    }
    [Fact]
    public async Task OnStarting_ShouldGetInfoResponse()
    {
        var pubSubClient = new PubSubServiceClient(_url, _apiKey, _hub);

        var uri = await pubSubClient.GetClientAccessUriAsync(_userId, null, new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup) });

        using IWebsocketClient client = new WebsocketClient(new Uri(uri));
        
        string? received = null;
        var receivedEvent = new ManualResetEvent(false);

        client.MessageReceived.Subscribe(msg =>
        {
            var response = JsonSerializer.Deserialize<Response>(msg.Text);
            
            switch (response.type)
            {
                case "echo":
                {
                    _testOutputHelper.WriteLine(response.data.connectionId);
                    
                    pubSubClient.SendToConnectionAsync(response.data.connectionId, new Message
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
                    _testOutputHelper.WriteLine(msg.Text);
                    
                    received = msg.Text;
                    receivedEvent.Set();
                    break;
                }
            }
        });

        await client.Start();

        var message = JsonSerializer.Serialize(new
        {
            type = "echo"
        });
        
        client.Send(message);

        receivedEvent.WaitOne(TimeSpan.FromSeconds(30));

        var expected = JsonSerializer.Serialize(new {from = "server", dataType = "json", data = new { Message = "Hello"}, success = false});
        
        Assert.Equal(expected, received);
    }

}