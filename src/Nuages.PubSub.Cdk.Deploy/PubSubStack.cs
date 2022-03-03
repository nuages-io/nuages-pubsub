using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class PubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    public static void CreateStack(Construct scope, ConfigOptions options)
    {
        var stack = new PubSubStack(scope, options.StackName, new StackProps
        {
            StackName = options.StackName,
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        })
        {
            WebSocketDomainName = options.WebSocket.Domain,
            WebSocketCertificateArn = options.WebSocket.CertificateArn,
            ApiDomainName = options.Api.Domain,
            ApiCertificateArn = options.Api.CertificateArn,
            ApiApiKey = options.Api.ApiKey,
            VpcId = options.VpcId,
            SecurityGroupId = options.SecurityGroupId,
            DatabaseProxyArn = options.DatabaseDbProxy.Arn,
            DatabaseProxyEndpoint = options.DatabaseDbProxy.Endpoint,
            DatabaseProxyName = options.DatabaseDbProxy.Name,
            DatabaseProxyUser = options.DatabaseDbProxy.UserName
        };

        //stack.InitializeFromOptions(options);
        
        stack.BuildStack();
    }
    
    // ReSharper disable once UnusedParameter.Local
    private PubSubStack(Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
    }
}