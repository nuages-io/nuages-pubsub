using System;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Services.Tests;

namespace Nuages.PubSub.Storage.EntityFramework.Tests;

// ReSharper disable once UnusedType.Global
public class TestsPubSubStorageInMemory : TestPubSubStorageBase
{
    public TestsPubSubStorageInMemory()
    {
        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new InMemoryPubSubDbContext(contextOptions);
        
        PubSubStorage = new PubSubStorageEntityFramework<InMemoryPubSubDbContext>(context);
        Hub = "Hub";
        Sub = "sub-test";
        
        PubSubStorage.TruncateAllData();
        PubSubStorage.Initialize();
    }
}