using System.Net;
using System.Text;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Service;

public class PubSubService : IPubSubService
{
    private readonly IPubSubStorage _pubSubStorage;

    public PubSubService(IPubSubStorage pubSubStorage)
    {
        _pubSubStorage = pubSubStorage;
    }
    
    public virtual async Task<APIGatewayProxyResponse> SendToOneAsync(string url, string hub,  string connectionId, string content)
    {
        await SendMessageAsync(url, hub, new List<string>{ connectionId } , content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    public virtual async Task<APIGatewayProxyResponse> SendToAllAsync(string url, string hub, string content)
    {
        var ids = _pubSubStorage.GetAllConnectionIds(hub).ToList();
        
        await SendMessageAsync(url, hub, ids,  content);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }

    

    protected virtual async Task SendMessageAsync(string url, string hub, IEnumerable<string> connectionIds,  string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content!));
        
        using var apiGateway = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = url
        });
        
        foreach (var connectionId in connectionIds)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = stream
            };

            try
            {
                stream.Position = 0;
                    
                await apiGateway.PostToConnectionAsync(postConnectionRequest);

            }
            catch (AmazonServiceException e)
            {
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our collection.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    await _pubSubStorage.DeleteAsync(hub, postConnectionRequest.ConnectionId);
                }
               
            }
        }
        
       
    }
}