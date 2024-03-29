using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Nuages.PubSub.API.Endpoints.Model;
using Nuages.PubSub.Services;
// ReSharper disable UnusedMember.Global

namespace Nuages.PubSub.API.Endpoints;

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
    
    /// <summary>
    /// Send a message to all connections
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="hub">Hub name</param>
    /// <param name="message"></param>
    /// <exception cref="ArgumentException"></exception>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
    
    /// <summary>
    /// Close all connections
    /// </summary>
    /// <param name="hub">Hub name</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    [HttpDelete("close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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