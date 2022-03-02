
using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Nuages.Web;

namespace Nuages.PubSub.Cdk.Deploy;

//ReSharper disable once ClassNeverInstantiated.Global
//ReSharper disable once ArrangeTypeModifiers
[ExcludeFromCodeCoverage]
sealed class Program
{
    // ReSharper disable once UnusedParameter.Global
    public static void Main(string[] args)
    {
        var configManager = new ConfigurationManager();

        var configuration = configManager
            .AddJsonFile("appsettings.json",  false, true)
            .AddEnvironmentVariables()
            .Build();
        
        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
        
        if (config.ParameterStore.Enabled)
        {
            configManager.AddSystemsManager(configureSource =>
            {
                configureSource.Path = config.ParameterStore.Path;
                configureSource.Optional = true;
            });
        }

        if (config.AppConfig.Enabled)
        {
            configManager.AddAppConfig(config.AppConfig.ApplicationId,  
                config.AppConfig.EnvironmentId, 
                config.AppConfig.ConfigProfileId,true);
        }

        var options = configuration.Get<ConfigOptions>();
        
        var app = new App();

        if (args.Contains("--pipeline"))
        {
            PubSubStackWithPipeline.Create(app, options);
        }
        else
        {
            PubSubStack.CreateStack(app, options);
        }
        
        app.Synth();
    }
}
