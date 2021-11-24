using System.Text;
using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Nuages.PubSub.Lambda.Routes.Echo;

// ReSharper disable once UnusedType.Global
public class EchoRoute : PubSubRouteBase, IEchoRoute
{
    public async Task<APIGatewayProxyResponse> EchoAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var domainName = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;

            var endpoint = $"https://{domainName}/{stage}";
            context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");

            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ECHO ConnectionId: {connectionId}");

            var result = new
            {
                target = "echo",
                payload = new
                {
                    ConnectionId = connectionId
                }
            };

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));

            //var apiClient = ApiGatewayManagementApiClientFactory(endpoint);
            using var apiClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = endpoint
            });
            
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = stream
            };

            await apiClient.PostToConnectionAsync(postConnectionRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(result)
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