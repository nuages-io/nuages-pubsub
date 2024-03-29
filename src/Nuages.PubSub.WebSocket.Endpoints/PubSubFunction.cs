using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Authorize;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Connect;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Disconnect;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Echo;
using Nuages.PubSub.WebSocket.Endpoints.Routes.JoinLeave;
using Nuages.PubSub.WebSocket.Endpoints.Routes.Send;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedParameter.Global

namespace Nuages.PubSub.WebSocket.Endpoints;

public class PubSubFunction
{
    private IEchoRoute? _echoRoute;
    private IDisconnectRoute? _disconnectRoute;
    private IConnectRoute? _connectRoute;
    private IAuthorizeRoute? _authorizeRoute;
    private ISendRoute? _sendMessageRoute;
    private IJoinRoute? _joinRoute;
    private ILeaveRoute? _leaveRoute;

    protected void LoadRoutes(ServiceProvider serviceProvider)
    {
        _echoRoute = serviceProvider.GetRequiredService<IEchoRoute>();
        _disconnectRoute = serviceProvider.GetRequiredService<IDisconnectRoute>();
        _connectRoute = serviceProvider.GetRequiredService<IConnectRoute>();
        _authorizeRoute = serviceProvider.GetRequiredService<IAuthorizeRoute>();
        _sendMessageRoute = serviceProvider.GetRequiredService<ISendRoute>();
        _joinRoute = serviceProvider.GetRequiredService<IJoinRoute>();
        _leaveRoute = serviceProvider.GetRequiredService<ILeaveRoute>();
    }
    
    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> EchoHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        return await _echoRoute!.EchoAsync(request, context);
    }

    // ReSharper disable once UnusedMember.Global
    public  async Task<APIGatewayProxyResponse> OnDisconnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        await OnBeforeDisconnectAsync(request, context);
        
        var res = await _disconnectRoute!.DisconnectAsync(request, context);

        return await OnAfterDisconnectAsync(request, context, res);
    }
    
    protected virtual async Task<APIGatewayProxyResponse> OnAfterDisconnectAsync(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeDisconnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> OnConnectHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        await OnBeforeConnectAsync(request, context);
        
        var res = await _connectRoute!.ConnectAsync(request, context);

        return await OnAfterConnectAsync(request, context, res);
    }

    protected virtual async Task<APIGatewayProxyResponse> OnAfterConnectAsync(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeConnectAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        await OnBeforeAuthorize(input, context);
        
        var res = await _authorizeRoute!.AuthorizeAsync(input, context);
        
        return await OnAfterAuthorize(input, context, res);
    }

    protected virtual async Task<APIGatewayCustomAuthorizerResponse> OnAfterAuthorize(APIGatewayCustomAuthorizerRequest input, ILambdaContext context, APIGatewayCustomAuthorizerResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeAuthorize(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> SendHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        await OnBeforeSend(request, context);
        
        var res = await _sendMessageRoute!.SendAsync(request, context);
        
        return await OnAfterSend(request, context, res);
    }

    protected virtual async Task<APIGatewayProxyResponse> OnAfterSend(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeSend(APIGatewayProxyRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> JoinHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        await OnBeforeJoin(request, context);
        
        var res = await _joinRoute!.JoinAsync(request, context);
        
        return await OnAfterJoin(request, context, res);
    }

    protected virtual async Task<APIGatewayProxyResponse> OnAfterJoin(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeJoin(APIGatewayProxyRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task<APIGatewayProxyResponse> LeaveHandlerAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        await OnBeforeLeave(request, context);
        
        var res = await _leaveRoute!.LeaveAsync(request, context);
        
        return await OnAfterLeave(request, context, res);
    }

    protected virtual async Task<APIGatewayProxyResponse> OnAfterLeave(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        return await Task.FromResult(res);
    }

    protected virtual async Task OnBeforeLeave(APIGatewayProxyRequest request, ILambdaContext context)
    {
        await Task.CompletedTask;
    }
}