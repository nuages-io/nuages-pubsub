using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Routes.Send;

public interface ISendRoute
{
    public  Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}