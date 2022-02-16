using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;

// ReSharper disable once UnusedType.Global
public class JoinOrLeaveRoute 
{
    private readonly IPubSubService _pubSubService;

    protected JoinOrLeaveRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    protected async Task<APIGatewayProxyResponse> JoinOrLeaveAsync(APIGatewayProxyRequest request,
        ILambdaContext context, bool join)
    {

        string? ackId = null;
        var connectionId = "";
        
        try
        {
            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));
            
            var inMessage = GetInboundMessage(request);

            ackId = inMessage.ackId;
            
            var hub = request.GetHub();
            connectionId = request.RequestContext.ConnectionId;

            var ackIsValid = await _pubSubService.CreateAckAsync(hub, connectionId, ackId);
            if (!ackIsValid)
            {
                await _pubSubService.SendAckToConnectionAsync(hub, connectionId, ackId!, false, PubSubAckResult.Duplicate);
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = $"Duplicate ackId : {inMessage.ackId}"
                };
            }
            
            var hasPermission = await _pubSubService.CheckPermissionAsync(hub, PubSubPermission.JoinOrLeaveGroup, request.RequestContext.ConnectionId, inMessage.group);

            if (hasPermission)
            {
                if (join)
                    await _pubSubService.AddConnectionToGroupAsync(hub, inMessage.group,  connectionId);
                else
                {
                    await _pubSubService.RemoveConnectionFromGroupAsync(request.GetHub(), inMessage.group, connectionId);
                }
                
                if (!string.IsNullOrEmpty(ackId))
                {
                    await _pubSubService.SendAckToConnectionAsync(hub, connectionId,ackId, true);
                }
                
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200
                };
            }
            
            if (!string.IsNullOrEmpty(ackId))
                await _pubSubService.SendAckToConnectionAsync(hub, connectionId, ackId, false, PubSubAckResult.Forbidden);
                
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            
            if (!string.IsNullOrEmpty(ackId))
                await _pubSubService.SendAckToConnectionAsync(request.GetHub(), connectionId, ackId, false, PubSubAckResult.InternalServerError);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }

    [ExcludeFromCodeCoverage]
    private static PubSubInboundGroupMessage GetInboundMessage(APIGatewayProxyRequest request)
    {
        var inMessage = JsonSerializer.Deserialize<PubSubInboundGroupMessage>(request.Body);

        if (string.IsNullOrEmpty(inMessage!.group) )
            throw new NullReferenceException("group must be provided");
        
        return inMessage;
    }
}