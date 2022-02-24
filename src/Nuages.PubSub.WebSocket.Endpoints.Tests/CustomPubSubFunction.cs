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

        serviceCollection
            .AddPubSubLambdaRoutes(configuration);

        serviceCollection.AddSingleton(pubSubService);

        serviceCollection.Configure<PubSubOptions>(configuration.GetSection("Nuages:PubSub"));
        serviceCollection.Configure<PubSubExternalAuthOption>(configuration.GetSection("Nuages:ExternalAuth"));
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        LoadRoutes(serviceProvider);

        PubSubOpt = serviceProvider.GetRequiredService<IOptions<PubSubOptions>>();
        PubSubExternalAuthOption = serviceProvider.GetRequiredService<IOptions<PubSubExternalAuthOption>>();
    }

    public IOptions<PubSubOptions> PubSubOpt { get; set; }
    public IOptions<PubSubExternalAuthOption> PubSubExternalAuthOption { get; set; }
}