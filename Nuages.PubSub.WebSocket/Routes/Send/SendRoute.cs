using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Model;

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
            var endpoint = $"https://{request.RequestContext.DomainName}";
            if (endpoint.ToLower().EndsWith("amazonaws.com"))
                endpoint += $"/{request.RequestContext.Stage}";
            
            context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");
            
            var message = JsonSerializer.Deserialize<SendModel>(request.Body);
            if (message == null)
                throw new NullReferenceException("message is null");

            if (string.IsNullOrEmpty(message.group) )
                throw new NullReferenceException("group must be provided");
            
            var hasPermission = await _pubSubService.CheckPermissionAsync(request.GetHub(), PubSubPermission.SendMessageToGroup, request.RequestContext.ConnectionId, message.group);

            if (hasPermission)
            {
                var result = new MessageModel
                {
                    group = message.group,
                    from = "group",
                    fromSub = request.GetSub(),
                    type = "message",
                    datatype = message.datatype,
                    data = new
                    {
                        message.data
                    }
                };

                return await _pubSubService.SendToGroupAsync(request.GetHub(), message.group, JsonSerializer.Serialize(result),  
                                    message.noEcho ? new List<string> { request.RequestContext.ConnectionId} : null);
            }
           
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