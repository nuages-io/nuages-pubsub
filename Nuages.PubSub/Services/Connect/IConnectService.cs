using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Services.Connect;

public interface IConnectService
{
    
    Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request,
        ILambdaContext context);
    
}

