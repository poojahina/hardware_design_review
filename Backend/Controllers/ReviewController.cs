using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ReviewOrchestrator _orchestrator;

    public ReviewController(ReviewOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpGet("design")]
    public IActionResult GetDesign()
    {
        return Ok(MockData.GetDesign());
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze()
    {
        var result = await _orchestrator.RunAsync();
        return Ok(result);
    }
}
