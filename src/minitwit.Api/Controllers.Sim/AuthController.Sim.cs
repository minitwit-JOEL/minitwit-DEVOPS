using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Infrastructure.Dtos.Sim;
using IAuthService = minitwit.Application.Interfaces.Sim.IAuthService;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("api/sim")]
public class AuthControllerSim : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthControllerSim(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromQuery] int latest = -1)
    {
        var result = await _authService.Register(latest, request.Username, request.Email, request.Pwd);

        if (!result.IsSuccess)
        {
            return BadRequest(new { status = 400, error_msg = result.ErrorMessage });
        }

        return NoContent();
    }
}