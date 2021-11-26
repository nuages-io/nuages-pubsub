namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToGroupAsync(string group, string message)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_url, _audience, group, message);
    }
    
    public async Task CloseGroupConnectionsAsync(string groupId)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_url, _audience, groupId);
    }
    
    public async Task<bool> GroupExistsAsync(string connectionId)
    {
        var webService = new GroupClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_audience, connectionId);
    }
}