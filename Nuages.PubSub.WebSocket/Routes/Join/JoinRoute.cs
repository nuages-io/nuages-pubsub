using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Join;

// ReSharper disable once UnusedType.Global
public class JoinRoute : IJoinRoute
{
    private readonly IPubSubService _pubSubService;

    public JoinRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    public async Task<APIGatewayProxyResponse> JoinAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            context.Logger.LogLine(request.Body);
            
            var inMessage = JsonSerializer.Deserialize<PubSubInboundGroupMessage>(request.Body);
            if (inMessage == null)
                throw new NullReferenceException("message is null");

            if (string.IsNullOrEmpty(inMessage.group) )
                throw new NullReferenceException("group must be provided");
            
            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));
            
            var connectionId = request.RequestContext.ConnectionId;
            
            await _pubSubService.AddConnectionToGroupAsync(request.GetHub(), inMessage.group,  connectionId, request.GetSub());
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200
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