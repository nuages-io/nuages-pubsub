using System.Net;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;

namespace Nuages.PubSub.Services;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public partial class PubSubService
{
    public virtual async Task<APIGatewayProxyResponse> SendToConnectionAsync(string hub,  string connectionId, PubSubMessage message)
    {
        await SendMessageAsync(hub, new List<string>{ connectionId } , message);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseConnectionAsync(string hub, string connectionId)
    {
        await CloseConnectionsAsync(hub, new List<string> { connectionId});
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionId)
    {
        try
        {
            using var apiGateway = CreateApiGateway(_pubSubOptions.Uri!);
            var response = await apiGateway.GetConnectionAsync(new GetConnectionRequest
            {
                ConnectionId = connectionId
            });
        
            return await _pubSubStorage.ConnectionExistsAsync(hub, connectionId);
        }
        catch (AmazonServiceException e)
        {
            Console.WriteLine(e.Message);
            // API Gateway returns a status of 410 GONE then the connection is no
            // longer available. If this happens, delete the identifier
            // from our collection.
            if (e.StatusCode == HttpStatusCode.Gone)
            {
                await DisconnectAsync(hub, connectionId);
            }

            return false;
        }
       
    }
}