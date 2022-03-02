using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;


namespace Nuages.PubSub.WebSocket.Endpoints.Tests;

public class CustomPubSubFunction : PubSubFunction
{

    public CustomPubSubFunction(IPubSubService pubSubService)
    {
        var configManager = new ConfigurationManager();

        var builder = configManager
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.local.json", false, true);


        IConfiguration configuration = builder
            .Build();
            
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton(configuration);

        var pubSubBuilder = new PubSubBuilder(serviceCollection, configuration);
        pubSubBuilder
            .AddPubSubLambdaRoutes(configuration);

        serviceCollection.AddSingleton(pubSubService);

        serviceCollection.Configure<PubSubOptions>(configuration.GetSection("Nuages:PubSub"));
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);

        PubSubOpt = serviceProvider.GetRequiredService<IOptions<PubSubOptions>>();
    }

    public IOptions<PubSubOptions> PubSubOpt { get; set; }
}