using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public partial class PubSubService
{
    public async Task<APIGatewayProxyResponse> SendToGroupAsync(string url, string audience, string group, string content)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForGroupAsync(audience, group);
        
        await SendMessageAsync(url, audience, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseGroupConnectionsAsync(string url, string audience, string group)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForGroupAsync(audience, group);

        await CloseConnectionsAsync(url, audience, ids);
    }

    public async Task<bool> GroupExistsAsync(string audience, string group)
    {
        return await _pubSubStorage.GroupHasConnectionsAsync(audience, group);
    }

    public async Task AddConnectionToGroupAsync(string audience, string group, string connectionId)
    {
        await _pubSubStorage.AddConnectionToGroupAsync(audience, group, connectionId);
    }

    public async Task RemoveConnectionFromGroupAsync(string audience, string group, string connectionId)
    {
        await _pubSubStorage.RemoveConnectionFromGroupAsync(audience, group, connectionId);
    }

    public async Task AddUserToGroupAsync(string audience, string group, string userId)
    {
        await _pubSubStorage.AddUserToGroupAsync(audience, group, userId);
    }

    public async Task RemoveUserFromGroupAsync(string audience, string group, string userId)
    {
        await _pubSubStorage.RemoveUserFromGroupAsync(audience, group, userId);
    }

    public async Task RemoveUserFromAllGroupsAsync(string audience, string userId)
    {
        await _pubSubStorage.RemoveUserFromAllGroupsAsync(audience, userId);
    }
}