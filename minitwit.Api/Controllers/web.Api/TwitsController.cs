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

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestProcessedCommandId()
    {
        // Call the service to get the latest processed command ID
        var latestProcessedCommandId = await _twitsService.GetLatestProcessedCommandId();

        // Return the result in JSON format
        return Ok(new { latest = latestProcessedCommandId });
    }

    [HttpGet("msgs")]
    public async Task<IActionResult> GetMessages([FromQuery] int no = 100)
    {
        var messages = await _twitsService.GetMessages(no);
        return Ok(messages);
    }


    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetMessagesForUser(string username, [FromQuery] int noMsgs = 100)
    {
        try
        {
            // Get messages for the user from the service
            var messages = await _twitsService.GetMessagesForUser(username, noMsgs);

            // Return the list of messages
            return Ok(messages);
        }
        catch (ArgumentException)
        {
            // If user not found, return 404
            return NotFound("User not found");
        }
        catch (Exception)
        {
            // Handle other unexpected errors
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [Authorize]
    [HttpPost("msgs/{username}")]
    public async Task<IActionResult> PostMessageForUser(string username, [FromBody] string content)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized("User is not logged in");
            }

            // Ensure that the logged-in user is the same as the one in the URL
            if (username != User.FindFirstValue(ClaimTypes.Name))
            {
                return Unauthorized("You can only post messages as yourself");
            }

            // Call the service to post the message for the user
            await _twitsService.PostMessagesForUser(username, content);

            // Return 204 No Content on successful post
            return NoContent();
        }
        catch (ArgumentException)
        {
            // If user not found, return 404
            return NotFound("User not found");
        }
        catch (Exception)
        {
            // Handle other unexpected errors
            return StatusCode(500, "An unexpected error occurred");
        }
    }

}