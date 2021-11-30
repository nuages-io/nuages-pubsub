using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Routes.Join;

namespace Nuages.PubSub.WebSocket.Routes.Leave;

// ReSharper disable once UnusedType.Global
public class LeaveRoute : ILeaveRoute
{
    private readonly IPubSubService _pubSubService;

    public LeaveRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    public async Task<APIGatewayProxyResponse> LeaveAsync(APIGatewayProxyRequest request,
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
            
            var hasPermission = await _pubSubService.CheckPermissionAsync(request.GetHub(), PubSubPermission.JoinOrLeaveGroup, request.RequestContext.ConnectionId, inMessage.group);

            if (hasPermission)
            {
                await _pubSubService.RemoveConnectionFromGroupAsync(request.GetHub(), inMessage.group, connectionId);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200
                };
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
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}