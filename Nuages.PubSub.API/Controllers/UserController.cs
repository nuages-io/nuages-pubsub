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
    public async Task SendAsync(string url, string audience, string userId, string message)
    {
        await _pubSubService.SendToUserAsync(url, audience, userId, message);
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string url, string audience, string userId)
    {
        await _pubSubService.CloseUserConnectionsAsync(url, audience,  userId);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string audience, string userId)
    {
        return await _pubSubService.UserExistsAsync(audience,  userId);
    }
}