using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver.Refinement;

/// <summary>
/// Computes confidence scores for complexity analysis results.
/// Takes into account multiple factors including:
/// - Source of the analysis (theoretical vs numerical)
/// - Verification results
/// - Stability of numerical fits
/// - Theorem applicability
/// </summary>
public sealed class ConfidenceScorer : IConfidenceScorer
{
    /// <summary>Base confidence weights for different analysis sources.</summary>
    private static readonly IReadOnlyDictionary<AnalysisSource, double> SourceWeights =
        new Dictionary<AnalysisSource, double>
        {
            { AnalysisSource.TheoreticalExact, 1.0 },
            { AnalysisSource.TheoreticalApproximate, 0.9 },
            { AnalysisSource.NumericalFit, 0.7 },
            { AnalysisSource.Heuristic, 0.5 },
            { AnalysisSource.Unknown, 0.3 }
        };

    public static ConfidenceScorer Instance { get; } = new();

    /// <summary>
    /// Computes an overall confidence score for a complexity result.
    /// </summary>
    public ConfidenceAssessment ComputeConfidence(
        ComplexityExpression expression,
        AnalysisContext context)
    {
        var factors = new List<ConfidenceFactor>();
        var warnings = new List<string>();

        // Factor 1: Source of analysis
        var sourceScore = ComputeSourceConfidence(context.Source);
        factors.Add(new ConfidenceFactor("Analysis Source", sourceScore, context.Source.ToString()));

        // Factor 2: Expression complexity
        var complexityScore = ComputeExpressionComplexity(expression);
        factors.Add(new ConfidenceFactor("Expression Simplicity", complexityScore,
            GetComplexityDescription(expression)));

        // Factor 3: Verification status
        var verificationScore = ComputeVerificationConfidence(context.Verification);
        factors.Add(new ConfidenceFactor("Verification", verificationScore,
            context.Verification?.ToString() ?? "Not verified"));

        // Factor 4: Stability (if numerical)
        if (context.NumericalSamples is not null)
        {
            var stabilityScore = ComputeStabilityConfidence(context.NumericalSamples);
            factors.Add(new ConfidenceFactor("Numerical Stability", stabilityScore,
                $"R² = {context.NumericalSamples.RSquared:F4}"));
        }

        // Factor 5: Theorem applicability
        if (context.TheoremResult is not null)
        {
            var theoremScore = ComputeTheoremConfidence(context.TheoremResult);
            factors.Add(new ConfidenceFactor("Theorem Applicability", theoremScore,
                context.TheoremResult.Explanation));
        }

        // Compute overall score (weighted geometric mean)
        var overallScore = ComputeOverallScore(factors);

        // Generate warnings
        if (overallScore < 0.5)
            warnings.Add("Low confidence - result may be inaccurate");
        if (context.Source == AnalysisSource.Heuristic)
            warnings.Add("Result based on heuristics - verify manually");
        if (context.Verification == VerificationStatus.Failed)
            warnings.Add("Verification failed - result may not satisfy the recurrence");

        return new ConfidenceAssessment
        {
            OverallScore = overallScore,
            Factors = factors.ToImmutableList(),
            Level = ClassifyConfidenceLevel(overallScore),
            Warnings = warnings.ToImmutableList(),
            Recommendation = GenerateRecommendation(overallScore, factors)
        };
    }

    /// <summary>
    /// Computes confidence for a theorem applicability result.
    /// </summary>
    public double ComputeTheoremConfidence(TheoremApplicability result)
    {
        return result switch
        {
            MasterTheoremApplicable master => ComputeMasterTheoremConfidence(master),
            AkraBazziApplicable akra => ComputeAkraBazziConfidence(akra),
            LinearRecurrenceSolved linear => 0.95, // Linear recurrences are well-understood
            TheoremNotApplicable => 0.3, // No theorem applies
            _ => 0.5
        };
    }

    /// <summary>
    /// Computes confidence for a refinement result.
    /// </summary>
    public double ComputeRefinementConfidence(RefinementResult result)
    {
        if (!result.Success)
            return 0.0;

        var baseConfidence = result.ConfidenceScore;

        // Adjust based on method
        var methodFactor = result.Method switch
        {
            "Slack variable optimization via numerical fitting" => 0.8,
            "Constant factor optimization" => 0.9,
            _ => 0.7
        };

        return baseConfidence * methodFactor;
    }

    /// <summary>
    /// Computes combined confidence when multiple analyses agree.
    /// </summary>
    public double ComputeConsensusConfidence(IReadOnlyList<double> confidences)
    {
        if (confidences.Count == 0)
            return 0.0;

        if (confidences.Count == 1)
            return confidences[0];

        // If multiple analyses agree with high confidence, boost the overall score
        var avgConfidence = confidences.Average();
        var minConfidence = confidences.Min();
        var variance = confidences.Select(c => Math.Pow(c - avgConfidence, 2)).Average();

        // Low variance (agreement) increases confidence
        var agreementBonus = variance < 0.01 ? 0.1 : variance < 0.05 ? 0.05 : 0;

        return Math.Min(1.0, Math.Max(minConfidence, avgConfidence + agreementBonus));
    }

    #region Private Methods

    private double ComputeSourceConfidence(AnalysisSource source)
    {
        return SourceWeights.TryGetValue(source, out var weight) ? weight : 0.5;
    }

    private double ComputeExpressionComplexity(ComplexityExpression expression)
    {
        // Simpler expressions are more trustworthy
        return expression switch
        {
            ConstantComplexity => 1.0,
            LinearComplexity => 0.95,
            VariableComplexity => 0.95,
            LogarithmicComplexity => 0.9,
            PolynomialComplexity p => p.Degree <= 3 ? 0.9 : 0.8,
            PolyLogComplexity => 0.85,
            ExponentialComplexity => 0.8,
            FactorialComplexity => 0.8,
            BinaryOperationComplexity bin => Math.Min(
                ComputeExpressionComplexity(bin.Left),
                ComputeExpressionComplexity(bin.Right)) * 0.9,
            ConditionalComplexity => 0.7,
            RecurrenceComplexity => 0.6, // Unsolved recurrence
            _ => 0.5
        };
    }

    private double ComputeVerificationConfidence(VerificationStatus? status)
    {
        return status switch
        {
            VerificationStatus.Proven => 1.0,
            VerificationStatus.NumericallyVerified => 0.85,
            VerificationStatus.PartiallyVerified => 0.7,
            VerificationStatus.NotVerified => 0.5,
            VerificationStatus.Failed => 0.2,
            null => 0.5
        };
    }

    private double ComputeStabilityConfidence(NumericalFitData data)
    {
        // Higher R² means better fit
        var r2Score = data.RSquared;

        // Lower standard error is better
        var errorScore = 1.0 / (1.0 + data.StandardError);

        // More sample points increase confidence
        var sampleScore = Math.Min(1.0, data.SampleCount / 10.0);

        return (r2Score + errorScore + sampleScore) / 3.0;
    }

    private double ComputeMasterTheoremConfidence(MasterTheoremApplicable master)
    {
        var baseConfidence = 0.95; // Master Theorem is well-established

        // Adjust based on case
        baseConfidence *= master.Case switch
        {
            MasterTheoremCase.Case1 => 1.0,
            MasterTheoremCase.Case2 => 1.0,
            MasterTheoremCase.Case3 => master.RegularityVerified == true ? 1.0 : 0.9,
            MasterTheoremCase.Gap => 0.7,
            _ => 0.8
        };

        // Adjust based on epsilon (larger epsilon = more confident)
        if (master.Epsilon > 0.5)
            baseConfidence *= 1.0;
        else if (master.Epsilon > 0.1)
            baseConfidence *= 0.95;
        else if (master.Epsilon > 0.01)
            baseConfidence *= 0.9;
        else
            baseConfidence *= 0.8; // Near boundary

        return baseConfidence;
    }

    private double ComputeAkraBazziConfidence(AkraBazziApplicable akra)
    {
        var baseConfidence = 0.9; // Akra-Bazzi is well-established but more complex

        // Adjust based on number of terms
        var termCount = akra.Terms.Count;
        baseConfidence *= termCount switch
        {
            1 => 1.0,
            2 => 0.95,
            3 => 0.9,
            _ => 0.85
        };

        // Adjust based on integral evaluation
        if (akra.IntegralTerm is not null)
            baseConfidence *= 0.95; // Integral was evaluated

        return baseConfidence;
    }

    private double ComputeOverallScore(IReadOnlyList<ConfidenceFactor> factors)
    {
        if (factors.Count == 0)
            return 0.5;

        // Weighted geometric mean (more sensitive to low values)
        var product = 1.0;
        var totalWeight = 0.0;

        foreach (var factor in factors)
        {
            var weight = GetFactorWeight(factor.Name);
            product *= Math.Pow(Math.Max(0.01, factor.Score), weight);
            totalWeight += weight;
        }

        return Math.Pow(product, 1.0 / totalWeight);
    }

    private double GetFactorWeight(string factorName)
    {
        return factorName switch
        {
            "Analysis Source" => 1.5,
            "Verification" => 1.3,
            "Theorem Applicability" => 1.2,
            "Numerical Stability" => 1.0,
            "Expression Simplicity" => 0.8,
            _ => 1.0
        };
    }

    private ConfidenceLevel ClassifyConfidenceLevel(double score)
    {
        return score switch
        {
            >= 0.9 => ConfidenceLevel.VeryHigh,
            >= 0.75 => ConfidenceLevel.High,
            >= 0.5 => ConfidenceLevel.Medium,
            >= 0.25 => ConfidenceLevel.Low,
            _ => ConfidenceLevel.VeryLow
        };
    }

    private string GetComplexityDescription(ComplexityExpression expression)
    {
        return expression switch
        {
            ConstantComplexity => "Simple constant",
            LinearComplexity => "Simple linear",
            PolynomialComplexity p => $"Polynomial degree {p.Degree}",
            PolyLogComplexity pl => $"Polylog n^{pl.PolyDegree}·log^{pl.LogExponent} n",
            BinaryOperationComplexity => "Composite expression",
            RecurrenceComplexity => "Unsolved recurrence",
            _ => expression.ToBigONotation()
        };
    }

    private string GenerateRecommendation(double score, IReadOnlyList<ConfidenceFactor> factors)
    {
        if (score >= 0.9)
            return "Result is highly confident and can be used directly.";

        if (score >= 0.75)
            return "Result is confident but consider verifying critical applications.";

        if (score >= 0.5)
        {
            var weakestFactor = factors.OrderBy(f => f.Score).FirstOrDefault();
            return weakestFactor is not null
                ? $"Moderate confidence. Consider improving: {weakestFactor.Name}"
                : "Moderate confidence. Consider additional verification.";
        }

        return "Low confidence. Recommend manual verification or alternative analysis approaches.";
    }

    #endregion
}

#region Types

/// <summary>
/// Interface for confidence scoring.
/// </summary>
public interface IConfidenceScorer
{
    ConfidenceAssessment ComputeConfidence(ComplexityExpression expression, AnalysisContext context);
    double ComputeTheoremConfidence(TheoremApplicability result);
    double ComputeRefinementConfidence(RefinementResult result);
    double ComputeConsensusConfidence(IReadOnlyList<double> confidences);
}

/// <summary>
/// Complete confidence assessment for a complexity result.
/// </summary>
public sealed record ConfidenceAssessment
{
    public double OverallScore { get; init; }
    public ConfidenceLevel Level { get; init; }
    public ImmutableList<ConfidenceFactor> Factors { get; init; } = ImmutableList<ConfidenceFactor>.Empty;
    public ImmutableList<string> Warnings { get; init; } = ImmutableList<string>.Empty;
    public string? Recommendation { get; init; }
}

/// <summary>
/// A single factor contributing to confidence.
/// </summary>
public sealed record ConfidenceFactor(string Name, double Score, string Description);

/// <summary>
/// Confidence level classification.
/// </summary>
public enum ConfidenceLevel
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

/// <summary>
/// Source of complexity analysis.
/// </summary>
public enum AnalysisSource
{
    TheoreticalExact,      // Proven theorem application
    TheoreticalApproximate, // Theorem with approximations
    NumericalFit,          // Numerical curve fitting
    Heuristic,             // Rule-based heuristics
    Unknown                // Unknown source
}

/// <summary>
/// Verification status of a result.
/// </summary>
public enum VerificationStatus
{
    Proven,              // Mathematically proven
    NumericallyVerified, // Verified numerically
    PartiallyVerified,   // Some conditions verified
    NotVerified,         // Not yet verified
    Failed               // Verification failed
}

/// <summary>
/// Context for confidence analysis.
/// </summary>
public sealed record AnalysisContext
{
    public AnalysisSource Source { get; init; } = AnalysisSource.Unknown;
    public VerificationStatus? Verification { get; init; }
    public TheoremApplicability? TheoremResult { get; init; }
    public NumericalFitData? NumericalSamples { get; init; }
    public ImmutableList<string> Methods { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Data from numerical fitting.
/// </summary>
public sealed record NumericalFitData
{
    public double RSquared { get; init; }
    public double StandardError { get; init; }
    public int SampleCount { get; init; }
    public ImmutableList<(double n, double value)> Samples { get; init; }
        = ImmutableList<(double, double)>.Empty;
}

#endregion
