
using Amazon.CDK;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Samples.WebSocket.Deploy;

//ReSharper disable once ClassNeverInstantiated.Global
//ReSharper disable once ArrangeTypeModifiers
sealed class Program
{
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  true, true)
            .AddEnvironmentVariables();
        
        IConfiguration configuration = builder.Build();
        
        var app = new App();

        var name = configuration.GetSection("Nuages:PubSub:StackName").Value;
        
        var stack = new MyNuagesPubSubStack(configuration, app, name, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        }); 
      
        //Initialize context required by custom domain. You may also set the values in the cdk.json file (see CDK documentation for more info)

       
        stack.Node.SetContext(MyNuagesPubSubStack.ContextDomainName,  configuration.GetSection("Nuages:PubSub:WebSocket:Domain").Value);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCertificateArn, configuration.GetSection("Nuages:PubSub:WebSocket:CertificateArn").Value);
        
        stack.Node.SetContext(MyNuagesPubSubStack.ContextDomainNameApi,  configuration.GetSection("Nuages:PubSub:API:Domain").Value);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCertificateArnApi, configuration.GetSection("Nuages:PubSub:API:CertificateArn").Value);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextApiKeyApi, configuration.GetSection("Nuages:PubSub:API:ApiKey").Value);
        
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCreateDynamoDbStorage, configuration.GetSection("Nuages:PubSub:CreateDynamoDbStorage").Value);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextTableNamePrefix, configuration.GetSection("Nuages:PubSub:TableNamePrefix").Value);
        
        stack.CreateTemplate();

        app.Synth();
    }
}