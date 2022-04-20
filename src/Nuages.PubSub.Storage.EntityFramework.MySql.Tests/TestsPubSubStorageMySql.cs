using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Services.Tests;
using Xunit;

namespace Nuages.PubSub.Storage.EntityFramework.MySql.Tests;

[Collection("MySql")]
// ReSharper disable once UnusedType.Global
public class TestsPubSubStorageMySql : TestPubSubStorageBase
{
    public TestsPubSubStorageMySql()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        var connectionString = configuration["ConnectionStrings:MySql"];
        
        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseMySQL(connectionString)
            .Options;

        var context = new MySqlPubSubDbContext(contextOptions);
        
        PubSubStorage = new PubSubStorageEntityFramework<MySqlPubSubDbContext>(context);
        Hub = "Hub";
        Sub = "sub-test";
        PubSubStorage.TruncateAllData();
        PubSubStorage.Initialize();
    }
}