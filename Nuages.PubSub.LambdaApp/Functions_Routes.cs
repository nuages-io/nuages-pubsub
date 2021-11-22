#region

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

#endregion

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        // ReSharper disable once UnusedMember.Global
        public async Task<APIGatewayProxyResponse> EchoHandler(APIGatewayProxyRequest request,
            ILambdaContext context)
        {
            return await _pubSubService.Echo(request, context);
        }
    }
}