using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.WebSocket.Routes.Connect;

namespace Nuages.PubSub.WebSocket.Application.CallBacks;

public class OnConnectedCallback : IOnConnectedCallback
{
    public async Task OnConnectedAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Console.WriteLine("Connected!!!");

        await Task.CompletedTask;
    }
}