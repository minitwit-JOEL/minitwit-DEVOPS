using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using minitwit.Application.Services;
using LoginRequest = minitwit.Infrastructure.Dtos.Requests.LoginRequest;

namespace minitwit.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly DbService _dbService;

    public AuthController(DbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var userId = HttpContext.Session.GetString("user_id");

        if (string.IsNullOrEmpty(userId))
        {
            return Ok("User already logged in");
        }
        
        var query = "SELECT * FROM user WHERE username = @Username";
        SqliteParameter[] parameters = [
            new SqliteParameter("@Username", request.Username), 
        ];
        List<Dictionary<string, string>> user = _dbService.query_db(query, parameters, true);

        if (user[0].Count == 0) 
        {
            return Unauthorized("Invalid username or password");
        }
    
        var utf8 = new UTF8Encoding();
        var sha1 = SHA1.Create();
        var hash = utf8.GetString(sha1.ComputeHash(utf8.GetBytes(request.Password)));
    
        if (hash != user[0]["password"])
        {
            return Unauthorized("Invalid username or password");
        }

        /* Note: Here the flask application first displays to the user
            that they were logged in and then redirects the user.
            This are possible because it is server-side rendered.
            For now, this is unimplemented in this version of the application. */

        HttpContext.Session.SetString("user_id", user[0]["user_id"]);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("user_id");
        return Ok();
    }
}