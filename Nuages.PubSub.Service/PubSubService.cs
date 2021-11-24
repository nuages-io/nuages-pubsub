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

    public async Task<APIGatewayProxyResponse> SendToAllAsync(string url, string content)
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content!));
        
        var apiGateway = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            ServiceURL = url
        });
        
        var items = _pubSubStorage.GetAllConnectionIds();

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var item in items)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item,
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
                    await _pubSubStorage.DeleteAsync(postConnectionRequest.ConnectionId);
                }
               
            }
        }
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK
        };
    }
}