using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Routes.Authorize;

public interface IAuthorizeRoute
{
    Task<APIGatewayCustomAuthorizerResponse> Authorize(APIGatewayCustomAuthorizerRequest input, ILambdaContext context);
}