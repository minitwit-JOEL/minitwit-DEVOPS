using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowController(IFollowService followService)
    {
        _followService = followService;
    }

    [HttpPost("{username}/follow")]
    public async Task<IActionResult> Follow(string username)
    {
        var currentUserId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("User is not logged in");
        }
        
        var follow = await _followService.AddFollow(int.Parse(currentUserId), username);
        
        return Ok(follow);
    }

    [HttpPost("{username}/unfollow")]
    public async Task<IActionResult> Unfollow(string username)
    {
        var currentUserId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("User is not logged in");
        }

        var unfollow = await _followService.RemoveFollow(int.Parse(currentUserId), username);
       
        return Ok(unfollow);
    }
}