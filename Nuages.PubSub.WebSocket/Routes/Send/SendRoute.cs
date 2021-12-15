using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Send;

public class SendRoute : ISendRoute
{   
    private readonly IPubSubService _pubSubService;

    public SendRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }

    public async Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var connectionId = "";
        string? ackId = null;
        
        try
        {
            context.Logger.LogLine(request.Body);
            
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
            
            var hasPermission = await _pubSubService.CheckPermissionAsync(request.GetHub(), PubSubPermission.SendMessageToGroup, request.RequestContext.ConnectionId, inMessage.group);

            if (hasPermission)
            {
                var message = new PubSubMessage
                {
                    group = inMessage.group,
                    from = PubSubMessageSource.group,
                    fromSub = request.GetUserId(),
                    type = "message",
                    dataType = inMessage.datatype,
                    data = inMessage.data
                };

                
                if (!string.IsNullOrEmpty(ackId))
                {
                    await _pubSubService.SendAckToConnectionAsync(hub, connectionId,ackId, true);
                }
                
                context.Logger.LogLine($"Message Payload: {JsonSerializer.Serialize(message) }");
                
                return await _pubSubService.SendToGroupAsync(request.GetHub(), inMessage.group, message,  
                                    inMessage.noEcho ? new List<string> { request.RequestContext.ConnectionId} : null);
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
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to send message: {e.Message}"
            };
        }
    }

    [ExcludeFromCodeCoverage]
    private static PubSubInboundGroupMessage GetInboundMessage(APIGatewayProxyRequest request)
    {
        var inMessage = JsonSerializer.Deserialize<PubSubInboundGroupMessage>(request.Body);
        if (inMessage == null)
            throw new NullReferenceException("message is null");

        if (string.IsNullOrEmpty(inMessage.group))
            throw new NullReferenceException("group must be provided");
        
        return inMessage;
    }
}