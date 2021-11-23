#region

using System;
using System.IO;
using Amazon.ApiGatewayManagementApi;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.MongoDB;

#endregion


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        private IEchoService _echoService;
        private IDisconnectService _disconnectService;
        private IConnectService _connectService;
        private IAuthorizeService _authorizeService;
        private IBroadcastMessageService _broadcastMessageService;

        public Functions()
        {
            BuildConfiguration();

            BuildServices();

            ApiGatewayManagementApiClientFactory =
                endpoint =>
                    new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                    {
                        ServiceURL = endpoint
                    });

            ClientFactory = new MongoClientFactory();
        }

        private void BuildServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_configuration);
            serviceCollection.AddNuagesMongoDb(_configuration);
            
            serviceCollection.AddPubSub();

            _serviceProvider = serviceCollection.BuildServiceProvider();

            _echoService = _serviceProvider.GetRequiredService<IEchoService>();
            _disconnectService = _serviceProvider.GetRequiredService<IDisconnectService>();
            _connectService = _serviceProvider.GetRequiredService<IConnectService>();
            _authorizeService = _serviceProvider.GetRequiredService<IAuthorizeService>();
            _broadcastMessageService = _serviceProvider.GetRequiredService<IBroadcastMessageService>();
        }

        private void BuildConfiguration()
        {
            var configManager = new ConfigurationManager();

            var builder = configManager
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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

            _configuration = builder
                .Build();
        }

        private Func<string, AmazonApiGatewayManagementApiClient> ApiGatewayManagementApiClientFactory { get; }

        private MongoClientFactory ClientFactory { get; }
    }
}