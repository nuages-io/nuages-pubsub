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
using Nuages.Web;

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
            .AddEnvironmentVariables();
     
        IConfiguration configuration = builder.Build();
        
        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
        if (config.ParameterStore.Enabled)
        {
            builder.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
            });
        }

        if (config.AppConfig.Enabled)
        {
            builder.AddAppConfig(config.AppConfig.ApplicationId,  
                config.AppConfig.EnvironmentId, 
                config.AppConfig.ConfigProfileId,true);
        }
        
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);
            
        var pubSubBuilder = 
            serviceCollection
                .AddPubSubLambdaRoutes(configuration)
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
                    config.ConnectionString = configuration["Nuages:Data:Mongo:ConnectionString"];
                });
                break;
            }
            case "SqlServer":
            {
                pubSubBuilder.AddPubSubSqlServerStorage(config =>
                {
                    config.UseSqlServer(configuration["Nuages:Data:SqlServer:ConnectionString"]);
                });

                break;
            }
            case "MySql":
            {
                
                pubSubBuilder.AddPubSubMySqlStorage(config =>
                {
                    var connectionString = configuration["Nuages:Data:MySql:ConnectionString"];
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