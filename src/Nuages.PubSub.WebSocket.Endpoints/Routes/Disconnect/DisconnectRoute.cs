using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Disconnect;

// ReSharper disable once UnusedType.Global
public class DisconnectRoute : IDisconnectRoute
{
    private readonly IPubSubService _pubSubService;

    public DisconnectRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }

    public async Task<APIGatewayProxyResponse> DisconnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            await _pubSubService.CloseConnectionAsync(request.GetHub(), request.RequestContext.ConnectionId);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}