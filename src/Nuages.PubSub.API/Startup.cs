using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.EntityFramework.MySql;
using Nuages.PubSub.Storage.EntityFramework.SqlServer;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.API;

[ExcludeFromCodeCoverage]
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static IConfiguration _configuration = null!;

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(_configuration);

        var pubSubBuilder = services
            .AddPubSubService(_configuration);

        var storage = _configuration["Nuages:PubSub:Data:Storage"];
        
        switch (storage)
        {
            case "DynamoDB":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDB":
            {
                pubSubBuilder.AddPubSubMongoStorage(config =>
                {
                    config.ConnectionString = _configuration["Nuages:PubSub:Data:ConnectionString"];
                });
                break;
            }
            case "SqlServer":
            {
                pubSubBuilder.AddPubSubSqlServerStorage(config =>
                {
                    config.UseSqlServer(_configuration["Nuages:PubSub:Data:ConnectionString"]);
                });

                break;
            }
            case "MySQL":
            {
                pubSubBuilder.AddPubSubMySqlStorage(config =>
                {
                    var connectionString = _configuration["Nuages:PubSub:Data:ConnectionString"];
                    config.UseMySQL(connectionString);
                });

                break;
            }
            default:
            {
                throw new NotSupportedException("Storage not supported");
            }
        }

        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
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
        
        services.AddHealthChecks();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            var stackName = _configuration.GetSection("Nuages:PubSub:StackName").Value;

            AWSXRayRecorder.InitializeInstance(_configuration);
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
                async context =>
                {
                    await context.Response.WriteAsync("PubSub");
                });
            endpoints.MapHealthChecks("health");
        });

        app.UseOpenApi();
        app.UseSwaggerUi3();
    }
}