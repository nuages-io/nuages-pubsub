using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public partial class PubSubService
{
    public async Task<APIGatewayProxyResponse> SendToUserAsync(string url, string audience, string userId, string content)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForUserAsync(audience, userId);
        
        await SendMessageAsync(url, audience, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
    
    public async Task CloseUserConnectionsAsync(string url, string audience, string userId)
    {
        var ids = await _pubSubStorage.GetConnectionIdsForUserAsync(audience, userId);

        await CloseConnectionsAsync(url, audience, ids);
    }
    
    public async Task<bool> UserExistsAsync(string audience, string userId)
    {
        return await _pubSubStorage.UserHasConnectionsAsync(audience, userId);
    }
}