using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ReviewOrchestrator _orchestrator;
    private readonly AzureOpenAIService _azureOpenAI;

    public ReviewController(ReviewOrchestrator orchestrator, AzureOpenAIService azureOpenAI)
    {
        _orchestrator = orchestrator;
        _azureOpenAI = azureOpenAI;
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

    [HttpPost("analyze-image")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> AnalyzeImage(IFormFile image)
    {
        if (image.Length == 0)
        {
            return BadRequest("Upload a schematic image file.");
        }

        if (!image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only image files are supported.");
        }

        var result = await _azureOpenAI.AnalyzeSchematicImageAsync(image, MockData.GetDesign());
        return Ok(result);
    }
}
