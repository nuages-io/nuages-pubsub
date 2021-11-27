using Nuages.PubSub.Services;
using Nuages.PubSub.Storage.Mongo;

namespace Nuages.PubSub.API;

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
            
        services.Configure<WebSocketOptions>(_configuration.GetSection("Nuages:WebSocket"));
        
        services
            .AddPubSubService(_configuration)
            .AddPubSubMongoStorage();
        
        services.AddControllers();

        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Nuages WebSocket Service";
                //document.Info.Description = "A simple ASP.NET Core web API";
                //document.Info.TermsOfService = "None";
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
        
        //services.AddMvc();
        
         // services.AddSwaggerGen(c =>
        // {
        //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nuages WebSocket Service API", Version = "v1" });
        // });
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
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            //endpoints.MapSwagger();
            
        });
        
        app.UseOpenApi();
        app.UseSwaggerUi3();

        //app.UseMvc();
        
        // app.UseSwagger();
        //
        // app.UseSwaggerUI(c =>
        // {
        //     c.SwaggerEndpoint("v1/swagger.json", "My API V1");
        // });
    }
}