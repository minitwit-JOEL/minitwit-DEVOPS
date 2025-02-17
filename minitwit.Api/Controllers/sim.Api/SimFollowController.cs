using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;
using minitwit.Application.Services;

namespace minitwit.Controllers;

[ApiController]
[Route("/")]
public class SimFollowController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ISimService _simService;

    public SimFollowController(IFollowService followService, ISimService simService)
    {
        _followService = followService;
        _simService = simService;
    }

    [HttpGet("fllws/{username}")]
    public async Task<IActionResult> GetUsersTwits(
        string username, 
        [FromBody] string unfollow,
        [FromBody] string follow)
    {
        HttpRequest request = HttpContext.Request;
        // The lines below is just for clarity, might want to delete it, and use "unfollow" directly
        string unfollows_username = unfollow;
        string follows_username = follow;
        /* 
           This function should contain the GET part of the 
           correponsing python function "messages_per_user" in
           "minitwit_sim_api.py". That is the first block of the
           if statement "if request.method == GET:"
        */

        throw new NotImplementedException();
    }

    [HttpPost("flls/{username}"), ActionName("GetUsersTwits")]
    public async Task<IActionResult> GetUsersTwitsPost(string username, [FromQuery] int no = 100)
    {
        HttpRequest request = HttpContext.Request;
        // The line below is just for clarity, might want to delete it, and use "no" directly
        int number_of_followers = no;
        /* 
           This function should contain the POST part of the 
           correponsing python function "messages_per_user" in
           "minitwit_sim_api.py". That is the first block of the
           if statement "elif request.method == POST:"
        */

        throw new NotImplementedException();
    }
}