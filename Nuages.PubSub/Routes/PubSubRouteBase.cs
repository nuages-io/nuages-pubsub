using Amazon.ApiGatewayManagementApi;

namespace Nuages.PubSub.Routes;

public class PubSubRouteBase
{
    protected Func<string, AmazonApiGatewayManagementApiClient> ApiGatewayManagementApiClientFactory { get; }
    
    protected PubSubRouteBase()
    {
        ApiGatewayManagementApiClientFactory =
            endpoint =>
                new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = endpoint
                });
    
    }
}