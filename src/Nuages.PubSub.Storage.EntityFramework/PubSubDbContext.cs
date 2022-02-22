using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Nuages.PubSub.Storage.EntityFramework.DataModel;

#pragma warning disable CS8618

namespace Nuages.PubSub.Storage.EntityFramework;

public abstract class PubSubDbContext : DbContext
{
    public DbSet<PubSubAck> Acks { get; set; }
    public DbSet<PubSubConnection> Connections { get; set; }
    public DbSet<PubSubGroupConnection> Groups { get; set; }
    public DbSet<PubSubGroupUser> GroupUsers { get; set; }
    
    public PubSubDbContext(DbContextOptions context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        var valueComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());
        
        modelBuilder.Entity<PubSubConnection>()
            .Property(e => e.Permissions)
            .HasConversion(
                v => string.Join(",", v!),
                v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata
            .SetValueComparer(valueComparer);
        
        modelBuilder.Entity<PubSubAck>()
            .HasKey(c => new { c.Hub, c.ConnectionId, c.AckId });
        
        modelBuilder.Entity<PubSubConnection>()
            .HasKey(c => new { c.Hub, c.ConnectionId });
        
        modelBuilder.Entity<PubSubConnection>()
            .HasIndex(c => new { c.Hub });
        
        modelBuilder.Entity<PubSubConnection>()
            .HasIndex(c => new { c.Hub, c.UserId });
        
        modelBuilder.Entity<PubSubGroupConnection>()
            .HasKey(c => new { c.Hub, c.Group, c.ConnectionId });
        
        modelBuilder.Entity<PubSubGroupConnection>()
            .HasIndex(c => new { c.Hub, c.Group });

        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasKey(c => new { c.Hub, c.Group, c.UserId });
        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasIndex(c => new { c.Hub, c.Group });
        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasIndex(c => new { c.Hub, c.UserId });
        
    }

    //public abstract Task DeleteAllConnectionsAsync();
    
    
}