using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Routes.Connect;

public interface IConnectRoute
{
    
    Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}

