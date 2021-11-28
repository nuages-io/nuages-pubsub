using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string hub, string content, List<string>? excludedIds = null)
    {
        var connections = await _pubSubStorage.GetAllConnectionAsync(hub);
        if (excludedIds != null)
        {
            connections = connections.Where(c => !excludedIds.Contains(c.ConnectionId));
        }

        connections = connections.Where(c => !IsExpired(c));
        
        await SendMessageAsync(hub, connections.Select(c => c.ConnectionId),  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
    
    public async Task CloseAllConnectionsAsync(string hub)
    {
        var connections = await _pubSubStorage.GetAllConnectionAsync(hub);

        await CloseConnectionsAsync(hub, connections.Select(c => c.ConnectionId));
    }
}