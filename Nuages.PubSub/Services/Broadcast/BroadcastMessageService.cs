using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services.Broadcast;

public class BroadcastMessageService : PubSubServiceBase, IBroadcastMessageService
{
    private readonly IPubSubStorage _storage;

    public BroadcastMessageService(IPubSubStorage storage)
    {
        _storage = storage;
    }
    
    public async Task<APIGatewayProxyResponse> Broadcast(APIGatewayProxyRequest request, ILambdaContext context)
    {
            try
            {
                // Construct the API Gateway endpoint that incoming message will be broadcasted to.
                var domainName = request.RequestContext.DomainName;
                var stage = request.RequestContext.Stage;

                var endpoint = $"https://{domainName}/{stage}";
                context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");

                // The body will look something like this: {"message":"sendmessage", "data":"What are you doing?"}
                var message = JsonDocument.Parse(request.Body);

                // Grab the data from the JSON body which is the message to broadcasted.
                if (!message.RootElement.TryGetProperty("data", out var dataProperty))
                {
                    context.Logger.LogLine("Failed to find data element in JSON document");
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int) HttpStatusCode.BadRequest
                    };
                }

                var data = dataProperty.GetString();
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(data!));

                // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
                var apiClient = ApiGatewayManagementApiClientFactory(endpoint);

                var items = _storage.GetAllConnectionIds();

                // Loop through all of the connections and broadcast the message out to the connections.
                var count = 0;
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
                        context.Logger.LogLine($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                        stream.Position = 0;
                        await apiClient.PostToConnectionAsync(postConnectionRequest);
                        count++;
                    }
                    catch (AmazonServiceException e)
                    {
                        // API Gateway returns a status of 410 GONE then the connection is no
                        // longer available. If this happens, delete the identifier
                        // from our collection.
                        if (e.StatusCode == HttpStatusCode.Gone)
                        {
                            await _storage.DeleteAsync(postConnectionRequest.ConnectionId);
                        }
                        else
                        {
                            context.Logger.LogLine(
                                $"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                            context.Logger.LogLine(e.StackTrace);
                        }
                    }
                }

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Body = "Data sent to " + count + " connection" + (count == 1 ? "" : "s")
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error disconnecting: " + e.Message);
                context.Logger.LogLine(e.StackTrace);
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.InternalServerError,
                    Body = $"Failed to send message: {e.Message}"
                };
            }
    }
}