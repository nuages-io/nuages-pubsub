namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToGroupAsync(string group, Message message)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync( _hub, group, message).ConfigureAwait(false);
    }
    
    public async Task CloseGroupConnectionsAsync(string group)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync( _hub, group).ConfigureAwait(false);
    }
    
    public async Task<bool> GroupExistsAsync(string connectionId)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }
    
    public async Task AddConnectionToGroupAsync( string group, string connectionid)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.AddConnectionToGroupAsync( _hub, group, connectionid).ConfigureAwait(false);
    }
    
    public async Task RemoveConnectionFromGroupAsync(string group, string connectionid)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.RemoveConnectionFromGroupAsync( _hub, group, connectionid).ConfigureAwait(false);
    }
    
    public async Task<bool> IsConnectionInGroupAsync(string group, string connectionid)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.IsConnectionInGroupAsync( _hub, group, connectionid).ConfigureAwait(false);
    }
}