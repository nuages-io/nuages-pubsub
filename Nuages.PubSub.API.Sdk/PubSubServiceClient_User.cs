namespace Nuages.PubSub.API.Sdk;

public partial class PubSubServiceClient
{
    public async Task SendToUserAsync(string userId, Message message)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.SendAsync(_hub, userId, message).ConfigureAwait(false);
    }
    
    public async Task CloseUserConnectionsAsync(string userId)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.CloseAsync(_hub, userId).ConfigureAwait(false);
    }
    
    public async Task<bool> UserExistsAsync(string connectionId)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        return await webService.ExistsAsync(_hub, connectionId).ConfigureAwait(false);
    }
    
    public async Task AddUserToGroupAsync(string userId, string group)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.AddUserToGroupAsync(_hub, userId, group).ConfigureAwait(false);
    }
    
    public async Task RemoveUserFromGroupAsync(string userId, string group)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.RemoveUserFromGroupAsync(_hub, userId, group).ConfigureAwait(false);
    }
    
    public async Task RemoveUserFromGroupAsync(string userId)
    {
        var webService = new UserClient(_httpClient)
        {
            BaseUrl = _url
        };
    
        await webService.RemoveUserFromAllGroupsAsync(_hub, userId).ConfigureAwait(false);
    }
}