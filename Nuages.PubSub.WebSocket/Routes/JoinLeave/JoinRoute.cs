using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.JoinLeave;

// ReSharper disable once UnusedType.Global
public class JoinRoute : JoinOrLeaveRoute, IJoinRoute
{
    public JoinRoute(IPubSubService pubSubService) : base(pubSubService)
    {
    }
    
    public async Task<APIGatewayProxyResponse> JoinAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        return await JoinOrLeaveAsync(request, context, true);
    }

}