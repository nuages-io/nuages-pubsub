using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Routes.Broadcast;

public interface IBroadcastMessageRoute
{
    public  Task<APIGatewayProxyResponse> Broadcast(APIGatewayProxyRequest request,
        ILambdaContext context);
}