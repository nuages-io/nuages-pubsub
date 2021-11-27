using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public partial class PubSubService
{
    public async Task<APIGatewayProxyResponse> SendToUserAsync(string hub, string userId, string content)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForUserAsync(hub, userId);
        
        await SendMessageAsync(hub, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
    
    public async Task CloseUserConnectionsAsync(string hub, string userId)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForUserAsync(hub, userId);

        await CloseConnectionsAsync(hub, ids);
    }
    
    public async Task<bool> UserExistsAsync(string hub, string userId)
    {
        return await _pubSubStorage.UserHasConnectionsAsync(hub, userId);
    }
}