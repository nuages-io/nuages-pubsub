using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Constructs;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.WebSocket.API;

namespace Nuages.PubSub.Cdk.Deploy;

[ExcludeFromCodeCoverage]
public class MyNuagesPubSubStack : NuagesPubSubWebSocketCdkStack<PubSubFunction>
{
  

    // ReSharper disable once UnusedParameter.Local
    public MyNuagesPubSubStack(IConfiguration configuration, Construct scope, string id, IStackProps? props = null) 
        : base(scope, id, props)
    {
        WebSocketAsset = "./src/Nuages.PubSub.WebSocket.API/bin/Release/net6.0/linux-x64/publish";
        ApiAsset = "./src/Nuages.PubSub.API/bin/Release/net6.0/linux-x64/publish";

    }

}