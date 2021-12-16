using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Websocket.Client;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class BaseTest
{
    protected ITestOutputHelper _testOutputHelper;
    protected string _url;
    protected string _apiKey;
    protected string _userId;
    protected string _hub;
    protected string _group;

    protected PubSubServiceClient _pubSubClient;
    
    protected BaseTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        _url = configuration.GetSection("Url").Value;
        _apiKey = configuration.GetSection("ApiKey").Value;
        _userId = "user-" + Guid.NewGuid();
        _hub = "hub-" + Guid.NewGuid();
        _group = "group-" + Guid.NewGuid();
        
        _pubSubClient = new PubSubServiceClient(_url, _apiKey, _hub);
    }
    
    protected async Task<IWebsocketClient> CreateWebsocketClient()
    {
        var uri = await _pubSubClient.GetClientAccessUriAsync(_userId, null,
            new List<string> { nameof(PubSubPermission.SendMessageToGroup), nameof(PubSubPermission.JoinOrLeaveGroup) });

        IWebsocketClient client = new WebsocketClient(new Uri(uri));
        return client;
    }
    
    
    protected static void SendEcho(IWebsocketClient client)
    {
        var message = JsonSerializer.Serialize(new
        {
            type = "echo"
        });

        client.Send(message);
    }
}