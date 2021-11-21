#region

using System;
using System.Collections.Generic;
using Amazon.ApiGatewayManagementApi;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nuages.Lambda;
using Nuages.MongoDB;

#endregion


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Nuages.PubSub.LambdaApp
{
    // ReSharper disable once UnusedType.Global
    public partial class Functions
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Functions()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            
            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
            _configuration = configService.GetConfiguration();
            
            serviceCollection.AddNuagesMongoDb(_configuration);
            
            ApiGatewayManagementApiClientFactory =
                endpoint =>
                    new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                    {
                        ServiceURL = endpoint
                    });

            ClientFactory = new MongoClientFactory();
        }

        private void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEnvironmentService, EnvironmentService>();
            serviceCollection.AddTransient<IConfigurationService, ConfigurationService>();
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