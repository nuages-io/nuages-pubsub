using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace Nuages.PubSub.WebSocket.Cdk
{
    public class NuagesPubSubWebSocketCdkStack : Stack
    {
        internal NuagesPubSubWebSocketCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var bucket = new Bucket(this, "MyFirstBucket", new BucketProps
            {
                Versioned = true
            });
        }
    }
}
