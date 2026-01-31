using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// TDD tests for mutual recursion detection and solving.
/// These tests are EXPECTED TO FAIL until the feature is implemented.
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

    [Fact(Skip = "TDD: Mutual recursion not yet implemented")]
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
        var result = await AnalyzeMethodAsync(code, "A");

        _output.WriteLine($"A(n) complexity: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Mutual recursion not yet implemented")]
    public async Task ThreeWayMutualRecursion_Detects()
    {
        // A → B → C → A cycle
        const string code = @"
public class ThreeWay
{
    public int A(int n) { if (n <= 0) return 0; return 1 + B(n - 1); }
    public int B(int n) { if (n <= 0) return 0; return 1 + C(n - 1); }
    public int C(int n) { if (n <= 0) return 0; return 1 + A(n - 1); }
}";
        var result = await AnalyzeMethodAsync(code, "A");

        _output.WriteLine($"Three-way mutual: {result?.ToBigONotation()}");

        // T(n) = T(n-3) + O(1) → O(n)
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Mutual recursion not yet implemented")]
    public async Task EvenOdd_MutualRecursion_Detects()
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
        var result = await AnalyzeMethodAsync(code, "IsEven");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Mutual recursion not yet implemented")]
    public async Task MutualWithDifferentWork_AnalyzesCorrectly()
    {
        // A does O(n) work, B does O(1) work
        const string code = @"
public class AsymmetricMutual
{
    public int A(int n, int[] arr)
    {
        if (n <= 0) return 0;
        int sum = 0;
        for (int i = 0; i < arr.Length; i++) sum += arr[i]; // O(n) work
        return sum + B(n - 1, arr);
    }

    public int B(int n, int[] arr)
    {
        if (n <= 0) return 0;
        return 1 + A(n - 1, arr); // O(1) work
    }
}";
        var result = await AnalyzeMethodAsync(code, "A");

        _output.WriteLine($"Asymmetric mutual: {result?.ToBigONotation()}");

        // T(n) = T(n-2) + O(m) where m is array length → O(n*m)
        Assert.NotNull(result);
    }

    [Fact]  // FindCycles() is now implemented
    public async Task CallGraph_DetectsMutualRecursionCycle()
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

    #region Helpers

    private async Task<ComplexityExpression?> AnalyzeMethodAsync(string code, string methodName)
    {
        var compilation = CreateCompilation(code);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var semanticModel = compilation.GetSemanticModel(tree);

        var methodDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (methodDecl == null) return null;

        var extractor = new RoslynComplexityExtractor(semanticModel);
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
            PolynomialComplexity p => p.Degree == 1,
            PolyLogComplexity pl => pl.PolyDegree == 1 && pl.LogExponent == 0,
            _ => false
        };
        Assert.True(isLinear, $"Expected O(n), got {expr.ToBigONotation()}");
    }

    #endregion
}
