using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Connect;

// ReSharper disable once UnusedType.Global
public class ConnectRoute : IConnectRoute
{
    private readonly IPubSubService _pubSubService;
    private readonly IOnConnectedCallback? _onConnectedCallback;

    public ConnectRoute(IPubSubService  pubSubService, IOnConnectedCallback? onConnectedCallback = null)
    {
        _pubSubService = pubSubService;
        _onConnectedCallback = onConnectedCallback;
    }
    
    public async Task<APIGatewayProxyResponse> ConnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sub = request.GetSub();
        
        try
        {
            var connectionId = request.RequestContext.ConnectionId;

            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));

            await _pubSubService.ConnectAsync(request.GetHub(), connectionId, sub);

            var roles = GetRoles(request);
            foreach (var r in roles)
            {
                var name = r.Split(".");
                    
                await _pubSubService.GrantPermissionAsync(request.GetHub(), Enum.Parse<PubSubPermission>(name.First()), connectionId,
                    name.Length > 1 ? name[1] : null);
            }
            
            if (_onConnectedCallback != null)
            {
                await _onConnectedCallback.OnConnectedAsync(request, context);
            }
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = connectionId
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error connecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to connect: {e.Message}"
            };
        }
    }

    protected virtual string[] GetRoles(APIGatewayProxyRequest request)
    {
        var roleClaim = request.RequestContext.Authorizer["roles"];
        if (roleClaim != null)
        {
            return roleClaim.ToString()!.Split(" ");
        }

        return Array.Empty<string>();
    }
}