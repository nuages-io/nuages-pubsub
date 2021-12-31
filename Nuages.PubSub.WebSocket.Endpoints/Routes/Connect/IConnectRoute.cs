using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Connect;

public interface IConnectRoute
{
    
    Task<APIGatewayProxyResponse> ConnectAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}

