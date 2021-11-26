using System.Net;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToConnectionAsync(string url, string audience,  string connectionId, string content)
    {
        await SendMessageAsync(url, audience, new List<string>{ connectionId } , content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseConnectionAsync(string url, string audience, string connectionId)
    {
        var api = CreateApiGateway(url);

        await api.DeleteConnectionAsync(new DeleteConnectionRequest
        {
            ConnectionId = connectionId
        });

        await _pubSubStorage.DeleteAsync(audience, connectionId);
    }

    public async Task<bool> ConnectionExistsAsync(string audience, string connectionId)
    {
        return await _pubSubStorage.ConnectionExistsAsync(audience, connectionId);
    }
}