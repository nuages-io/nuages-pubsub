using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Services;

public partial class PubSubService
{
    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        return await _pubSubStorage.IsUserInGroupAsync(hub, group, userId);
    }

    public async Task<APIGatewayProxyResponse> SendToUserAsync(string hub, string userId, PubSubMessage message, List<string>? excludedIds = null)
    {
        var connections = _pubSubStorage.GetConnectionsForUserAsync(hub, userId);
        if (excludedIds != null)
        {
            connections = connections.Where(c => !excludedIds.Contains(c.ConnectionId));
        }

        connections = connections.Where(c => !c.IsExpired());
        
        await SendMessageAsync(hub, connections.Select(c => c.ConnectionId),  message);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseUserConnectionsAsync(string hub, string userId)
    {
        var connections = _pubSubStorage.GetConnectionsForUserAsync(hub, userId);

        await CloseConnectionsAsync(hub, connections.Select(c => c.ConnectionId));
    }
    
    public async Task<bool> UserExistsAsync(string hub, string userId)
    {
        return await _pubSubStorage.UserHasConnectionsAsync(hub, userId);
    }
}