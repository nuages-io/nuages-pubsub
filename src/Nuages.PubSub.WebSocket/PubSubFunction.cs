using System.Diagnostics.CodeAnalysis;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.EntityFramework.SqlServer;
using Nuages.PubSub.Storage.Mongo;
using Nuages.PubSub.WebSocket.Endpoints;
using Nuages.Web;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.WebSocket;

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
        
        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>()!;
        
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
        
        
        // var secretProvider = new AWSSecretProvider();
        // secretProvider.TransformSecret(configManager, "Nuages:PubSub:Data:ConnectionString");
        // secretProvider.TransformSecret(configManager, "Nuages:PubSub:Auth:Secret");
        //
        
        configManager.TransformSecrets();
        
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);

        //serviceCollection.AddAWSService<IAmazonSecretsManager>();
        serviceCollection.AddSecretsProvider();
        
        var pubSubBuilder = serviceCollection.AddPubSubService(configuration);
        
        var pubSubRouteBuilder =
            pubSubBuilder
                .AddPubSubLambdaRoutes();
            
        var enableExternalAUth = configuration.GetValue<bool>("Nuages:PubSub:ExternalAuth:Enabled");
        if (enableExternalAUth)
            pubSubRouteBuilder.UseExternalAuthRoute();

        var storage = configuration.GetSection("Nuages:PubSub:Data:Storage").Value;
        switch (storage)
        {
            case "DynamoDB":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDB":
            {
                pubSubBuilder.AddPubSubMongoStorage(dbConfig =>
                {
                    dbConfig.ConnectionString = configuration["Nuages:PubSub:Data:ConnectionString"]!;
                }, ServiceLifetime.Singleton);
                break;
            }
            case "SqlServer":
            {
                pubSubBuilder.AddPubSubSqlServerStorage(dbConfig =>
                {
                    dbConfig.UseSqlServer(configuration["Nuages:PubSub:Data:ConnectionString"]);
                });

                break;
            }
            case "MySQL":
            {
                
                pubSubBuilder.AddPubSubMySqlStorage(dbConfig =>
                {
                    var connectionString = configuration["Nuages:PubSub:Data:ConnectionString"]!;
                    dbConfig.UseMySQL(connectionString);
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