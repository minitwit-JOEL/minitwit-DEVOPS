using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

[ApiController]
[Route("api/twit")]
public class TwitsController : ControllerBase
{
    private readonly ITwitsService _twitsService;
    
    public TwitsController(ITwitsService twitsService)
    {
        _twitsService = twitsService;
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicTwits([FromQuery] int page = default)
    {
        var twits = await _twitsService.GetPublicTimeline(page);

        return Ok(twits);
    }
    
    [HttpGet("feed")]
    public async Task<IActionResult> GetPrivateTwits([FromQuery] int page = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            var publicTwits = await _twitsService.GetPublicTimeline(page);
            return Ok(new
            {
                id = 0,
                twits = publicTwits
            });
        }
        
        var privateTwits = await _twitsService.GetFeed(int.Parse(userId), page);

        return Ok(new
        {
            id = 1,
            twits = privateTwits
        });
    }

    [Authorize]
    [HttpGet("user/{authorName}")]
    public async Task<IActionResult> GetUsersTwits(string authorName, [FromQuery] int page = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not logged in");
        }
        
        var twits = await _twitsService.GetUsersTwitsByName(authorName, page);
        
        return Ok(twits);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostTwit([FromBody] string message)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized("User was not logged in");
        }

        var twit = await _twitsService.PostTwit(parsedUserId, message);

        return Ok(twit);
    }
}