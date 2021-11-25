using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.WebSocket.Routes.Authorize;
using Nuages.PubSub.WebSocket.Routes.Connect;
using Nuages.PubSub.WebSocket.Routes.Disconnect;
using Nuages.PubSub.WebSocket.Routes.Echo;
using Nuages.PubSub.WebSocket.Routes.Send;

namespace Nuages.PubSub.WebSocket;

public class PubSubFunction
{
    private IEchoRoute? _echoRoute;
    private IDisconnectRoute? _disconnectRoute;
    private IConnectRoute? _connectRoute;
    private IAuthorizeRoute? _authorizeRoute;
    private ISendRoute? _sendMessageRoute;
    
    protected void GetRequiredServices(ServiceProvider serviceProvider)
    {
        _echoRoute = serviceProvider.GetRequiredService<IEchoRoute>();
        _disconnectRoute = serviceProvider.GetRequiredService<IDisconnectRoute>();
        _connectRoute = serviceProvider.GetRequiredService<IConnectRoute>();
        _authorizeRoute = serviceProvider.GetRequiredService<IAuthorizeRoute>();
        _sendMessageRoute = serviceProvider.GetRequiredService<ISendRoute>();
    }
    
    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> EchoHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_echoRoute == null)
            throw new NullReferenceException("_echoService is null");
        
        return await _echoRoute.EchoAsync(request, context);
    }

    // ReSharper disable once UnusedMember.Global
    public  async Task<APIGatewayProxyResponse> OnDisconnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_disconnectRoute == null)
            throw new NullReferenceException("_disconnectService is null");
        return await _disconnectRoute.DisconnectAsync(request, context);
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> OnConnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_connectRoute == null)
            throw new NullReferenceException("_connectService is null");
        return await _connectRoute.ConnectAsync(request, context);
    }
        
    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        if (_authorizeRoute == null)
            throw new NullReferenceException("_authorizeService is null");
        
        return await _authorizeRoute.AuthorizeAsync(input, context);
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> SendHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        if (_sendMessageRoute == null)
            throw new NullReferenceException("_sendMessageRoute is null");
        
        return await _sendMessageRoute.SendAsync(request, context);
    }
}