using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.EntityFramework.SqlServer;
using Nuages.PubSub.Storage.Mongo;
using Nuages.Web;

var builder = WebApplication.CreateBuilder(args);

var configurationBuilder = builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();

var isLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));

var config = builder.Configuration.GetSection("ApplicationConfig").Get<ApplicationConfig>()!;

if (config.ParameterStore.Enabled)
{
    configurationBuilder.AddSystemsManager(configureSource =>
    {
        configureSource.Path = config.ParameterStore.Path;
        configureSource.Optional = true;
    });
}

if (config.AppConfig.Enabled)
{
    configurationBuilder.AddAppConfig(config.AppConfig.ApplicationId,
        config.AppConfig.EnvironmentId,
        config.AppConfig.ConfigProfileId, true);
}


var configuration = configurationBuilder.Build();

builder.Configuration.TransformSecrets();

builder.Services.AddSingleton(configuration);

// builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddSecretsProvider();

var pubSubBuilder = builder.Services
    .AddPubSubService(configuration);

var storage = configuration["Nuages:PubSub:Data:Storage"];

switch (storage)
{
    case "DynamoDB":
    {
        pubSubBuilder.AddPubSubDynamoDbStorage();
        break;
    }
    case "MongoDB":
    {
        pubSubBuilder.AddPubSubMongoStorage(configMongo =>
        {
            configMongo.ConnectionString = configuration["Nuages:PubSub:Data:ConnectionString"]!;
        }, isLambda ? ServiceLifetime.Singleton : ServiceLifetime.Scoped);
        break;
    }
    case "SqlServer":
    {
        pubSubBuilder.AddPubSubSqlServerStorage(configSqlServer =>
        {
            configSqlServer.UseSqlServer(configuration["Nuages:PubSub:Data:ConnectionString"]);
        }, isLambda ? ServiceLifetime.Singleton : ServiceLifetime.Scoped);

        break;
    }
    case "MySQL":
    {
        pubSubBuilder.AddPubSubMySqlStorage(configMySql =>
        {
            var connectionString = configuration["Nuages:PubSub:Data:ConnectionString"]!;
            configMySql.UseMySQL(connectionString);
        }, isLambda ? ServiceLifetime.Singleton : ServiceLifetime.Scoped);

        break;
    }
    default:
    {
        throw new NotSupportedException("Storage not supported");
    }
}

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSwaggerDocument(configSwagger =>
{
    configSwagger.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "Nuages PubSub";

        document.Info.Contact = new NSwag.OpenApiContact
        {
            Name = "Nuages.io",
            Email = "martin@nuages.io",
            Url = "https://github.com/nuages-io/nuages-pubsub"
        };
        document.Info.License = new NSwag.OpenApiLicense
        {
            Name = "Use under LICENCE",
            Url = "http://www.apache.org/licenses/LICENSE-2.0"
        };
    };
});

builder.Services.AddHealthChecks();

// Add services to the container.
//builder.Services.AddControllers();

// Add AWS Lambda support. When application is run in Lambda Kestrel is swapped out as the web server with Amazon.Lambda.AspNetCoreServer. This
// package will act as the webserver translating request and responses between the Lambda event source and ASP.NET Core.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    var stackName = configuration.GetSection("Nuages:PubSub:StackName").Value;

    AWSXRayRecorder.InitializeInstance(configuration);
    AWSSDKHandler.RegisterXRayForAllServices();

    app.UseXRay(stackName);
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGet("/",
        async context => { await context.Response.WriteAsync("PubSub"); });
    endpoints.MapHealthChecks("health");
});

app.UseOpenApi();
app.UseSwaggerUi3();

app.Run();