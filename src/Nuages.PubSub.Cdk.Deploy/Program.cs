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

        var configuration = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.deploy.json",  true, true)
            .AddEnvironmentVariables().Build();
        
        var options = configuration.Get<ConfigOptions>();
        
        var app = new App();
        
        var stack = new MyNuagesPubSubStack(configuration, app, options.StackName, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });

        stack.InitializeContextFromOptions(options);
        
        stack.CreateTemplate();

        app.Synth();
    }
}