using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Storage.EntityFramework;

namespace NUages.PubSub.Storage.EntityFramework.Tests;

public class MySqlPubSubContext : PubSubDbContext
{

    public MySqlPubSubContext(DbContextOptions context) : base(context)
    {
    }
}

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