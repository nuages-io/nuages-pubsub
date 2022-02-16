using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;

public interface ILeaveRoute
{
    
    Task<APIGatewayProxyResponse> LeaveAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}