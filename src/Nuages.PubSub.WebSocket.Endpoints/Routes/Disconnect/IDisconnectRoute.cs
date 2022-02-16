using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Disconnect;

public interface IDisconnectRoute
{
    Task<APIGatewayProxyResponse> DisconnectAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}