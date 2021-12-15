// ReSharper disable UnusedMemberInSuper.Global
namespace Nuages.PubSub.Services.Storage;

public interface IPubSubConnection
{
    public string Id { get; set; }
    public string ConnectionId { get; set; } 
    public string UserId { get; set; } 
    public DateTime CreatedOn { get;  set;}
    public DateTime? ExpireOn { get;  set;}
    public string Hub { get; set;} 

    public List<string>? Permissions { get; set;}
}

public static class WebSocketConnectionExtensions
{
    public static bool IsExpired(this IPubSubConnection pubSubConnection)
    {
        if (!pubSubConnection.ExpireOn.HasValue)
            return false;

        return pubSubConnection.ExpireOn < DateTime.UtcNow;
    }
    
   
    
    public static void AddPermission(this IPubSubConnection pubSubConnection, string permissionString)
    {
        pubSubConnection.Permissions ??= new List<string>();
            
        pubSubConnection.Permissions.Add(permissionString);

    }

}