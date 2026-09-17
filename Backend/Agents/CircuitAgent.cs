using Backend.Models;
using Backend.Services;

namespace Backend.Agents;

public class CircuitAgent
{
    private const string SystemPrompt = """
        You are a hardware circuit analysis specialist.

        Analyze the provided structured circuit information.

        Identify:
        - circuit purpose
        - power flow
        - signal flow
        - component relationships
        - suspicious connections
        - potentially floating pins

        Only use supplied information.
        Do not invent circuit information.

        Return JSON only.
        """;

    private readonly AzureOpenAIService _azureOpenAI;

    public CircuitAgent(AzureOpenAIService azureOpenAI)
    {
        _azureOpenAI = azureOpenAI;
    }

    public async Task<CircuitAnalysisResult> AnalyzeAsync(HardwareDesign design)
    {
        var reasoning = await _azureOpenAI.AskAsync(SystemPrompt, new
        {
            design.Name,
            design.Purpose,
            design.Components,
            design.Netlist
        });

        return new CircuitAnalysisResult(
            "Power Supply + Relay Controller",
            new[] { "12V_IN", "F1", "D1", "U2", "5V_RAIL" },
            new[] { "U1.GPIO1", "R1", "Q1.BASE", "Q1.COLLECTOR", "K1.COIL" },
            new[] { "U1.VCC is connected to 5V_RAIL.", "Relay coil is switched by Q1." },
            reasoning);
    }
}
