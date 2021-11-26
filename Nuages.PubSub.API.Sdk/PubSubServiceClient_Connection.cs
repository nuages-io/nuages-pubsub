namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToConnectionAsync(string connectionid, string message)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_url, _audience, connectionid, message);
        
    }

    public async Task CloseConnectionAsync(string connectionId)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_url, _audience, connectionId);
    }
    
    public async Task<bool> ConnectionExistsAsync(string connectionId)
    {
        var webService = new ConnectionClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_audience, connectionId);
    }
}