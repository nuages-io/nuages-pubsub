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
    public async Task SendAsync( string hub, string connectionId, string message)
    {
        await _pubSubService.SendToConnectionAsync(hub,  connectionId, new PubSubMessage());
    }
    
    [HttpDelete("Close")]
    public async Task CloseAsync( string hub, string connectionId)
    {
        await _pubSubService.CloseConnectionAsync(hub,  connectionId);
    }
    
    [HttpGet("Exists")]
    public async Task<bool> ExistsAsync(string hub, string connectionId)
    {
        return await _pubSubService.ConnectionExistsAsync(hub,  connectionId);
    }
}