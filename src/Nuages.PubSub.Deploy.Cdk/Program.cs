using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Microsoft.Extensions.Configuration;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Cdk;
using Nuages.Web;

namespace Nuages.PubSub.Deploy.Cdk;

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
        
        var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>()!;
        
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

        var secretProvider = new AWSSecretProvider();
        secretProvider.TransformSecrets(configManager);
        
        var options = configuration.GetSection("ConfigOptions").Get<ConfigOptions>()!;
        var runtimeOptions = configuration.GetSection("RuntimeOptions").Get<RuntimeOptions>()!;
        
        var app = new App();

        PubSubStack.CreateStack(app, options, runtimeOptions);
        
        app.Synth();
    }
}
