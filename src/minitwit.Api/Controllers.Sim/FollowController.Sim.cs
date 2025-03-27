using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Application.Interfaces.Sim;
using IFollowService = minitwit.Application.Interfaces.Sim.IFollowService;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("api/sim")]
public class FollowControllerSim : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ISimService _simService;

    public FollowControllerSim(IFollowService followService, ISimService simService)
    {
        _followService = followService;
        _simService = simService;
    }

    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetFollowerNames(string username, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new { status = 401, error_msg = "You are not authorized to use this resource!" });
        }

        try
        {
            var followerNames = await _followService.GetFollowerNames(latest, username, no);
        
            return Ok(new { follows = followerNames });
        } catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> GetUsersTwitsPost(string username, [FromBody] FollowRequest request, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new { status = 401, error_msg = "You are not authorized to use this resource!" });
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(request.Follow))
            {
                await _followService.Follow(latest, username, request.Follow);
                return NoContent();
            }

            if (!string.IsNullOrWhiteSpace(request.Unfollow))
            {
                await _followService.Unfollow(latest, username, request.Unfollow);
                return NoContent();
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new
                { status = 500, error_msg = "Invalid request: Provide either 'follow' or 'unfollow'." });
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }
    
    public record FollowRequest(string? Follow, string? Unfollow);
}