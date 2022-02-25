using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Nuages.PubSub.Storage.EntityFramework.DataModel;

#pragma warning disable CS8618

namespace Nuages.PubSub.Storage.EntityFramework;

public abstract class PubSubDbContext : DbContext
{
    public DbSet<PubSubAck> Acks { get; set; }
    public DbSet<PubSubConnection> Connections { get; set; }
    public DbSet<PubSubGroupConnection> GroupConnections { get; set; }
    public DbSet<PubSubGroupUser> GroupUsers { get; set; }

    protected PubSubDbContext(DbContextOptions context) : base(context)
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
            .HasKey(c => new { c.Hub, Group = c.GroupName, c.ConnectionId });
        
        modelBuilder.Entity<PubSubGroupConnection>()
            .HasIndex(c => new { c.Hub, Group = c.GroupName });

        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasKey(c => new { c.Hub, Group = c.GroupName, c.UserId });
        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasIndex(c => new { c.Hub, Group = c.GroupName });
        
        modelBuilder.Entity<PubSubGroupUser>()
            .HasIndex(c => new { c.Hub, c.UserId });
        
    }

    public virtual  async Task DeleteConnectionFromAllGroupConnectionAsync(string hub, string connectionId)
    {
        GroupConnections.RemoveRange(GroupConnections
            .Where(c => c.Hub == hub && c.ConnectionId == connectionId));
        
        await SaveChangesAsync();
    }
    
    public virtual async Task DeleteAckForConnectionAsync(string hub, string connectionId)
    {
        Acks.RemoveRange(Acks
            .Where(c => c.Hub == hub && c.ConnectionId == connectionId));
        
        await SaveChangesAsync();
    }

    public virtual async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        Connections.RemoveRange(Connections
            .Where(c => c.Hub == hub && c.ConnectionId == connectionId));
        
        await SaveChangesAsync();
    }

    public virtual async Task DeleteConnectionFromGroupConnectionAsync(string hub, string group, string connectionId)
    {
        var exising = GroupConnections
            .SingleOrDefault(c => c.Hub == hub && c.GroupName == group && c.ConnectionId == connectionId);

        if (exising != null)
        {
            GroupConnections.Remove(exising);
            await SaveChangesAsync();
        }
    }

    public virtual async Task DeleteUserFromGroupConnectionAsync(string hub, string group, string userId)
    {
        GroupConnections.RemoveRange(GroupConnections.Where( c => c.Hub == hub && c.GroupName == group && c.UserId == userId));
        
        await SaveChangesAsync();
    }
    
    public virtual async Task DeleteUserFromGroupUserAsync(string hub, string group, string userId)
    {
        GroupUsers.RemoveRange(GroupUsers.Where( c => c.Hub == hub && c.GroupName == group && c.UserId == userId));
        
        await SaveChangesAsync();
    }

    public virtual async Task DeleteUserFromAllGroupConnectionsAsync(string hub, string userId)
    {
        GroupConnections.RemoveRange(GroupConnections.Where( c => c.Hub == hub  && c.UserId == userId));
        await SaveChangesAsync();
    }
    
    public virtual async Task DeleteUserFromAllGroupUsersAsync(string hub, string userId)
    {
        GroupUsers.RemoveRange(GroupUsers.Where( c => c.Hub == hub  && c.UserId == userId));
        await SaveChangesAsync();
    }
    
    
}