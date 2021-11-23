using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services.Authorize;
using Nuages.PubSub.Services.Broadcast;
using Nuages.PubSub.Services.Connect;
using Nuages.PubSub.Services.Disconnect;
using Nuages.PubSub.Services.Echo;

namespace Nuages.PubSub;

public class PubSubFunction
{
    private IEchoService? _echoService;
    private IDisconnectService? _disconnectService;
    private IConnectService? _connectService;
    private IAuthorizeService? _authorizeService;
    private IBroadcastMessageService? _broadcastMessageService;
    
    protected void GetServices(ServiceProvider serviceProvider)
    {
        _echoService = serviceProvider.GetRequiredService<IEchoService>();
        _disconnectService = serviceProvider.GetRequiredService<IDisconnectService>();
        _connectService = serviceProvider.GetRequiredService<IConnectService>();
        _authorizeService = serviceProvider.GetRequiredService<IAuthorizeService>();
        _broadcastMessageService = serviceProvider.GetRequiredService<IBroadcastMessageService>();
    }
    
    public async Task<APIGatewayProxyResponse> EchoHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_echoService == null)
            throw new NullReferenceException("_echoService is null");
        
        return await _echoService.Echo(request, context);
    }

    public  async Task<APIGatewayProxyResponse> OnDisconnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_disconnectService == null)
            throw new NullReferenceException("_disconnectService is null");
        return await _disconnectService.Disconnect(request, context);
    }

    public async Task<APIGatewayProxyResponse> OnConnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_connectService == null)
            throw new NullReferenceException("_connectService is null");
        return await _connectService.Connect(request, context);
    }
        
    public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        if (_authorizeService == null)
            throw new NullReferenceException("_authorizeService is null");
        
        return await _authorizeService.Authorize(input, context);
    }

    public async Task<APIGatewayProxyResponse> BroadcastMessageHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_broadcastMessageService == null)
            throw new NullReferenceException("_broadcastMessageService is null");
        
        return await _broadcastMessageService.Broadcast(request, context);
    }
}