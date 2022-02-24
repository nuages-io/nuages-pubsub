using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.EntityFramework.SqlServer;
using Nuages.PubSub.Storage.Mongo;
using Nuages.PubSub.WebSocket.Endpoints;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.WebSocket.API;

// ReSharper disable once ClassNeverInstantiated.Global
[ExcludeFromCodeCoverage]
public class PubSubFunction : Nuages.PubSub.WebSocket.Endpoints.PubSubFunction
{
    public PubSubFunction() 
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  true, true)
            .AddEnvironmentVariables();
     
        var name = Environment.GetEnvironmentVariable("Nuages__PubSub__StackName");

        if (name != null)
        {
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = $"/{name}";
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                configureSource.Optional = true;
            });
            
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = $"/{name}/WebSocket";
                configureSource.ReloadAfter = TimeSpan.FromMinutes(15);
                configureSource.Optional = true;
            });
        }
        
        IConfiguration configuration = builder.Build();
            
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);
            
        var pubSubBuilder = 
            serviceCollection.AddPubSubLambdaRoutes(configuration)
            .AddPubSubService();

        var storage = configuration.GetSection("Nuages:Data:Storage").Value;
        switch (storage)
        {
            case "DynamoDb":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDb":
            {
                pubSubBuilder.AddPubSubMongoStorage(config =>
                {
                    config.ConnectionString = configuration["Nuages:Mongo:ConnectionString"];
                    config.DatabaseName = configuration["Nuages:Mongo:DatabaseName"];
                });
                break;
            }
            case "SqlServer":
            {
                pubSubBuilder.AddPubSubSqlServerStorage(config =>
                {
                    config.UseSqlServer(configuration["Nuages:SqlServer:ConnectionString"]);
                });

                break;
            }
            case "MySql":
            {
                
                pubSubBuilder.AddPubSubMySqlStorage(config =>
                {
                    var connectionString = configuration["Nuages:MySql:ConnectionString"];
                    Console.WriteLine("ConnectionString=" + connectionString);
                    // var serverVersion = ServerVersion.AutoDetect(connectionString);
                    // Console.WriteLine("serverVersion=" + serverVersion);
                    // config.UseMySql(connectionString, serverVersion);
                    config.UseMySQL(connectionString);
                });

                break;
            }
            default:
            {
                throw new NotSupportedException("Storage not supported");
            }
        }

        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);
    }
}