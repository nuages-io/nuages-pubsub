using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Routes.Disconnect;

public interface IDisconnectRoute
{
    Task<APIGatewayProxyResponse> Disconnect(APIGatewayProxyRequest request,
        ILambdaContext context);
}