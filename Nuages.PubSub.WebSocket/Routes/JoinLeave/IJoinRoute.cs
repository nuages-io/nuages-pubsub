using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.JoinLeave;

public interface IJoinRoute
{
    
    Task<APIGatewayProxyResponse> JoinAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}