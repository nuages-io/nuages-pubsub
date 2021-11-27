using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.WebSocket.Routes.Connect;

// ReSharper disable once UnusedType.Global
public class ConnectRoute : IConnectRoute
{
    private readonly IPubSubStorage _storage;

    public ConnectRoute(IPubSubStorage storage)
    {
        _storage = storage;
    }
    
    public async Task<APIGatewayProxyResponse> ConnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var sub = request.RequestContext.Authorizer.SingleOrDefault(c => c.Key == "sub").Value.ToString();

        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ConnectionId: {connectionId} User: {sub}");

            context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));

            await _storage.InsertAsync(request.GetHub(), connectionId, sub!);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = connectionId
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