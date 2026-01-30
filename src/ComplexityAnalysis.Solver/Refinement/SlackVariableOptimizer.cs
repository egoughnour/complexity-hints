using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver.Refinement;

/// <summary>
/// Optimizes complexity bounds by finding the tightest valid constants.
/// Uses numerical verification to determine actual constant factors
/// and asymptotic tightness.
///
/// For example, if analysis yields O(n²), this optimizer can determine
/// if the actual bound is Θ(n²) or if a tighter O(n log n) might apply.
/// </summary>
public sealed class SlackVariableOptimizer : ISlackVariableOptimizer
{
    /// <summary>Sample points for numerical verification.</summary>
    private readonly int[] _samplePoints = { 10, 100, 1000, 10000, 100000 };

    /// <summary>Tolerance for ratio comparisons.</summary>
    private const double Tolerance = 0.1;

    /// <summary>Maximum iterations for optimization.</summary>
    private const int MaxIterations = 100;

    public static SlackVariableOptimizer Instance { get; } = new();

    /// <summary>
    /// Refines a complexity bound by finding tighter constants.
    /// </summary>
    public RefinementResult Refine(
        ComplexityExpression expression,
        Variable variable,
        ComplexityExpression? lowerBound = null,
        ComplexityExpression? upperBound = null)
    {
        // Evaluate the expression at sample points
        var samples = EvaluateSamples(expression, variable);
        if (samples.Count < 3)
        {
            return RefinementResult.Unchanged(expression, "Insufficient sample points for refinement");
        }

        // Determine the actual growth rate
        var growthAnalysis = AnalyzeGrowth(samples);

        // Try to find a tighter bound
        var tighterBound = FindTighterBound(growthAnalysis, variable);

        if (tighterBound is not null && IsTighter(tighterBound, expression, variable))
        {
            var confidence = ComputeConfidence(tighterBound, samples, variable);
            return new RefinementResult
            {
                Success = true,
                OriginalExpression = expression,
                RefinedExpression = tighterBound,
                ConfidenceScore = confidence,
                Method = "Slack variable optimization via numerical fitting",
                Diagnostics = ImmutableList.Create(
                    $"Growth rate analysis: {growthAnalysis}",
                    $"Fitted form: {tighterBound.ToBigONotation()}")
            };
        }

        // Try to optimize constant factors
        var optimizedConstants = OptimizeConstants(expression, samples, variable);
        if (optimizedConstants is not null)
        {
            return new RefinementResult
            {
                Success = true,
                OriginalExpression = expression,
                RefinedExpression = optimizedConstants,
                ConfidenceScore = 0.8,
                Method = "Constant factor optimization",
                Diagnostics = ImmutableList.Create(
                    $"Optimized expression: {optimizedConstants.ToBigONotation()}")
            };
        }

        return RefinementResult.Unchanged(expression, "No tighter bound found");
    }

    /// <summary>
    /// Refines a recurrence solution with verification.
    /// </summary>
    public RecurrenceRefinementResult RefineRecurrence(
        RecurrenceRelation recurrence,
        TheoremApplicability theoremResult)
    {
        if (!theoremResult.IsApplicable || theoremResult.Solution is null)
        {
            return new RecurrenceRefinementResult
            {
                Success = false,
                ErrorMessage = "Cannot refine non-applicable theorem result"
            };
        }

        var solution = theoremResult.Solution;
        var variable = recurrence.Variable;

        // Numerically verify the solution
        var verification = VerifyRecurrenceSolution(recurrence, solution, variable);

        if (!verification.IsValid)
        {
            // Try to find a better solution
            var correctedSolution = FindBetterSolution(recurrence, verification.Samples, variable);
            if (correctedSolution is not null)
            {
                return new RecurrenceRefinementResult
                {
                    Success = true,
                    OriginalSolution = solution,
                    RefinedSolution = correctedSolution,
                    ConfidenceScore = 0.7,
                    Method = "Numerical correction",
                    Verification = verification
                };
            }
        }

        // Solution verified - try to tighten constants
        var tightened = TightenSolutionBounds(solution, verification, variable);

        return new RecurrenceRefinementResult
        {
            Success = true,
            OriginalSolution = solution,
            RefinedSolution = tightened ?? solution,
            ConfidenceScore = verification.ConfidenceScore,
            Method = tightened is not null ? "Bound tightening" : "Verification only",
            Verification = verification
        };
    }

    /// <summary>
    /// Finds tighter bounds for Master Theorem gap cases.
    /// </summary>
    public GapRefinementResult RefineGap(
        RecurrenceRelation recurrence,
        double logBA,
        double fDegree)
    {
        var gap = Math.Abs(fDegree - logBA);
        var variable = recurrence.Variable;

        // For small gaps, use series expansion
        if (gap < 0.1)
        {
            return RefineSmallGap(recurrence, logBA, fDegree, gap, variable);
        }

        // For medium gaps, use interpolation
        if (gap < 0.5)
        {
            return RefineMediumGap(recurrence, logBA, fDegree, variable);
        }

        // Large gaps typically mean one side dominates
        return RefineLargeGap(recurrence, logBA, fDegree, variable);
    }

    #region Private Methods

    private IReadOnlyList<(double n, double value)> EvaluateSamples(
        ComplexityExpression expression, Variable variable)
    {
        var results = new List<(double n, double value)>();

        foreach (var n in _samplePoints)
        {
            var assignments = new Dictionary<Variable, double> { { variable, n } };
            var value = expression.Evaluate(assignments);
            if (value.HasValue && !double.IsInfinity(value.Value) && !double.IsNaN(value.Value))
            {
                results.Add((n, value.Value));
            }
        }

        return results;
    }

    private GrowthAnalysis AnalyzeGrowth(IReadOnlyList<(double n, double value)> samples)
    {
        if (samples.Count < 2)
            return new GrowthAnalysis(GrowthType.Unknown, 0, 0);

        // Compute growth ratios
        var ratios = new List<double>();
        var logRatios = new List<double>();

        for (int i = 1; i < samples.Count; i++)
        {
            var (n1, v1) = samples[i - 1];
            var (n2, v2) = samples[i];

            if (v1 > 0 && n1 > 0)
            {
                var ratio = v2 / v1;
                var nRatio = n2 / n1;

                ratios.Add(ratio);

                // For polynomial growth: v2/v1 ≈ (n2/n1)^k
                // So k ≈ log(v2/v1) / log(n2/n1)
                var k = Math.Log(ratio) / Math.Log(nRatio);
                logRatios.Add(k);
            }
        }

        if (ratios.Count == 0)
            return new GrowthAnalysis(GrowthType.Unknown, 0, 0);

        // Analyze the growth pattern
        var avgK = logRatios.Average();
        var stdK = ComputeStdDev(logRatios, avgK);

        // Determine growth type
        if (stdK < 0.2) // Stable polynomial exponent
        {
            // Check for log factor by examining residuals
            var hasLogFactor = DetectLogFactor(samples, avgK);
            var logExponent = hasLogFactor ? EstimateLogExponent(samples, avgK) : 0;

            if (Math.Abs(avgK) < 0.1)
            {
                return hasLogFactor
                    ? new GrowthAnalysis(GrowthType.Logarithmic, 0, logExponent)
                    : new GrowthAnalysis(GrowthType.Constant, 0, 0);
            }

            return hasLogFactor
                ? new GrowthAnalysis(GrowthType.PolyLog, avgK, logExponent)
                : new GrowthAnalysis(GrowthType.Polynomial, avgK, 0);
        }

        // Check for exponential growth
        if (ratios.All(r => r > 1.5))
        {
            var base_ = ratios.Average();
            return new GrowthAnalysis(GrowthType.Exponential, base_, 0);
        }

        return new GrowthAnalysis(GrowthType.Unknown, avgK, 0);
    }

    private bool DetectLogFactor(IReadOnlyList<(double n, double value)> samples, double polynomialDegree)
    {
        // Divide out the polynomial component and check if residual is logarithmic
        var residuals = samples.Select(s =>
        {
            var polynomialPart = Math.Pow(s.n, polynomialDegree);
            return polynomialPart > 0 ? s.value / polynomialPart : 0;
        }).ToList();

        // Check if residuals grow logarithmically
        var logN = samples.Select(s => Math.Log(s.n)).ToList();
        var correlation = ComputeCorrelation(logN, residuals);

        return Math.Abs(correlation) > 0.9;
    }

    private double EstimateLogExponent(IReadOnlyList<(double n, double value)> samples, double polynomialDegree)
    {
        // Estimate j in n^k * log^j(n)
        // Divide out n^k and fit log^j(n)

        var residuals = samples.Select(s =>
        {
            var polynomialPart = Math.Pow(s.n, polynomialDegree);
            return polynomialPart > 0 ? s.value / polynomialPart : 1;
        }).ToList();

        // Try j = 1, 2, 3 and see which fits best
        double bestJ = 1;
        double bestError = double.MaxValue;

        for (int j = 1; j <= 3; j++)
        {
            var error = ComputeFitError(samples.Select(s => s.n).ToList(), residuals, j);
            if (error < bestError)
            {
                bestError = error;
                bestJ = j;
            }
        }

        return bestJ;
    }

    private double ComputeFitError(IReadOnlyList<double> nValues, IReadOnlyList<double> residuals, int logExponent)
    {
        var error = 0.0;
        for (int i = 0; i < nValues.Count; i++)
        {
            var predicted = Math.Pow(Math.Log(nValues[i]), logExponent);
            var actual = residuals[i];
            if (predicted > 0)
            {
                error += Math.Pow(Math.Log(actual / predicted), 2);
            }
        }
        return error;
    }

    private ComplexityExpression? FindTighterBound(GrowthAnalysis analysis, Variable variable)
    {
        return analysis.Type switch
        {
            GrowthType.Constant => ConstantComplexity.One,
            GrowthType.Logarithmic => new LogarithmicComplexity(1, variable),
            GrowthType.Polynomial => CreatePolynomial(analysis.Parameter, variable),
            GrowthType.PolyLog => new PolyLogComplexity(analysis.Parameter, (int)analysis.LogExponent, variable),
            GrowthType.Exponential => new ExponentialComplexity(analysis.Parameter, variable),
            _ => null
        };
    }

    private ComplexityExpression CreatePolynomial(double degree, Variable variable)
    {
        var roundedDegree = Math.Round(degree, 1);

        // Check for common cases
        if (Math.Abs(roundedDegree - 1.0) < 0.1)
            return new LinearComplexity(1, variable);

        if (Math.Abs(roundedDegree - 2.0) < 0.1)
            return PolynomialComplexity.OfDegree(2, variable);

        if (Math.Abs(roundedDegree - 3.0) < 0.1)
            return PolynomialComplexity.OfDegree(3, variable);

        // Non-integer degree (likely from Akra-Bazzi)
        return PolyLogComplexity.Polynomial(roundedDegree, variable);
    }

    private bool IsTighter(ComplexityExpression candidate, ComplexityExpression original, Variable variable)
    {
        // Compare asymptotic growth
        // candidate is tighter if it grows slower or equal but with smaller constants

        foreach (var n in new[] { 1000, 10000, 100000 })
        {
            var assignments = new Dictionary<Variable, double> { { variable, n } };
            var candidateVal = candidate.Evaluate(assignments);
            var originalVal = original.Evaluate(assignments);

            if (!candidateVal.HasValue || !originalVal.HasValue)
                continue;

            if (candidateVal.Value > originalVal.Value * (1 + Tolerance))
                return false; // Candidate grows faster
        }

        return true;
    }

    private ComplexityExpression? OptimizeConstants(
        ComplexityExpression expression,
        IReadOnlyList<(double n, double value)> samples,
        Variable variable)
    {
        // For polynomial expressions, optimize the leading coefficient
        if (expression is PolynomialComplexity poly)
        {
            var degree = poly.Degree;
            var optimalCoeff = FitLeadingCoefficient(samples, degree);

            if (Math.Abs(optimalCoeff - poly.LeadingCoefficient) > Tolerance)
            {
                return new PolynomialComplexity(
                    ImmutableDictionary<int, double>.Empty.Add(degree, optimalCoeff),
                    variable);
            }
        }

        return null;
    }

    private double FitLeadingCoefficient(IReadOnlyList<(double n, double value)> samples, int degree)
    {
        // Least squares fit: c = Σ(v_i / n_i^d) / count
        var coefficients = samples.Select(s => s.value / Math.Pow(s.n, degree)).ToList();
        return coefficients.Average();
    }

    private VerificationResult VerifyRecurrenceSolution(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        Variable variable)
    {
        var recurrenceValues = new List<(double n, double value)>();
        var solutionValues = new List<(double n, double value)>();

        // Compute recurrence by unrolling
        foreach (var n in _samplePoints)
        {
            var assignments = new Dictionary<Variable, double> { { variable, n } };

            var recVal = UnrollRecurrence(recurrence, n, new Dictionary<double, double>());
            var solVal = solution.Evaluate(assignments);

            if (recVal.HasValue && solVal.HasValue)
            {
                recurrenceValues.Add((n, recVal.Value));
                solutionValues.Add((n, solVal.Value));
            }
        }

        // Check if solution matches recurrence
        var isValid = true;
        var maxRatio = 0.0;

        for (int i = 0; i < recurrenceValues.Count; i++)
        {
            var ratio = recurrenceValues[i].value / solutionValues[i].value;
            maxRatio = Math.Max(maxRatio, ratio);

            if (ratio > 10 || ratio < 0.1)
            {
                isValid = false;
            }
        }

        return new VerificationResult
        {
            IsValid = isValid,
            Samples = recurrenceValues,
            SolutionSamples = solutionValues,
            MaxRatio = maxRatio,
            ConfidenceScore = isValid ? Math.Min(1.0, 1.0 / maxRatio) : 0.5
        };
    }

    private double? UnrollRecurrence(RecurrenceRelation recurrence, double n, Dictionary<double, double> memo)
    {
        if (n <= 1)
            return recurrence.BaseCase.Evaluate(new Dictionary<Variable, double> { { recurrence.Variable, n } }) ?? 1;

        if (memo.TryGetValue(n, out var cached))
            return cached;

        var assignments = new Dictionary<Variable, double> { { recurrence.Variable, n } };
        var nonRecursive = recurrence.NonRecursiveWork.Evaluate(assignments) ?? 0;

        var recursive = 0.0;
        foreach (var term in recurrence.Terms)
        {
            var subSize = term.ScaleFactor * n;
            if (subSize >= n) return null; // Non-reducing

            var subResult = UnrollRecurrence(recurrence, subSize, memo);
            if (!subResult.HasValue) return null;

            recursive += term.Coefficient * subResult.Value;
        }

        var result = recursive + nonRecursive;
        memo[n] = result;
        return result;
    }

    private ComplexityExpression? FindBetterSolution(
        RecurrenceRelation recurrence,
        IReadOnlyList<(double n, double value)> samples,
        Variable variable)
    {
        // Analyze the actual growth from samples
        var growth = AnalyzeGrowth(samples);

        // Create expression matching the observed growth
        return FindTighterBound(growth, variable);
    }

    private ComplexityExpression? TightenSolutionBounds(
        ComplexityExpression solution,
        VerificationResult verification,
        Variable variable)
    {
        if (verification.MaxRatio <= 1.1)
            return null; // Already tight

        // Try to incorporate the constant factor
        var avgRatio = verification.Samples
            .Zip(verification.SolutionSamples, (r, s) => r.value / s.value)
            .Average();

        if (avgRatio > 1.5 && solution is PolyLogComplexity polyLog)
        {
            // Multiply by constant factor
            return new BinaryOperationComplexity(
                new ConstantComplexity(avgRatio),
                BinaryOp.Multiply,
                solution);
        }

        return null;
    }

    private GapRefinementResult RefineSmallGap(
        RecurrenceRelation recurrence,
        double logBA,
        double fDegree,
        double gap,
        Variable variable)
    {
        // For small gaps, the solution is close to n^logBA with possible log factors
        // Use perturbation expansion

        // The solution is approximately: n^logBA * log^k(n) for some k
        var baseExponent = logBA;
        var logExponent = gap < 0.05 ? 1 : 0;

        var solution = logExponent > 0
            ? new PolyLogComplexity(baseExponent, logExponent, variable)
            : PolyLogComplexity.Polynomial(baseExponent, variable);

        return new GapRefinementResult
        {
            Success = true,
            RefinedSolution = solution,
            Method = "Small gap perturbation expansion",
            ConfidenceScore = 0.85,
            Gap = gap,
            Diagnostics = ImmutableList.Create(
                $"Gap = {gap:F4} is small enough for perturbation analysis",
                $"Base exponent: {baseExponent:F4}",
                $"Log factor: log^{logExponent}(n)")
        };
    }

    private GapRefinementResult RefineMediumGap(
        RecurrenceRelation recurrence,
        double logBA,
        double fDegree,
        Variable variable)
    {
        // For medium gaps, use interpolation between the two bounds
        var avgExponent = (logBA + fDegree) / 2;

        // Check which side is likely dominant via sampling
        var samples = new List<(double n, double recurrence, double nLogBA, double nFDegree)>();

        foreach (var n in _samplePoints.Take(3))
        {
            var recVal = UnrollRecurrence(recurrence, n, new Dictionary<double, double>());
            if (recVal.HasValue)
            {
                samples.Add((n, recVal.Value, Math.Pow(n, logBA), Math.Pow(n, fDegree)));
            }
        }

        // Determine which bound is tighter
        double solution_exponent;
        if (samples.Count > 0)
        {
            var avgLogBA_ratio = samples.Average(s => s.recurrence / s.nLogBA);
            var avgFDegree_ratio = samples.Average(s => s.recurrence / s.nFDegree);

            solution_exponent = avgLogBA_ratio < avgFDegree_ratio ? logBA : fDegree;
        }
        else
        {
            solution_exponent = avgExponent;
        }

        return new GapRefinementResult
        {
            Success = true,
            RefinedSolution = PolyLogComplexity.Polynomial(solution_exponent, variable),
            Method = "Medium gap interpolation",
            ConfidenceScore = 0.7,
            Gap = Math.Abs(fDegree - logBA),
            Diagnostics = ImmutableList.Create(
                $"Interpolated between n^{logBA:F2} and n^{fDegree:F2}",
                $"Selected exponent: {solution_exponent:F4}")
        };
    }

    private GapRefinementResult RefineLargeGap(
        RecurrenceRelation recurrence,
        double logBA,
        double fDegree,
        Variable variable)
    {
        // For large gaps, one side clearly dominates
        var dominantExponent = fDegree > logBA ? fDegree : logBA;
        var dominantSide = fDegree > logBA ? "f(n)" : "n^(log_b a)";

        return new GapRefinementResult
        {
            Success = true,
            RefinedSolution = PolyLogComplexity.Polynomial(dominantExponent, variable),
            Method = "Large gap dominance",
            ConfidenceScore = 0.9,
            Gap = Math.Abs(fDegree - logBA),
            Diagnostics = ImmutableList.Create(
                $"Large gap ({Math.Abs(fDegree - logBA):F2}) means {dominantSide} dominates",
                $"Solution: Θ(n^{dominantExponent:F2})")
        };
    }

    private double ComputeConfidence(
        ComplexityExpression expression,
        IReadOnlyList<(double n, double value)> samples,
        Variable variable)
    {
        // Compute R² value for the fit
        var predicted = samples.Select(s =>
        {
            var assignments = new Dictionary<Variable, double> { { variable, s.n } };
            return expression.Evaluate(assignments) ?? 0;
        }).ToList();

        var actual = samples.Select(s => s.value).ToList();

        var meanActual = actual.Average();
        var ssRes = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();
        var ssTot = actual.Select(a => Math.Pow(a - meanActual, 2)).Sum();

        var r2 = ssTot > 0 ? 1 - ssRes / ssTot : 0;
        return Math.Max(0, Math.Min(1, r2));
    }

    private double ComputeStdDev(IReadOnlyList<double> values, double mean)
    {
        if (values.Count < 2) return 0;
        var variance = values.Select(v => Math.Pow(v - mean, 2)).Average();
        return Math.Sqrt(variance);
    }

    private double ComputeCorrelation(IReadOnlyList<double> x, IReadOnlyList<double> y)
    {
        if (x.Count != y.Count || x.Count < 2) return 0;

        var meanX = x.Average();
        var meanY = y.Average();

        var covariance = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var varX = x.Select(xi => Math.Pow(xi - meanX, 2)).Sum();
        var varY = y.Select(yi => Math.Pow(yi - meanY, 2)).Sum();

        var denominator = Math.Sqrt(varX * varY);
        return denominator > 0 ? covariance / denominator : 0;
    }

    #endregion
}

#region Result Types

/// <summary>
/// Interface for slack variable optimization.
/// </summary>
public interface ISlackVariableOptimizer
{
    RefinementResult Refine(
        ComplexityExpression expression,
        Variable variable,
        ComplexityExpression? lowerBound = null,
        ComplexityExpression? upperBound = null);

    RecurrenceRefinementResult RefineRecurrence(
        RecurrenceRelation recurrence,
        TheoremApplicability theoremResult);

    GapRefinementResult RefineGap(
        RecurrenceRelation recurrence,
        double logBA,
        double fDegree);
}

/// <summary>
/// Result of general refinement.
/// </summary>
public sealed record RefinementResult
{
    public bool Success { get; init; }
    public ComplexityExpression? OriginalExpression { get; init; }
    public ComplexityExpression? RefinedExpression { get; init; }
    public double ConfidenceScore { get; init; }
    public string? Method { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }

    public static RefinementResult Unchanged(ComplexityExpression expr, string reason) =>
        new()
        {
            Success = false,
            OriginalExpression = expr,
            RefinedExpression = expr,
            ErrorMessage = reason
        };
}

/// <summary>
/// Result of recurrence refinement.
/// </summary>
public sealed record RecurrenceRefinementResult
{
    public bool Success { get; init; }
    public ComplexityExpression? OriginalSolution { get; init; }
    public ComplexityExpression? RefinedSolution { get; init; }
    public double ConfidenceScore { get; init; }
    public string? Method { get; init; }
    public VerificationResult? Verification { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Result of gap refinement.
/// </summary>
public sealed record GapRefinementResult
{
    public bool Success { get; init; }
    public ComplexityExpression? RefinedSolution { get; init; }
    public string? Method { get; init; }
    public double ConfidenceScore { get; init; }
    public double Gap { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Verification result for numerical checking.
/// </summary>
public sealed record VerificationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<(double n, double value)> Samples { get; init; } = Array.Empty<(double, double)>();
    public IReadOnlyList<(double n, double value)> SolutionSamples { get; init; } = Array.Empty<(double, double)>();
    public double MaxRatio { get; init; }
    public double ConfidenceScore { get; init; }
}

/// <summary>
/// Analysis of growth pattern.
/// </summary>
public sealed record GrowthAnalysis(GrowthType Type, double Parameter, double LogExponent);

/// <summary>
/// Types of growth patterns.
/// </summary>
public enum GrowthType
{
    Unknown,
    Constant,
    Logarithmic,
    Polynomial,
    PolyLog,
    Exponential,
    Factorial
}

#endregion
