// using Amazon.CDK;
//
// namespace Nuages.PubSub.WebSocket.Cdk;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once ArrangeTypeModifiers
// sealed class Program
// {
//     public static void Main(string[] args)
//     {
//         var app = new App();
//             
//         var stack = new NuagesPubSubWebSocketCdkStack(app, "NuagesPubSub", new StackProps
//         {
//             Env = new Environment
//             {
//                 Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
//                 Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
//             }
//         });
//             
//         stack.CreateTemplate();
//             
//         
//         app.Synth();
//     }
// }