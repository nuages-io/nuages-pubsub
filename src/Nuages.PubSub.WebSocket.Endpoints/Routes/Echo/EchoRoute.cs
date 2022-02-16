using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Echo;

// ReSharper disable once UnusedType.Global
public class EchoRoute : IEchoRoute
{
    private readonly IPubSubService _pubSubService;

    public EchoRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    public async Task<APIGatewayProxyResponse> EchoAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));
            
            var message = new PubSubMessage
            {
                from = PubSubMessageSource.self,
                type = "echo",
                data = new
                {
                    connectionId = request.RequestContext.ConnectionId
                }
            };
            
            context.Logger.LogLine($"Message Payload: {JsonSerializer.Serialize(message) }");
            
            return await _pubSubService.SendToConnectionAsync(request.GetHub(),  request.RequestContext.ConnectionId, message);
            
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