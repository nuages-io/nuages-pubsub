﻿
using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Cdk.Deploy;

//ReSharper disable once ClassNeverInstantiated.Global
//ReSharper disable once ArrangeTypeModifiers
[ExcludeFromCodeCoverage]
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.deploy.json",  true, true)
            .AddEnvironmentVariables();
        
        IConfiguration configuration = builder.Build();
        
        var options = configuration.Get<ConfigOptions>();
        
        var app = new App();

        var name = options.StackName;
        Console.WriteLine($"StakName = {name}");
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
        
        stack.Node.SetContext(MyNuagesPubSubStack.ContextStorage, options.Env.Data.Storage ?? "");
        stack.Node.SetContext(MyNuagesPubSubStack.ContextConnectionString, options.Env.Data.ConnectionString ?? "");
        stack.Node.SetContext(MyNuagesPubSubStack.ContextDatabaseName, options.Env.Data.DatabaseName ?? "");
        stack.Node.SetContext(MyNuagesPubSubStack.ContextAudience, options.Env.PubSub.Audience ?? "");
        stack.Node.SetContext(MyNuagesPubSubStack.ContextIssuer, options.Env.PubSub.Issuer ?? "");
        stack.Node.SetContext(MyNuagesPubSubStack.ContextSecret, options.Env.PubSub.Secret ?? "");
        
        stack.CreateTemplate();

        app.Synth();
    }
}