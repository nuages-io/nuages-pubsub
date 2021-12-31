using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.API.Endpoints.Model;
using Nuages.PubSub.Services;
// ReSharper disable UnusedMember.Global

namespace Nuages.PubSub.API.Endpoints;

[Route("api/group")]
public class GroupController
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;

    public GroupController(IPubSubService pubSubService, IHostEnvironment environment)
    {
        _pubSubService = pubSubService;
        _environment = environment;
    }
    
    [HttpPost("send")]
    public async Task SendAsync(string hub, string group, [FromBody] Message message)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.SendAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            await _pubSubService.SendToGroupAsync( hub,  group, new PubSubMessage
            {
                type = message.type,
                data = message.data,
                dataType = message.dataType.ToString(),
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> CloseAsync(string hub, string group)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.CloseAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            await _pubSubService.CloseGroupConnectionsAsync( hub,  group);
            
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
    public async Task<bool> ExistsAsync(string hub, string group)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.ExistsAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            return await _pubSubService.GroupExistsAsync(hub,  group);
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

    [HttpGet("connections/exists")]
    public async Task<bool> IsConnectionInGroupAsync(string hub, string group, string connectionId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.IsConnectionInGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            return await _pubSubService.IsConnectionInGroupAsync(hub,  group, connectionId);
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

    [HttpPut("connections/add")]
    public async Task AddConnectionToGroupAsync(string hub, string group, string connectionId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.AddConnectionToGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.AddConnectionToGroupAsync(hub,  group, connectionId);
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

    [HttpDelete("connections/remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RemoveConnectionFromGroupAsync(string hub, string group, string connectionId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.RemoveConnectionFromGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            if (string.IsNullOrEmpty(connectionId))
                throw new ArgumentException("connectionId must be provided");
            
            await _pubSubService.RemoveConnectionFromGroupAsync(hub,  group, connectionId);
            
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
}