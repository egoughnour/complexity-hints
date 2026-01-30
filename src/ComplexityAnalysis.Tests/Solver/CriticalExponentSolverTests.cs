using ComplexityAnalysis.Solver;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Tests for the critical exponent solver used in Akra-Bazzi theorem.
/// </summary>
public class CriticalExponentSolverTests
{
    private readonly ICriticalExponentSolver _solver = MathNetCriticalExponentSolver.Instance;

    /// <summary>
    /// Binary divide and conquer: 2·(1/2)^p = 1 → p = 1
    /// </summary>
    [Fact]
    public void BinaryDivideAndConquer_SolvesToOne()
    {
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(1.0, p.Value, precision: 6);
    }

    /// <summary>
    /// Binary search: 1·(1/2)^p = 1 → p = 0
    /// </summary>
    [Fact]
    public void BinarySearch_SolvesToZero()
    {
        var terms = new[] { (Coefficient: 1.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(0.0, p.Value, precision: 6);
    }

    /// <summary>
    /// Karatsuba: 3·(1/2)^p = 1 → p = log₂(3) ≈ 1.585
    /// </summary>
    [Fact]
    public void Karatsuba_SolvesToLog2Of3()
    {
        var terms = new[] { (Coefficient: 3.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(3) / Math.Log(2);
        Assert.Equal(expected, p.Value, precision: 6);
    }

    /// <summary>
    /// Strassen: 7·(1/2)^p = 1 → p = log₂(7) ≈ 2.807
    /// </summary>
    [Fact]
    public void Strassen_SolvesToLog2Of7()
    {
        var terms = new[] { (Coefficient: 7.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(7) / Math.Log(2);
        Assert.Equal(expected, p.Value, precision: 6);
    }

    /// <summary>
    /// Unbalanced partition: 1·(1/3)^p + 1·(2/3)^p = 1 → p = 1
    /// </summary>
    [Fact]
    public void UnbalancedPartition_SolvesToOne()
    {
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 1.0/3),
            (Coefficient: 1.0, ScaleFactor: 2.0/3)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        // (1/3)^1 + (2/3)^1 = 1/3 + 2/3 = 1 ✓
        Assert.Equal(1.0, p.Value, precision: 4);
    }

    /// <summary>
    /// Multiple terms: 2·(1/4)^p + 1·(1/2)^p = 1
    /// Need to find p such that 2/4^p + 1/2^p = 1
    /// </summary>
    [Fact]
    public void MultipleTerms_ConvergesToSolution()
    {
        var terms = new[]
        {
            (Coefficient: 2.0, ScaleFactor: 0.25),  // 2T(n/4)
            (Coefficient: 1.0, ScaleFactor: 0.5)   // T(n/2)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);

        // Verify the solution
        var sum = _solver.EvaluateSum(terms.ToList(), p.Value);
        Assert.Equal(1.0, sum, precision: 6);
    }

    /// <summary>
    /// Tests the evaluation function directly.
    /// </summary>
    [Fact]
    public void EvaluateSum_ComputesCorrectly()
    {
        var terms = new[]
        {
            (Coefficient: 2.0, ScaleFactor: 0.5),  // 2·(1/2)^p
        };

        // At p=1: 2·(1/2)^1 = 1
        var sumAtOne = _solver.EvaluateSum(terms.ToList(), 1.0);
        Assert.Equal(1.0, sumAtOne, precision: 6);

        // At p=0: 2·(1/2)^0 = 2
        var sumAtZero = _solver.EvaluateSum(terms.ToList(), 0.0);
        Assert.Equal(2.0, sumAtZero, precision: 6);

        // At p=2: 2·(1/2)^2 = 0.5
        var sumAtTwo = _solver.EvaluateSum(terms.ToList(), 2.0);
        Assert.Equal(0.5, sumAtTwo, precision: 6);
    }

    /// <summary>
    /// Tests the derivative function.
    /// </summary>
    [Fact]
    public void EvaluateDerivative_ComputesCorrectly()
    {
        var terms = new[]
        {
            (Coefficient: 2.0, ScaleFactor: 0.5),
        };

        // Derivative of 2·(1/2)^p = 2·(1/2)^p · ln(1/2) = -2·ln(2)·(1/2)^p
        // At p=1: -2·ln(2)·(1/2) = -ln(2) ≈ -0.693
        var deriv = _solver.EvaluateDerivative(terms.ToList(), 1.0);
        var expected = -Math.Log(2);
        Assert.Equal(expected, deriv, precision: 4);
    }

    /// <summary>
    /// Tests known critical exponents from the precomputed table.
    /// </summary>
    [Theory]
    [InlineData(2, 2, 1.0)]      // Binary D&C
    [InlineData(1, 2, 0.0)]      // Binary search
    [InlineData(3, 2, 1.585)]    // Karatsuba (approximate)
    [InlineData(7, 2, 2.807)]    // Strassen (approximate)
    [InlineData(4, 2, 2.0)]      // a=4, b=2
    [InlineData(8, 2, 3.0)]      // a=8, b=2
    public void KnownExponents_MatchExpected(int a, int b, double expectedApprox)
    {
        var terms = new[] { (Coefficient: (double)a, ScaleFactor: 1.0 / b) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(expectedApprox, p.Value, precision: 2);
    }
}
