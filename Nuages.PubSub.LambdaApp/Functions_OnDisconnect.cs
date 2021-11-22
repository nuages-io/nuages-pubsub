#region

using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.DataModel;

#endregion

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> OnDisconnectHandler(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            try
            {
                var connectionId = request.RequestContext.ConnectionId;
                context.Logger.LogLine($"ConnectionId: {connectionId}");

                var repository = _serviceProvider.GetRequiredService<IWebSocketRepository>();

                await repository.DeleteOneAsync(c => c.ConnectionId == connectionId);

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = "Disconnected."
                };
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
}