
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

        var useCustomDomain = Convert.ToBoolean(configuration.GetSection("Nuages:PubSub:UseCustomDomainName").Value);
        if (useCustomDomain)
        {
            stack.Node.SetContext("Nuages/PubSub/UseCustomDomainName", useCustomDomain);
            stack.Node.SetContext("Nuages/PubSub/DomainName",  configuration.GetSection("Nuages:PubSub:DomainName").Value);
            stack.Node.SetContext("Nuages/PubSub/DomainNameApi",  configuration.GetSection("Nuages:PubSub:DomainNameApi").Value);
            stack.Node.SetContext("Nuages/PubSub/CertificateArn", configuration.GetSection("Nuages:PubSub:CertificateArn").Value);
        }
        
        stack.Node.SetContext("Nuages/PubSub/Storage", configuration.GetSection("Nuages:PubSub:Storage").Value);
        stack.Node.SetContext("Nuages/PubSub/CreateDynamoDbStorage", configuration.GetSection("Nuages:PubSub:CreateDynamoDbStorage").Value);
        stack.Node.SetContext("Nuages/PubSub/TableNamePrefix", configuration.GetSection("Nuages:PubSub:TableNamePrefix").Value);
        
        stack.CreateTemplate();

        app.Synth();
    }
}