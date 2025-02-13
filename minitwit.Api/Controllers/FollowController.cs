using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using minitwit.Application.Services;

namespace minitwit.Controllers;

public class FollowController : ControllerBase
{
    private readonly DbService _dbService;

    public FollowController(DbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost("{username}/follow")]
    public async Task<IActionResult> Follow(string username)
    {
        var currentUserId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        long? otherId = _dbService.get_user_id(username);
        if (otherId is null)
        {
            return BadRequest("No user found with the given username");
        }
        
        var query = "INSERT INTO follower (who_id, whom_id) VALUES (@CurrentUserId, @WhomId)";

        SqliteParameter[] parameters = new[]
        {
            new SqliteParameter("CurrentUserId", currentUserId),
            new SqliteParameter("WhomId", otherId)
        };
       
        _dbService.query_db(query, parameters);

        // we could return a json msg here? 
        return Ok();
    }

    [HttpPost("{username}/unfollow")]
    public async Task<IActionResult> Unfollow(string username)
    {
        var currentUserId = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized();
        }

        long? whomId = _dbService.get_user_id(username);

        if (whomId is null)
        {
            return BadRequest("No user found with the given username");
        }

        var query = "DELETE FROM follower WHERE who_id = @CurrentUserId AND whom_id = @WhomId";

        SqliteParameter[] parameters = new[]
        {
            new SqliteParameter("CurrentUserId", currentUserId),
            new SqliteParameter("WhomId", whomId)
        };

        // again we could send a msg here? 
        return Ok();
    }
}