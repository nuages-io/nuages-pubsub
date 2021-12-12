namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToUserAsync(string userId, string message)
    {
        var webService = new UserClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_hub, userId, message).ConfigureAwait(false);
    }
    
    public async Task CloseUserConnectionsAsync(string userId)
    {
        var webService = new UserClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub, userId).ConfigureAwait(false);
    }
    
    public async Task<bool> UserExistsAsync(string connectionId)
    {
        var webService = new UserClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }
}