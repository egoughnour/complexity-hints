using MathNet.Numerics.RootFinding;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Solves for the critical exponent p in Akra-Bazzi theorem.
/// Uses MathNet.Numerics for root finding.
/// </summary>
public interface ICriticalExponentSolver
{
    /// <summary>
    /// Solves Σᵢ aᵢ · bᵢ^p = 1 for p.
    /// </summary>
    /// <param name="terms">The (aᵢ, bᵢ) pairs from the recurrence.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <param name="maxIterations">Maximum iterations for root finding.</param>
    /// <returns>The critical exponent p, or null if no solution found.</returns>
    double? Solve(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double tolerance = 1e-10,
        int maxIterations = 100);

    /// <summary>
    /// Evaluates Σᵢ aᵢ · bᵢ^p for a given p.
    /// </summary>
    double EvaluateSum(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double p);

    /// <summary>
    /// Evaluates the derivative d/dp[Σᵢ aᵢ · bᵢ^p] = Σᵢ aᵢ · bᵢ^p · ln(bᵢ).
    /// </summary>
    double EvaluateDerivative(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double p);
}

/// <summary>
/// Standard implementation using MathNet.Numerics root finding.
/// </summary>
public sealed class MathNetCriticalExponentSolver : ICriticalExponentSolver
{
    public static readonly MathNetCriticalExponentSolver Instance = new();

    public double? Solve(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double tolerance = 1e-10,
        int maxIterations = 100)
    {
        if (terms.Count == 0)
            return null;

        // Validate: all coefficients > 0, all scale factors in (0, 1)
        if (!terms.All(t => t.Coefficient > 0 && t.ScaleFactor > 0 && t.ScaleFactor < 1))
            return null;

        // Define f(p) = Σᵢ aᵢ · bᵢ^p - 1
        // We want to find p where f(p) = 0
        double f(double p) => EvaluateSum(terms, p) - 1;
        double df(double p) => EvaluateDerivative(terms, p);

        // For Akra-Bazzi with 0 < bᵢ < 1 and aᵢ > 0:
        // - As p → -∞: Σᵢ aᵢ · bᵢ^p → +∞ (since bᵢ < 1, bᵢ^p → ∞)
        // - As p → +∞: Σᵢ aᵢ · bᵢ^p → 0
        // - f(p) is strictly decreasing
        // So there's exactly one root if f(0) > 0 (i.e., Σᵢ aᵢ > 1)
        // or exactly one root if f(0) < 0 (i.e., Σᵢ aᵢ < 1)

        // Find a bracketing interval
        double sumAtZero = EvaluateSum(terms, 0); // = Σᵢ aᵢ

        double lowerBound, upperBound;

        if (sumAtZero > 1)
        {
            // Root is at positive p
            lowerBound = 0;
            upperBound = 1;
            // Expand upper bound until f(upperBound) < 0
            while (f(upperBound) > 0 && upperBound < 1000)
                upperBound *= 2;
        }
        else if (sumAtZero < 1)
        {
            // Root is at negative p
            upperBound = 0;
            lowerBound = -1;
            // Expand lower bound until f(lowerBound) > 0
            while (f(lowerBound) < 0 && lowerBound > -1000)
                lowerBound *= 2;
        }
        else
        {
            // Exactly sumAtZero == 1, so p = 0 is the root
            return 0;
        }

        // Use Newton-Raphson with MathNet.Numerics
        try
        {
            // Try Newton-Raphson first (faster convergence)
            var result = NewtonRaphson.FindRoot(f, df, lowerBound, upperBound, tolerance, maxIterations);
            return result;
        }
        catch
        {
            // Fall back to Brent's method (more robust)
            try
            {
                var result = Brent.FindRoot(f, lowerBound, upperBound, tolerance, maxIterations);
                return result;
            }
            catch
            {
                return null;
            }
        }
    }

    public double EvaluateSum(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double p)
    {
        return terms.Sum(t => t.Coefficient * Math.Pow(t.ScaleFactor, p));
    }

    public double EvaluateDerivative(
        IReadOnlyList<(double Coefficient, double ScaleFactor)> terms,
        double p)
    {
        // d/dp[Σᵢ aᵢ · bᵢ^p] = Σᵢ aᵢ · bᵢ^p · ln(bᵢ)
        return terms.Sum(t => t.Coefficient * Math.Pow(t.ScaleFactor, p) * Math.Log(t.ScaleFactor));
    }
}

/// <summary>
/// Known solutions for common recurrence patterns.
/// Used for verification and optimization.
/// </summary>
public static class KnownCriticalExponents
{
    /// <summary>
    /// For T(n) = aT(n/b) + f(n): p = log_b(a).
    /// </summary>
    public static double MasterTheorem(double a, double b) =>
        Math.Log(a) / Math.Log(b);

    /// <summary>
    /// For T(n) = 2T(n/2) + f(n): p = 1.
    /// </summary>
    public static double BinaryDivideAndConquer => 1.0;

    /// <summary>
    /// For T(n) = T(n/2) + f(n): p = 0.
    /// </summary>
    public static double BinarySearch => 0.0;

    /// <summary>
    /// For T(n) = 3T(n/2) + f(n): p = log_2(3) ≈ 1.585.
    /// </summary>
    public static double Karatsuba => Math.Log(3) / Math.Log(2);

    /// <summary>
    /// For T(n) = 7T(n/2) + f(n): p = log_2(7) ≈ 2.807 (Strassen).
    /// </summary>
    public static double Strassen => Math.Log(7) / Math.Log(2);
}
