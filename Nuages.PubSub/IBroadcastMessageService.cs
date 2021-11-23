using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub;

public interface IBroadcastMessageService
{
    public  Task<APIGatewayProxyResponse> Broadcast(APIGatewayProxyRequest request,
        ILambdaContext context);
}