using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Lambda.Routes.Disconnect;

public interface IDisconnectRoute
{
    Task<APIGatewayProxyResponse> DisconnectAsync(APIGatewayProxyRequest request,
        ILambdaContext context);
}