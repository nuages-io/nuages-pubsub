using Microsoft.EntityFrameworkCore;

namespace Nuages.PubSub.Storage.EntityFramework.MySql;

public class MySqlPubSubDbContext : PubSubDbContext
{
    public MySqlPubSubDbContext(DbContextOptions context) : base(context)
    {
      
    }

    public override  async Task DeleteConnectionFromAllGroupConnectionAsync(string hub, string connectionId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupConnections where hub = {hub} and ConnectionId = {connectionId}");
    }
 
    public override async Task DeleteAckForConnectionAsync(string hub, string connectionId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from Acks where hub = {hub} and ConnectionId = {connectionId}");
    }
    
    public override async Task DeleteConnectionAsync(string hub, string connectionId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from Connections where hub = {hub} and ConnectionId = {connectionId}");
    }
    
    public override async Task DeleteConnectionFromGroupConnectionAsync(string hub, string group, string connectionId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupConnections where hub = {hub} and GroupName = {group} and ConnectionId = {connectionId}");
    }
    
    public override async Task DeleteUserFromGroupConnectionAsync(string hub, string group, string userId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupConnections where hub = {hub} and GroupName = {group} and UserId = {userId}");
    }
    
    public override async Task DeleteUserFromGroupUserAsync(string hub, string group, string userId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupUsers where hub = {hub} and GroupName = {group} and UserId = {userId}");
    }
    
    public override async Task DeleteUserFromAllGroupConnectionsAsync(string hub, string userId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupConnections where hub = {hub} and UserId = {userId}");
    }
    
    public override async Task DeleteUserFromAllGroupUsersAsync(string hub, string userId)
    {
        await Database.ExecuteSqlInterpolatedAsync($"delete from GroupUsers where hub = {hub} and UserId = {userId}");
    }
}