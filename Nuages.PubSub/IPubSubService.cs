using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub;

public interface IPubSubService
{
    
    Task<APIGatewayProxyResponse> Echo(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}