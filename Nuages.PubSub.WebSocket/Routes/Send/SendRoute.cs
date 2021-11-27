using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Services;
using Nuages.PubSub.WebSocket.Model;

namespace Nuages.PubSub.WebSocket.Routes.Send;

public class SendRoute : ISendRoute
{
    private const string Target = "sendmessage";
    
    private readonly IPubSubService _pubSubService;

    public SendRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }

    public async Task<APIGatewayProxyResponse> SendAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var endpoint = $"https://{request.RequestContext.DomainName}";
            if (endpoint.ToLower().EndsWith("amazonaws.com"))
                endpoint += $"/{request.RequestContext.Stage}";
            context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");
            
            var message = JsonSerializer.Deserialize<SendModel>(request.Body);
            if (message == null)
                throw new NullReferenceException("message is null");
            
            var result = new
            {
                target = Target,
                payload = new
                {
                    message.data
                }
            };

            return await _pubSubService.SendToAllAsync(request.GetHub(), JsonSerializer.Serialize(result));
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