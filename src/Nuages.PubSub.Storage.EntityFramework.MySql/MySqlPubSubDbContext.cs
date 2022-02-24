using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Storage.EntityFramework.MySql;

public class MySqlPubSubDbContext : PubSubDbContext
{
    public MySqlPubSubDbContext(DbContextOptions context) : base(context)
    {
      
    }

}

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class MySqlPubSubContextFactory : IDesignTimeDbContextFactory<MySqlPubSubDbContext>
{
    public MySqlPubSubDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine("Shoud not be called");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<PubSubDbContext>();

        var connectionString =  configuration["ConnectionStrings:MySql"];

        optionsBuilder
            .UseMySQL(connectionString);

        return new MySqlPubSubDbContext(optionsBuilder.Options);
    }
}