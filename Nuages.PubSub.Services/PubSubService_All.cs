using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string url, string audience, string content)
    {
        var ids = await _pubSubStorage.GetAllConnectionIdsAsync(audience);
        
        await SendMessageAsync(url, audience, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
    
    public async Task CloseAllConnectionsAsync(string url, string audience)
    {
        var ids = await _pubSubStorage.GetAllConnectionIdsAsync(audience);

        await CloseConnectionsAsync(url, audience, ids);
    }
}