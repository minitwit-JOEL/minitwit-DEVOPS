using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

[Authorize]
[ApiController]
[Route("api/follow/{username}")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    [HttpGet]
    public async Task<bool> CheckFollowerByUsername(string username)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        try
        {
            await _followService.CheckFollowByUsername(int.Parse(userId), username);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Follow(string username)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not logged in");
        }
        
        var follow = await _followService.AddFollow(int.Parse(userId), username);
        
        return Ok(follow);
    }

    [HttpPost("unfollow")]
    public async Task<IActionResult> Unfollow(string username)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not logged in");
        }

        var unfollow = await _followService.RemoveFollow(int.Parse(userId), username);
       
        return Ok(unfollow);
    }
}