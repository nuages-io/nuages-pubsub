using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;

public interface IJoinRoute
{
    
    Task<APIGatewayProxyResponse> JoinAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}