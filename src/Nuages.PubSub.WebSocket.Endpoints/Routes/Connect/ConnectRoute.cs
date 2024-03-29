using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Endpoints.Routes.Connect;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ConnectRoute : IConnectRoute
{
    private readonly IPubSubService _pubSubService;

    public ConnectRoute(IPubSubService  pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    public async Task<APIGatewayProxyResponse> ConnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var userId = request.GetUserId();
        
        try
        {
            await _pubSubService.ConnectAsync(request.GetHub(), request.RequestContext.ConnectionId, userId);

            await ProcessRolesAsync(request, request.RequestContext.ConnectionId);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = request.RequestContext.ConnectionId
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error connecting: " + e.Message);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to connect: {e.Message}"
            };
        }
    }

    private async Task ProcessRolesAsync(APIGatewayProxyRequest request, string connectionId)
    {
        var roles = GetRoles(request);
        foreach (var r in roles)
        {
            var name = r.Split(".");

            await _pubSubService.GrantPermissionAsync(request.GetHub(), Enum.Parse<PubSubPermission>(name.First()),
                connectionId,
                name.Length > 1 ? name[1] : null);
        }
    }

    protected virtual IEnumerable<string> GetRoles(APIGatewayProxyRequest request)
    {
        if (!request.RequestContext.Authorizer.ContainsKey("roles"))
            return new List<string>();
        
        var roleClaim = request.RequestContext.Authorizer["roles"];
        return roleClaim != null ? roleClaim.ToString()!.Split(" ") : Array.Empty<string>();
    }
}