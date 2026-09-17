using Backend.Models;
using Backend.Services;

namespace Backend.Agents;

public class DatasheetAgent
{
    private const string SystemPrompt = """
        You are a hardware component datasheet specialist.

        Extract only design-relevant engineering constraints
        from the supplied mock datasheet information.

        Do not add requirements that are not present.

        Return JSON only.
        """;

    private readonly AzureOpenAIService _azureOpenAI;

    public DatasheetAgent(AzureOpenAIService azureOpenAI)
    {
        _azureOpenAI = azureOpenAI;
    }

    public async Task<DatasheetAnalysisResult> AnalyzeAsync(HardwareDesign design)
    {
        var reasoning = await _azureOpenAI.AskAsync(SystemPrompt, new
        {
            design.Components,
            design.Datasheets
        });

        var constraints = design.Datasheets
            .SelectMany(datasheet => datasheet.Constraints.Select(constraint =>
                new ExtractedConstraint(
                    datasheet.Component,
                    constraint.Type,
                    constraint.Min,
                    constraint.Max,
                    constraint.Required,
                    constraint.ApprovedValue)))
            .ToList();

        return new DatasheetAnalysisResult(constraints, reasoning);
    }
}
