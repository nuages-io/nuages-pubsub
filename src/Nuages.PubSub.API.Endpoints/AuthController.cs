using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nuages.AWS.Secrets;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.API.Endpoints;

[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;
    private readonly IAWSSecretProvider _secretProvider;
    private readonly PubSubOptions _options;

    public AuthController(IPubSubService pubSubService, IOptions<PubSubOptions> options, IHostEnvironment environment, IAWSSecretProvider secretProvider)
    {
        _pubSubService = pubSubService;
        _environment = environment;
        _secretProvider = secretProvider;
        _options = options.Value;
    }

    class SecretValue
    {
        public string Value { get; set; } = string.Empty;
    }
    
    // GET
    [HttpGet("getclienttoken")]
    public async Task<ActionResult<string>> GetClientAccessTokenAsync(
        string userId, 
        int? expiresAfterSeconds = null, IEnumerable<string>? roles = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AuthController.GetClientAccessTokenAsync");
            
            if (string.IsNullOrEmpty(_options.Auth.Secret))
                throw new ArgumentException("secret must be provided");
            
            var secret = await _secretProvider.GetSecretAsync<SecretValue>(_options.Auth.Secret);

            if (secret == null)
                throw new ArgumentException("secret can't be read");

            var issuer = _options.Auth.Issuer;
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException("issuer must be provided");

            var audience = _options.Auth.Audience;
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentException("audience must be provided");
            
            var token = _pubSubService.GenerateToken(issuer, audience, userId, roles ?? new List<string>(), secret.Value,
                expiresAfterSeconds);

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
        int? expiresAfterSeconds = null, IEnumerable<string>? roles = null, string? token = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AuthController.GetClientAccessUriAsync");

            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");

            if (string.IsNullOrEmpty(token))
            {
                if (string.IsNullOrEmpty(_options.Auth.Secret))
                    throw new ArgumentException("secret must be provided");
            
                var secret = await _secretProvider.GetSecretAsync<SecretValue>(_options.Auth.Secret);

                if (secret == null)
                    throw new ArgumentException("secret can't be read");

                var issuer = _options.Auth.Issuer;
                if (string.IsNullOrEmpty(issuer))
                    throw new ArgumentException("issuer must be provided");
            
                var audience = _options.Auth.Audience;
                if (string.IsNullOrEmpty(issuer))
                    throw new ArgumentException("audience must be provided");

                token = _pubSubService.GenerateToken(issuer, audience, userId, roles ?? new List<string>(), secret.Value,
                    expiresAfterSeconds);
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