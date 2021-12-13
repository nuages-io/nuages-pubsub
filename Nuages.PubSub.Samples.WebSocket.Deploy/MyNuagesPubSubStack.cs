using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Samples.WebSocket.Deploy;

public class MyNuagesPubSubStack : Cdk.NuagesPubSubWebSocketCdkStack<PubSubFunction>
{
    // ReSharper disable once UnusedParameter.Local
    public MyNuagesPubSubStack(IConfiguration configuration, Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        Asset = "./Nuages.PubSub.Samples.WebSocket/bin/Release/net6.0/linux-x64/publish";
        WebApiAsset = "./Nuages.PubSub.Samples.API/bin/Release/net6.0/linux-x64/publish";
    }
}