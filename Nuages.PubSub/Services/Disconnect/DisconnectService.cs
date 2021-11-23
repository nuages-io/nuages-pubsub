using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Storage;

namespace Nuages.PubSub.Services.Disconnect;

// ReSharper disable once UnusedType.Global
public class DisconnectService : PubSubServiceBase, IDisconnectService
{
    private readonly IPubSubStorage _storage;

    public DisconnectService(IPubSubStorage storage)
    {
        _storage = storage;
    }
    
    public async Task<APIGatewayProxyResponse> Disconnect(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ConnectionId: {connectionId}");

            await _storage.DeleteAsync(connectionId);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Disconnected."
            };
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}