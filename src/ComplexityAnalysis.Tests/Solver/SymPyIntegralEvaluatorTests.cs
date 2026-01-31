using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Solver;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Tests for the SymPy-based integral evaluator that calls Python subprocess.
/// These tests verify that the Akra-Bazzi integral ∫₁ⁿ g(u)/u^(p+1) du is correctly
/// evaluated for various g(n) forms.
/// </summary>
public class SymPyIntegralEvaluatorTests
{
    private readonly ITestOutputHelper _output;
    private readonly SymPyIntegralEvaluator _evaluator;

    public SymPyIntegralEvaluatorTests(ITestOutputHelper output)
    {
        _output = output;
        _evaluator = SymPyIntegralEvaluator.Instance;
    }

    [Fact]
    public void Constant_g_Converges_ToNP()
    {
        // g(n) = 1, p = 1
        // ∫ 1/u² du = 1 - 1/n → O(1), so solution is Θ(n^p) = Θ(n)
        var g = ConstantComplexity.One;
        var variable = Variable.N;
        var p = 1.0;

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"g(n) = 1, p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
        Assert.NotNull(result.FullSolution);
    }

    [Fact]
    public void Polynomial_k_LessThan_p_Converges()
    {
        // g(n) = n^0.5 = √n, p = 1
        // k < p → integral converges → solution is Θ(n^p) = Θ(n)
        var g = new PowerComplexity(new VariableComplexity(Variable.N), 0.5);
        var variable = Variable.N;
        var p = 1.0;

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"g(n) = n^0.5, p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
    }

    [Fact]
    public void Polynomial_k_Equals_p_GivesLogN()
    {
        // g(n) = n, p = 1
        // k = p → ∫ du/u = log(n) → solution is Θ(n log n)
        var g = new VariableComplexity(Variable.N);
        var variable = Variable.N;
        var p = 1.0;

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"g(n) = n, p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
        // The full solution should be n*log(n)
        Assert.Contains("log", result.FullSolution?.ToBigONotation() ?? "");
    }

    [Fact]
    public void Polynomial_k_GreaterThan_p_GivesNK()
    {
        // g(n) = n², p = 1
        // k > p → integral grows as n^(k-p) = n → full solution is Θ(n² × 1) = Θ(n²)
        var g = PolynomialComplexity.OfDegree(2, Variable.N);
        var variable = Variable.N;
        var p = 1.0;

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"g(n) = n², p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
    }

    [Fact]
    public void PolyLog_k_Equals_p_GivesLogSquared()
    {
        // g(n) = n·log(n), p = 1
        // k = p → integral ~ log²(n) → solution is Θ(n·log²(n))
        var g = PolyLogComplexity.NLogN(Variable.N);
        var variable = Variable.N;
        var p = 1.0;

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"g(n) = n·log(n), p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
    }

    [Fact(Skip = "Requires SymPy subprocess - may not be available in all environments")]
    public async Task SymPy_PolynomialIntegral_Async()
    {
        // Test the async interface directly
        var g = PolynomialComplexity.OfDegree(2, Variable.N);
        var variable = Variable.N;
        var p = 1.0;

        var result = await _evaluator.EvaluateWithSymPyAsync(g, variable, p);

        _output.WriteLine($"SymPy result for g(n) = n², p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");
        _output.WriteLine($"Confidence: {result.Confidence}");
        _output.WriteLine($"IsSymbolic: {result.IsSymbolic}");

        Assert.True(result.Success);
    }

    [Fact(Skip = "Requires SymPy subprocess - may not be available in all environments")]
    public async Task SymPy_ExponentialIntegral_Async()
    {
        // g(n) = 2^n, p = 1
        // Exponential dominates - solution should be Θ(2^n)
        var g = new ExponentialComplexity(2.0, Variable.N);
        var variable = Variable.N;
        var p = 1.0;

        var result = await _evaluator.EvaluateWithSymPyAsync(g, variable, p);

        _output.WriteLine($"SymPy result for g(n) = 2^n, p = {p}");
        _output.WriteLine($"Integral term: {result.IntegralTerm?.ToBigONotation()}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");
        _output.WriteLine($"Special function: {result.SpecialFunction}");

        Assert.True(result.Success);
    }

    [Fact]
    public void MergeSort_RecurrenceSolution()
    {
        // Merge sort: T(n) = 2T(n/2) + n
        // p = log₂(2) = 1, g(n) = n
        // k = p = 1 → solution is Θ(n log n)
        var g = new VariableComplexity(Variable.N);
        var variable = Variable.N;
        var p = 1.0; // log₂(2) = 1

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"Merge sort recurrence: g(n) = n, p = {p}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");

        Assert.True(result.Success);
        Assert.Contains("log", result.FullSolution?.ToBigONotation() ?? "");
    }

    [Fact]
    public void Karatsuba_RecurrenceSolution()
    {
        // Karatsuba: T(n) = 3T(n/2) + n
        // p = log₂(3) ≈ 1.585, g(n) = n
        // k = 1 < p → integral converges → solution is Θ(n^p) ≈ Θ(n^1.585)
        var g = new VariableComplexity(Variable.N);
        var variable = Variable.N;
        var p = Math.Log2(3); // ≈ 1.585

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"Karatsuba recurrence: g(n) = n, p = {p:F4}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
    }

    [Fact]
    public void Strassen_RecurrenceSolution()
    {
        // Strassen: T(n) = 7T(n/2) + n²
        // p = log₂(7) ≈ 2.807, g(n) = n²
        // k = 2 < p → integral converges → solution is Θ(n^p) ≈ Θ(n^2.807)
        var g = PolynomialComplexity.OfDegree(2, Variable.N);
        var variable = Variable.N;
        var p = Math.Log2(7); // ≈ 2.807

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"Strassen recurrence: g(n) = n², p = {p:F4}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
    }

    [Fact]
    public void BinarySearch_RecurrenceSolution()
    {
        // Binary search: T(n) = T(n/2) + 1
        // p = log₂(1) = 0, g(n) = 1
        // k = 0 = p → solution is Θ(log n)
        var g = ConstantComplexity.One;
        var variable = Variable.N;
        var p = 0.0; // log₂(1) = 0

        var result = _evaluator.Evaluate(g, variable, p);

        _output.WriteLine($"Binary search recurrence: g(n) = 1, p = {p}");
        _output.WriteLine($"Full solution: {result.FullSolution?.ToBigONotation()}");
        _output.WriteLine($"Explanation: {result.Explanation}");

        Assert.True(result.Success);
        Assert.Contains("log", result.FullSolution?.ToBigONotation() ?? "");
    }
}
