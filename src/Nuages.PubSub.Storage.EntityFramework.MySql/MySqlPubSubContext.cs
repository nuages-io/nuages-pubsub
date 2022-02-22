using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nuages.PubSub.Storage.EntityFramework.MySql;

public class MySqlPubSubContext : PubSubDbContext
{
    public MySqlPubSubContext(DbContextOptions context) : base(context)
    {
      
    }

}

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class MySqlPubSubContextFactory : IDesignTimeDbContextFactory<MySqlPubSubContext>
{
    public MySqlPubSubContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<PubSubDbContext>();

        var connectionString =  configuration["ConnectionStrings:MySql"];

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        optionsBuilder
            .UseMySql(connectionString, serverVersion);

        return new MySqlPubSubContext(optionsBuilder.Options);
    }
}