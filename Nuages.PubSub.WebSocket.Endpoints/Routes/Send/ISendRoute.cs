using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Send;

public interface ISendRoute
{
    public  Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}