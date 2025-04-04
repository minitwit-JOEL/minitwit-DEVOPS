using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers.Api;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No user found");
        }

        var user = await _userService.GetLoggedInUser(int.Parse(userId));

        return Ok(new
        {
            id = user!.Id,
            username = user.Username,
            email = user.Email,
        });
    }
    
    [Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult> GetUser(string username)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No user found");
        }

        var user = await _userService.GetUserByUsernameAsync(username);

        return Ok(new
        {
            id = user!.Id,
            username = user.Username,
            email = user.Email,
        });
    }
}