using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;

// ReSharper disable once UnusedType.Global
public class LeaveRoute : JoinOrLeaveRoute, ILeaveRoute
{
    public LeaveRoute(IPubSubService pubSubService):base (pubSubService)
    {
    }

    public async Task<APIGatewayProxyResponse> LeaveAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {

        return await JoinOrLeaveAsync(request, context, false);
    }
}