using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Result of checking the regularity condition for Master Theorem Case 3.
/// The regularity condition requires: a·f(n/b) ≤ c·f(n) for some c &lt; 1 and all sufficiently large n.
/// </summary>
public sealed record RegularityResult
{
    /// <summary>Whether the regularity condition holds.</summary>
    public required bool Holds { get; init; }

    /// <summary>
    /// The best (smallest) constant c found such that a·f(n/b) ≤ c·f(n).
    /// Null if regularity doesn't hold or couldn't be determined.
    /// </summary>
    public double? BestC { get; init; }

    /// <summary>Human-readable explanation of the verification.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Confidence level (0.0 to 1.0) in the result.</summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>The sample points used for numerical verification.</summary>
    public IReadOnlyList<double>? SamplePoints { get; init; }

    /// <summary>
    /// Creates a result indicating regularity holds.
    /// </summary>
    public static RegularityResult Success(double bestC, string reasoning, double confidence = 1.0) =>
        new()
        {
            Holds = true,
            BestC = bestC,
            Reasoning = reasoning,
            Confidence = confidence
        };

    /// <summary>
    /// Creates a result indicating regularity does not hold.
    /// </summary>
    public static RegularityResult Failure(string reasoning, double confidence = 1.0) =>
        new()
        {
            Holds = false,
            Reasoning = reasoning,
            Confidence = confidence
        };

    /// <summary>
    /// Creates a result indicating regularity could not be determined.
    /// </summary>
    public static RegularityResult Indeterminate(string reasoning) =>
        new()
        {
            Holds = false,
            Reasoning = reasoning,
            Confidence = 0.0
        };
}

/// <summary>
/// Verifies the regularity condition for Master Theorem Case 3.
///
/// The regularity condition states: a·f(n/b) ≤ c·f(n) for some c &lt; 1
/// and all sufficiently large n.
///
/// This is equivalent to requiring that f(n) grows "regularly" without
/// wild oscillations that could invalidate Case 3.
/// </summary>
public interface IRegularityChecker
{
    /// <summary>
    /// Checks if the regularity condition holds for the given parameters.
    /// </summary>
    /// <param name="a">Number of subproblems (a in T(n) = aT(n/b) + f(n)).</param>
    /// <param name="b">Division factor (b in T(n) = aT(n/b) + f(n)).</param>
    /// <param name="f">The non-recursive work function f(n).</param>
    /// <param name="variable">The variable (typically n).</param>
    /// <returns>Result indicating whether regularity holds.</returns>
    RegularityResult CheckRegularity(
        double a,
        double b,
        ComplexityExpression f,
        Variable variable);
}

/// <summary>
/// Numerical implementation of regularity checking using sampling.
///
/// For common polynomial forms, regularity can be verified analytically:
/// - f(n) = n^k: a·(n/b)^k ≤ c·n^k → a/b^k ≤ c, so c = a/b^k
///   For Case 3, k > log_b(a), so b^k > a, thus a/b^k &lt; 1 ✓
///
/// For more complex forms, we use numerical sampling.
/// </summary>
public sealed class NumericalRegularityChecker : IRegularityChecker
{
    private readonly IExpressionClassifier _classifier;

    /// <summary>Default sample points for numerical verification.</summary>
    private static readonly double[] DefaultSamplePoints =
    {
        100, 500, 1000, 5000, 10000, 50000, 100000, 500000, 1000000
    };

    /// <summary>Tolerance for numerical comparisons.</summary>
    private const double Tolerance = 1e-9;

    /// <summary>Maximum acceptable c value (must be strictly less than 1).</summary>
    private const double MaxC = 0.9999;

    public NumericalRegularityChecker(IExpressionClassifier? classifier = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
    }

    public static NumericalRegularityChecker Instance { get; } = new();

    public RegularityResult CheckRegularity(
        double a,
        double b,
        ComplexityExpression f,
        Variable variable)
    {
        // Validate inputs
        if (a <= 0)
            return RegularityResult.Failure($"Invalid a={a}: must be positive");
        if (b <= 1)
            return RegularityResult.Failure($"Invalid b={b}: must be greater than 1");

        // Try analytical verification first for known forms
        var analyticalResult = TryAnalyticalVerification(a, b, f, variable);
        if (analyticalResult != null)
            return analyticalResult;

        // Fall back to numerical sampling
        return NumericalVerification(a, b, f, variable, DefaultSamplePoints);
    }

    /// <summary>
    /// Attempts analytical verification for common f(n) forms.
    /// </summary>
    private RegularityResult? TryAnalyticalVerification(
        double a,
        double b,
        ComplexityExpression f,
        Variable variable)
    {
        var classification = _classifier.Classify(f, variable);

        return classification.Form switch
        {
            // f(n) = c·n^k
            // Regularity: a·f(n/b) = a·c·(n/b)^k = (a/b^k)·c·n^k = (a/b^k)·f(n)
            // So c_reg = a/b^k, need c_reg < 1, i.e., a < b^k
            ExpressionForm.Polynomial when classification.PrimaryParameter.HasValue =>
                VerifyPolynomialRegularity(a, b, classification.PrimaryParameter.Value),

            // f(n) = c·n^k·log^j(n)
            // For large n, the polynomial term dominates, so same analysis applies
            // with an additional log factor that doesn't affect the ratio significantly
            ExpressionForm.PolyLog when classification.PrimaryParameter.HasValue =>
                VerifyPolyLogRegularity(a, b, classification.PrimaryParameter.Value, classification.LogExponent ?? 0),

            _ => null // Can't verify analytically
        };
    }

    private RegularityResult VerifyPolynomialRegularity(double a, double b, double k)
    {
        // c = a / b^k
        var c = a / Math.Pow(b, k);

        if (c < MaxC)
        {
            return RegularityResult.Success(
                c,
                $"Polynomial regularity: a·f(n/b) = (a/b^k)·f(n) = {c:F6}·f(n) < f(n)",
                confidence: 1.0);
        }

        return RegularityResult.Failure(
            $"Polynomial regularity fails: c = a/b^k = {c:F6} ≥ 1",
            confidence: 1.0);
    }

    private RegularityResult VerifyPolyLogRegularity(double a, double b, double k, double j)
    {
        // For f(n) = n^k · log^j(n):
        // a·f(n/b) = a·(n/b)^k · log^j(n/b)
        //          = (a/b^k)·n^k · log^j(n/b)
        //          = (a/b^k)·n^k · (log(n) - log(b))^j
        //
        // For large n: log(n/b) ≈ log(n), so ratio approaches a/b^k
        // The log factor gives us extra room since (log(n) - log(b))^j < log^j(n)

        var baseC = a / Math.Pow(b, k);

        if (baseC < MaxC)
        {
            // The actual c is slightly better due to log factor
            // For j > 0: (log(n) - log(b))^j / log^j(n) < 1, improving the bound
            var effectiveC = baseC * Math.Pow(0.9, j); // Conservative estimate

            return RegularityResult.Success(
                Math.Min(baseC, effectiveC),
                $"PolyLog regularity: base ratio a/b^k = {baseC:F6}, log factor improves bound",
                confidence: 0.95);
        }

        return RegularityResult.Failure(
            $"PolyLog regularity likely fails: base ratio a/b^k = {baseC:F6} ≥ 1",
            confidence: 0.95);
    }

    /// <summary>
    /// Numerical verification by sampling f(n) at multiple points.
    /// </summary>
    private RegularityResult NumericalVerification(
        double a,
        double b,
        ComplexityExpression f,
        Variable variable,
        double[] samplePoints)
    {
        var ratios = new List<double>();

        foreach (var n in samplePoints)
        {
            var assignments = new Dictionary<Variable, double> { { variable, n } };
            var fn = f.Evaluate(assignments);

            var scaledAssignments = new Dictionary<Variable, double> { { variable, n / b } };
            var fnb = f.Evaluate(scaledAssignments);

            if (!fn.HasValue || !fnb.HasValue || fn.Value <= Tolerance)
            {
                continue; // Skip invalid evaluations
            }

            // Compute ratio: a·f(n/b) / f(n)
            var ratio = a * fnb.Value / fn.Value;
            ratios.Add(ratio);
        }

        if (ratios.Count < 3)
        {
            return RegularityResult.Indeterminate(
                "Insufficient valid sample points for numerical verification");
        }

        var maxRatio = ratios.Max();
        var avgRatio = ratios.Average();

        // Check if all ratios are bounded by some c < 1
        if (maxRatio < MaxC)
        {
            return RegularityResult.Success(
                maxRatio,
                $"Numerical verification: max ratio = {maxRatio:F6} < 1 across {ratios.Count} samples",
                confidence: 0.85)
            {
                SamplePoints = samplePoints.Take(ratios.Count).ToList()
            };
        }

        // Check if ratios are trending toward a value < 1
        if (avgRatio < 0.95 && ratios.TakeLast(3).Max() < MaxC)
        {
            return RegularityResult.Success(
                ratios.TakeLast(3).Max(),
                $"Numerical verification: ratio converging to ~{avgRatio:F4}, recent max = {ratios.TakeLast(3).Max():F6}",
                confidence: 0.7)
            {
                SamplePoints = samplePoints.Take(ratios.Count).ToList()
            };
        }

        return RegularityResult.Failure(
            $"Numerical verification failed: max ratio = {maxRatio:F6} ≥ 1",
            confidence: 0.85)
        {
            SamplePoints = samplePoints.Take(ratios.Count).ToList()
        };
    }
}
