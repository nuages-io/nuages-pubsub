using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Nuages.PubSub.Cdk;
using Nuages.PubSub.WebSocket;

namespace Nuages.PubSub.Deploy.Cdk;

[ExcludeFromCodeCoverage]
public class PubSubStack : PubSubWebSocketCdkStack<PubSubFunction>
{
    public static void CreateStack(Construct scope, ConfigOptions options, RuntimeOptions runtimeOptions)
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
            ConfigOptions = options,
            RuntimeOptions = runtimeOptions
        };
        
        stack.BuildStack();
    }

    // ReSharper disable once UnusedParameter.Local
    private PubSubStack(Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";
        WebApiHandler = "Nuages.PubSub.API";
    }
}