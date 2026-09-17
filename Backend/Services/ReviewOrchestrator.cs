using Backend.Agents;
using Backend.Data;
using Backend.Models;

namespace Backend.Services;

public class ReviewOrchestrator
{
    private readonly CircuitAgent _circuitAgent;
    private readonly DatasheetAgent _datasheetAgent;
    private readonly ComplianceAgent _complianceAgent;
    private readonly ReviewerAgent _reviewerAgent;

    public ReviewOrchestrator(
        CircuitAgent circuitAgent,
        DatasheetAgent datasheetAgent,
        ComplianceAgent complianceAgent,
        ReviewerAgent reviewerAgent)
    {
        _circuitAgent = circuitAgent;
        _datasheetAgent = datasheetAgent;
        _complianceAgent = complianceAgent;
        _reviewerAgent = reviewerAgent;
    }

    public async Task<ReviewResult> RunAsync()
    {
        var design = MockData.GetDesign();

        var circuitResult = await _circuitAgent.AnalyzeAsync(design);
        var datasheetResult = await _datasheetAgent.AnalyzeAsync(design);
        var complianceResult = await _complianceAgent.AnalyzeAsync(design, circuitResult, datasheetResult);

        return await _reviewerAgent.ReviewAsync(design, circuitResult, datasheetResult, complianceResult);
    }
}
