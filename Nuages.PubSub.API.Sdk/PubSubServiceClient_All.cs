namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToAllAsync(string message)
    {
        var webService = new AllClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_hub, message);
    }
    
    public async Task CloseAllConnectionsAsync()
    {
        var webService = new AllClient(HttpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub);
    }
}