using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.Mongo;
using Nuages.PubSub.WebSocket;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.Samples.WebSocket;

// ReSharper disable once ClassNeverInstantiated.Global
public class PubSubFunction : Nuages.PubSub.WebSocket.PubSubFunction
{
    public PubSubFunction() 
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json",  false, true)
            .AddJsonFile("appsettings.prod.json",  true, true)
            .AddEnvironmentVariables();
     
        IConfiguration configuration = builder.Build();
            
        AWSSDKHandler.RegisterXRayForAllServices();
        AWSXRayRecorder.InitializeInstance(configuration);
        
        var serviceCollection = new ServiceCollection();

        serviceCollection
            .AddSingleton(configuration);
            
        var pubSubBuilder = 
            serviceCollection.AddPubSubLambdaRoutes(configuration)
            .AddPubSubService();

        var storage = configuration.GetSection("Nuages:PubSub:Storage").Value;
        switch (storage)
        {
            case "DynamoDb":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDb":
            {
                pubSubBuilder.AddPubSubMongoStorage();
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