using Microsoft.EntityFrameworkCore;

namespace Nuages.PubSub.Storage.EntityFramework;

public class InMemoryPubSubDbContext : PubSubDbContext
{
    public InMemoryPubSubDbContext(DbContextOptions context) : base(context)
    {
    }
}