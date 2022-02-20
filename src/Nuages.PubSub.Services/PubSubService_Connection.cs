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
        await SendMessageAsync(hub, new List<string>{ connectionId }.ToAsyncEnumerable() , message);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public async Task CloseConnectionAsync(string hub, string connectionId)
    {
        await CloseConnectionsAsync(hub, new List<string> { connectionId}.ToAsyncEnumerable());
    }

    public async Task<bool> ConnectionExistsAsync(string hub, string connectionId)
    {
        try
        {
            using var apiGateway = CreateApiGateway(_pubSubOptions.Uri!);
            Console.WriteLine($"uri = {_pubSubOptions.Uri} COnnectionId = {connectionId} hub = {hub}");
            
            await apiGateway.GetConnectionAsync(new GetConnectionRequest
            {
                ConnectionId = connectionId
            });

            var res =  await _pubSubStorage.ConnectionExistsAsync(hub, connectionId);

            if (!res)
            {
                Console.WriteLine($"Connection {connectionId} does not exists");
            }

            return res;
        }
        catch (GoneException)
        {
            Console.WriteLine($"Connection {connectionId} is gone");
            await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
            return false;
        }
        catch (AmazonServiceException e)
        {
            Console.WriteLine(e.Message);
            // API Gateway returns a status of 410 GONE then the connection is no
            // longer available. If this happens, delete the identifier
            // from our collection.
            if (e.StatusCode == HttpStatusCode.Gone)
            {
                await _pubSubStorage.DeleteConnectionAsync(hub, connectionId);
            }

            Console.WriteLine($"Exception {e.Message}");
            
            return false;
        }
       
    }
}