using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver.Refinement;

/// <summary>
/// Handles near-boundary cases where standard theorems have gaps.
/// Uses perturbation analysis and Taylor expansion to derive tighter bounds.
///
/// Key cases:
/// 1. Master Theorem gap: f(n) = Θ(n^d) where d ≈ log_b(a)
/// 2. Akra-Bazzi boundary: p ≈ integer values
/// 3. Logarithmic factor boundaries: log^k(n) where k is non-integer
/// </summary>
public sealed class PerturbationExpansion : IPerturbationExpansion
{
    /// <summary>Threshold for considering values "near" each other.</summary>
    private const double NearThreshold = 0.1;

    /// <summary>Maximum order of Taylor expansion.</summary>
    private const int MaxTaylorOrder = 4;

    /// <summary>Tolerance for numerical comparisons.</summary>
    private const double Tolerance = 1e-9;

    public static PerturbationExpansion Instance { get; } = new();

    /// <summary>
    /// Expands a recurrence solution near a boundary case.
    /// </summary>
    public PerturbationResult ExpandNearBoundary(
        RecurrenceRelation recurrence,
        BoundaryCase boundaryCase)
    {
        return boundaryCase.Type switch
        {
            BoundaryCaseType.MasterTheoremCase1To2 =>
                ExpandMasterCase1To2(recurrence, boundaryCase),
            BoundaryCaseType.MasterTheoremCase2To3 =>
                ExpandMasterCase2To3(recurrence, boundaryCase),
            BoundaryCaseType.AkraBazziIntegerExponent =>
                ExpandAkraBazziInteger(recurrence, boundaryCase),
            BoundaryCaseType.LogarithmicBoundary =>
                ExpandLogarithmicBoundary(recurrence, boundaryCase),
            _ => PerturbationResult.Failed("Unknown boundary case type")
        };
    }

    /// <summary>
    /// Detects if a recurrence is near a boundary case.
    /// </summary>
    public BoundaryCase? DetectBoundary(RecurrenceRelation recurrence, TheoremApplicability result)
    {
        if (!recurrence.FitsMasterTheorem && !recurrence.FitsAkraBazzi)
            return null;

        if (recurrence.FitsMasterTheorem)
        {
            var term = recurrence.Terms[0];
            var a = term.Coefficient;
            var b = 1.0 / term.ScaleFactor;
            var logBA = Math.Log(a) / Math.Log(b);

            // Extract f(n) degree
            var fDegree = ExtractPolynomialDegree(recurrence.NonRecursiveWork, recurrence.Variable);

            if (fDegree.HasValue)
            {
                var diff = fDegree.Value - logBA;

                // Near Case 1/Case 2 boundary
                if (Math.Abs(diff) < NearThreshold && diff < 0)
                {
                    return new BoundaryCase
                    {
                        Type = BoundaryCaseType.MasterTheoremCase1To2,
                        LogBA = logBA,
                        FDegree = fDegree.Value,
                        Delta = diff,
                        Description = $"f(n) = Θ(n^{fDegree.Value:F4}) is near boundary with n^{logBA:F4}"
                    };
                }

                // Near Case 2/Case 3 boundary
                if (Math.Abs(diff) < NearThreshold && diff > 0)
                {
                    return new BoundaryCase
                    {
                        Type = BoundaryCaseType.MasterTheoremCase2To3,
                        LogBA = logBA,
                        FDegree = fDegree.Value,
                        Delta = diff,
                        Description = $"f(n) = Θ(n^{fDegree.Value:F4}) is near boundary with n^{logBA:F4}"
                    };
                }
            }
        }

        // Check for Akra-Bazzi near-integer exponent
        if (result is AkraBazziApplicable akra)
        {
            var p = akra.CriticalExponent;
            var nearestInt = Math.Round(p);

            if (Math.Abs(p - nearestInt) < NearThreshold && Math.Abs(p - nearestInt) > Tolerance)
            {
                return new BoundaryCase
                {
                    Type = BoundaryCaseType.AkraBazziIntegerExponent,
                    CriticalExponent = p,
                    NearestInteger = (int)nearestInt,
                    Delta = p - nearestInt,
                    Description = $"Critical exponent p = {p:F4} is near integer {nearestInt}"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Performs Taylor expansion of the Akra-Bazzi integral near a singular point.
    /// </summary>
    public TaylorExpansionResult TaylorExpandIntegral(
        ComplexityExpression g,
        Variable variable,
        double p,
        double expansionPoint)
    {
        // For g(n) = n^k, the Akra-Bazzi integral is ∫₁ⁿ g(u)/u^(p+1) du
        // Near p = k, this has logarithmic terms

        var gDegree = ExtractPolynomialDegree(g, variable);
        if (!gDegree.HasValue)
        {
            return TaylorExpansionResult.Failed("Cannot extract polynomial degree from g(n)");
        }

        var k = gDegree.Value;
        var delta = p - k;

        // When p → k:
        // ∫₁ⁿ u^(k-p-1) du = ∫₁ⁿ u^(-δ-1) du
        // = [u^(-δ) / (-δ)]₁ⁿ for δ ≠ 0
        // = [log u]₁ⁿ = log n for δ = 0

        var terms = new List<TaylorTerm>();

        if (Math.Abs(delta) < Tolerance)
        {
            // Exactly at the boundary: log(n) term
            terms.Add(new TaylorTerm(0, new LogarithmicComplexity(1, variable)));
        }
        else
        {
            // Near the boundary: expand in powers of δ
            // (n^(-δ) - 1) / (-δ) ≈ log(n) + δ·log²(n)/2 + O(δ²)

            // Leading term
            terms.Add(new TaylorTerm(0, new LogarithmicComplexity(1, variable)));

            // First correction
            if (MaxTaylorOrder >= 1)
            {
                var logSquared = new BinaryOperationComplexity(
                    new LogarithmicComplexity(1, variable),
                    BinaryOp.Multiply,
                    new LogarithmicComplexity(1, variable));

                terms.Add(new TaylorTerm(1, new BinaryOperationComplexity(
                    new ConstantComplexity(0.5),
                    BinaryOp.Multiply,
                    logSquared)));
            }

            // Higher order corrections
            for (int order = 2; order <= Math.Min(MaxTaylorOrder, 3); order++)
            {
                var coefficient = 1.0 / Factorial(order + 1);
                var logPower = CreateLogPower(variable, order + 1);

                terms.Add(new TaylorTerm(order,
                    new BinaryOperationComplexity(
                        new ConstantComplexity(coefficient),
                        BinaryOp.Multiply,
                        logPower)));
            }
        }

        return new TaylorExpansionResult
        {
            Success = true,
            ExpansionPoint = expansionPoint,
            Terms = terms.ToImmutableList(),
            LeadingOrder = 0,
            Delta = delta
        };
    }

    #region Private Methods

    private PerturbationResult ExpandMasterCase1To2(
        RecurrenceRelation recurrence,
        BoundaryCase boundary)
    {
        // Near Case 1/Case 2: f(n) = O(n^(log_b(a) - ε)) for small ε
        // Solution transitions from Θ(n^(log_b(a))) to Θ(n^(log_b(a)) log n)

        var variable = recurrence.Variable;
        var logBA = boundary.LogBA;
        var delta = Math.Abs(boundary.Delta);

        // Perturbation expansion: T(n) = n^(log_b(a)) · (1 + δ·log(n) + O(δ²))
        // For small δ, this gives approximately n^(log_b(a)) with logarithmic corrections

        ComplexityExpression solution;
        var confidence = 0.0;

        if (delta < 0.01)
        {
            // Very close to boundary: include log factor
            solution = new PolyLogComplexity(logBA, 1, variable);
            confidence = 0.9;
        }
        else if (delta < 0.05)
        {
            // Close to boundary: use average
            solution = PolyLogComplexity.Polynomial(logBA, variable);
            confidence = 0.8;
        }
        else
        {
            // Further from boundary: Case 1 dominates
            solution = PolyLogComplexity.Polynomial(logBA, variable);
            confidence = 0.85;
        }

        return new PerturbationResult
        {
            Success = true,
            Solution = solution,
            Confidence = confidence,
            Method = "Master Theorem Case 1/2 perturbation",
            Expansion = CreateExpansionTerms(logBA, delta, variable),
            Diagnostics = ImmutableList.Create(
                $"δ = {delta:F6}",
                $"log_b(a) = {logBA:F4}",
                $"Correction order: {(delta < 0.01 ? "logarithmic" : "polynomial")}")
        };
    }

    private PerturbationResult ExpandMasterCase2To3(
        RecurrenceRelation recurrence,
        BoundaryCase boundary)
    {
        // Near Case 2/Case 3: f(n) = Ω(n^(log_b(a) + ε)) for small ε
        // Solution transitions from Θ(n^(log_b(a)) log n) to Θ(f(n))

        var variable = recurrence.Variable;
        var logBA = boundary.LogBA;
        var fDegree = boundary.FDegree;
        var delta = boundary.Delta;

        ComplexityExpression solution;
        var confidence = 0.0;

        if (delta < 0.01)
        {
            // Very close to boundary: Case 2 with possible extra log
            solution = new PolyLogComplexity(logBA, 1, variable);
            confidence = 0.85;
        }
        else if (delta < 0.05)
        {
            // Transitioning: f(n) starting to dominate
            // Use geometric mean of the two cases
            var avgDegree = (logBA + fDegree) / 2;
            solution = PolyLogComplexity.Polynomial(avgDegree, variable);
            confidence = 0.75;
        }
        else
        {
            // Case 3 dominates
            solution = PolyLogComplexity.Polynomial(fDegree, variable);
            confidence = 0.9;
        }

        return new PerturbationResult
        {
            Success = true,
            Solution = solution,
            Confidence = confidence,
            Method = "Master Theorem Case 2/3 perturbation",
            Expansion = CreateExpansionTerms(logBA, delta, variable),
            Diagnostics = ImmutableList.Create(
                $"δ = {delta:F6}",
                $"log_b(a) = {logBA:F4}",
                $"f(n) degree = {fDegree:F4}")
        };
    }

    private PerturbationResult ExpandAkraBazziInteger(
        RecurrenceRelation recurrence,
        BoundaryCase boundary)
    {
        // Near-integer critical exponent: p ≈ k for integer k
        // Logarithmic corrections appear: T(n) = n^k · log^j(n) for some j

        var variable = recurrence.Variable;
        var p = boundary.CriticalExponent;
        var k = boundary.NearestInteger;
        var delta = p - k;

        // Expand the integral around p = k
        var g = recurrence.NonRecursiveWork;
        var integralExpansion = TaylorExpandIntegral(g, variable, p, k);

        ComplexityExpression solution;
        var confidence = 0.0;

        if (Math.Abs(delta) < 0.001)
        {
            // Extremely close: definitely has log factor
            solution = new PolyLogComplexity(k, 1, variable);
            confidence = 0.95;
        }
        else if (Math.Abs(delta) < 0.05)
        {
            // Close: likely has log factor
            var logExponent = EstimateLogExponent(delta);
            solution = new PolyLogComplexity(k, logExponent, variable);
            confidence = 0.85;
        }
        else
        {
            // Not that close: use exact p
            solution = PolyLogComplexity.Polynomial(p, variable);
            confidence = 0.9;
        }

        return new PerturbationResult
        {
            Success = true,
            Solution = solution,
            Confidence = confidence,
            Method = "Akra-Bazzi near-integer perturbation",
            Expansion = integralExpansion.Terms,
            Diagnostics = ImmutableList.Create(
                $"p = {p:F6}",
                $"Nearest integer k = {k}",
                $"δ = p - k = {delta:F6}")
        };
    }

    private PerturbationResult ExpandLogarithmicBoundary(
        RecurrenceRelation recurrence,
        BoundaryCase boundary)
    {
        // When the solution involves log^k(n) for non-integer k
        // This can arise from certain Akra-Bazzi integrals

        var variable = recurrence.Variable;

        // Approximate non-integer log exponent with nearest integer
        // and add correction terms

        var logExponent = boundary.LogExponent;
        var nearestLogExponent = (int)Math.Round(logExponent);

        var correction = logExponent - nearestLogExponent;

        ComplexityExpression solution;

        if (Math.Abs(correction) < 0.1)
        {
            // Use integer log exponent
            solution = new PolyLogComplexity(boundary.PolynomialDegree, nearestLogExponent, variable);
        }
        else
        {
            // Keep fractional exponent (represented as product)
            var basePart = new PolyLogComplexity(boundary.PolynomialDegree, nearestLogExponent, variable);
            var correctionPart = new LogOfComplexity(
                new PowerComplexity(new VariableComplexity(variable), correction));
            solution = new BinaryOperationComplexity(basePart, BinaryOp.Multiply, correctionPart);
        }

        return new PerturbationResult
        {
            Success = true,
            Solution = solution,
            Confidence = 0.8,
            Method = "Logarithmic boundary perturbation",
            Diagnostics = ImmutableList.Create(
                $"Log exponent = {logExponent:F4}",
                $"Approximation = {nearestLogExponent}",
                $"Correction = {correction:F4}")
        };
    }

    private ImmutableList<TaylorTerm> CreateExpansionTerms(double baseExponent, double delta, Variable variable)
    {
        var terms = new List<TaylorTerm>
        {
            new(0, PolyLogComplexity.Polynomial(baseExponent, variable))
        };

        if (Math.Abs(delta) > Tolerance)
        {
            // First-order correction: δ · n^d · log(n)
            terms.Add(new TaylorTerm(1,
                new PolyLogComplexity(baseExponent, 1, variable)));
        }

        return terms.ToImmutableList();
    }

    private int EstimateLogExponent(double delta)
    {
        // For small δ, the log exponent is approximately 1
        // For larger δ, it may be 0 (no log factor)
        return Math.Abs(delta) < 0.02 ? 1 : 0;
    }

    private double? ExtractPolynomialDegree(ComplexityExpression expr, Variable variable)
    {
        return expr switch
        {
            ConstantComplexity => 0,
            VariableComplexity v when v.Var.Equals(variable) => 1,
            LinearComplexity l when l.Var.Equals(variable) => 1,
            PolynomialComplexity p when p.Var.Equals(variable) => p.Degree,
            PolyLogComplexity pl when pl.Variable.Equals(variable) => pl.PolynomialDegree,
            _ => null
        };
    }

    private ComplexityExpression CreateLogPower(Variable variable, int power)
    {
        ComplexityExpression result = new LogarithmicComplexity(1, variable);
        for (int i = 1; i < power; i++)
        {
            result = new BinaryOperationComplexity(result, BinaryOp.Multiply,
                new LogarithmicComplexity(1, variable));
        }
        return result;
    }

    private static double Factorial(int n)
    {
        double result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    #endregion
}

#region Types

/// <summary>
/// Interface for perturbation expansion.
/// </summary>
public interface IPerturbationExpansion
{
    PerturbationResult ExpandNearBoundary(RecurrenceRelation recurrence, BoundaryCase boundaryCase);
    BoundaryCase? DetectBoundary(RecurrenceRelation recurrence, TheoremApplicability result);
    TaylorExpansionResult TaylorExpandIntegral(ComplexityExpression g, Variable variable, double p, double expansionPoint);
}

/// <summary>
/// Result of perturbation expansion.
/// </summary>
public sealed record PerturbationResult
{
    public bool Success { get; init; }
    public ComplexityExpression? Solution { get; init; }
    public double Confidence { get; init; }
    public string? Method { get; init; }
    public ImmutableList<TaylorTerm> Expansion { get; init; } = ImmutableList<TaylorTerm>.Empty;
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }

    public static PerturbationResult Failed(string reason) =>
        new() { Success = false, ErrorMessage = reason };
}

/// <summary>
/// Description of a boundary case.
/// </summary>
public sealed record BoundaryCase
{
    public BoundaryCaseType Type { get; init; }
    public double LogBA { get; init; }
    public double FDegree { get; init; }
    public double CriticalExponent { get; init; }
    public int NearestInteger { get; init; }
    public double Delta { get; init; }
    public double LogExponent { get; init; }
    public double PolynomialDegree { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Types of boundary cases.
/// </summary>
public enum BoundaryCaseType
{
    /// <summary>Near boundary between Master Theorem Case 1 and Case 2.</summary>
    MasterTheoremCase1To2,

    /// <summary>Near boundary between Master Theorem Case 2 and Case 3.</summary>
    MasterTheoremCase2To3,

    /// <summary>Akra-Bazzi critical exponent near an integer.</summary>
    AkraBazziIntegerExponent,

    /// <summary>Logarithmic exponent boundary.</summary>
    LogarithmicBoundary
}

/// <summary>
/// Result of Taylor expansion.
/// </summary>
public sealed record TaylorExpansionResult
{
    public bool Success { get; init; }
    public double ExpansionPoint { get; init; }
    public ImmutableList<TaylorTerm> Terms { get; init; } = ImmutableList<TaylorTerm>.Empty;
    public int LeadingOrder { get; init; }
    public double Delta { get; init; }
    public string? ErrorMessage { get; init; }

    public static TaylorExpansionResult Failed(string reason) =>
        new() { Success = false, ErrorMessage = reason };
}

/// <summary>
/// A term in a Taylor expansion.
/// </summary>
public sealed record TaylorTerm(int Order, ComplexityExpression Expression);

#endregion
