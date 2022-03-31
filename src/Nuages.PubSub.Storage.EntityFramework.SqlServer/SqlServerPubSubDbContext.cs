using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer;

public class SqlServerPubSubDbContext : PubSubDbContext
{

    public SqlServerPubSubDbContext(DbContextOptions context) : base(context)
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

// ReSharper disable once UnusedType.Global
[ExcludeFromCodeCoverage]
public class SqlServerPubSubContextFactory : IDesignTimeDbContextFactory<SqlServerPubSubDbContext>
{
    public SqlServerPubSubDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.sqlserver.json", true)
            .Build();
        
        var optionsBuilder = new DbContextOptionsBuilder<PubSubDbContext>();

        var connectionString =  configuration["ConnectionStrings:SqlServer"];
        
        optionsBuilder
            .UseSqlServer(connectionString);

        return new SqlServerPubSubDbContext(optionsBuilder.Options);
    }
}
