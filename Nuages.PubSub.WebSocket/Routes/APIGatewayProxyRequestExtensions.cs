using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.WebSocket.Routes;

// ReSharper disable once InconsistentNaming
public static class APIGatewayProxyRequestExtensions
{
    public static string GetHub(this APIGatewayProxyRequest request)
    {
        return request.RequestContext.Authorizer["nuageshub"].ToString() ?? throw new InvalidOperationException();
    }
}