using Amazon.ApiGatewayManagementApi;

namespace Nuages.PubSub.Services.Tests;

public class FakeApiGatewayProvider : IAmazonApiGatewayManagementApiClientProvider
{
    private readonly IAmazonApiGatewayManagementApi _apiGatewayManagementApi;

    public FakeApiGatewayProvider(IAmazonApiGatewayManagementApi apiGatewayManagementApi)
    {
        _apiGatewayManagementApi = apiGatewayManagementApi;
    }
    
    public IAmazonApiGatewayManagementApi Create(string url, string region)
    {
        return _apiGatewayManagementApi;
    }
}