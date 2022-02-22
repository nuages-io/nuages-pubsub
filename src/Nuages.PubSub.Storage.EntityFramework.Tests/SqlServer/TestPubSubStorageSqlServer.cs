using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Storage.EntityFramework;
using Xunit;

namespace NUages.PubSub.Storage.EntityFramework.Tests.SqlServer;

[Collection("SqlServer")]
// ReSharper disable once UnusedType.Global
public class TestPubSubStorageSqlServer : TestPubSubStorageBase
{
    public TestPubSubStorageSqlServer()
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