using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.Connect;

public interface IOnConnectedCallback
{
    Task OnConnectedAsync(APIGatewayProxyRequest request, ILambdaContext context);
}