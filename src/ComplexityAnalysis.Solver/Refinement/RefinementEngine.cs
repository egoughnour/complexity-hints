using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Progress;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver.Refinement;

/// <summary>
/// Main refinement engine that coordinates all refinement components.
/// Implements Phase C of the complexity analysis pipeline.
///
/// Pipeline:
/// 1. Receive initial solution from theorem solver (Phase B)
/// 2. Detect boundary cases and apply perturbation expansion
/// 3. Optimize slack variables for tighter bounds
/// 4. Verify via induction
/// 5. Compute confidence score
/// </summary>
public sealed class RefinementEngine : IRefinementEngine
{
    private readonly ISlackVariableOptimizer _slackOptimizer;
    private readonly IPerturbationExpansion _perturbation;
    private readonly IInductionVerifier _verifier;
    private readonly IConfidenceScorer _scorer;

    public RefinementEngine(
        ISlackVariableOptimizer? slackOptimizer = null,
        IPerturbationExpansion? perturbation = null,
        IInductionVerifier? verifier = null,
        IConfidenceScorer? scorer = null)
    {
        _slackOptimizer = slackOptimizer ?? SlackVariableOptimizer.Instance;
        _perturbation = perturbation ?? PerturbationExpansion.Instance;
        _verifier = verifier ?? InductionVerifier.Instance;
        _scorer = scorer ?? ConfidenceScorer.Instance;
    }

    public static RefinementEngine Instance { get; } = new();

    /// <summary>
    /// Refines a complexity solution through the full pipeline.
    /// </summary>
    public RefinementPipelineResult Refine(
        RecurrenceRelation recurrence,
        TheoremApplicability theoremResult,
        IAnalysisProgress? progress = null)
    {
        var diagnostics = new List<string>();
        var stages = new List<RefinementStage>();

        progress?.OnPhaseStarted(AnalysisPhase.Refinement);

        if (!theoremResult.IsApplicable || theoremResult.Solution is null)
        {
            progress?.OnPhaseCompleted(AnalysisPhase.Refinement, new PhaseResult
            {
                Phase = AnalysisPhase.Refinement,
                Success = false,
                ErrorMessage = "No applicable theorem result to refine"
            });

            return new RefinementPipelineResult
            {
                Success = false,
                ErrorMessage = "Cannot refine non-applicable theorem result"
            };
        }

        var currentSolution = theoremResult.Solution;
        var variable = recurrence.Variable;

        // Stage 1: Boundary case detection and perturbation
        progress?.OnProgressUpdated(10, "Detecting boundary cases");
        var boundaryCase = _perturbation.DetectBoundary(recurrence, theoremResult);

        if (boundaryCase is not null)
        {
            diagnostics.Add($"Boundary case detected: {boundaryCase.Description}");

            var perturbResult = _perturbation.ExpandNearBoundary(recurrence, boundaryCase);
            stages.Add(new RefinementStage
            {
                Name = "Perturbation Expansion",
                InputSolution = currentSolution,
                OutputSolution = perturbResult.Solution ?? currentSolution,
                Confidence = perturbResult.Confidence,
                Success = perturbResult.Success,
                Diagnostics = perturbResult.Diagnostics
            });

            if (perturbResult.Success && perturbResult.Solution is not null)
            {
                currentSolution = perturbResult.Solution;
                diagnostics.Add($"Applied perturbation: {perturbResult.Method}");
            }
        }

        // Stage 2: Slack variable optimization
        progress?.OnProgressUpdated(30, "Optimizing slack variables");
        // Note: RefineRecurrence uses theoremResult.Solution as starting point
        var slackResult = _slackOptimizer.RefineRecurrence(recurrence, theoremResult);

        stages.Add(new RefinementStage
        {
            Name = "Slack Variable Optimization",
            InputSolution = currentSolution,
            OutputSolution = slackResult.RefinedSolution ?? currentSolution,
            Confidence = slackResult.ConfidenceScore,
            Success = slackResult.Success,
            Diagnostics = ImmutableList.Create(slackResult.Method ?? "N/A")
        });

        if (slackResult.Success && slackResult.RefinedSolution is not null)
        {
            currentSolution = slackResult.RefinedSolution;
            diagnostics.Add($"Slack optimization: {slackResult.Method}");
        }

        // Stage 3: Gap refinement (if Master Theorem gap)
        if (theoremResult is TheoremNotApplicable notApplicable &&
            notApplicable.Reason.Contains("gap"))
        {
            progress?.OnProgressUpdated(50, "Refining theorem gap");

            var logBA = ExtractLogBA(recurrence);
            var fDegree = ExtractFDegree(recurrence);

            if (logBA.HasValue && fDegree.HasValue)
            {
                var gapResult = _slackOptimizer.RefineGap(recurrence, logBA.Value, fDegree.Value);

                stages.Add(new RefinementStage
                {
                    Name = "Gap Refinement",
                    InputSolution = currentSolution,
                    OutputSolution = gapResult.RefinedSolution ?? currentSolution,
                    Confidence = gapResult.ConfidenceScore,
                    Success = gapResult.Success,
                    Diagnostics = gapResult.Diagnostics
                });

                if (gapResult.Success && gapResult.RefinedSolution is not null)
                {
                    currentSolution = gapResult.RefinedSolution;
                    diagnostics.Add($"Gap refinement: {gapResult.Method}");
                }
            }
        }

        // Stage 4: Induction verification
        progress?.OnProgressUpdated(70, "Verifying via induction");
        var verificationResult = _verifier.VerifyRecurrenceSolution(recurrence, currentSolution);

        stages.Add(new RefinementStage
        {
            Name = "Induction Verification",
            InputSolution = currentSolution,
            OutputSolution = currentSolution,
            Confidence = verificationResult.ConfidenceScore,
            Success = verificationResult.Verified,
            Diagnostics = verificationResult.Diagnostics
        });

        diagnostics.Add($"Verification: {(verificationResult.Verified ? "Passed" : "Failed")}");

        // Stage 5: Confidence scoring
        progress?.OnProgressUpdated(90, "Computing confidence");
        var analysisContext = new AnalysisContext
        {
            Source = DetermineSource(theoremResult),
            Verification = verificationResult.Verified
                ? VerificationStatus.NumericallyVerified
                : VerificationStatus.Failed,
            TheoremResult = theoremResult,
            Methods = diagnostics.ToImmutableList()
        };

        var confidenceAssessment = _scorer.ComputeConfidence(currentSolution, analysisContext);

        progress?.OnPhaseCompleted(AnalysisPhase.Refinement, new PhaseResult
        {
            Phase = AnalysisPhase.Refinement,
            Success = true,
            ItemsProcessed = stages.Count
        });

        return new RefinementPipelineResult
        {
            Success = true,
            OriginalSolution = theoremResult.Solution,
            RefinedSolution = currentSolution,
            Stages = stages.ToImmutableList(),
            ConfidenceAssessment = confidenceAssessment,
            Verification = verificationResult,
            Diagnostics = diagnostics.ToImmutableList()
        };
    }

    /// <summary>
    /// Performs quick refinement without full verification.
    /// </summary>
    public QuickRefinementResult QuickRefine(
        ComplexityExpression expression,
        Variable variable)
    {
        // Try to simplify/tighten the expression
        var refinement = _slackOptimizer.Refine(expression, variable);

        if (refinement.Success && refinement.RefinedExpression is not null)
        {
            return new QuickRefinementResult
            {
                Success = true,
                RefinedExpression = refinement.RefinedExpression,
                Confidence = refinement.ConfidenceScore,
                Method = refinement.Method
            };
        }

        return new QuickRefinementResult
        {
            Success = false,
            RefinedExpression = expression,
            Confidence = 0.5,
            ErrorMessage = refinement.ErrorMessage
        };
    }

    /// <summary>
    /// Verifies a proposed bound without refinement.
    /// </summary>
    public BoundVerificationResult VerifyBound(
        RecurrenceRelation recurrence,
        ComplexityExpression bound,
        BoundType boundType)
    {
        return boundType switch
        {
            BoundType.BigO => _verifier.VerifyUpperBound(recurrence, bound),
            BoundType.Omega => _verifier.VerifyLowerBound(recurrence, bound),
            BoundType.Theta => VerifyTightBound(recurrence, bound),
            _ => new BoundVerificationResult { Holds = false, ErrorMessage = "Unknown bound type" }
        };
    }

    private BoundVerificationResult VerifyTightBound(
        RecurrenceRelation recurrence,
        ComplexityExpression bound)
    {
        var upperResult = _verifier.VerifyUpperBound(recurrence, bound);
        var lowerResult = _verifier.VerifyLowerBound(recurrence, bound);

        return new BoundVerificationResult
        {
            Holds = upperResult.Holds && lowerResult.Holds,
            Constant = (upperResult.Constant + lowerResult.Constant) / 2,
            Threshold = Math.Max(upperResult.Threshold, lowerResult.Threshold),
            Violations = upperResult.Violations.Concat(lowerResult.Violations).ToImmutableList(),
            Diagnostics = ImmutableList.Create(
                $"Upper bound: {(upperResult.Holds ? "✓" : "✗")}",
                $"Lower bound: {(lowerResult.Holds ? "✓" : "✗")}")
        };
    }

    private AnalysisSource DetermineSource(TheoremApplicability result)
    {
        return result switch
        {
            MasterTheoremApplicable => AnalysisSource.TheoreticalExact,
            AkraBazziApplicable => AnalysisSource.TheoreticalExact,
            LinearRecurrenceSolved => AnalysisSource.TheoreticalExact,
            TheoremNotApplicable => AnalysisSource.Heuristic,
            _ => AnalysisSource.Unknown
        };
    }

    private double? ExtractLogBA(RecurrenceRelation recurrence)
    {
        if (!recurrence.FitsMasterTheorem)
            return null;

        var term = recurrence.Terms[0];
        var a = term.Coefficient;
        var b = 1.0 / term.ScaleFactor;
        return Math.Log(a) / Math.Log(b);
    }

    private double? ExtractFDegree(RecurrenceRelation recurrence)
    {
        var f = recurrence.NonRecursiveWork;
        var variable = recurrence.Variable;

        return f switch
        {
            ConstantComplexity => 0,
            VariableComplexity v when v.Var.Equals(variable) => 1,
            LinearComplexity l when l.Var.Equals(variable) => 1,
            PolynomialComplexity p when p.Var.Equals(variable) => p.Degree,
            PolyLogComplexity pl when pl.Var.Equals(variable) => pl.PolyDegree,
            _ => null
        };
    }
}

#region Types

/// <summary>
/// Interface for the refinement engine.
/// </summary>
public interface IRefinementEngine
{
    RefinementPipelineResult Refine(
        RecurrenceRelation recurrence,
        TheoremApplicability theoremResult,
        IAnalysisProgress? progress = null);

    QuickRefinementResult QuickRefine(ComplexityExpression expression, Variable variable);

    BoundVerificationResult VerifyBound(
        RecurrenceRelation recurrence,
        ComplexityExpression bound,
        BoundType boundType);
}

/// <summary>
/// Complete result of the refinement pipeline.
/// </summary>
public sealed record RefinementPipelineResult
{
    public bool Success { get; init; }
    public ComplexityExpression? OriginalSolution { get; init; }
    public ComplexityExpression? RefinedSolution { get; init; }
    public ImmutableList<RefinementStage> Stages { get; init; } = ImmutableList<RefinementStage>.Empty;
    public ConfidenceAssessment? ConfidenceAssessment { get; init; }
    public InductionResult? Verification { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Returns true if the solution was improved during refinement.
    /// </summary>
    public bool WasImproved =>
        Success && OriginalSolution is not null && RefinedSolution is not null &&
        !ReferenceEquals(OriginalSolution, RefinedSolution);
}

/// <summary>
/// A single stage in the refinement pipeline.
/// </summary>
public sealed record RefinementStage
{
    public required string Name { get; init; }
    public ComplexityExpression? InputSolution { get; init; }
    public ComplexityExpression? OutputSolution { get; init; }
    public double Confidence { get; init; }
    public bool Success { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Result of quick refinement.
/// </summary>
public sealed record QuickRefinementResult
{
    public bool Success { get; init; }
    public ComplexityExpression? RefinedExpression { get; init; }
    public double Confidence { get; init; }
    public string? Method { get; init; }
    public string? ErrorMessage { get; init; }
}

#endregion
