using Amazon.ApiGatewayManagementApi;

namespace Nuages.PubSub.Services;

public interface IAmazonApiGatewayManagementApiClientProvider
{
    IAmazonApiGatewayManagementApi  Create(string url, string region);
}

public class AmazonApiGatewayManagementApiClientProvider : IAmazonApiGatewayManagementApiClientProvider
{
    public IAmazonApiGatewayManagementApi Create(string url, string region)
    {
        return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
        {
            AuthenticationRegion = region,
            ServiceURL = url
        });
    }
}