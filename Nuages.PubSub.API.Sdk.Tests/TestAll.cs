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



}