using Amazon.ApiGatewayManagementApi;

namespace Nuages.PubSub.Services;

public class PubSubServiceBase
{
    
    protected Func<string, AmazonApiGatewayManagementApiClient> ApiGatewayManagementApiClientFactory { get; }

    
    public PubSubServiceBase()
    {
        ApiGatewayManagementApiClientFactory =
            endpoint =>
                new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = endpoint
                });

    }
}