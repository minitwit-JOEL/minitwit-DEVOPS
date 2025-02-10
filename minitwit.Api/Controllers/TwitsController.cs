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
    
    [HttpGet]
    public async Task<IActionResult> GetTwits()
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
}