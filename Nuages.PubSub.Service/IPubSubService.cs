using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Service;

public interface IPubSubService
{
    Task<APIGatewayProxyResponse> SendToAllAsync(string url, string content);
}