﻿
using Amazon.CDK;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Samples.WebSocket.Deploy;

//ReSharper disable once ClassNeverInstantiated.Global
//ReSharper disable once ArrangeTypeModifiers
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  false, true)
            //.AddJsonFile("appsettings.prod.test.json",  false, true)
            .AddEnvironmentVariables();
        
        IConfiguration configuration = builder.Build();
        
        var options = configuration.Get<ConfigOptions>();
        
        var app = new App();

        var name = options.StackName;
        
        var stack = new MyNuagesPubSubStack(configuration, app, name, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        }); 

        stack.Node.SetContext(MyNuagesPubSubStack.ContextDomainName,  options.WebSocket.Domain);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCertificateArn,options.WebSocket.CertificateArn);
        
        stack.Node.SetContext(MyNuagesPubSubStack.ContextDomainNameApi,  options.Api.Domain);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCertificateArnApi, options.Api.CertificateArn);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextApiKeyApi, options.Api.ApiKey);
        
        stack.Node.SetContext(MyNuagesPubSubStack.ContextCreateDynamoDbStorage, options.CreateDynamoDbStorage);
        stack.Node.SetContext(MyNuagesPubSubStack.ContextTableNamePrefix, options.TableNamePrefix);
        
        stack.CreateTemplate();

        app.Synth();
    }
}