using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;

namespace Nuages.PubSub.Services.Tests;

public class FakeApiGateway : IAmazonApiGatewayManagementApi
{
    public FakeApiGateway()
    {
        Config = new AmazonApiGatewayManagementApiConfig();
    }
    
    public IClientConfig Config { get; }
    
    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public async Task<DeleteConnectionResponse> DeleteConnectionAsync(DeleteConnectionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return await Task.FromResult(
        new DeleteConnectionResponse
        {
            HttpStatusCode = HttpStatusCode.OK
        });
    }

    public async Task<GetConnectionResponse> GetConnectionAsync(GetConnectionRequest request, CancellationToken cancellationToken = new CancellationToken())
    {
        return await Task.FromResult(
            new GetConnectionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
    }

    public async Task<PostToConnectionResponse> PostToConnectionAsync(PostToConnectionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return await Task.FromResult(
            new PostToConnectionResponse
            {
                HttpStatusCode = HttpStatusCode.OK
            });
    }
}

