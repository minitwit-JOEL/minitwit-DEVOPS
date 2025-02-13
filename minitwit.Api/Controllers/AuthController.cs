using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using LoginRequest = minitwit.Infrastructure.Dtos.Requests.LoginRequest;
using RegisterRequest = minitwit.Infrastructure.Dtos.Requests.RegisterRequest;

namespace minitwit.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userId = HttpContext.Session.GetString("user_id");

        if (!string.IsNullOrEmpty(userId))
        {
            return BadRequest("User is already logged in");
        }
        
        var user = await _authService.Login(request.Username, request.Password);
        HttpContext.Session.SetString("user_id", user.Id.ToString());
        return Ok();

    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = HttpContext.Session.GetString("user_id");

        if (!string.IsNullOrEmpty(userId))
        {
            return BadRequest("User is already logged out");
        }
        
        HttpContext.Session.Remove("user_id");
        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userId = HttpContext.Session.GetString("user_id");
        
        if (!string.IsNullOrEmpty(userId))
        {
            return Ok("User is already logged in");
        }
    
        await _authService.Register(request.Username, request.Email, request.Password, request.ConfirmPassword);
        
        HttpContext.Session.SetString("message", "You were successfully registered and can login now");

        return Ok("Successfully registered and can login now");
    }
}