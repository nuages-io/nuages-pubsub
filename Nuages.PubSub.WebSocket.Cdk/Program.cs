using Amazon.CDK;

namespace Nuages.PubSub.WebSocket.Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            
            var stack = new NuagesPubSubWebSocketCdkStack(app, "NuagesPubSubWebSocketCdkStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
            });
            
            stack.BuildTheThing();
            
            app.Synth();
        }
    }
}
