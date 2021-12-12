namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToConnectionAsync(string connectionid, string message)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_hub, connectionid, message).ConfigureAwait(false);
        
    }

    public async Task CloseConnectionAsync(string connectionId)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub, connectionId).ConfigureAwait(false);
    }
    
    public async Task<bool> ConnectionExistsAsync(string connectionId)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }
}