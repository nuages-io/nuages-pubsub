using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nuages.PubSub.Storage.Tests;
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

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;

        var context = new MySqlPubSubContext(contextOptions);
        
        PubSubStorage = new PubSubStorageEntityFramework<MySqlPubSubContext>(context);
        Hub = "Hub";
        Sub = "sub-test";
        PubSubStorage.TruncateAllData();
        PubSubStorage.Initialize();
    }
}