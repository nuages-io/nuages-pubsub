using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.Disconnect;

public interface IDisconnectRoute
{
    Task<APIGatewayProxyResponse> DisconnectAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}