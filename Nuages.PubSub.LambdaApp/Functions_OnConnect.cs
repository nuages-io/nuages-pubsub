#region

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Nuages.PubSub.LambdaApp.DataModel;

#endregion

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> OnConnectHandler(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            var sub = request.RequestContext.Authorizer.SingleOrDefault(c => c.Key == "sub").Value.ToString();

            try
            {
                var connectionId = request.RequestContext.ConnectionId;
                context.Logger.LogLine($"ConnectionId: {connectionId} User: {sub}");

                context.Logger.LogLine(JsonSerializer.Serialize(request.RequestContext));

                var repository = _serviceProvider.GetRequiredService<IWebSocketRepository>();
             
                await repository.InsertOneAsync(new WebSocketConnection
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    ConnectionId = connectionId,
                    Sub = sub!
                });

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Connected."
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("Error connecting: " + e.Message);
                context.Logger.LogLine(e.StackTrace);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = $"Failed to connect: {e.Message}"
                };
            }
        }
    }
}