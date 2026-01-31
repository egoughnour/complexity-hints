using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// Tests for proper linear recurrence solving (T(n-k) patterns).
/// Uses the characteristic polynomial method implemented in LinearRecurrenceSolver.
///
/// Linear recurrence: T(n) = Σᵢ aᵢT(n-i) + f(n)
/// Solution methods: Characteristic equation, generating functions
/// </summary>
public class LinearRecurrenceTests
{
    private readonly ITestOutputHelper _output;
    private readonly LinearRecurrenceSolver _solver;

    public LinearRecurrenceTests(ITestOutputHelper output)
    {
        _output = output;
        _solver = LinearRecurrenceSolver.Instance;
    }

    #region Simple Linear Recurrences (Summation)

    [Fact]
    public void LinearRecurrence_ConstantWork_SolvesToLinear()
    {
        // T(n) = T(n-1) + O(1) → O(n)
        // This is summation: Σᵢ₌₁ⁿ 1 = n
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0 },  // T(n-1)
            nonRecursiveWork: ConstantComplexity.One,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-1) + 1: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        AssertLinear(result.Solution);
    }

    [Fact]
    public void LinearRecurrence_LinearWork_SolvesToQuadratic()
    {
        // T(n) = T(n-1) + O(n) → O(n²)
        // Summation: Σᵢ₌₁ⁿ i = n(n+1)/2 = O(n²)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0 },
            nonRecursiveWork: new LinearComplexity(1, Variable.N),
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-1) + n: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        AssertQuadratic(result.Solution);
    }

    [Fact]
    public void LinearRecurrence_LogWork_SolvesToNLogN()
    {
        // T(n) = T(n-1) + O(log n) → O(n log n)
        // Summation: Σᵢ₌₁ⁿ log(i) ≈ n log n - n
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0 },
            nonRecursiveWork: new LogarithmicComplexity(1, Variable.N),
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-1) + log n: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        AssertNLogN(result.Solution);
    }

    #endregion

    #region Fibonacci-Type Recurrences

    [Fact]
    public void Fibonacci_SolvesToExponential()
    {
        // T(n) = T(n-1) + T(n-2) + O(1)
        // Characteristic: x² = x + 1 → x = φ = (1+√5)/2
        // Solution: O(φⁿ) ≈ O(1.618ⁿ)
        var recurrence = LinearRecurrenceRelation.Fibonacci();

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"Fibonacci: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");
        _output.WriteLine($"Dominant root: {result?.DominantRoot}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity, 
            $"Expected ExponentialComplexity, got {result.Solution.GetType().Name}");
        
        // Base should be φ ≈ 1.618
        if (result.Solution is ExponentialComplexity exp)
        {
            var goldenRatio = (1 + Math.Sqrt(5)) / 2; // φ ≈ 1.618
            _output.WriteLine($"Expected base ≈ {goldenRatio:F4}, got {exp.Base:F4}");
            Assert.Equal(goldenRatio, exp.Base, precision: 2);
        }
    }

    [Fact]
    public void Tribonacci_SolvesToExponential()
    {
        // T(n) = T(n-1) + T(n-2) + T(n-3) + O(1)
        // Base ≈ 1.839 (tribonacci constant)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0, 1.0, 1.0 },
            nonRecursiveWork: ConstantComplexity.One,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"Tribonacci: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity or BinaryOperationComplexity,
            $"Expected exponential solution, got {result.Solution.GetType().Name}");
        
        // Dominant root should be tribonacci constant ≈ 1.839
        Assert.NotNull(result.DominantRoot);
        Assert.True(result.DominantRoot.Magnitude > 1.8 && result.DominantRoot.Magnitude < 1.9,
            $"Expected tribonacci constant ≈ 1.839, got {result.DominantRoot.Magnitude:F3}");
    }

    [Fact]
    public void TwoCallWithConstantCoeff_SolvesCorrectly()
    {
        // T(n) = 2T(n-1) + O(1)
        // Characteristic: x = 2 → Solution: O(2ⁿ)
        var recurrence = LinearRecurrenceRelation.Exponential(2.0);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"2T(n-1) + 1: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity,
            $"Expected ExponentialComplexity, got {result.Solution.GetType().Name}");
        
        if (result.Solution is ExponentialComplexity exp)
        {
            Assert.Equal(2.0, exp.Base, precision: 4);
        }
    }

    [Fact]
    public void ThreeCall_SolvesCorrectly()
    {
        // T(n) = 3T(n-1) + O(1)
        // Characteristic: x = 3 → Solution: O(3ⁿ)
        var recurrence = LinearRecurrenceRelation.Exponential(3.0);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"3T(n-1) + 1: {result?.Solution.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity);
        
        if (result.Solution is ExponentialComplexity exp)
        {
            Assert.Equal(3.0, exp.Base, precision: 4);
        }
    }

    #endregion

    #region Higher Order Linear Recurrences

    [Fact]
    public void ThirdOrder_AllOnes_Solves()
    {
        // T(n) = T(n-1) + T(n-2) + T(n-3)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0, 1.0, 1.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"3rd order all 1s: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        Assert.NotNull(result.DominantRoot);
        // Should have dominant root > 1 (tribonacci constant ≈ 1.839)
        Assert.True(result.DominantRoot.Magnitude > 1.0);
    }

    [Fact]
    public void FourthOrder_MixedCoefficients_Solves()
    {
        // T(n) = 2T(n-1) + T(n-2) - T(n-3) + 3T(n-4)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 2.0, 1.0, -1.0, 3.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"4th order mixed: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        Assert.NotNull(result.DominantRoot);
    }

    #endregion

    #region Characteristic Equation Method

    [Fact]
    public void CharacteristicRoots_DistinctReal_SolvesCorrectly()
    {
        // T(n) = 5T(n-1) - 6T(n-2)
        // Characteristic: x² - 5x + 6 = 0 → x = 2, 3
        // Solution: c₁·2ⁿ + c₂·3ⁿ = O(3ⁿ)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 5.0, -6.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"Distinct roots: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        
        // Should find roots 2 and 3
        Assert.Equal(2, result.Roots.Length);
        var roots = result.Roots.Select(r => r.RealPart).OrderBy(r => r).ToArray();
        Assert.Equal(2.0, roots[0], precision: 4);
        Assert.Equal(3.0, roots[1], precision: 4);

        // Dominant root is 3
        Assert.Equal(3.0, result.DominantRoot?.Magnitude ?? 0, precision: 4);
        Assert.True(result.Solution is ExponentialComplexity);
    }

    [Fact]
    public void CharacteristicRoots_Repeated_SolvesCorrectly()
    {
        // T(n) = 4T(n-1) - 4T(n-2)
        // Characteristic: x² - 4x + 4 = 0 → x = 2 (repeated)
        // Solution: (c₁ + c₂n)·2ⁿ = O(n·2ⁿ)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 4.0, -4.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"Repeated root: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");
        _output.WriteLine($"Dominant root: {result?.DominantRoot}");
        _output.WriteLine($"Has polynomial factor: {result?.HasPolynomialFactor}");

        Assert.NotNull(result);
        Assert.NotNull(result.DominantRoot);
        
        // Should have repeated root 2
        Assert.Equal(2.0, result.DominantRoot.Magnitude, precision: 4);
        Assert.True(result.DominantRoot.IsRepeated, "Root should be marked as repeated");
        Assert.Equal(2, result.DominantRoot.Multiplicity);
        
        // Solution should be O(n · 2ⁿ) due to repeated root
        Assert.True(result.HasPolynomialFactor, "Solution should have polynomial factor from repeated root");
    }

    [Fact]
    public void CharacteristicRoots_Complex_SolvesCorrectly()
    {
        // T(n) = T(n-2)
        // Characteristic: x² = 1 → x = ±1
        // Solution: bounded oscillation = O(1)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 0.0, 1.0 },  // 0·T(n-1) + 1·T(n-2)
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-2): {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        
        // Roots should be +1 and -1
        Assert.Equal(2, result.Roots.Length);
        
        // Dominant magnitude is 1, so bounded = O(1)
        Assert.Equal(1.0, result.DominantRoot?.Magnitude ?? 0, precision: 4);
    }

    [Fact]
    public void CharacteristicRoots_ComplexConjugates_SolvesCorrectly()
    {
        // T(n) = 2T(n-1) - 2T(n-2)
        // Characteristic: x² - 2x + 2 = 0 → x = 1 ± i
        // |x| = √2 → O((√2)ⁿ)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 2.0, -2.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"Complex roots: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Roots: {string.Join(", ", result?.Roots.Select(r => r.ToString()) ?? Array.Empty<string>())}");

        Assert.NotNull(result);
        
        // Should have complex conjugate roots with magnitude √2 ≈ 1.414
        Assert.NotNull(result.DominantRoot);
        Assert.Equal(Math.Sqrt(2), result.DominantRoot.Magnitude, precision: 3);
    }

    #endregion

    #region Non-Homogeneous Terms

    [Fact]
    public void NonHomogeneous_Polynomial_SolvesCorrectly()
    {
        // T(n) = T(n-1) + n²
        // Summation: Σᵢ₌₁ⁿ i² = n(n+1)(2n+1)/6 = O(n³)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0 },
            nonRecursiveWork: PolynomialComplexity.OfDegree(2, Variable.N),
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-1) + n²: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        AssertCubic(result.Solution);
    }

    [Fact]
    public void NonHomogeneous_Exponential_SolvesCorrectly()
    {
        // T(n) = T(n-1) + 2ⁿ
        // Summation: Σᵢ₌₁ⁿ 2ⁱ = 2ⁿ⁺¹ - 2 = O(2ⁿ)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 1.0 },
            nonRecursiveWork: new ExponentialComplexity(2, Variable.N),
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"T(n-1) + 2ⁿ: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity,
            $"Expected ExponentialComplexity, got {result.Solution.GetType().Name}");
    }

    [Fact]
    public void NonHomogeneous_ExponentialWithExponentialRecurrence_SolvesCorrectly()
    {
        // T(n) = 2T(n-1) + 3ⁿ
        // Homogeneous: O(2ⁿ), Particular: O(3ⁿ)
        // Solution: max(O(2ⁿ), O(3ⁿ)) = O(3ⁿ)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 2.0 },
            nonRecursiveWork: new ExponentialComplexity(3, Variable.N),
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"2T(n-1) + 3ⁿ: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"Method: {result?.Method}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ExponentialComplexity);
        
        if (result.Solution is ExponentialComplexity exp)
        {
            // Should be dominated by 3^n
            Assert.True(exp.Base >= 3.0, $"Expected base >= 3, got {exp.Base}");
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void SingleCoefficient_Zero_SolvesCorrectly()
    {
        // T(n) = 0·T(n-1) + 1 = 1
        // This is just T(n) = 1 = O(1)
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 0.0 },
            nonRecursiveWork: ConstantComplexity.One,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"0·T(n-1) + 1: {result?.Solution.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result.Solution is ConstantComplexity or LinearComplexity);
    }

    [Fact]
    public void Summation_Factory_Works()
    {
        // Use the factory method for summation
        var recurrence = LinearRecurrenceRelation.Summation(
            new LinearComplexity(1, Variable.N));

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Summation factory: {result?.Solution.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(recurrence.IsSummation);
        AssertQuadratic(result.Solution);
    }

    [Fact]
    public void HighOrder_ConvergesToDominant()
    {
        // T(n) = 2T(n-1) + T(n-2) + T(n-3) + T(n-4)
        // Should be dominated by largest root
        var recurrence = LinearRecurrenceRelation.Create(
            coefficients: new[] { 2.0, 1.0, 1.0, 1.0 },
            nonRecursiveWork: ConstantComplexity.Zero,
            variable: Variable.N);

        var result = _solver.Solve(recurrence);

        _output.WriteLine($"Recurrence: {recurrence}");
        _output.WriteLine($"High order: {result?.Solution.ToBigONotation()}");
        _output.WriteLine($"All roots: {string.Join(", ", result?.Roots.Select(r => $"{r.Magnitude:F3}") ?? Array.Empty<string>())}");
        _output.WriteLine($"Dominant: {result?.DominantRoot?.Magnitude:F4}");

        Assert.NotNull(result);
        Assert.NotNull(result.DominantRoot);
        // Dominant should be > 2 (since we have 2T(n-1) plus more)
        Assert.True(result.DominantRoot.Magnitude > 2.0);
    }

    #endregion

    #region Helpers

    private static void AssertLinear(ComplexityExpression expr)
    {
        var isLinear = expr is LinearComplexity ||
            (expr is PolynomialComplexity p && Math.Abs(p.Degree - 1) < 0.1) ||
            (expr is PolyLogComplexity pl && Math.Abs(pl.PolyDegree - 1) < 0.1 && pl.LogExponent == 0);
        Assert.True(isLinear, $"Expected O(n), got {expr.ToBigONotation()}");
    }

    private static void AssertQuadratic(ComplexityExpression expr)
    {
        var isQuadratic = (expr is PolynomialComplexity p && Math.Abs(p.Degree - 2) < 0.1) ||
            (expr is PolyLogComplexity pl && Math.Abs(pl.PolyDegree - 2) < 0.1 && pl.LogExponent == 0);
        Assert.True(isQuadratic, $"Expected O(n²), got {expr.ToBigONotation()}");
    }

    private static void AssertCubic(ComplexityExpression expr)
    {
        var isCubic = (expr is PolynomialComplexity p && Math.Abs(p.Degree - 3) < 0.1) ||
            (expr is PolyLogComplexity pl && Math.Abs(pl.PolyDegree - 3) < 0.1 && pl.LogExponent == 0);
        Assert.True(isCubic, $"Expected O(n³), got {expr.ToBigONotation()}");
    }

    private static void AssertNLogN(ComplexityExpression expr)
    {
        var isNLogN = expr is PolyLogComplexity pl && 
            Math.Abs(pl.PolyDegree - 1) < 0.1 && 
            Math.Abs(pl.LogExponent - 1) < 0.1;
        Assert.True(isNLogN, $"Expected O(n log n), got {expr.ToBigONotation()}");
    }

    #endregion
}
