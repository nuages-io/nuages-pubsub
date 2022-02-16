using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.API.Endpoints.Model;
using Nuages.PubSub.Services;
// ReSharper disable UnusedMember.Global

namespace Nuages.PubSub.API.Endpoints;

[Route("api/user")]
public class UserController
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;

    public UserController(IPubSubService pubSubService, IHostEnvironment environment)
    {
        _pubSubService = pubSubService;
        _environment = environment;
    }
    
    [HttpPost("send")]
    public async Task SendAsync(string hub, string userId, [FromBody] Message message)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.SendAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            await _pubSubService.SendToUserAsync(hub, userId,new PubSubMessage
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
    public async Task<ActionResult> CloseAsync(string hub, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.CloseAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            await _pubSubService.CloseUserConnectionsAsync(hub,  userId);
            
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
    public async Task<bool> ExistsAsync(string hub, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.ExistsAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            return await _pubSubService.UserExistsAsync(hub,  userId);
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

    [HttpPut("groups/add")]
    public async Task AddUserToGroupAsync(string hub, string group, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.AddUserToGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            await _pubSubService.AddUserToGroupAsync(hub, group,  userId);
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

    [HttpDelete("groups/remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RemoveUserFromGroupAsync(string hub, string group, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.AddUserToGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            await _pubSubService.RemoveUserFromGroupAsync(hub, group,  userId);
            
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

    [HttpGet("groups/exists")]
    public async Task<bool> IsUserInGroupAsync(string hub, string group, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("GroupController.IsUserInGroupAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(group))
                throw new ArgumentException("group must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("connectionId must be provided");
            
            return await _pubSubService.IsUserInGroupAsync(hub,  group, userId);
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

    
    [HttpDelete("groups/removeall")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RemoveUserFromAllGroupsAsync(string hub, string userId)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("UserController.RemoveUserFromAllGroupsAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub must be provided");
            
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("userId must be provided");
            
            await _pubSubService.RemoveUserFromAllGroupsAsync(hub,  userId);
            
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