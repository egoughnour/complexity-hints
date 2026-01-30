using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Core;

/// <summary>
/// Comprehensive tests for complexity expression arithmetic, composition, and evaluation.
/// Verifies correct behavior of expression operations following composition rules.
/// </summary>
public class ExpressionArithmeticTests
{
    private readonly ITestOutputHelper _output;
    private static readonly Variable N = Variable.N;
    private static readonly Variable M = Variable.M;

    public ExpressionArithmeticTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Evaluation Tests

    [Theory]
    [MemberData(nameof(EvaluationTestCases))]
    public void Expression_EvaluatesCorrectly(
        string name, Func<Variable, ComplexityExpression> factory, double input, double expected, double tolerance)
    {
        var expr = factory(N);
        var assignments = new Dictionary<Variable, double> { { N, input } };

        var result = expr.Evaluate(assignments);

        _output.WriteLine($"{name}: {expr.ToBigONotation()} at n={input} = {result}");

        Assert.NotNull(result);
        Assert.Equal(expected, result.Value, precision: (int)Math.Ceiling(-Math.Log10(tolerance)));
    }

    public static IEnumerable<object[]> EvaluationTestCases => new[]
    {
        // Constant
        new object[] { "Constant_42", (Func<Variable, ComplexityExpression>)(v => new ConstantComplexity(42)), 100.0, 42.0, 0.001 },
        new object[] { "Constant_0", (Func<Variable, ComplexityExpression>)(v => new ConstantComplexity(0)), 100.0, 0.0, 0.001 },

        // Linear
        new object[] { "Linear_1x_at_100", (Func<Variable, ComplexityExpression>)(v => new LinearComplexity(1, v)), 100.0, 100.0, 0.001 },
        new object[] { "Linear_2x_at_50", (Func<Variable, ComplexityExpression>)(v => new LinearComplexity(2, v)), 50.0, 100.0, 0.001 },
        new object[] { "Linear_0.5x_at_200", (Func<Variable, ComplexityExpression>)(v => new LinearComplexity(0.5, v)), 200.0, 100.0, 0.001 },

        // Polynomial
        new object[] { "Quadratic_at_10", (Func<Variable, ComplexityExpression>)(v => PolyLogComplexity.Polynomial(2, v)), 10.0, 100.0, 0.001 },
        new object[] { "Cubic_at_10", (Func<Variable, ComplexityExpression>)(v => PolyLogComplexity.Polynomial(3, v)), 10.0, 1000.0, 0.001 },
        new object[] { "Quartic_at_10", (Func<Variable, ComplexityExpression>)(v => PolyLogComplexity.Polynomial(4, v)), 10.0, 10000.0, 0.001 },

        // Logarithmic
        new object[] { "Log2_at_1024", (Func<Variable, ComplexityExpression>)(v => new LogarithmicComplexity(1, v, 2)), 1024.0, 10.0, 0.001 },
        new object[] { "Log10_at_1000", (Func<Variable, ComplexityExpression>)(v => new LogarithmicComplexity(1, v, 10)), 1000.0, 3.0, 0.001 },
        new object[] { "2Log2_at_256", (Func<Variable, ComplexityExpression>)(v => new LogarithmicComplexity(2, v, 2)), 256.0, 16.0, 0.001 },

        // PolyLog (n^k * log^j n)
        new object[] { "NLogN_at_100", (Func<Variable, ComplexityExpression>)(v => PolyLogComplexity.NLogN(v)), 100.0, 100 * Math.Log2(100), 0.01 },
        new object[] { "NLog2N_at_100", (Func<Variable, ComplexityExpression>)(v => new PolyLogComplexity(1, 2, v)), 100.0, 100 * Math.Pow(Math.Log2(100), 2), 0.01 },
        new object[] { "N2LogN_at_10", (Func<Variable, ComplexityExpression>)(v => new PolyLogComplexity(2, 1, v)), 10.0, 100 * Math.Log2(10), 0.01 },

        // Exponential
        new object[] { "2^n_at_10", (Func<Variable, ComplexityExpression>)(v => new ExponentialComplexity(2, v)), 10.0, 1024.0, 0.001 },
        new object[] { "3^n_at_5", (Func<Variable, ComplexityExpression>)(v => new ExponentialComplexity(3, v)), 5.0, 243.0, 0.001 },

        // Factorial
        new object[] { "n!_at_5", (Func<Variable, ComplexityExpression>)(v => new FactorialComplexity(v)), 5.0, 120.0, 0.001 },
        new object[] { "n!_at_10", (Func<Variable, ComplexityExpression>)(v => new FactorialComplexity(v)), 10.0, 3628800.0, 0.001 },
    };

    #endregion

    #region Composition Tests - Sequential (Addition)

    [Theory]
    [InlineData("O(n)+O(n)", 100, 200)]
    [InlineData("O(n)+O(1)", 100, 101)]
    [InlineData("O(n^2)+O(n)", 10, 110)]
    [InlineData("O(log n)+O(1)", 1024, 11)]
    public void Sequential_Composition_AddsCorrectly(string description, double input, double expected)
    {
        var (left, right) = ParseComposition(description);
        var composed = new BinaryOperationComplexity(left, BinaryOp.Plus, right);
        var assignments = new Dictionary<Variable, double> { { N, input } };

        var result = composed.Evaluate(assignments);

        _output.WriteLine($"{description} at n={input}: {result}");

        Assert.NotNull(result);
        Assert.Equal(expected, result.Value, precision: 2);
    }

    #endregion

    #region Composition Tests - Nested (Multiplication)

    [Theory]
    [InlineData("O(n)*O(n)", 10, 100)]
    [InlineData("O(n)*O(log n)", 1024, 10240)]
    [InlineData("O(n^2)*O(log n)", 100, 10000 * 6.64)] // approx
    public void Nested_Composition_MultipliesCorrectly(string description, double input, double expected)
    {
        var (left, right) = ParseComposition(description);
        var composed = new BinaryOperationComplexity(left, BinaryOp.Multiply, right);
        var assignments = new Dictionary<Variable, double> { { N, input } };

        var result = composed.Evaluate(assignments);

        _output.WriteLine($"{description} at n={input}: {result}");

        Assert.NotNull(result);
        // Use larger tolerance for compound expressions
        Assert.InRange(result.Value, expected * 0.9, expected * 1.1);
    }

    #endregion

    #region Composition Tests - Branching (Max)

    [Theory]
    [InlineData("max(O(n), O(log n))", 100, 100)]        // O(n) dominates
    [InlineData("max(O(n^2), O(n))", 10, 100)]           // O(n²) dominates
    [InlineData("max(O(1), O(n))", 50, 50)]              // O(n) dominates
    [InlineData("max(O(n log n), O(n))", 1024, 10240)]   // O(n log n) dominates
    public void Branching_Composition_TakesMax(string description, double input, double expected)
    {
        var (left, right) = ParseComposition(description.Replace("max(", "").TrimEnd(')'));
        var composed = new BinaryOperationComplexity(left, BinaryOp.Max, right);
        var assignments = new Dictionary<Variable, double> { { N, input } };

        var result = composed.Evaluate(assignments);

        _output.WriteLine($"{description} at n={input}: {result}");

        Assert.NotNull(result);
        Assert.InRange(result.Value, expected * 0.9, expected * 1.1);
    }

    #endregion

    #region Free Variables Tests

    [Theory]
    [InlineData("Constant", 0)]
    [InlineData("LinearN", 1)]
    [InlineData("BinaryN_M", 2)]
    public void FreeVariables_CountedCorrectly(string testCase, int expectedCount)
    {
        var expr = testCase switch
        {
            "Constant" => (ComplexityExpression)new ConstantComplexity(5),
            "LinearN" => new LinearComplexity(1, N),
            "BinaryN_M" => new BinaryOperationComplexity(
                new LinearComplexity(1, N),
                BinaryOp.Plus,
                new LinearComplexity(1, M)),
            _ => throw new ArgumentException()
        };

        var freeVars = expr.FreeVariables;

        _output.WriteLine($"{testCase}: {string.Join(", ", freeVars.Select(v => v.Name))}");

        Assert.Equal(expectedCount, freeVars.Count);
    }

    #endregion

    #region Substitution Tests

    [Fact]
    public void Substitution_ReplacesVariable()
    {
        var expr = new LinearComplexity(2, N);
        var replacement = new ConstantComplexity(50);

        var substituted = expr.Substitute(N, replacement);

        var result = substituted.Evaluate(new Dictionary<Variable, double>());

        Assert.NotNull(result);
        Assert.Equal(100.0, result.Value); // 2 * 50
    }

    [Fact]
    public void Substitution_PreservesUnrelatedVariables()
    {
        var expr = new LinearComplexity(1, N);
        var replacement = new ConstantComplexity(50);

        // Substitute M (not in expression)
        var substituted = expr.Substitute(M, replacement);

        // Should be unchanged
        var assignments = new Dictionary<Variable, double> { { N, 100 } };
        var result = substituted.Evaluate(assignments);

        Assert.NotNull(result);
        Assert.Equal(100.0, result.Value);
    }

    [Fact]
    public void Substitution_WorksWithBinaryOperations()
    {
        // O(n) + O(m)
        var expr = new BinaryOperationComplexity(
            new LinearComplexity(1, N),
            BinaryOp.Plus,
            new LinearComplexity(1, M));

        // Substitute n with 2m
        var replacement = new LinearComplexity(2, M);
        var substituted = expr.Substitute(N, replacement);

        // Result should be O(2m) + O(m) = O(3m) when evaluated
        var assignments = new Dictionary<Variable, double> { { M, 10 } };
        var result = substituted.Evaluate(assignments);

        Assert.NotNull(result);
        Assert.Equal(30.0, result.Value); // 2*10 + 10
    }

    #endregion

    #region BigO Notation Generation Tests

    [Theory]
    [InlineData("Constant1", "O(1)")]
    [InlineData("Linear", "O(n)")]
    [InlineData("Quadratic", "O(n²)")]
    [InlineData("Cubic", "O(n³)")]
    [InlineData("Logarithmic", "O(log n)")]
    [InlineData("Exponential2", "O(2^n)")]
    [InlineData("Factorial", "O(n!)")]
    public void ToBigONotation_GeneratesCorrectString(string testCase, string expectedContains)
    {
        var expr = testCase switch
        {
            "Constant1" => (ComplexityExpression)new ConstantComplexity(1),
            "Linear" => new LinearComplexity(1, N),
            "Quadratic" => PolyLogComplexity.Polynomial(2, N),
            "Cubic" => PolyLogComplexity.Polynomial(3, N),
            "Logarithmic" => new LogarithmicComplexity(1, N),
            "Exponential2" => new ExponentialComplexity(2, N),
            "Factorial" => new FactorialComplexity(N),
            _ => throw new ArgumentException()
        };

        var notation = expr.ToBigONotation();

        _output.WriteLine($"{testCase}: {notation}");

        Assert.Contains(expectedContains.Replace("O(", "").Replace(")", "").ToLower(),
            notation.ToLower());
    }

    #endregion

    #region Asymptotic Comparison Tests

    [Theory]
    [MemberData(nameof(AsymptoticComparisonCases))]
    public void AsymptoticComparison_CorrectOrdering(
        string smaller, string larger, double largeN)
    {
        var smallerExpr = ParseComplexity(smaller);
        var largerExpr = ParseComplexity(larger);
        var assignments = new Dictionary<Variable, double> { { N, largeN } };

        var smallerVal = smallerExpr.Evaluate(assignments);
        var largerVal = largerExpr.Evaluate(assignments);

        _output.WriteLine($"At n={largeN}: {smaller}={smallerVal}, {larger}={largerVal}");

        Assert.NotNull(smallerVal);
        Assert.NotNull(largerVal);
        Assert.True(smallerVal.Value < largerVal.Value,
            $"Expected {smaller} < {larger} for large n");
    }

    public static IEnumerable<object[]> AsymptoticComparisonCases => new[]
    {
        // Polynomial ordering
        new object[] { "O(1)", "O(log n)", 1000.0 },
        new object[] { "O(log n)", "O(n)", 1000.0 },
        new object[] { "O(n)", "O(n log n)", 1000.0 },
        new object[] { "O(n log n)", "O(n^2)", 1000.0 },
        new object[] { "O(n^2)", "O(n^3)", 100.0 },

        // Exponential dominates polynomial
        new object[] { "O(n^3)", "O(2^n)", 20.0 },
        new object[] { "O(n^10)", "O(2^n)", 50.0 },

        // Factorial dominates exponential
        new object[] { "O(2^n)", "O(n!)", 15.0 },
    };

    #endregion

    #region PolyLog Specific Tests

    [Theory]
    [InlineData(1, 0, 100, 100)]           // n^1 * log^0 = n
    [InlineData(1, 1, 1024, 10240)]         // n * log n
    [InlineData(2, 0, 10, 100)]            // n^2
    [InlineData(0, 1, 1024, 10)]           // log n
    [InlineData(2, 1, 10, 100 * 3.322)]    // n^2 * log n
    [InlineData(1, 2, 1024, 1024 * 100)]   // n * log^2 n
    public void PolyLog_EvaluatesCorrectly(
        double polyDegree, double logExponent, double input, double expected)
    {
        var expr = new PolyLogComplexity(polyDegree, logExponent, N);
        var assignments = new Dictionary<Variable, double> { { N, input } };

        var result = expr.Evaluate(assignments);

        _output.WriteLine($"n^{polyDegree}·log^{logExponent}(n) at n={input}: {result}");

        Assert.NotNull(result);
        Assert.InRange(result.Value, expected * 0.9, expected * 1.1);
    }

    [Fact]
    public void PolyLog_FactoryMethods_CreateCorrectForms()
    {
        // NLogN should create n^1 * log^1 n
        var nlogn = PolyLogComplexity.NLogN(N);
        Assert.Equal(1.0, nlogn.PolyDegree);
        Assert.Equal(1.0, nlogn.LogExponent);

        // Polynomial should create n^k * log^0 n
        var quad = PolyLogComplexity.Polynomial(2, N);
        Assert.Equal(2.0, quad.PolyDegree);
        Assert.Equal(0.0, quad.LogExponent);
    }

    #endregion

    #region Helper Methods

    private static ComplexityExpression ParseComplexity(string notation)
    {
        var lower = notation.ToLower().Replace("o(", "").Replace(")", "").Trim();

        return lower switch
        {
            "1" => new ConstantComplexity(1),
            "n" => new LinearComplexity(1, N),
            "log n" or "logn" => new LogarithmicComplexity(1, N),
            "n log n" or "nlogn" or "n·log n" => PolyLogComplexity.NLogN(N),
            "n^2" or "n²" => PolyLogComplexity.Polynomial(2, N),
            "n^3" or "n³" => PolyLogComplexity.Polynomial(3, N),
            "n^10" => PolyLogComplexity.Polynomial(10, N),
            "2^n" => new ExponentialComplexity(2, N),
            "n!" => new FactorialComplexity(N),
            _ => new ConstantComplexity(1)
        };
    }

    private static (ComplexityExpression left, ComplexityExpression right) ParseComposition(string description)
    {
        var parts = description.Replace("O(", "").Split(new[] { ")+O(", ")*O(", "), O(" },
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            parts = description.Split(new[] { "+", "*", "," }, StringSplitOptions.TrimEntries);

        var left = ParseComplexity(parts[0].Trim().TrimEnd(')'));
        var right = ParseComplexity(parts[1].Trim().TrimEnd(')'));

        return (left, right);
    }

    #endregion
}
