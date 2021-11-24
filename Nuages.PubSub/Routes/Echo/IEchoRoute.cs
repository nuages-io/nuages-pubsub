using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Routes.Echo;

public interface IEchoRoute
{
    
    Task<APIGatewayProxyResponse> Echo(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}