#region

using System;
using System.Collections.Generic;
using System.IO;
using Amazon.ApiGatewayManagementApi;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Lambda;
using Nuages.MongoDB;
using Nuages.PubSub.LambdaApp.DataModel;

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
            serviceCollection.AddScoped<IWebSocketRepository, WebSocketRepository>();
            serviceCollection.AddNuagesMongoDb(_configuration);

            _serviceProvider = serviceCollection.BuildServiceProvider();
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

        private static APIGatewayCustomAuthorizerResponse CreateResponse(bool success, string methodArn,
            Dictionary<string, string> claims = null)
        {
            string principal = null;
            claims?.TryGetValue("sub", out principal);

            var contextOutput = new APIGatewayCustomAuthorizerContextOutput();

            if (claims != null)
                foreach (var keyValuePair in claims.Keys)
                    contextOutput[keyValuePair] = claims[keyValuePair];

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = principal ?? "user",
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Statement =
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                        {
                            Action = new HashSet<string> {"execute-api:Invoke"},
                            Effect = success
                                ? "Allow"
                                : "Deny",
                            Resource = new HashSet<string> {methodArn}
                        }
                    }
                },
                Context = contextOutput
            };
        }
    }
}