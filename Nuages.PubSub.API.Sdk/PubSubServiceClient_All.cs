namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToAllAsync(Message message)
    {
        var webService = new AllClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        
        await webService.SendAsync(_hub, message).ConfigureAwait(false);
    }
    
    public async Task CloseAllConnectionsAsync()
    {
        var webService = new AllClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub).ConfigureAwait(false);
    }
}