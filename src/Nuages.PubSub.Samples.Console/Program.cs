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
using Nuages.PubSub.API.Sdk;
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

    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        System.Console.WriteLine("Starting Nuages.PubSub.WebSocket.Endpoints Console!");
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();


        System.Console.WriteLine("Getting url...");

        const string hub = "hub";
        const string user = "user";

        var client = new PubSubServiceClient(_configuration["Nuages:API:Url"], _configuration["Nuages:API:ApiKey"], hub );
        var url = client.GetClientAccessUriAsync(user, new List<string>
        {
            // "SendMessageToGroup",
            // "JoinOrLeaveGroup"
        }).Result;

        LogData(url);
            
        await ConnectAsync(url);
    }

    private static void LogLine()
    {
        System.Console.WriteLine("----------------------------");
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