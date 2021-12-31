using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes;

// ReSharper disable once InconsistentNaming
public static class APIGatewayProxyRequestExtensions
{
    public static string GetHub(this APIGatewayProxyRequest request)
    {
        return request.RequestContext.Authorizer["nuageshub"].ToString() ?? throw new InvalidOperationException();
    }

    public static string GetUserId(this APIGatewayProxyRequest request)
    {
        return request.RequestContext.Authorizer["sub"].ToString() ?? throw new InvalidOperationException();
    }
}