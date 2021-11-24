﻿using System.Text;
using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Nuages.PubSub.Service;

namespace Nuages.PubSub.Lambda.Routes.Echo;

// ReSharper disable once UnusedType.Global
public class EchoRoute : PubSubRouteBase, IEchoRoute
{
    private readonly IPubSubService _pubSubService;

    public EchoRoute(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    public async Task<APIGatewayProxyResponse> EchoAsync(APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            var endpoint = $"https://{request.RequestContext.DomainName}/{request.RequestContext.Stage}";
            context.Logger.LogLine($"API Gateway management endpoint: {endpoint}");

            var connectionId = request.RequestContext.ConnectionId;
            context.Logger.LogLine($"ECHO ConnectionId: {connectionId}");

            var result = new
            {
                target = "echo",
                payload = new
                {
                    ConnectionId = connectionId
                }
            };

            return await _pubSubService.SendToOneAsync(endpoint, connectionId, JsonSerializer.Serialize(result) );
            
        }
        catch (Exception e)
        {
            context.Logger.LogLine("Error disconnecting: " + e.Message);
            context.Logger.LogLine(e.StackTrace);
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Failed to disconnect: {e.Message}"
            };
        }
    }
}