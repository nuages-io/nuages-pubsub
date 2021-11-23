using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub;

public interface IAuthorizeService
{
    Task<APIGatewayCustomAuthorizerResponse> Authorize(APIGatewayCustomAuthorizerRequest input, ILambdaContext context);
}