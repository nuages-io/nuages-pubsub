namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToConnectionAsync(string connectionid, Message message)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_hub, connectionid, message).ConfigureAwait(false);
    }

    public async Task CloseConnectionAsync(string connectionId)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub, connectionId).ConfigureAwait(false);
    }
    
    public async Task<bool> ConnectionExistsAsync(string connectionId)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }

    public async Task GrantPermissionAsync(PubSubPermission permission, string connectionId, string? target = null)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };

        await webService.GrantPermissionAsync(_hub, permission, connectionId, target);
    }
    
    public async Task RevokePermissionAsync( PubSubPermission permission, string connectionId, string? target = null)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };

        await webService.RevokePermissionAsync(_hub, permission, connectionId, target);
    }
    
    public async Task<bool> CheckPermissionAsync(PubSubPermission permission, string connectionId, string? target = null)
    {
        var webService = new ConnectionClient(_httpClient)
        {
            BaseUrl = _url
        };

        return await webService.CheckPermissionAsync(_hub, permission, connectionId, target);
    }
}