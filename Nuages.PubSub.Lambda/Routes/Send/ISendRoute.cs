using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Lambda.Routes.Send;

public interface ISendRoute
{
    public  Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}