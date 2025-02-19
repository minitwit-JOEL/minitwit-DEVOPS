using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Application.Interfaces.Sim;
using ITwitsService = minitwit.Application.Interfaces.Sim.ITwitsService;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("/")]
public class TwitsControllerSim : ControllerBase
{
    private readonly ITwitsService _twitsService;
    private readonly ISimService _simService;

    public TwitsControllerSim (ITwitsService twitsService, ISimService simService) 
    {
        _twitsService = twitsService;
        _simService = simService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestProcessedCommandId()
    {
        var latestProcessedCommandId = await _simService.GetLatestProcessedCommandId();
        
        return Ok(new { latest = latestProcessedCommandId });
    }
    
    [HttpGet("msgs")]
    public async Task<IActionResult> GetMessages([FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }
        var messages = await _twitsService.GetMessages(no);
        return Ok(messages);   
    }


    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetMessagesForUser(string username, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }
        
        try
        {
            var messages = await _twitsService.GetMessagesForUser(username, no);

            return Ok(messages);
        }
        catch (ArgumentException)
        {
            return NotFound(new { status = 404, error_msg = "User not found" });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessageForUser(string username, [FromBody] MessageRequest msgRequest)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }

        try
        {
            await _twitsService.PostMessagesForUser(username, msgRequest.Content);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound(new { status = 404, error_msg = "User not found" });
        }
    }
    
    public record MessageRequest(string Content);
}