// using NLog.Web;
// using Nuages.AWS.Secrets;
// using Nuages.PubSub.Services;
// using Nuages.Web;
//
// namespace Nuages.PubSub.API;
// // ReSharper disable once UnusedType.Global
// public class LambdaEntryPoint :
//     Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
// {
//     protected override void Init(IWebHostBuilder builder)
//     {
//         builder.UseStartup<Startup>();
//     }
//
//     protected override void Init(IHostBuilder builder)
//     {
//         // ReSharper disable once UnusedParameter.Local
//         builder.ConfigureAppConfiguration((context, configBuilder) =>
//         {
//             configBuilder.AddJsonFile("appsettings.json", false, true);
//             configBuilder.AddEnvironmentVariables();
//
//             var configuration = configBuilder.Build();
//            
//             var config = configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>();
//         
//             if (config.ParameterStore.Enabled)
//             {
//                 configBuilder.AddSystemsManager(configureSource =>
//                 {
//                     configureSource.Path = config.ParameterStore.Path;
//                     configureSource.Optional = true;
//                 });
//             }
//
//             if (config.AppConfig.Enabled)
//             {
//                 configBuilder.AddAppConfig(config.AppConfig.ApplicationId,  
//                     config.AppConfig.EnvironmentId, 
//                     config.AppConfig.ConfigProfileId,true);
//             }
//             
//             configuration = configBuilder.Build();
//             
//             var secretProvider = new AWSSecretProvider();
//             secretProvider.TransformSecret<SecretValue>(configBuilder, configuration, "Nuages:PubSub:Data:ConnectionString");
//             secretProvider.TransformSecret<SecretValue>(configBuilder, configuration, "Nuages:PubSub:Auth:Secret");
//         }).UseNLog();
//
//     }
// }