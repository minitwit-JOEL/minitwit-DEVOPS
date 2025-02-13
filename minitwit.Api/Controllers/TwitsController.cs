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
        var userId = HttpContext.Session.GetString("user_id");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not logged in");
        }
        
        var twits = await _twitsService.GetFeed(int.Parse (userId), page);

        return Ok(twits);
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUsersTwits([FromQuery] int authorId, [FromQuery] int page = default)
    {
        var userId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User is not logged in");
        }
        
        var twits = await _twitsService.GetUsersTwits(authorId, page);
        
        return Ok(twits);
    }
    
    [HttpPost]
    public async Task<IActionResult> PostTwit([FromBody] string message)
    {
        var userId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized("User was not logged in");
        }

        var twit = await _twitsService.PostTwit(parsedUserId, message);
        
        HttpContext.Session.SetString("message", "Your message was recorded");

        return Ok(twit);
    }
}