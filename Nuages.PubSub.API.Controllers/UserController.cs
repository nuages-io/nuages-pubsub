using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class UserController
{
    private readonly IPubSubService _pubSubService;

    public UserController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    [HttpPost("Send")]
    public async Task SendAsync(string hub, string userId, string message)
    {
        await _pubSubService.SendToUserAsync(hub, userId, new PubSubMessage());
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string hub, string userId)
    {
        await _pubSubService.CloseUserConnectionsAsync(hub,  userId);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string hub, string userId)
    {
        return await _pubSubService.UserExistsAsync(hub,  userId);
    }

    Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        throw new NotImplementedException();
    }

    Task RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        throw new NotImplementedException();
    }

    Task RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        throw new NotImplementedException();
    }
}