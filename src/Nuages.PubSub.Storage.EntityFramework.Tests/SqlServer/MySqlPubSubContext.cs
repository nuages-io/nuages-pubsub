using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Storage.EntityFramework;

namespace NUages.PubSub.Storage.EntityFramework.Tests.SqlServer;

public class SqlServerPubSubContext : PubSubDbContext
{

    public SqlServerPubSubContext(DbContextOptions context) : base(context)
    {
    }
}

// ReSharper disable once UnusedType.Global
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