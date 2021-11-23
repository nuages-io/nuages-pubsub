using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MongoDB.Bson;
using Nuages.PubSub.DataModel;

namespace Nuages.PubSub;

// ReSharper disable once UnusedType.Global
public class ConnectService : PubSubServiceBase, IConnectService
{
    private readonly IWebSocketRepository _webSocketRepository;

    public ConnectService(IWebSocketRepository webSocketRepository)
    {
        _webSocketRepository = webSocketRepository;
    }
    
    public async Task<APIGatewayProxyResponse> Connect(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sub = request.RequestContext.Authorizer.SingleOrDefault(c => c.Key == "sub").Value.ToString();

        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ConnectionId: {connectionId} User: {sub}");

            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));

            await _webSocketRepository.InsertOneAsync(new WebSocketConnection
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ConnectionId = connectionId,
                Sub = sub!
            });

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Connected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error connecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to connect: {e.Message}"
            };
        }
    }
}