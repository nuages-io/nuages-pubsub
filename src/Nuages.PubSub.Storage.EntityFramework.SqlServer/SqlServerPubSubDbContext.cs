using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer;

public class SqlServerPubSubDbContext : PubSubDbContext
{

    public SqlServerPubSubDbContext(DbContextOptions context) : base(context)
    {
    }
}

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<SqlServerPubSubDbContext>
{
    public SqlServerPubSubDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<PubSubDbContext>();

        var connectionString =  configuration["ConnectionStrings:SqlServer"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new SqlServerPubSubDbContext(optionsBuilder.Options);
    }
}