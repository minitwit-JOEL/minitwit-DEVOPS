using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

[ApiController]
[Route("/")]
public class SimUserController : ControllerBase 
{
    private readonly IUserService _userService;

    public SimUserController (IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> PostTwit(
        [FromBody] string username, 
        [FromBody] string email, 
        [FromBody] string pwd)
    {
        HttpRequest request = HttpContext.Request;
        throw new NotImplementedException();
    }

}