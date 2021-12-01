using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.WebSocket.Routes.Connect;

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

    protected virtual IEnumerable<string> GetRoles(APIGatewayProxyRequest request)
    {
        var roleClaim = request.RequestContext.Authorizer["roles"];
        return roleClaim != null ? roleClaim.ToString()!.Split(" ") : Array.Empty<string>();
    }
}