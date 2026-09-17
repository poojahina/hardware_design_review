namespace Backend.Models;

public record HardwareDesign(
    string Name,
    string Purpose,
    IReadOnlyList<Component> Components,
    IReadOnlyList<NetConnection> Netlist,
    IReadOnlyList<Datasheet> Datasheets,
    IReadOnlyList<EngineeringRule> EngineeringRules);

public record Component(string Reference, string Name, string Type, string? Value);

public record NetConnection(string From, string To);

public record Datasheet(string Component, IReadOnlyList<DatasheetConstraint> Constraints);

public record DatasheetConstraint(
    string Type,
    double? Min = null,
    double? Max = null,
    string? Required = null,
    string? ApprovedValue = null);

public record EngineeringRule(string Code, string Description);

public record CircuitAnalysisResult(
    string CircuitType,
    IReadOnlyList<string> PowerFlow,
    IReadOnlyList<string> SignalFlow,
    IReadOnlyList<string> Observations,
    string LlmReasoning);

public record DatasheetAnalysisResult(
    IReadOnlyList<ExtractedConstraint> Constraints,
    string LlmReasoning);

public record ExtractedConstraint(
    string Component,
    string Type,
    double? Min = null,
    double? Max = null,
    string? Required = null,
    string? ApprovedValue = null);

public record ComplianceResult(IReadOnlyList<Finding> Findings, string LlmReasoning);

public record Finding(
    string Component,
    string Title,
    string Issue,
    string Severity,
    string Expected,
    string Detected,
    string Evidence,
    string Rule,
    string Reason,
    string Recommendation,
    IReadOnlyList<TraceStep> Traceability);

public record TraceStep(string Stage, string Detail);

public record ReviewResult(
    string OverallStatus,
    int Score,
    string Summary,
    DesignMetrics Metrics,
    IReadOnlyList<Finding> Findings);

public record DesignMetrics(
    int Components,
    int Nets,
    int Datasheets,
    int Findings,
    int Critical,
    int High,
    int Medium);
