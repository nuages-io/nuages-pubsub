using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Lambda.Routes.Disconnect;

// ReSharper disable once UnusedType.Global
public class DisconnectRoute : PubSubRouteBase, IDisconnectRoute
{
    private readonly IPubSubStorage _storage;

    public DisconnectRoute(IPubSubStorage storage)
    {
        _storage = storage;
    }

    public async Task<APIGatewayProxyResponse> DisconnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ConnectionId: {connectionId}");

            await _storage.DeleteAsync(request.RequestContext.ApiId, connectionId);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}