using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

        var user = await _authService.Login(request.Username, request.Password);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var jwtKey = "jg4mywvyYnFJgJhLT+6AmMllIL4t/86qY75kt42HRmV4=";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = "Oliver",
            Audience = "Oliver",
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            token = tokenString
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User is already logged out");
        }

        return Ok();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var user = await _authService.Register(request.Username, request.Email, request.Password, request.ConfirmPassword);
            return NoContent(); // 204 No Content
        }
        catch (ArgumentException ex)
        {
            var errorResponse = new { status = 400, error_msg = ex.Message };
            return BadRequest(errorResponse);
        }
        catch (Exception e)
        {
            var errorResponse = new { status = 500, error_msg = "An unexpected error occurred." };
            return StatusCode(500, errorResponse);
        }
    }
}
