using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly IPubSubService _pubSubService;
    private readonly PubSubOptions _options;

    public AuthController(IPubSubService pubSubService, IOptions<PubSubOptions> options)
    {
        _pubSubService = pubSubService;
        _options = options.Value;
    }
    
    // GET
    [HttpGet("GetClientAccessToken")]
    public async Task<ActionResult<string>> GetClientAccessTokenAsync(
        string userId, string audience,
        TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null)
    {
        var secret = _options.Secret;
        if (string.IsNullOrEmpty(secret))
            throw new Exception("secret must be provided");
        
        var issuer = _options.Issuer;
        if (string.IsNullOrEmpty(issuer))
            throw new Exception("issuer must be provided");
        
        var token = _pubSubService.GenerateToken(issuer, audience, userId, roles ?? new List<string>(), secret, expiresAfter);

        return await Task.FromResult(token);
    }

}