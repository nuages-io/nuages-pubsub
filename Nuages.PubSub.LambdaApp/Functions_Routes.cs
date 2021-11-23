#region

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
// ReSharper disable UnusedMember.Global

#endregion

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        public async Task<APIGatewayProxyResponse> EchoHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return await _echoService.Echo(request, context);
        }

        public  async Task<APIGatewayProxyResponse> OnDisconnectHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return await _disconnectService.Disconnect(request, context);
        }

        public async Task<APIGatewayProxyResponse> OnConnectHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return await _connectService.Connect(request, context);
        }
        
        public async Task<APIGatewayCustomAuthorizerResponse> OnAuthorizeHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
        {
            return await _authorizeService.Authorize(input, context);
        }

        public async Task<APIGatewayProxyResponse> BroadcastMessageHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return await _broadcastMessageService.Broadcast(request, context);
        }
    }
}