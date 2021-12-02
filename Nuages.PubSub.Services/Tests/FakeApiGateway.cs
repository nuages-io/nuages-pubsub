using System.Net;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;

namespace Nuages.PubSub.Services.Tests;

public class FakeApiGateway : IAmazonApiGatewayManagementApi
{
    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.OK;

    public readonly List<(PostToConnectionRequest, PostToConnectionResponse)> PostRequestResponse = new ();
    public readonly List<(GetConnectionRequest, GetConnectionResponse)> GetRequestResponse = new ();
    public readonly List<(DeleteConnectionRequest, DeleteConnectionResponse)> DeleteRequestResponse = new ();
    
    public FakeApiGateway()
    {
        Config = new AmazonApiGatewayManagementApiConfig();
    }
    
    public IClientConfig Config { get; }
    
    public void Dispose()
    {
        
    }

    public async Task<DeleteConnectionResponse> DeleteConnectionAsync(DeleteConnectionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (HttpStatusCode != HttpStatusCode.OK)
            throw new AmazonServiceException("Error")
            {
                StatusCode = HttpStatusCode
            };

        var response = new DeleteConnectionResponse
        {
            HttpStatusCode = HttpStatusCode
        };

        DeleteRequestResponse.Add((request, response));
        
        return await Task.FromResult(response);
    }

    public async Task<GetConnectionResponse> GetConnectionAsync(GetConnectionRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        if (HttpStatusCode != HttpStatusCode.OK)
            throw new AmazonServiceException("Error")
            {
                StatusCode = HttpStatusCode
            };

        var response = new GetConnectionResponse
        {
            HttpStatusCode = HttpStatusCode
        };
            
        GetRequestResponse.Add((request, response));
        
        return await Task.FromResult(response);
    }

    public async Task<PostToConnectionResponse> PostToConnectionAsync(PostToConnectionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (HttpStatusCode != HttpStatusCode.OK)
            throw new AmazonServiceException("Error")
            {
                StatusCode = HttpStatusCode
            };

        var response = new PostToConnectionResponse
        {
            HttpStatusCode = HttpStatusCode
        };
        
        PostRequestResponse.Add((request, response));
        
        return await Task.FromResult(response);
    }
}

