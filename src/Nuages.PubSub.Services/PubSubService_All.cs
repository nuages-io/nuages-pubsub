using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string hub, PubSubMessage message, List<string>? excludedIds = null)
    {
        var connections =  _pubSubStorage.GetAllConnectionAsync(hub);
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
    
    public async Task CloseAllConnectionsAsync(string hub)
    {
        var connections =  _pubSubStorage.GetAllConnectionAsync(hub);

        await CloseConnectionsAsync(hub, connections.Select(c => c.ConnectionId));
    }
}