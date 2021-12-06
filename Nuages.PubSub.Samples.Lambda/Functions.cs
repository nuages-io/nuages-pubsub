#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo;
using Nuages.PubSub.WebSocket;

#endregion

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.Samples.Lambda;

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class Functions : PubSubFunction
{
    public Functions() 
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  true, true)
            .AddEnvironmentVariables();

        var configurationManagerPath = configManager.GetValue<string>("Nuages:ConfigurationManagerPath");

        if (!string.IsNullOrEmpty(configurationManagerPath))
        {
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = configurationManagerPath;
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                configureSource.Optional = true;
            });
        }

        IConfiguration configuration = builder.Build();
            
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration)
            .AddPubSubLambdaRoutes(configuration)
            .AddPubSubService()
            .AddPubSubMongoStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
    }
}