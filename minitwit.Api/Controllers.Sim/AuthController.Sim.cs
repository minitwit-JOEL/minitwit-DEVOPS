using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Infrastructure.Dtos.Sim;
using IAuthService = minitwit.Application.Interfaces.Sim.IAuthService;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("/")]
public class AuthControllerSim : ControllerBase
{
    private readonly IAuthService _authService;
    
    public AuthControllerSim(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.Register(request.Username, request.Email, request.Password);

        if (!result.IsSuccess)
        {
            return BadRequest(new { status = 400, error_msg = result.ErrorMessage });
        }

        return NoContent();
    }
}