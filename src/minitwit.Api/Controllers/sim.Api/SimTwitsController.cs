using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces;

namespace minitwit.Controllers;

[ApiController]
[Route("/")]
public class SimTwitsController : ControllerBase
{
    private readonly ITwitsService _twitsService;
    private readonly ISimService _simSerice;

    public SimTwitsController (ITwitsService twitsService, ISimService simService) 
    {
        _twitsService = twitsService;
        _simSerice = simService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest() 
    {
        throw new NotImplementedException();
    }

    [HttpGet("msgs")]
    public async Task<IActionResult> GetPublicTwits() 
    {
        HttpRequest request = HttpContext.Request;
        throw new NotImplementedException();    
    }


    [HttpGet("msgs/{username}")]
    public async Task<IActionResult> GetUsersTwits(string username)
    {
        HttpRequest request = HttpContext.Request;
        /* 
           This function should contain the GET part of the 
           correponsing python function "messages_per_user" in
           "minitwit_sim_api.py". That is the first block of the
           if statement "if request.method == GET:"
        */

        throw new NotImplementedException();
    }

    [HttpPost("msgs/{username}"), ActionName("GetUsersTwits")]
    public async Task<IActionResult> GetUsersTwitsPost(string username, [FromBody] string content)
    {
        HttpRequest request = HttpContext.Request;
        /* 
           This function should contain the POST part of the 
           correponsing python function "messages_per_user" in
           "minitwit_sim_api.py". That is the first block of the
           if statement "elif request.method == POST:"
        */

        throw new NotImplementedException();
    }
}