using Microsoft.AspNetCore.Mvc;
using minitwit.Application.Interfaces.Sim;

namespace minitwit.Controllers.Sim;

[ApiController]
[Route("api/sim")]
public class SimController : ControllerBase
{
    private readonly ISimService _simService;

    public SimController(ISimService simService)
    {
        _simService = simService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestProcessedActionId()
    {
        var latestProcessedId = await _simService.GetLatestProcessedCommandId();
        return Ok(new { latest = latestProcessedId });
    }
}