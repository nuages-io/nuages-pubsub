using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Controllers;

[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;
    private readonly PubSubOptions _options;

    public AuthController(IPubSubService pubSubService, IOptions<PubSubOptions> options, IHostEnvironment environment)
    {
        _pubSubService = pubSubService;
        _environment = environment;
        _options = options.Value;
    }

    // GET
    [HttpGet("getclienttoken")]
    public async Task<ActionResult<string>> GetClientAccessTokenAsync(
        string userId, 
        TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AuthController.GetClientAccessTokenAsync");
            
            var secret = _options.Secret;
            if (string.IsNullOrEmpty(secret))
                throw new ArgumentException("secret must be provided");

            var issuer = _options.Issuer;
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException("issuer must be provided");

            var audience = _options.Audience;
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException("audience must be provided");
            
            var token = _pubSubService.GenerateToken(issuer, audience, userId, roles ?? new List<string>(), secret,
                expiresAfter);

            return await Task.FromResult(token);
        }
        catch (Exception e)
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpGet("getclienturi")]
    public async Task<ActionResult<string>> GetClientAccessUriAsync(
        string userId, string hub,
        TimeSpan? expiresAfter = default, IEnumerable<string>? roles = null, string? token = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AuthController.GetClientAccessUriAsync");

            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");

            if (string.IsNullOrEmpty(token))
            {
                var secret = _options.Secret;
                if (string.IsNullOrEmpty(secret))
                    throw new ArgumentException("secret must be provided");

                var issuer = _options.Issuer;
                if (string.IsNullOrEmpty(issuer))
                    throw new ArgumentException("issuer must be provided");
            
                var audience = _options.Audience;
                if (string.IsNullOrEmpty(issuer))
                    throw new ArgumentException("audience must be provided");

                token = _pubSubService.GenerateToken(issuer, audience, userId, roles ?? new List<string>(), secret,
                    expiresAfter);
            }
            
            var uri = $"{_options.Uri}?hub={hub}&access_token={token}";

            return await Task.FromResult(uri);
        }
        catch (Exception e)
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}