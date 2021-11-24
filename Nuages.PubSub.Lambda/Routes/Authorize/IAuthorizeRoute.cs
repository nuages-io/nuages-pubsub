using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Lambda.Routes.Authorize;

public interface IAuthorizeRoute
{
    Task<APIGatewayCustomAuthorizerResponse> AuthorizeAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context);
}