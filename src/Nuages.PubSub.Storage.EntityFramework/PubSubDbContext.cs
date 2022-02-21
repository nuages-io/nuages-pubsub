using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var valueComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
        
        modelBuilder.Entity<PubSubConnection>()
            .Property(e => e.Permissions)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata
            .SetValueComparer(valueComparer);
    }
}