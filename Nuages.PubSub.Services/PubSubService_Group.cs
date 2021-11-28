using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public partial class PubSubService
{
    public async Task<APIGatewayProxyResponse> SendToGroupAsync(string hub, string group, string content, List<string>? excludedIds = null)
    {
        var connections = await _pubSubStorage.GetConnectionsForGroupAsync(hub, group);

        if (excludedIds != null)
        {
            connections = connections.Where(i => !excludedIds.Contains(i.ConnectionId));
        }

        connections = connections.Where(c => !IsExpired(c));
        
        await SendMessageAsync(hub, connections.Select(c => c.ConnectionId),  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseGroupConnectionsAsync(string hub, string group)
    {
        var connections = await _pubSubStorage.GetConnectionsForGroupAsync(hub, group);

        await CloseConnectionsAsync(hub, connections.Select(c => c.ConnectionId));
    }

    public async Task<bool> GroupExistsAsync(string hub, string group)
    {
        return await _pubSubStorage.GroupHasConnectionsAsync(hub, group);
    }

    public async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        await _pubSubStorage.AddConnectionToGroupAsync(hub, group, connectionId);
    }

    public async Task RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        await _pubSubStorage.RemoveConnectionFromGroupAsync(hub, group, connectionId);
    }

    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        await _pubSubStorage.AddUserToGroupAsync(hub, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        await _pubSubStorage.RemoveUserFromGroupAsync(hub, group, userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        await _pubSubStorage.RemoveUserFromAllGroupsAsync(hub, userId);
    }
}