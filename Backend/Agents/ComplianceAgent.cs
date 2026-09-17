using Backend.Models;
using Backend.Services;

namespace Backend.Agents;

public class ComplianceAgent
{
    private readonly AzureOpenAIService _azureOpenAI;

    public ComplianceAgent(AzureOpenAIService azureOpenAI)
    {
        _azureOpenAI = azureOpenAI;
    }

    public async Task<ComplianceResult> AnalyzeAsync(
        HardwareDesign design,
        CircuitAnalysisResult circuitResult,
        DatasheetAnalysisResult datasheetResult)
    {
        var findings = new List<Finding>();

        CheckU1Voltage(design, datasheetResult, findings);
        CheckU1Decoupling(design, datasheetResult, findings);
        CheckR1Value(design, datasheetResult, findings);
        CheckRelayFlyback(design, datasheetResult, findings);

        var reasoning = await _azureOpenAI.AskAsync(
            "You are a hardware compliance specialist. Explain the supplied deterministic findings using only supplied evidence. Return JSON only.",
            new { design, circuitResult, datasheetResult, findings });

        return new ComplianceResult(findings, reasoning);
    }

    private static void CheckU1Voltage(HardwareDesign design, DatasheetAnalysisResult datasheetResult, List<Finding> findings)
    {
        var constraint = datasheetResult.Constraints.First(c => c.Component == "U1" && c.Type == "OperatingVoltage");
        var connection = design.Netlist.FirstOrDefault(n => n.From == "5V_RAIL" && n.To == "U1.VCC");
        var detectedVoltage = 5.0;

        if (connection is not null && constraint.Max.HasValue && detectedVoltage > constraint.Max.Value)
        {
            findings.Add(new Finding(
                "U1",
                "Voltage Domain Violation",
                "U1 supply voltage exceeds the datasheet operating voltage range.",
                "CRITICAL",
                $"{constraint.Min:0.0}V - {constraint.Max:0.0}V",
                "5V",
                "U1.VCC -> 5V_RAIL",
                "PWR001",
                "The U1 supply connection exceeds the supplied operating voltage constraint.",
                "Review the U1 power supply rail or component selection.",
                new[]
                {
                    new TraceStep("Circuit", "U1.VCC"),
                    new TraceStep("Netlist", "5V_RAIL"),
                    new TraceStep("Datasheet Agent", "Max Voltage = 3.6V"),
                    new TraceStep("Compliance Agent", "PWR001 violated"),
                    new TraceStep("Reviewer Agent", "CRITICAL")
                }));
        }
    }

    private static void CheckU1Decoupling(HardwareDesign design, DatasheetAnalysisResult datasheetResult, List<Finding> findings)
    {
        var constraint = datasheetResult.Constraints.First(c => c.Component == "U1" && c.Type == "Decoupling");
        var hasDecouplingBetweenU1PowerAndGround = design.Netlist.Any(n => n.From.Contains("C", StringComparison.OrdinalIgnoreCase) && n.To == "U1.VCC");

        if (!hasDecouplingBetweenU1PowerAndGround)
        {
            findings.Add(new Finding(
                "U1",
                "Missing Decoupling Capacitor",
                "U1 requires local decoupling between VCC and GND.",
                "HIGH",
                constraint.Required ?? "100nF between VCC and GND",
                "Missing",
                "U1.VCC -> 5V_RAIL and no capacitor is connected between U1.VCC and GND in the supplied netlist.",
                "DEC001",
                "The mock netlist does not show the required U1 decoupling capacitor.",
                "Add or verify a 100nF capacitor between U1.VCC and GND.",
                new[]
                {
                    new TraceStep("Circuit", "U1.VCC / U1.GND"),
                    new TraceStep("Datasheet Agent", "100nF between VCC and GND"),
                    new TraceStep("Netlist", "No matching capacitor connection"),
                    new TraceStep("Compliance Agent", "DEC001 violated"),
                    new TraceStep("Reviewer Agent", "HIGH")
                }));
        }
    }

    private static void CheckR1Value(HardwareDesign design, DatasheetAnalysisResult datasheetResult, List<Finding> findings)
    {
        var expected = datasheetResult.Constraints.First(c => c.Component == "R1" && c.Type == "ApprovedDesignValue").ApprovedValue;
        var actual = design.Components.First(c => c.Reference == "R1").Value;

        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(new Finding(
                "R1",
                "Incorrect Component Value",
                "R1 value does not match the approved design value.",
                "MEDIUM",
                expected ?? "10kOhm",
                actual ?? "Unknown",
                "R1 component value = 1kOhm",
                "VAL001",
                "The component value differs from the supplied approved design value.",
                "Update R1 to the approved value or revise the approved design constraint.",
                new[]
                {
                    new TraceStep("Circuit", "R1"),
                    new TraceStep("Component Value", "1kOhm"),
                    new TraceStep("Datasheet Agent", "Approved Value = 10kOhm"),
                    new TraceStep("Compliance Agent", "VAL001 violated"),
                    new TraceStep("Reviewer Agent", "MEDIUM")
                }));
        }
    }

    private static void CheckRelayFlyback(HardwareDesign design, DatasheetAnalysisResult datasheetResult, List<Finding> findings)
    {
        var constraint = datasheetResult.Constraints.First(c => c.Component == "K1" && c.Type == "FlybackProtection");
        var hasFlybackDiodeAcrossCoil = design.Netlist.Any(n =>
            (n.From.Contains("D", StringComparison.OrdinalIgnoreCase) || n.To.Contains("D", StringComparison.OrdinalIgnoreCase)) &&
            (n.From.Contains("K1.COIL", StringComparison.OrdinalIgnoreCase) || n.To.Contains("K1.COIL", StringComparison.OrdinalIgnoreCase)));

        if (!hasFlybackDiodeAcrossCoil)
        {
            findings.Add(new Finding(
                "K1",
                "Missing Relay Flyback Protection",
                "Relay coil does not show flyback protection in the supplied netlist.",
                "HIGH",
                "Flyback diode",
                "Missing",
                "K1.COIL connects to Q1.COLLECTOR and 5V_RAIL; no diode is connected across K1.COIL.",
                "REL001",
                constraint.Required ?? "Flyback diode is recommended across the relay coil.",
                "Add a flyback diode across the relay coil and verify polarity.",
                new[]
                {
                    new TraceStep("Circuit", "K1.COIL"),
                    new TraceStep("Netlist", "Q1.COLLECTOR -> K1.COIL -> 5V_RAIL"),
                    new TraceStep("Datasheet Agent", "Flyback diode recommended"),
                    new TraceStep("Compliance Agent", "REL001 violated"),
                    new TraceStep("Reviewer Agent", "HIGH")
                }));
        }
    }
}
