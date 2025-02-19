using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

[ApiController]
[Route("/")]
public class SimUserController : ControllerBase
{
    private readonly IUserService _userService;

    public SimUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> PostTwit(
        [FromBody] User user)
    {
        HttpRequest request = HttpContext.Request;
        throw new NotImplementedException();
    }

    public record User(string username, string email, string pwd);

}