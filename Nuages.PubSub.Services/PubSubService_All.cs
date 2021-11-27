using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string hub, string content)
    {
        var ids = await _pubSubStorage.GetAllConnectionIdsAsync(hub);
        
        await SendMessageAsync(hub, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
    
    public async Task CloseAllConnectionsAsync(string hub)
    {
        var ids = await _pubSubStorage.GetAllConnectionIdsAsync(hub);

        await CloseConnectionsAsync(hub, ids);
    }
}