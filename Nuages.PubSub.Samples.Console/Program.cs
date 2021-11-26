#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.API.Sdk;

#endregion

namespace Nuages.PubSub.Samples.Console;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ArrangeTypeModifiers
class Program
{
    private static readonly object ConsoleLock = new();

    private static string _audience;

    private static ClientWebSocket _webSocket;

    private static IConfigurationRoot _configuration;
    private static string _wssUrl;
    private static string _apiUrl;
    private static string _apiKey;
    private static PubSubServiceClient _pubSubClient;

    // ReSharper disable once UnusedParameter.Local
    private static async Task Main(string[] args)
    {
        System.Console.WriteLine("Starting Nuages.PubSub.WebSocket Console!");
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        _audience = _configuration.GetSection("Auth:Audience").Value;
        _wssUrl = _configuration.GetSection("WebSocket:Url").Value;
        _apiUrl = _configuration.GetSection("WebSocket:ApiUrl").Value;
        _apiKey = _configuration.GetSection("WebSocket:ApiKey").Value;
        
        _pubSubClient = new PubSubServiceClient(_apiUrl, _apiKey, _audience);
        
        System.Console.WriteLine("Getting Token...");

        var token = GenerateToken();
        LogData(token);
        
        System.Console.WriteLine("Try connect to Server with Uri");
        var url = string.Format(_wssUrl, token);

        LogData(url);
            
        await Connect(url);
    }

    private static void LogLine()
    {
        System.Console.WriteLine("----------------------------");
    }

    
    private static string GenerateToken(string userId = "auth0|619a95dfd84c9a0068fd57cd")
    {
        return _pubSubClient.GetClientAccessToken(userId, TimeSpan.FromDays(7), new List<string>
        {
            "pubsub.sendToGroup"
        }).Result;
        
        // var mySecret = "";
        // var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
        //
        // var myIssuer = "https://pubsub.nuages.org";
        // var myAudience = "PubSubAudience";
        //
        // var tokenHandler = new JwtSecurityTokenHandler();
        // var tokenDescriptor = new SecurityTokenDescriptor
        // {
        //     Subject = new ClaimsIdentity(new []
        //     {
        //         new Claim("sub", userId)
        //     }),
        //     Expires = DateTime.UtcNow.AddDays(7),
        //     Issuer = myIssuer,
        //     Audience = myAudience,
        //     SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        // };
        //
        // var token = tokenHandler.CreateToken(tokenDescriptor);
        // return tokenHandler.WriteToken(token);
    }
    
    // private static async Task<string> GetTokenAsync()
    // {
    //     var client = new HttpClient();
    //
    //     var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
    //     {
    //         Address = $"{_authority}/oauth/token",
    //
    //         ClientId = _clientId,
    //         ClientSecret = _secret,
    //         Scope = "connect:pubsub",
    //         UserName = _email,
    //         Password = _password,
    //         Parameters =
    //         {
    //             { "audience", _audience}
    //         }
    //     });
    //
    //     return response.AccessToken;
    // }
    
    private static async Task Connect(string uri)
    {
        try
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

            System.Console.WriteLine("Nuages.PubSub.WebSocket Connected");
            System.Console.WriteLine("Sending echo message...");
                
            var msg = new {type = "echo", data = ""};
            await SendMessageToSocketAsync(_webSocket, msg);

            await Task.WhenAll(ReceiveFromSocket(_webSocket), SendToSocket(_webSocket));
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

    private static async Task SendToSocket(WebSocket webSocket)
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

    private static async Task SendMessageToSocketAsync(WebSocket webSocket, object msg)
    {
        var data = JsonSerializer.Serialize(msg);
        LogData($"Sending message {data}");
        var dataToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(data));
        
        await webSocket.SendAsync(dataToSend, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private static async Task ReceiveFromSocket(WebSocket webSocket)
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