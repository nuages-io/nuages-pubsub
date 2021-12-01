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
        try
        {
            context.Logger.LogLine(request.Body);
            
            var inMessage = JsonSerializer.Deserialize<PubSubInboundGroupMessage>(request.Body);
            if (inMessage == null)
                throw new NullReferenceException("message is null");

            if (string.IsNullOrEmpty(inMessage.group) )
                throw new NullReferenceException("group must be provided");
            
            var hasPermission = await _pubSubService.CheckPermissionAsync(request.GetHub(), PubSubPermission.SendMessageToGroup, request.RequestContext.ConnectionId, inMessage.group);

            if (hasPermission)
            {
                var message = new PubSubMessage
                {
                    group = inMessage.group,
                    from = PubSubMessageSource.group,
                    fromSub = request.GetSub(),
                    type = "message",
                    dataType = inMessage.datatype,
                    data = inMessage.data
                };

                context.Logger.LogLine($"Message Payload: {JsonSerializer.Serialize(message) }");
                
                return await _pubSubService.SendToGroupAsync(request.GetHub(), inMessage.group, message,  
                                    inMessage.noEcho ? new List<string> { request.RequestContext.ConnectionId} : null);
            }
           
            context.Logger.LogLine("Not authorized to send message to group");
            
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
            
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to send message: {e.Message}"
            };
        }
    }
}