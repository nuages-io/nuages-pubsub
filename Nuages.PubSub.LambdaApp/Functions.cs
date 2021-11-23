#region

using System;
using System.IO;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Storage;

#endregion


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.LambdaApp;

// ReSharper disable once UnusedType.Global
public class Functions : PubSubFunction
{
    public Functions()
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",  false, true)
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

        IConfiguration configuration = builder
            .Build();
            
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(configuration);
            
        serviceCollection
            .AddPubSub()
            .AddPubSubMongoStorage(configuration);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        GetRequiredServices(serviceProvider);
    }
}