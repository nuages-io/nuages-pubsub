using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Service;

public interface IPubSubService
{
    Task<APIGatewayProxyResponse> SendToAllAsync(string url, string hub, string content);
    Task<APIGatewayProxyResponse> SendToOneAsync(string url, string hub, string connectionId, string content);
}