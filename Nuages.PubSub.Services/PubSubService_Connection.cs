using System.Net;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToConnectionAsync(string hub,  string connectionId, string content)
    {
        await SendMessageAsync(hub, new List<string>{ connectionId } , content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseConnectionAsync(string hub, string connectionId)
    {
        var api = CreateApiGateway(_pubSubOptions.Uri!);

        await api.DeleteConnectionAsync(new DeleteConnectionRequest
        {
            ConnectionId = connectionId
        });

        await _pubSubStorage.Disconnect(hub, connectionId);
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionId)
    {
        return await _pubSubStorage.ConnectionExistsAsync(hub, connectionId);
    }
}