using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub;

public interface IDisconnectService
{
    Task<APIGatewayProxyResponse> Disconnect(APIGatewayProxyRequest request,
        ILambdaContext context);
}