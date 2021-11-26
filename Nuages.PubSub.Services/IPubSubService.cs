using Amazon.Lambda.APIGatewayEvents;

namespace Nuages.PubSub.Services;

public interface IPubSubService
{
    Task<APIGatewayProxyResponse> SendToAllAsync(string url, string hub, string content);
    Task<APIGatewayProxyResponse> SendToOneAsync(string url, string hub, string connectionId, string content);

    //https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
    string GenerateToken(string issuer, string audience, string userId, IEnumerable<string> roles, string secret, TimeSpan? expireDelay = null);
}