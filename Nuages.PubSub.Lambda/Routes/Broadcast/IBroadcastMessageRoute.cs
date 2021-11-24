using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Lambda.Routes.Broadcast;

public interface IBroadcastMessageRoute
{
    public  Task<APIGatewayProxyResponse> BroadcastAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}