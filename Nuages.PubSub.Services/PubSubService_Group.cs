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
}