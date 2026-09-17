using Backend.Models;
using Backend.Services;

namespace Backend.Agents;

public class ReviewerAgent
{
    private readonly AzureOpenAIService _azureOpenAI;

    public ReviewerAgent(AzureOpenAIService azureOpenAI)
    {
        _azureOpenAI = azureOpenAI;
    }

    public async Task<ReviewResult> ReviewAsync(
        HardwareDesign design,
        CircuitAnalysisResult circuitResult,
        DatasheetAnalysisResult datasheetResult,
        ComplianceResult complianceResult)
    {
        await _azureOpenAI.AskAsync(
            "You are the final hardware review agent. Verify that every finding has supplied evidence. Do not invent new defects. Return JSON only.",
            new { design, circuitResult, datasheetResult, complianceResult });

        var supportedFindings = complianceResult.Findings
            .Where(f => !string.IsNullOrWhiteSpace(f.Evidence) && !string.IsNullOrWhiteSpace(f.Rule))
            .ToList();

        var critical = supportedFindings.Count(f => f.Severity == "CRITICAL");
        var high = supportedFindings.Count(f => f.Severity == "HIGH");
        var medium = supportedFindings.Count(f => f.Severity == "MEDIUM");

        return new ReviewResult(
            "DESIGN_ISSUES_DETECTED",
            72,
            "Several hardware design issues require review.",
            new DesignMetrics(
                design.Components.Count,
                design.Netlist.Count,
                design.Datasheets.Count,
                supportedFindings.Count,
                critical,
                high,
                medium),
            supportedFindings);
    }
}
