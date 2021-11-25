using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.Echo;

public interface IEchoRoute
{
    
    Task<APIGatewayProxyResponse> EchoAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}