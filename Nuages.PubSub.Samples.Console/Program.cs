#region

using System;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

#endregion

namespace Nuages.PubSub.Samples.Console;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ArrangeTypeModifiers
class Program
{
    private static readonly object ConsoleLock = new();


    private static string _email;
    private static string _password;
    private static string _clientId;
    private static string _secret;
    private static string _authority;
    private static string _audience;

    private static ClientWebSocket _webSocket;

    private static IConfigurationRoot _configuration;

    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        System.Console.WriteLine("Starting Nuages.PubSub.WebSocket Console!");
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        _email = _configuration.GetSection("Auth:UserName").Value;
        _password = _configuration.GetSection("Auth:Password").Value;
        _clientId = _configuration.GetSection("Auth:ClientId").Value;
        _secret = _configuration.GetSection("Auth:Secret").Value;
        _authority = _configuration.GetSection("Auth:Authority").Value;
        _audience = _configuration.GetSection("Auth:Audience").Value;

        System.Console.WriteLine("Getting Token...");

        var token = await GetTokenAsync();
            
        LogData(token);
           
        // var t = new JwtSecurityTokenHandler()
        //     .ReadJwtToken(token);
        //
        // var claims = t
        //     .Claims
        //     .ToDictionary(claim => claim.Type, claim => claim.Value);

        System.Console.WriteLine("Try connect to Server with Uri");
        var url = string.Format(_configuration.GetSection("Nuages.PubSub.WebSocket:Url").Value, token);

        LogData(url);
            
        await Connect(url);
    }

    private static void LogLine()
    {
        System.Console.WriteLine("----------------------------");
    }

    private static async Task Connect(string uri)
    {
        try
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

            System.Console.WriteLine("Nuages.PubSub.WebSocket Connected");
            System.Console.WriteLine("Sending echo message...");
                
            var msg = new {type = "echo", data = ""};
            await SendMessageAsync(_webSocket, msg);

            await Task.WhenAll(Receive(_webSocket), Send(_webSocket));
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
                System.Console.WriteLine("Nuages.PubSub.WebSocket closed.");
                System.Console.ResetColor();
            }
        }
    }

    private static async Task Send(WebSocket webSocket)
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

            var msg = new {type = "sendmessage", data = JsonSerializer.Serialize(o)};

            await SendMessageAsync(webSocket, msg);
        }
    }

    private static async Task SendMessageAsync(WebSocket webSocket, object msg)
    {
        var data = JsonSerializer.Serialize(msg);
        LogData($"Sending message {data}");
        var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
        await webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task Receive(WebSocket webSocket)
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

    private static async Task<string> GetTokenAsync()
    {
        var client = new HttpClient();

        var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = $"{_authority}/oauth/token",

            ClientId = _clientId,
            ClientSecret = _secret,
            Scope = "connect:pubsub",
            UserName = _email,
            Password = _password,
            Parameters =
            {
                { "audience", _audience}
            }
        });

        return response.AccessToken;
    }
}