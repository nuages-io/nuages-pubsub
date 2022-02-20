using Microsoft.EntityFrameworkCore;
using Nuages.PubSub.Storage.EntityFramework.DataModel;

#pragma warning disable CS8618

namespace Nuages.PubSub.Storage.EntityFramework;

public class PubSubDbContext : DbContext
{
    public virtual DbSet<PubSubAck> Acks { get; set; }
    public virtual DbSet<PubSubConnection> Connections { get; set; }
    public virtual DbSet<PubSubGroupConnection> Groups { get; set; }
    public virtual DbSet<PubSubGroupUser> GroupUsers { get; set; }
    
    public PubSubDbContext(DbContextOptions<PubSubDbContext> context) : base(context)
    {
    }

    
}