#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage;
using Nuages.PubSub.Storage.Mongo;

#endregion


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.WebSocket.Application;

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
            .AddPubSubLambdaRoutes(configuration)
            .AddPubSubService()
            .AddPubSubMongoStorage();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        GetRequiredServices(serviceProvider);

        var pubSubStorage = serviceProvider.GetRequiredService<IPubSubStorage>();
        pubSubStorage.InitializeAsync();
    }

    protected override async Task<APIGatewayProxyResponse> OnAfterConnectAsync(APIGatewayProxyRequest request, ILambdaContext context, APIGatewayProxyResponse res)
    {
        if (res.StatusCode == 200)
        {
            context.Logger.LogLine("200 connected!");
        }
        
        return await Task.FromResult(res);
    }
}