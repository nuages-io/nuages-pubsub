using Amazon.ApiGatewayManagementApi;

namespace Nuages.PubSub.Service;

public class PubSubService : IPubSubService
{
    private readonly AmazonApiGatewayManagementApiClient _apiGateway;

    public PubSubService(string url)
    {
        _apiGateway = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
            {
                ServiceURL = url
            });
    }

    // public async Task SendToAllAsync(string content)
    // {
    //     
    // }
}

public interface IPubSubService
{
}