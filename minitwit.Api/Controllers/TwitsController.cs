using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using minitwit.Application.Services;

namespace minitwit.Controllers;

[ApiController]
[Route("api/twit")]
public class TwitsController : ControllerBase
{
    private readonly DbService _dbService;

    public TwitsController(DbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("public")]
    public async Task<IActionResult> GetPublicTwits()
    {
        var query = @"
            SELECT message.*, user.* 
            FROM message, user 
            WHERE message.author_id = user.user_id 
              AND message.flagged = 0 
            ORDER BY message.pub_date DESC 
            LIMIT @Per_page";

        SqliteParameter[] parameters = [
            new SqliteParameter("@Per_page", _dbService.PerPage)
        ];

        return Ok(_dbService.query_db(query, parameters));
    }
    
    [HttpGet("private")]
    public async Task<IActionResult> GetPrivateTwits()
    {
        Console.WriteLine("We got a visitor from: " + 
                          HttpContext.Connection.RemoteIpAddress);

        var userId = HttpContext.Session.GetString("user_id");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var query = @"
            SELECT message.*, user.* from message, user
            WHERE message.flagged = 0 AND 
            message.author_id = user.user_id AND (
            user.user_id = @User_id OR 
            user.user_id IN (SELECT whom_id FROM follower
            WHERE who_id = @User_id))
            ORDER BY message.pub_date DESC LIMIT @Per_page
        ";

        SqliteParameter[] parameters = [
            new SqliteParameter("@User_id", userId), 
            new SqliteParameter("@Per_page", _dbService.PerPage)
        ];

        return Ok(_dbService.query_db(query, parameters));
    }

    [HttpGet]
    public async Task<IActionResult> GetPublicUsers([FromQuery] string username)
    {
        var userList = _dbService.query_db(
            "SELECT * FROM user WHERE username = @Username",
            new[] { new SqliteParameter("@Username", username) },
            one: true);

        if (userList.Count == 0)
        {
            return BadRequest("User not found");
        }

        var profileUser = userList[0];

        var following = false;
        var currentUserId = HttpContext.Session.GetString("user_id");

        if (!string.IsNullOrEmpty(currentUserId))
        {
            var followList = _dbService.query_db(
                "SELECT 1 FROM follower WHERE who_id = @CurrentUserId AND whom_id = @ProfileUserId",
                new[]
                {
                    new SqliteParameter("@CurrentUserId", currentUserId),
                    new SqliteParameter("@ProfileUserId", profileUser["user_id"])
                },
                one: true);
            following = followList.Count > 0;
        }

        var messages = _dbService.query_db(@"
        SELECT message.*, user.* 
        FROM message, user 
        WHERE user.user_id = message.author_id 
          AND user.user_id = @ProfileUserId
        ORDER BY message.pub_date DESC 
        LIMIT @PerPage",
            new[]
            {
                new SqliteParameter("@ProfileUserId", profileUser["user_id"]),
                new SqliteParameter("@PerPage", _dbService.PerPage)
            });


        return Ok(new
        {
            profile_user = profileUser,
            followed = following,
            messages = messages
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> PostTwit([FromBody] string message)
    {
        // Check if user is authenticated
        var userId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User was not logged in");
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            var pubDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Insert the message into the database
            var query = @"
                INSERT INTO message (author_id, text, pub_date, flagged)
                VALUES (@AuthorId, @Text, @PubDate, 0)";

            SqliteParameter[] parameters = new[]
            {
                new SqliteParameter("@AuthorId", userId),
                new SqliteParameter("@Text", message),
                new SqliteParameter("@PubDate", pubDate),
            };
            
            _dbService.query_db(query, parameters);

            HttpContext.Session.SetString("message", "Your message was recorded");
        }

        return Ok();
    }
}