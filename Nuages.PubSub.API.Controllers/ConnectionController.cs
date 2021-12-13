using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.API.Controllers.Model;
using Nuages.PubSub.Services;
// ReSharper disable UnusedMember.Global

namespace Nuages.PubSub.API.Controllers;

[Route("api/[controller]")]
public class ConnectionController
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;

    public ConnectionController(IPubSubService pubSubService, IHostEnvironment environment)
    {
        _pubSubService = pubSubService;
        _environment = environment;
    }
    
    [HttpPost("send")]
    public async Task SendAsync( string hub, string connectionId, [FromBody] Message message)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.SendAsync");
         
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.SendToConnectionAsync(hub,  connectionId, new PubSubMessage
            {
                type = message.type,
                data = message.data,
                dataType = message.dataType,
                from = PubSubMessageSource.server
            });
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
    
    [HttpDelete("close")]
    public async Task<ActionResult> CloseAsync( string hub, string connectionId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.CloseAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.CloseConnectionAsync(hub,  connectionId);

            return new StatusCodeResult(StatusCodes.Status204NoContent);

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
    
    [HttpGet("exists")]
    public async Task<bool> ExistsAsync(string hub, string connectionId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.ExistsAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            return await _pubSubService.ConnectionExistsAsync(hub,  connectionId);
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

    [HttpPatch("grantpermission")]
    public async Task GrantPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.GrantPermissionAsync");

            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.GrantPermissionAsync(hub, permission, connectionId, target);
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

    [HttpDelete("revokepermission")]
    public async Task RevokePermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.RevokePermissionAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.RevokePermissionAsync(hub, permission, connectionId, target);
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

    [HttpGet("checkPermission")]
    public async Task<bool> CheckPermissionAsync(string hub, PubSubPermission permission, string connectionId, string? target = null)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("ConnectionController.CheckPermissionAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            return await _pubSubService.CheckPermissionAsync(hub, permission, connectionId, target);
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