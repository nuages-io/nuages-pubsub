namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToGroupAsync(string group, string message)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync( _hub, group, message).ConfigureAwait(false);
    }
    
    public async Task CloseGroupConnectionsAsync(string groupId)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync( _hub, groupId).ConfigureAwait(false);
    }
    
    public async Task<bool> GroupExistsAsync(string connectionId)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }
}