using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class ConnectionController
{
    private readonly IPubSubService _pubSubService;

    public ConnectionController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }
    
    [HttpPost("Send")]
    public async Task SendAsync(string url, string audience, string connectionId, string message)
    {
        await _pubSubService.SendToConnectionAsync(url, audience,  connectionId, message);
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync(string url, string audience, string connectionId)
    {
        await _pubSubService.CloseConnectionAsync(url, audience,  connectionId);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string audience, string connectionId)
    {
        return await _pubSubService.ConnectionExistsAsync(audience,  connectionId);
    }
}