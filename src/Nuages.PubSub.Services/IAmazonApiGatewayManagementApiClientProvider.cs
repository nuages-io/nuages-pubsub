using System.Diagnostics.CodeAnalysis;
using Amazon.ApiGatewayManagementApi;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Services;

public interface IAmazonApiGatewayManagementApiClientProvider
{
    IAmazonApiGatewayManagementApi  Create(string url, string region);
}

[ExcludeFromCodeCoverage]
public class AmazonApiGatewayManagementApiClientProvider : IAmazonApiGatewayManagementApiClientProvider
{
    private readonly IConfiguration _configuration;

    public AmazonApiGatewayManagementApiClientProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public IAmazonApiGatewayManagementApi Create(string url, string region)
    {
        Console.WriteLine($"Crwate AmazonApiGatewayManagementApiClient : {url} {region}");
        return new AmazonApiGatewayManagementApiClient(_configuration["AccessKey"], _configuration["SecretKey"], new AmazonApiGatewayManagementApiConfig
        {
            AuthenticationRegion = region,
            ServiceURL = url
        });
    }
}