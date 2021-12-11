using System.Text.Json;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.DynamoDb;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.Samples.API;

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
            
        var storage = _configuration.GetSection("Nuages:PubSub:Storage").Value;
        Console.WriteLine($"Storage = {storage}");
        
        switch (storage)
        {
            case "DynamoDb":
            {
                pubSubBuilder.AddPubSubDynamoDbStorage();
                break;
            }
            case "MongoDb":
            {
                pubSubBuilder.AddPubSubMongoStorage();
                break;
            }
            default:
            {
                throw new NotSupportedException("Storage not supported");
            }
        }
        
        services.AddControllers();

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Nuages WebSocket Service";

                document.Info.Contact = new NSwag.OpenApiContact
                {
                    Name = "Nuages.io",
                    Email = string.Empty,
                    Url = "https://github.com/nuages-io/nuages-pubsub"
                };
                document.Info.License = new NSwag.OpenApiLicense
                {
                    Name = "Use under LICENCE",
                    Url = "http://www.apache.org/licenses/LICENSE-2.0"
                };
            };
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
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
                    var option = app.ApplicationServices.GetService <IOptions<PubSubOptions>>();
                    
                    await context.Response.WriteAsync(JsonSerializer.Serialize(option, new JsonSerializerOptions
                    { 
                        WriteIndented = true
                    }));
                });
        });
        
        app.UseOpenApi();
        app.UseSwaggerUi3();
    }
}