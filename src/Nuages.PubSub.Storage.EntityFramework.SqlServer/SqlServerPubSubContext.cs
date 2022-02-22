using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer;

public class SqlServerPubSubContext : PubSubDbContext
{

    public SqlServerPubSubContext(DbContextOptions context) : base(context)
    {
    }
}

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<SqlServerPubSubContext>
{
    public SqlServerPubSubContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<PubSubDbContext>();

        var connectionString =  configuration["ConnectionStrings:SqlServer"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new SqlServerPubSubContext(optionsBuilder.Options);
    }
}