using ComplexityAnalysis.Solver;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Extended tests for critical exponent solver covering:
/// - Large coefficient values
/// - Small scale factors
/// - Many-term recurrences (4+)
/// - Numerical stability edge cases
/// - Convergence behavior
/// - Irrational exponent precision
/// </summary>
public class ExtendedCriticalExponentTests
{
    private readonly ICriticalExponentSolver _solver = MathNetCriticalExponentSolver.Instance;

    #region Large Coefficient Tests

    [Theory]
    [InlineData(100, 2)]    // a=100 → p = log_2(100) ≈ 6.64
    [InlineData(256, 2)]    // a=256 → p = log_2(256) = 8
    [InlineData(1000, 2)]   // a=1000 → p = log_2(1000) ≈ 9.97
    [InlineData(1024, 2)]   // a=1024 → p = log_2(1024) = 10
    public void LargeCoefficient_ConvergesToCorrectExponent(int a, int b)
    {
        var terms = new[] { (Coefficient: (double)a, ScaleFactor: 1.0 / b) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(a) / Math.Log(b);
        Assert.Equal(expected, p.Value, precision: 4);
    }

    [Fact]
    public void VeryLargeCoefficient_StillConverges()
    {
        // a = 10000, b = 2 → p ≈ 13.29
        var terms = new[] { (Coefficient: 10000.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(10000) / Math.Log(2);
        Assert.Equal(expected, p.Value, precision: 3);
    }

    #endregion

    #region Small Scale Factor Tests

    [Theory]
    [InlineData(2, 10)]    // b=10 → scale=0.1
    [InlineData(3, 10)]    // Karatsuba-like with base 10
    [InlineData(2, 100)]   // b=100 → scale=0.01
    public void SmallScaleFactor_ConvergesToCorrectExponent(int a, int b)
    {
        var terms = new[] { (Coefficient: (double)a, ScaleFactor: 1.0 / b) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(a) / Math.Log(b);
        Assert.Equal(expected, p.Value, precision: 4);
    }

    [Fact]
    public void VerySmallScaleFactor_StillConverges()
    {
        // scale = 0.001 (b = 1000)
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 0.001) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        // 2 * 0.001^p = 1 → p = log(2) / log(1000) ≈ 0.1
        var expected = Math.Log(2) / Math.Log(1000);
        Assert.Equal(expected, p.Value, precision: 3);
    }

    #endregion

    #region Many-Term Recurrence Tests

    [Fact]
    public void ThreeTermRecurrence_Converges()
    {
        // T(n/3) + T(n/3) + T(n/3) + work = 3T(n/3) + work
        // 3 * (1/3)^p = 1 → p = 1
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 1.0/3),
            (Coefficient: 1.0, ScaleFactor: 1.0/3),
            (Coefficient: 1.0, ScaleFactor: 1.0/3)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(1.0, p.Value, precision: 4);
    }

    [Fact]
    public void FourTermRecurrence_Converges()
    {
        // T(n/4) + T(n/4) + T(n/4) + T(n/4) = 4T(n/4)
        // 4 * (1/4)^p = 1 → p = 1
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 0.25),
            (Coefficient: 1.0, ScaleFactor: 0.25),
            (Coefficient: 1.0, ScaleFactor: 0.25),
            (Coefficient: 1.0, ScaleFactor: 0.25)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(1.0, p.Value, precision: 4);
    }

    [Fact]
    public void FiveTermMixedRecurrence_Converges()
    {
        // Mixed: T(n/2) + T(n/3) + T(n/4) + T(n/5) + T(n/6)
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 0.5),
            (Coefficient: 1.0, ScaleFactor: 1.0/3),
            (Coefficient: 1.0, ScaleFactor: 0.25),
            (Coefficient: 1.0, ScaleFactor: 0.2),
            (Coefficient: 1.0, ScaleFactor: 1.0/6)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        // Verify the solution
        var sum = _solver.EvaluateSum(terms.ToList(), p.Value);
        Assert.Equal(1.0, sum, precision: 4);
    }

    [Fact]
    public void SixTermRecurrence_DifferentCoefficients_Converges()
    {
        // 2T(n/2) + 3T(n/3) + T(n/4)
        var terms = new[]
        {
            (Coefficient: 2.0, ScaleFactor: 0.5),
            (Coefficient: 3.0, ScaleFactor: 1.0/3),
            (Coefficient: 1.0, ScaleFactor: 0.25)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var sum = _solver.EvaluateSum(terms.ToList(), p.Value);
        Assert.Equal(1.0, sum, precision: 4);
    }

    #endregion

    #region Numerical Stability Tests

    [Fact]
    public void NearZeroExponent_Converges()
    {
        // 1 * (1/2)^p = 1 → p = 0
        var terms = new[] { (Coefficient: 1.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(0.0, p.Value, precision: 6);
    }

    [Fact]
    public void LargeExponent_Converges()
    {
        // 2^20 * (1/2)^p = 1 → p = 20
        var largeCoeff = Math.Pow(2, 20); // 1,048,576
        var terms = new[] { (Coefficient: largeCoeff, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(20.0, p.Value, precision: 2);
    }

    [Fact]
    public void CloseScaleFactors_StillDistinguishes()
    {
        // Two very close scale factors
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 0.500),
            (Coefficient: 1.0, ScaleFactor: 0.501)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var sum = _solver.EvaluateSum(terms.ToList(), p.Value);
        Assert.Equal(1.0, sum, precision: 4);
    }

    [Fact]
    public void SmallCoefficients_Converges()
    {
        // 0.1 * (1/2)^p + 0.9 * (1/3)^p = 1
        var terms = new[]
        {
            (Coefficient: 0.1, ScaleFactor: 0.5),
            (Coefficient: 0.9, ScaleFactor: 1.0/3)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var sum = _solver.EvaluateSum(terms.ToList(), p.Value);
        Assert.Equal(1.0, sum, precision: 4);
    }

    #endregion

    #region Irrational Exponent Precision Tests

    [Theory]
    [InlineData(3, 2, 6)]    // log_2(3) to 6 decimal places
    [InlineData(5, 2, 6)]    // log_2(5)
    [InlineData(7, 2, 6)]    // log_2(7) - Strassen
    [InlineData(11, 3, 5)]   // log_3(11)
    [InlineData(13, 4, 5)]   // log_4(13)
    public void IrrationalExponent_HighPrecision(int a, int b, int expectedPrecision)
    {
        var terms = new[] { (Coefficient: (double)a, ScaleFactor: 1.0 / b) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(a) / Math.Log(b);
        Assert.Equal(expected, p.Value, precision: expectedPrecision);
    }

    [Fact]
    public void GoldenRatioExponent_HighPrecision()
    {
        // φ = (1 + √5) / 2 ≈ 1.618
        // φ * (1/2)^p = 1 → p = log_2(φ) ≈ 0.694
        var phi = (1 + Math.Sqrt(5)) / 2;
        var terms = new[] { (Coefficient: phi, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        var expected = Math.Log(phi) / Math.Log(2);
        Assert.Equal(expected, p.Value, precision: 5);
    }

    #endregion

    #region Edge Cases and Invalid Inputs

    [Fact]
    public void InvalidCoefficient_Zero_ReturnsNull()
    {
        var terms = new[] { (Coefficient: 0.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void InvalidCoefficient_Negative_ReturnsNull()
    {
        var terms = new[] { (Coefficient: -1.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void InvalidScaleFactor_Zero_ReturnsNull()
    {
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 0.0) };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void InvalidScaleFactor_One_ReturnsNull()
    {
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 1.0) };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void InvalidScaleFactor_GreaterThanOne_ReturnsNull()
    {
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 1.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void EmptyTerms_ReturnsNull()
    {
        var terms = Array.Empty<(double, double)>();

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    [Fact]
    public void MixedValidInvalid_ReturnsNull()
    {
        // One valid, one invalid term
        var terms = new[]
        {
            (Coefficient: 2.0, ScaleFactor: 0.5),
            (Coefficient: -1.0, ScaleFactor: 0.25) // Invalid
        };

        var p = _solver.Solve(terms.ToList());

        Assert.Null(p);
    }

    #endregion

    #region Derivative Function Tests

    [Theory]
    [InlineData(1.0, -0.693)]   // At p=1: derivative = -ln(2)
    [InlineData(0.0, -1.386)]   // At p=0: derivative = 2*-ln(2)
    [InlineData(2.0, -0.347)]   // At p=2: derivative = 0.5*-ln(2)
    public void EvaluateDerivative_SingleTerm_ComputesCorrectly(double p, double expectedApprox)
    {
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 0.5) };

        var deriv = _solver.EvaluateDerivative(terms.ToList(), p);

        Assert.Equal(expectedApprox, deriv, precision: 2);
    }

    [Fact]
    public void EvaluateDerivative_MultiTerm_SumsCorrectly()
    {
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 0.5),
            (Coefficient: 1.0, ScaleFactor: 0.25)
        };

        var deriv = _solver.EvaluateDerivative(terms.ToList(), 1.0);

        // d/dp[0.5^p + 0.25^p] at p=1
        // = 0.5 * ln(0.5) + 0.25 * ln(0.25)
        // = 0.5 * (-0.693) + 0.25 * (-1.386)
        // ≈ -0.347 - 0.347 = -0.693
        Assert.True(deriv < 0); // Should be negative (monotonically decreasing)
    }

    #endregion

    #region Special Algorithm Patterns

    [Fact]
    public void SelectionAlgorithm_Converges()
    {
        // Median of medians: T(n) = T(n/5) + T(7n/10) + O(n)
        // Need to find p such that (1/5)^p + (7/10)^p = 1
        var terms = new[]
        {
            (Coefficient: 1.0, ScaleFactor: 0.2),
            (Coefficient: 1.0, ScaleFactor: 0.7)
        };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        // p should be close to 1 but slightly less
        Assert.True(p.Value < 1.1);
        Assert.True(p.Value > 0.5);
    }

    [Fact]
    public void ClosestPairAlgorithm_Converges()
    {
        // Closest pair: T(n) = 2T(n/2) + O(n log n)
        // Same as merge sort for the recurrence part
        var terms = new[] { (Coefficient: 2.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        Assert.Equal(1.0, p.Value, precision: 6);
    }

    [Fact]
    public void CoppersmithWinograd_LikePattern_Converges()
    {
        // Approximation of matrix multiplication improvements
        // T(n) = 7T(n/2) + n² (Strassen-like)
        var terms = new[] { (Coefficient: 7.0, ScaleFactor: 0.5) };

        var p = _solver.Solve(terms.ToList());

        Assert.NotNull(p);
        // log_2(7) ≈ 2.807
        Assert.Equal(Math.Log(7) / Math.Log(2), p.Value, precision: 4);
    }

    #endregion
}
