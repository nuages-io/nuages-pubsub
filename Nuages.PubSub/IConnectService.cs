using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub;

public interface IConnectService
{
    
    Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}

