using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Roslyn.Analysis;
using ComplexityAnalysis.Solver;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// Tests for mutual recursion detection and solving.
/// 
/// Mutual recursion: A() calls B(), B() calls A()
/// Requires: Call graph cycle detection + combined recurrence solving
/// </summary>
public class MutualRecursionTests
{
    private readonly ITestOutputHelper _output;

    public MutualRecursionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Detection Tests

    [Fact]
    public void CallGraph_DetectsMutualRecursionCycle()
    {
        const string code = @"
public class Cycle
{
    public void A() { B(); }
    public void B() { C(); }
    public void C() { A(); }
}";
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        // Should detect the cycle A → B → C → A
        var cycles = callGraph.FindCycles();

        Assert.NotEmpty(cycles);
        Assert.Contains(cycles, c => c.Count == 3);
    }

    [Fact]
    public void MutualRecursionDetector_FindsTwoWayCycle()
    {
        const string code = @"
public class TwoWay
{
    public int A(int n)
    {
        if (n <= 0) return 0;
        return 1 + B(n - 1);
    }

    public int B(int n)
    {
        if (n <= 0) return 0;
        return 1 + A(n - 1);
    }
}";
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var detector = new MutualRecursionDetector(semanticModel, callGraph);
        var cycles = detector.DetectCycles();

        _output.WriteLine($"Found {cycles.Count} cycles");
        foreach (var cycle in cycles)
        {
            _output.WriteLine($"  Cycle: {cycle.GetDescription()}");
        }

        Assert.Single(cycles);
        Assert.Equal(2, cycles[0].Length);
    }

    [Fact]
    public void MutualRecursionDetector_FindsThreeWayCycle()
    {
        const string code = @"
public class ThreeWay
{
    public int A(int n) { if (n <= 0) return 0; return 1 + B(n - 1); }
    public int B(int n) { if (n <= 0) return 0; return 1 + C(n - 1); }
    public int C(int n) { if (n <= 0) return 0; return 1 + A(n - 1); }
}";
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var detector = new MutualRecursionDetector(semanticModel, callGraph);
        var cycles = detector.DetectCycles();

        _output.WriteLine($"Found {cycles.Count} cycles");
        foreach (var cycle in cycles)
        {
            _output.WriteLine($"  Cycle: {cycle.GetDescription()}");
        }

        Assert.Single(cycles);
        Assert.Equal(3, cycles[0].Length);
    }

    [Fact]
    public void MutualRecursionDetector_IgnoresSingleMethodRecursion()
    {
        const string code = @"
public class Direct
{
    public int Factorial(int n)
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }
}";
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();
        var tree = compilation.SyntaxTrees.First();
        var semanticModel = compilation.GetSemanticModel(tree);

        var detector = new MutualRecursionDetector(semanticModel, callGraph);
        var cycles = detector.DetectCycles();

        // Direct recursion is not mutual recursion
        Assert.Empty(cycles);
    }

    #endregion

    #region Solving Tests

    [Fact]
    public void MutualRecurrenceSolver_SolvesTwoWayCycle_AsLinear()
    {
        // A(n) → B(n-1) → A(n-2) + combined work
        // T(n) = T(n-2) + O(1) → O(n)
        var system = new MutualRecurrenceSystem
        {
            Variable = Variable.N,
            Components =
            [
                new MutualRecurrenceComponent
                {
                    MethodName = "A",
                    NonRecursiveWork = ConstantComplexity.One,
                    Reduction = 1.0,
                    ScaleFactor = 0.99
                },
                new MutualRecurrenceComponent
                {
                    MethodName = "B",
                    NonRecursiveWork = ConstantComplexity.One,
                    Reduction = 1.0,
                    ScaleFactor = 0.99
                }
            ]
        };

        var solver = new MutualRecurrenceSolver();
        var result = solver.Solve(system);

        _output.WriteLine($"Solution: {result.Solution?.ToBigONotation()}");
        _output.WriteLine($"Method: {result.Method}");

        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
        AssertLinearComplexity(result.Solution);
    }

    [Fact]
    public void MutualRecurrenceSolver_SolvesThreeWayCycle_AsLinear()
    {
        // A → B → C → A with O(1) work each
        // T(n) = T(n-3) + O(1) → O(n)
        var system = new MutualRecurrenceSystem
        {
            Variable = Variable.N,
            Components =
            [
                new MutualRecurrenceComponent { MethodName = "A", NonRecursiveWork = ConstantComplexity.One },
                new MutualRecurrenceComponent { MethodName = "B", NonRecursiveWork = ConstantComplexity.One },
                new MutualRecurrenceComponent { MethodName = "C", NonRecursiveWork = ConstantComplexity.One }
            ]
        };

        var solver = new MutualRecurrenceSolver();
        var result = solver.Solve(system);

        _output.WriteLine($"Solution: {result.Solution?.ToBigONotation()}");
        _output.WriteLine($"Method: {result.Method}");

        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
        AssertLinearComplexity(result.Solution);
    }

    [Fact]
    public void MutualRecurrenceSolver_WithLinearWork_SolvesAsQuadratic()
    {
        // A does O(n) work, B does O(1) work
        // T(n) = T(n-2) + O(n) → O(n²)
        var system = new MutualRecurrenceSystem
        {
            Variable = Variable.N,
            Components =
            [
                new MutualRecurrenceComponent
                {
                    MethodName = "A",
                    NonRecursiveWork = new LinearComplexity(1.0, Variable.N),
                    Reduction = 1.0,
                    ScaleFactor = 0.99
                },
                new MutualRecurrenceComponent
                {
                    MethodName = "B",
                    NonRecursiveWork = ConstantComplexity.One,
                    Reduction = 1.0,
                    ScaleFactor = 0.99
                }
            ]
        };

        var solver = new MutualRecurrenceSolver();
        var result = solver.Solve(system);

        _output.WriteLine($"Solution: {result.Solution?.ToBigONotation()}");
        _output.WriteLine($"Method: {result.Method}");

        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
        AssertQuadraticComplexity(result.Solution);
    }

    #endregion

    #region End-to-End Tests

    [Fact]
    public async Task SimpleAB_Recursion_DetectsAsLinear()
    {
        // A(n) calls B(n-1), B(n) calls A(n-1)
        // Combined: effectively T(n) = T(n-2) + O(1) → O(n)
        const string code = @"
public class MutualRecursion
{
    public int A(int n)
    {
        if (n <= 0) return 0;
        return 1 + B(n - 1);
    }

    public int B(int n)
    {
        if (n <= 0) return 0;
        return 1 + A(n - 1);
    }
}";
        var result = await AnalyzeMethodWithCallGraphAsync(code, "A");

        _output.WriteLine($"A(n) complexity: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact]
    public async Task ThreeWayMutualRecursion_DetectsAsLinear()
    {
        // A → B → C → A cycle with O(1) work each
        const string code = @"
public class ThreeWay
{
    public int A(int n) { if (n <= 0) return 0; return 1 + B(n - 1); }
    public int B(int n) { if (n <= 0) return 0; return 1 + C(n - 1); }
    public int C(int n) { if (n <= 0) return 0; return 1 + A(n - 1); }
}";
        var result = await AnalyzeMethodWithCallGraphAsync(code, "A");

        _output.WriteLine($"Three-way mutual: {result?.ToBigONotation()}");

        // T(n) = T(n-3) + O(1) → O(n)
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact]
    public async Task EvenOdd_MutualRecursion_DetectsAsLinear()
    {
        // Classic even/odd mutual recursion
        const string code = @"
public class EvenOdd
{
    public bool IsEven(int n)
    {
        if (n == 0) return true;
        return IsOdd(n - 1);
    }

    public bool IsOdd(int n)
    {
        if (n == 0) return false;
        return IsEven(n - 1);
    }
}";
        var result = await AnalyzeMethodWithCallGraphAsync(code, "IsEven");

        _output.WriteLine($"Even/Odd: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact]
    public async Task MutualRecursion_AllMethodsInCycle_HaveSameComplexity()
    {
        const string code = @"
public class Symmetric
{
    public int A(int n) { if (n <= 0) return 0; return 1 + B(n - 1); }
    public int B(int n) { if (n <= 0) return 0; return 1 + A(n - 1); }
}";
        var resultA = await AnalyzeMethodWithCallGraphAsync(code, "A");
        var resultB = await AnalyzeMethodWithCallGraphAsync(code, "B");

        _output.WriteLine($"A(n): {resultA?.ToBigONotation()}");
        _output.WriteLine($"B(n): {resultB?.ToBigONotation()}");

        Assert.NotNull(resultA);
        Assert.NotNull(resultB);

        // Both should have the same asymptotic complexity
        AssertLinearComplexity(resultA);
        AssertLinearComplexity(resultB);
    }

    #endregion

    #region Helpers

    private async Task<ComplexityExpression?> AnalyzeMethodWithCallGraphAsync(string code, string methodName)
    {
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var semanticModel = compilation.GetSemanticModel(tree);

        var methodDecl = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (methodDecl == null) return null;

        var extractor = new RoslynComplexityExtractor(semanticModel, callGraph);
        return extractor.AnalyzeMethod(methodDecl);
    }

    private static Compilation CreateCompilation(string code)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        };

        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimePath != null)
        {
            var systemRuntime = Path.Combine(runtimePath, "System.Runtime.dll");
            if (File.Exists(systemRuntime))
                references.Add(MetadataReference.CreateFromFile(systemRuntime));
        }

        return CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(code) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void AssertLinearComplexity(ComplexityExpression expr)
    {
        var isLinear = expr switch
        {
            LinearComplexity => true,
            VariableComplexity => true, // O(n) is also represented as VariableComplexity
            PolynomialComplexity p => Math.Abs(p.Degree - 1) < 0.01,
            PolyLogComplexity pl => Math.Abs(pl.PolyDegree - 1) < 0.01 && pl.LogExponent == 0,
            _ => false
        };
        Assert.True(isLinear, $"Expected O(n), got {expr.ToBigONotation()}");
    }

    private static void AssertQuadraticComplexity(ComplexityExpression expr)
    {
        var isQuadratic = expr switch
        {
            PolynomialComplexity p => Math.Abs(p.Degree - 2) < 0.01,
            PolyLogComplexity pl => Math.Abs(pl.PolyDegree - 2) < 0.01 && pl.LogExponent == 0,
            _ => false
        };
        Assert.True(isQuadratic, $"Expected O(n²), got {expr.ToBigONotation()}");
    }

    #endregion
}
