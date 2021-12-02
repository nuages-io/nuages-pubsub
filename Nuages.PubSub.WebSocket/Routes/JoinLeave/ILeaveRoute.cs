using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.JoinLeave;

public interface ILeaveRoute
{
    
    Task<APIGatewayProxyResponse> LeaveAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}