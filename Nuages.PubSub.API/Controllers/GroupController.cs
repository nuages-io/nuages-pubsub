using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
    public async Task SendAsync(string hub, string group, string message)
    {
        await _pubSubService.SendToGroupAsync( hub,  group, message);
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string hub, string group)
    {
        await _pubSubService.CloseGroupConnectionsAsync( hub,  group);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string hub, string group)
    {
        return await _pubSubService.GroupExistsAsync(hub,  group);
    }
}