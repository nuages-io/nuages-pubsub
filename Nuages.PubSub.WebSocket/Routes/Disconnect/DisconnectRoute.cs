using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Disconnect;

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
            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));

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
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}