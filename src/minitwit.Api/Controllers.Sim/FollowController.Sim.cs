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
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
        }

        try
        {
            var followerNames = await _followService.GetFollowerNames(latest, username, no);
        
            return Ok(new { follows = followerNames });
        } catch (ArgumentException)
        {
            return NotFound(new { status = 404, error_msg = "User not found" });
        }
    }

    [HttpPost("fllws/{username}")]
    public async Task<IActionResult> GetUsersTwitsPost(string username, [FromBody] FollowRequest request, [FromQuery] int latest, [FromQuery] int no = 100)
    {
        if (!_simService.CheckIfRequestFromSimulator(Request))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { status = 403, error_msg = "You are not authorized to use this resource!" });
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

            return BadRequest(new
                { status = 400, error_msg = "Invalid request: Provide either 'follow' or 'unfollow'." });
        }
        catch (ArgumentException)
        {
            return NotFound(new { status = 404, error_msg = "User not found" });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500);
        }
    }
    
    public record FollowRequest(string? Follow, string? Unfollow);
}