using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Routes.Authorize;
using Nuages.PubSub.Routes.Broadcast;
using Nuages.PubSub.Routes.Connect;
using Nuages.PubSub.Routes.Disconnect;
using Nuages.PubSub.Routes.Echo;

namespace Nuages.PubSub;

public class PubSubFunction
{
    private IEchoRoute? _echoRoute;
    private IDisconnectRoute? _disconnectRoute;
    private IConnectRoute? _connectRoute;
    private IAuthorizeRoute? _authorizeRoute;
    private IBroadcastMessageRoute? _broadcastMessageRoute;
    
    protected void GetRequiredServices(ServiceProvider serviceProvider)
    {
        _echoRoute = serviceProvider.GetRequiredService<IEchoRoute>();
        _disconnectRoute = serviceProvider.GetRequiredService<IDisconnectRoute>();
        _connectRoute = serviceProvider.GetRequiredService<IConnectRoute>();
        _authorizeRoute = serviceProvider.GetRequiredService<IAuthorizeRoute>();
        _broadcastMessageRoute = serviceProvider.GetRequiredService<IBroadcastMessageRoute>();
    }
    
    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> EchoHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_echoRoute == null)
            throw new NullReferenceException("_echoService is null");
        
        return await _echoRoute.Echo(request, context);
    }

    // ReSharper disable once UnusedMember.Global
    public  async Task<APIGatewayProxyResponse> OnDisconnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_disconnectRoute == null)
            throw new NullReferenceException("_disconnectService is null");
        return await _disconnectRoute.Disconnect(request, context);
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> OnConnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_connectRoute == null)
            throw new NullReferenceException("_connectService is null");
        return await _connectRoute.Connect(request, context);
    }
        
    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        if (_authorizeRoute == null)
            throw new NullReferenceException("_authorizeService is null");
        
        return await _authorizeRoute.Authorize(input, context);
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> BroadcastMessageHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_broadcastMessageRoute == null)
            throw new NullReferenceException("_broadcastMessageService is null");
        
        return await _broadcastMessageRoute.Broadcast(request, context);
    }
}