using System;
using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Storage.EntityFramework;

namespace NUages.PubSub.Storage.EntityFramework.Tests.InMemory;

// ReSharper disable once UnusedType.Global
public class TestsPubSubStorageInMemory : TestPubSubStorageBase
{
    public TestsPubSubStorageInMemory()
    {
        var contextOptions = new DbContextOptionsBuilder<PubSubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PubSubDbContext(contextOptions);
        
        PubSubStorage = new PubSubStorageEntityFramework<PubSubDbContext>(context);
        Hub = "Hub";
        Sub = "sub-test";
        
        PubSubStorage.TruncateAllData();
    }
}