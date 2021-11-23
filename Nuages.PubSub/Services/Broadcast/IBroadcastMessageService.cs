using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Services.Broadcast;

public interface IBroadcastMessageService
{
    public  Task<APIGatewayProxyResponse> Broadcast(APIGatewayProxyRequest request,
        ILambdaContext context);
}