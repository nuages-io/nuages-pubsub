using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Constructs;
// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.WebSocket.Cdk
{
    public class NuagesPubSubWebSocketCdkStack : Stack
    {
        internal NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            
            new CfnParameter(this, "DomainName", new CfnParameterProps
            {
                Type = "String",
                Description = "Public DomainName"
            });

        }
    }
}
