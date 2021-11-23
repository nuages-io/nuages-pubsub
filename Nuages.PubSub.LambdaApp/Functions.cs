#region

using System;
using System.IO;
using Amazon.ApiGatewayManagementApi;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.MongoDB;
using Nuages.PubSub.Services.Authorize;
using Nuages.PubSub.Services.Broadcast;
using Nuages.PubSub.Services.Connect;
using Nuages.PubSub.Services.Disconnect;
using Nuages.PubSub.Services.Echo;

#endregion


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions : PubSubFunction
    {
        private IConfiguration _configuration;
       
        public Functions()
        {
            BuildConfiguration();
            BuildServices();
        }

        private void BuildServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(_configuration);
            serviceCollection.AddNuagesMongoDb(_configuration);
            serviceCollection.AddPubSub();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            GetServices(serviceProvider);
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
    }
}