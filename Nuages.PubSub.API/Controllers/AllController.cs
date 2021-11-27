using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class AllController
{
    private readonly IPubSubService _pubSubService;

    public AllController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    [HttpPost("Send")]
    public async Task SendAsync(string url, string audience, string message)
    {
        await _pubSubService.SendToAllAsync(url, audience, message);
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string url, string audience)
    {
        await _pubSubService.CloseAllConnectionsAsync(url, audience);
    }
}