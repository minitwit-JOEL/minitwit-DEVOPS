using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Application.Interfaces.Sim;
using minitwit.Infrastructure.Migrations;
using ITwitsService = minitwit.Application.Interfaces.Sim.ITwitsService;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("api/sim")]
public class TwitsControllerSim : ControllerBase
{
    private readonly ITwitsService _twitsService;
    private readonly ISimService _simService;

    public TwitsControllerSim (ITwitsService twitsService, ISimService simService) 
    {
        _twitsService = twitsService;
        _simService = simService;
    }
    
    [HttpGet("msgs")]
    public async Task<IActionResult> GetMessages([FromQuery] int latest, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }
        var messages = await _twitsService.GetMessages(latest, no);
        return Ok(messages);   
    }
    
    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetMessagesForUser(string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }
        
        try
        {
            var messages = await _twitsService.GetMessagesForUser(latest, username, no);

            return Ok(messages);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessageForUser(string username, [FromQuery] int latest, [FromBody] MessageRequest msgRequest)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }

        await _twitsService.PostMessagesForUser(latest, username, msgRequest.Content);
        return NoContent();
    }
    
    public record MessageRequest(string Content);
}