using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUages.PubSub.Storage.EntityFramework.Tests;
using Xunit;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer.Tests;

[Collection("SqlServer")]
// ReSharper disable once UnusedType.Global
public class TestsPubSubStorageSqlServer : TestPubSubStorageBase
{
    public TestsPubSubStorageSqlServer()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var connectionString = configuration["ConnectionStrings:SqlServer"];

        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var context = new SqlServerPubSubContext(contextOptions);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        PubSubStorage = new PubSubStorageEntityFramework<SqlServerPubSubContext>(context);
        Hub = "Hub";
        Sub = "sub-test";
    }
}