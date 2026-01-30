using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Core.Progress;

/// <summary>
/// The phases of complexity analysis.
/// </summary>
public enum AnalysisPhase
{
    /// <summary>
    /// Phase A: Static complexity extraction from AST/CFG.
    /// </summary>
    StaticExtraction,

    /// <summary>
    /// Phase B: Solving recurrence relations.
    /// </summary>
    RecurrenceSolving,

    /// <summary>
    /// Phase C: Refinement via slack variables and perturbation.
    /// </summary>
    Refinement,

    /// <summary>
    /// Phase D: Speculative analysis for partial code.
    /// </summary>
    SpeculativeAnalysis,

    /// <summary>
    /// Phase E: Hardware calibration and weight adjustment.
    /// </summary>
    Calibration
}

/// <summary>
/// Callback interface for receiving progress updates during complexity analysis.
/// Enables real-time feedback, logging, and early termination detection.
/// </summary>
public interface IAnalysisProgress
{
    /// <summary>
    /// Called when an analysis phase begins.
    /// </summary>
    void OnPhaseStarted(AnalysisPhase phase);

    /// <summary>
    /// Called when an analysis phase completes.
    /// </summary>
    void OnPhaseCompleted(AnalysisPhase phase, PhaseResult result);

    /// <summary>
    /// Called when a method's complexity has been analyzed.
    /// </summary>
    void OnMethodAnalyzed(MethodComplexityResult result);

    /// <summary>
    /// Called with intermediate results during analysis.
    /// </summary>
    void OnIntermediateResult(PartialComplexityResult result);

    /// <summary>
    /// Called when a recurrence relation is detected.
    /// </summary>
    void OnRecurrenceDetected(RecurrenceDetectionResult result);

    /// <summary>
    /// Called when a recurrence relation has been solved.
    /// </summary>
    void OnRecurrenceSolved(RecurrenceSolutionResult result);

    /// <summary>
    /// Called when a warning or issue is encountered.
    /// </summary>
    void OnWarning(AnalysisWarning warning);

    /// <summary>
    /// Called periodically with overall progress percentage.
    /// </summary>
    void OnProgressUpdated(double percentComplete, string? statusMessage = null);
}

/// <summary>
/// Result of a completed analysis phase.
/// </summary>
public record PhaseResult
{
    /// <summary>
    /// The phase that completed.
    /// </summary>
    public required AnalysisPhase Phase { get; init; }

    /// <summary>
    /// Whether the phase completed successfully.
    /// </summary>
    public bool Success { get; init; } = true;

    /// <summary>
    /// Duration of the phase.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Number of items processed in this phase.
    /// </summary>
    public int ItemsProcessed { get; init; }

    /// <summary>
    /// Optional error message if the phase failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata about the phase result.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of analyzing a single method's complexity.
/// </summary>
public record MethodComplexityResult
{
    /// <summary>
    /// The fully qualified name of the method.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// The file path containing the method.
    /// </summary>
    public string? FilePath { get; init; }

    /// <summary>
    /// Line number where the method is defined.
    /// </summary>
    public int? LineNumber { get; init; }

    /// <summary>
    /// The computed time complexity.
    /// </summary>
    public required ComplexityExpression TimeComplexity { get; init; }

    /// <summary>
    /// The computed space complexity (if available).
    /// </summary>
    public ComplexityExpression? SpaceComplexity { get; init; }

    /// <summary>
    /// Confidence level in the result (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Whether this result requires human review.
    /// </summary>
    public bool RequiresReview { get; init; }

    /// <summary>
    /// Reason for requiring review (if applicable).
    /// </summary>
    public string? ReviewReason { get; init; }
}

/// <summary>
/// Intermediate complexity result during analysis.
/// </summary>
public record PartialComplexityResult
{
    /// <summary>
    /// Description of what was analyzed.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The partial complexity expression.
    /// </summary>
    public required ComplexityExpression Complexity { get; init; }

    /// <summary>
    /// Whether this is a complete or partial result.
    /// </summary>
    public bool IsComplete { get; init; }

    /// <summary>
    /// Context about where this result comes from.
    /// </summary>
    public string? Context { get; init; }
}

/// <summary>
/// Result when a recurrence relation is detected.
/// </summary>
public record RecurrenceDetectionResult
{
    /// <summary>
    /// The method containing the recurrence.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// The detected recurrence pattern.
    /// </summary>
    public required RecurrenceComplexity Recurrence { get; init; }

    /// <summary>
    /// Type of recurrence detected.
    /// </summary>
    public RecurrenceType Type { get; init; }

    /// <summary>
    /// Whether this recurrence can be solved analytically.
    /// </summary>
    public bool IsSolvable { get; init; }

    /// <summary>
    /// Recommended solving approach.
    /// </summary>
    public SolvingApproach RecommendedApproach { get; init; }
}

/// <summary>
/// Types of recurrence relations.
/// </summary>
public enum RecurrenceType
{
    /// <summary>
    /// Linear recursion: T(n) = T(n-1) + f(n).
    /// </summary>
    Linear,

    /// <summary>
    /// Divide and conquer: T(n) = a·T(n/b) + f(n).
    /// </summary>
    DivideAndConquer,

    /// <summary>
    /// Multi-term: T(n) = Σᵢ aᵢ·T(bᵢ·n) + f(n).
    /// </summary>
    MultiTerm,

    /// <summary>
    /// Mutual recursion between multiple functions.
    /// </summary>
    Mutual,

    /// <summary>
    /// Non-standard recurrence requiring special handling.
    /// </summary>
    NonStandard
}

/// <summary>
/// Approaches for solving recurrence relations.
/// </summary>
public enum SolvingApproach
{
    /// <summary>
    /// Master Theorem for standard divide-and-conquer.
    /// </summary>
    MasterTheorem,

    /// <summary>
    /// Akra-Bazzi theorem for general multi-term recurrences.
    /// </summary>
    AkraBazzi,

    /// <summary>
    /// Direct expansion/substitution.
    /// </summary>
    Expansion,

    /// <summary>
    /// Numerical approximation.
    /// </summary>
    Numerical,

    /// <summary>
    /// Cannot be solved analytically.
    /// </summary>
    Unsolvable
}

/// <summary>
/// Result of solving a recurrence relation.
/// </summary>
public record RecurrenceSolutionResult
{
    /// <summary>
    /// The input recurrence that was solved.
    /// </summary>
    public required RecurrenceComplexity Input { get; init; }

    /// <summary>
    /// The closed-form solution.
    /// </summary>
    public required ComplexityExpression Solution { get; init; }

    /// <summary>
    /// The approach used to solve it.
    /// </summary>
    public required SolvingApproach ApproachUsed { get; init; }

    /// <summary>
    /// Confidence in the solution.
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Whether the solution is exact or an approximation.
    /// </summary>
    public bool IsExact { get; init; } = true;

    /// <summary>
    /// Additional notes about the solution.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Warning encountered during analysis.
/// </summary>
public record AnalysisWarning
{
    /// <summary>
    /// Unique warning code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable warning message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Severity of the warning.
    /// </summary>
    public WarningSeverity Severity { get; init; } = WarningSeverity.Warning;

    /// <summary>
    /// Location in source code (if applicable).
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Suggested action to resolve the warning.
    /// </summary>
    public string? SuggestedAction { get; init; }
}

/// <summary>
/// Severity levels for analysis warnings.
/// </summary>
public enum WarningSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Info,

    /// <summary>
    /// Warning that may affect accuracy.
    /// </summary>
    Warning,

    /// <summary>
    /// Error that prevents accurate analysis.
    /// </summary>
    Error
}

/// <summary>
/// Null implementation of IAnalysisProgress that ignores all callbacks.
/// </summary>
public sealed class NullAnalysisProgress : IAnalysisProgress
{
    public static readonly NullAnalysisProgress Instance = new();

    private NullAnalysisProgress() { }

    public void OnPhaseStarted(AnalysisPhase phase) { }
    public void OnPhaseCompleted(AnalysisPhase phase, PhaseResult result) { }
    public void OnMethodAnalyzed(MethodComplexityResult result) { }
    public void OnIntermediateResult(PartialComplexityResult result) { }
    public void OnRecurrenceDetected(RecurrenceDetectionResult result) { }
    public void OnRecurrenceSolved(RecurrenceSolutionResult result) { }
    public void OnWarning(AnalysisWarning warning) { }
    public void OnProgressUpdated(double percentComplete, string? statusMessage = null) { }
}

/// <summary>
/// Aggregates multiple progress handlers.
/// </summary>
public sealed class CompositeAnalysisProgress : IAnalysisProgress
{
    private readonly IReadOnlyList<IAnalysisProgress> _handlers;

    public CompositeAnalysisProgress(params IAnalysisProgress[] handlers) =>
        _handlers = handlers;

    public CompositeAnalysisProgress(IEnumerable<IAnalysisProgress> handlers) =>
        _handlers = handlers.ToList();

    public void OnPhaseStarted(AnalysisPhase phase)
    {
        foreach (var handler in _handlers)
            handler.OnPhaseStarted(phase);
    }

    public void OnPhaseCompleted(AnalysisPhase phase, PhaseResult result)
    {
        foreach (var handler in _handlers)
            handler.OnPhaseCompleted(phase, result);
    }

    public void OnMethodAnalyzed(MethodComplexityResult result)
    {
        foreach (var handler in _handlers)
            handler.OnMethodAnalyzed(result);
    }

    public void OnIntermediateResult(PartialComplexityResult result)
    {
        foreach (var handler in _handlers)
            handler.OnIntermediateResult(result);
    }

    public void OnRecurrenceDetected(RecurrenceDetectionResult result)
    {
        foreach (var handler in _handlers)
            handler.OnRecurrenceDetected(result);
    }

    public void OnRecurrenceSolved(RecurrenceSolutionResult result)
    {
        foreach (var handler in _handlers)
            handler.OnRecurrenceSolved(result);
    }

    public void OnWarning(AnalysisWarning warning)
    {
        foreach (var handler in _handlers)
            handler.OnWarning(warning);
    }

    public void OnProgressUpdated(double percentComplete, string? statusMessage = null)
    {
        foreach (var handler in _handlers)
            handler.OnProgressUpdated(percentComplete, statusMessage);
    }
}

/// <summary>
/// Logs progress to console output.
/// </summary>
public sealed class ConsoleAnalysisProgress : IAnalysisProgress
{
    private readonly bool _verbose;

    public ConsoleAnalysisProgress(bool verbose = false) => _verbose = verbose;

    public void OnPhaseStarted(AnalysisPhase phase) =>
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting phase: {phase}");

    public void OnPhaseCompleted(AnalysisPhase phase, PhaseResult result)
    {
        var status = result.Success ? "completed" : "failed";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Phase {phase} {status} in {result.Duration.TotalMilliseconds:F0}ms ({result.ItemsProcessed} items)");
        if (!result.Success && result.ErrorMessage != null)
            Console.WriteLine($"  Error: {result.ErrorMessage}");
    }

    public void OnMethodAnalyzed(MethodComplexityResult result)
    {
        if (_verbose)
            Console.WriteLine($"  {result.MethodName}: {result.TimeComplexity.ToBigONotation()}");
    }

    public void OnIntermediateResult(PartialComplexityResult result)
    {
        if (_verbose)
            Console.WriteLine($"  Intermediate: {result.Description} = {result.Complexity.ToBigONotation()}");
    }

    public void OnRecurrenceDetected(RecurrenceDetectionResult result) =>
        Console.WriteLine($"  Recurrence detected in {result.MethodName}: {result.Recurrence.ToBigONotation()}");

    public void OnRecurrenceSolved(RecurrenceSolutionResult result) =>
        Console.WriteLine($"  Solved: {result.Solution.ToBigONotation()} (via {result.ApproachUsed})");

    public void OnWarning(AnalysisWarning warning)
    {
        var prefix = warning.Severity switch
        {
            WarningSeverity.Info => "INFO",
            WarningSeverity.Warning => "WARN",
            WarningSeverity.Error => "ERROR",
            _ => "????"
        };
        Console.WriteLine($"  [{prefix}] {warning.Code}: {warning.Message}");
    }

    public void OnProgressUpdated(double percentComplete, string? statusMessage = null)
    {
        if (_verbose || percentComplete % 10 < 1)
            Console.WriteLine($"  Progress: {percentComplete:F0}% {statusMessage ?? ""}");
    }
}
