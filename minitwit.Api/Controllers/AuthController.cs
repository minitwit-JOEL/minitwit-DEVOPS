using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using minitwit.Application.Services;
using LoginRequest = minitwit.Infrastructure.Dtos.Requests.LoginRequest;
using RegisterRequest = minitwit.Infrastructure.Dtos.Requests.RegisterRequest;

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
            return Unauthorized();
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userId = HttpContext.Session.GetString("user_id");

        // Redirect if already logged in
        if (!string.IsNullOrEmpty(userId))
        {
            return Ok();
        }

        string? error = null;

        // Form validation
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            error = "You have to enter a username";
        }
        else if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            error = "You have to enter a valid email address";
        }
        else if (string.IsNullOrWhiteSpace(request.Password))
        {
            error = "You have to enter a password";
        }
        else if (request.Password != request.ConfirmPassword)
        {
            error = "The two passwords do not match";
        }
        else if (_dbService.get_user_id(request.Username) != null)
        {
            error = "The username is already taken";
        }

        if (error != null)
        {
            return BadRequest(error);
        }

        // Hash the password using SHA1
        var utf8 = new UTF8Encoding();
        var sha1 = SHA1.Create();
        var passwordHash = utf8.GetString(sha1.ComputeHash(utf8.GetBytes(request.Password)));

        // Insert user into the database
        var query = @"
            INSERT INTO user (username, email, pw_hash)
            VALUES (@Username, @Email, @PwHash)";

        SqliteParameter[] parameters =
        [
            new SqliteParameter("@Username", request.Username),
            new SqliteParameter("@Email", request.Email),
            new SqliteParameter("@PwHash", passwordHash)
        ]; 
        
        _dbService.query_db(query, parameters);
        

        HttpContext.Session.SetString("message", "You were successfully registered and can login now");

        return Ok();
    }
}