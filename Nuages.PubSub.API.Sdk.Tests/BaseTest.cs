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
    protected ITestOutputHelper TestOutputHelper;
    protected readonly string TestUrl;
    protected readonly string TestApiKey;
    protected readonly string TestUserId;
    protected readonly string TestHub;
    protected readonly string TestGroup;

    protected PubSubServiceClient PubSubClient;
    
    protected BaseTest(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        TestUrl = configuration.GetSection("Url").Value;
        TestApiKey = configuration.GetSection("ApiKey").Value;
        TestUserId = "user-" + Guid.NewGuid();
        TestHub = "hub-" + Guid.NewGuid();
        TestGroup = "group-" + Guid.NewGuid();
        
        PubSubClient = new PubSubServiceClient(TestUrl, TestApiKey, TestHub);
    }
    
    protected async Task<IWebsocketClient> CreateWebsocketClient()
    {
        var uri = await PubSubClient.GetClientAccessUriAsync(TestUserId, null,
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