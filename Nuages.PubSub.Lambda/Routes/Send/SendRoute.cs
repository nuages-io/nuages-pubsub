using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Lambda.Model;
using Nuages.PubSub.Service;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Lambda.Routes.Send;

public class SendRoute : PubSubRouteBase, ISendRoute
{
    private readonly IPubSubStorage _storage;
    private readonly IPubSubService _pubSubService;

    public SendRoute(IPubSubStorage storage, IPubSubService pubSubService)
    {
        _storage = storage;
        _pubSubService = pubSubService;
    }

    public async Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            // Construct the API Gateway endpoint that incoming message will be broadcasted to.
            var domainName = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;

            var endpoint = $"https://{domainName}/{stage}";
            context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");

            context.Logger.LogLine($"Data: {request.Body}");
            
            // The body will look something like this: {"type":"sendmessage", "data":"What are you doing?"}
            var message = JsonSerializer.Deserialize<SendModel>(request.Body);

            context.Logger.LogLine($"Reserialized: {JsonSerializer.Serialize(message)}");
            
            // Grab the data from the JSON body which is the message to broadcasted.
            // if (!message.RootElement.TryGetProperty("data", out var dataProperty))
            // {
            //     context.Logger.LogLine("Failed to find data element in JSON document");
            //     return new APIGatewayProxyResponse
            //     {
            //         StatusCode = (int)HttpStatusCode.BadRequest
            //     };
            // }

            return await _pubSubService.SendToAllAsync(endpoint, message!.data.ToString()!);
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = $"Failed to send message: {e.Message}"
            };
        }
    }
}