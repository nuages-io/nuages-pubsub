using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class GroupController
{
    private readonly IPubSubService _pubSubService;

    public GroupController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    [HttpPost("Send")]
    public async Task SendAsync(string url, string audience, string group, string message)
    {
        await _pubSubService.SendToGroupAsync(url, audience,  group, message);
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string url, string audience, string group)
    {
        await _pubSubService.CloseGroupConnectionsAsync(url, audience,  group);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string audience, string group)
    {
        return await _pubSubService.GroupExistsAsync(audience,  group);
    }
}