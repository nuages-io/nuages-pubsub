#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo;
// ReSharper disable SuggestBaseTypeForParameter

#endregion

namespace Nuages.PubSub.Samples.Console;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ArrangeTypeModifiers
[ExcludeFromCodeCoverage]
class Program
{
    private static readonly object ConsoleLock = new();

    private static ClientWebSocket _webSocket;

    private static IConfigurationRoot _configuration;
    private static string _wssUrl;
    private static string _issuer;
    private static string _secret;
    private static string _audience;

    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        System.Console.WriteLine("Starting Nuages.PubSub.WebSocket.Endpoints Console!");
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<IConfiguration>(_configuration);
            
        serviceCollection
            .AddPubSubService(_configuration)
            .AddPubSubMongoStorage(config =>
            {
                config.ConnectionString = _configuration["Mongo:ConnectionString"];
                config.DatabaseName = _configuration["Mongo:DatabaseName"];
            });
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        PubSubService = serviceProvider.GetRequiredService<IPubSubService>();
        
        _wssUrl = _configuration.GetSection("Nuages:WebSocket:Url").Value;
        _issuer = _configuration.GetSection("Nuages:PubSub:Issuer").Value;
        _secret = _configuration.GetSection("Nuages:PubSub:Secret").Value;
        _audience = _configuration.GetSection("Nuages:PubSub:Audience").Value;
        
        System.Console.WriteLine("Getting Token...");

        const string hub = "hub";
        const string user = "user";
        
        var token = GenerateToken(user, _audience);
        LogData(token);
        
        System.Console.WriteLine("Try connect to Server with Uri");
        var url = string.Format(_wssUrl, token, hub);

        LogData(url);
            
        await ConnectAsync(url);
    }

    private static IPubSubService PubSubService { get; set; }

    private static void LogLine()
    {
        System.Console.WriteLine("----------------------------");
    }

    
    private static string GenerateToken(string userId, string audience)
    {
        return PubSubService.GenerateToken(_issuer, audience, userId, new List<string>(), _secret,
            TimeSpan.FromDays(1));
    }
    
    private static async Task ConnectAsync(string uri)
    {
        try
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

            System.Console.WriteLine("Nuages.PubSub.WebSocket.Endpoints Connected");
            System.Console.WriteLine("Sending echo message...");
                
            var msg = new {type = "echo", data = ""};
            await SendMessageToSocketAsync(_webSocket, msg);

            await Task.WhenAll(ReceiveFromSocketAsync(_webSocket), SendToSocketAsync(_webSocket));
        }
        catch (Exception ex)
        {
            System.Console.WriteLine("Exception: {0}", ex);
        }
        finally
        {
            _webSocket?.Dispose();
            System.Console.WriteLine();

            lock (ConsoleLock)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("Nuages.PubSub.WebSocket.Endpoints closed.");
                System.Console.ResetColor();
            }
        }
    }

    private static async Task SendToSocketAsync(ClientWebSocket webSocket)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var o = new
            {
                target = "test",
                payload = new
                {
                    message = System.Console.ReadLine()
                }
            };

            var msg = new {type = "sendmessage", data = o};

            await SendMessageToSocketAsync(webSocket, msg);
        }
    }

    private static async Task SendMessageToSocketAsync(ClientWebSocket webSocket, object msg)
    {
        var data = JsonSerializer.Serialize(msg);
        LogData($"Sending message {data}");
        var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
        
        await webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task ReceiveFromSocketAsync(ClientWebSocket webSocket)
    {
        var buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                    CancellationToken.None);
            else
                LogStatus(true, buffer, result.Count);
        }
    }

    private static void LogStatus(bool receiving, byte[] buffer, int length)
    {
        lock (ConsoleLock)
        {
            System.Console.ForegroundColor = receiving ? ConsoleColor.Green : ConsoleColor.Gray;
            System.Console.WriteLine("{0} {1} bytes... ", receiving ? "Received" : "Sent", length);
            System.Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, length));
            System.Console.ResetColor();
        }
    }

    private static void LogData(string data)
    {
        lock (ConsoleLock)
        {
            LogLine();
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.WriteLine(data);
            System.Console.ResetColor();
            LogLine();
        }
    }

}