using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.API.Controllers.Model;
using Nuages.PubSub.Services;
// ReSharper disable UnusedMember.Global

namespace Nuages.PubSub.API.Controllers;

[Route("api/all")]
public class AllController
{
    private readonly IPubSubService _pubSubService;
    private readonly IHostEnvironment _environment;

    public AllController(IPubSubService pubSubService, IHostEnvironment environment)
    {
        _pubSubService = pubSubService;
        _environment = environment;
    }
    
    [HttpPost("send")]
    public async Task SendAsync(string hub, [FromBody] Message message)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AllController.SendAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub is required");
            
            if (message.data == null)
                throw new ArgumentException("message.data is required");
            
            await _pubSubService.SendToAllAsync(hub, new PubSubMessage
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
    public async Task<ActionResult> CloseAsync(string hub)
    {
        try
        {
            if (!_environment.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AllController.CloseAsync");
            
            if (string.IsNullOrEmpty(hub))
                throw new ArgumentException("hub is required");
            
            await _pubSubService.CloseAllConnectionsAsync( hub);
            
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