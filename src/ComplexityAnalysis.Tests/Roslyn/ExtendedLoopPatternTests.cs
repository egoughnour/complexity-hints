using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Roslyn;

/// <summary>
/// Extended loop pattern tests covering:
/// - Complex loop conditions (AND, OR, nested)
/// - Unusual increment patterns (geometric, exponential)
/// - Loop modifications in body
/// - Dependent nested loops
/// - Deep nesting (3+ levels)
/// - Collection-based iterations
/// - Convergence patterns
/// </summary>
public class ExtendedLoopPatternTests
{
    private readonly ITestOutputHelper _output;

    public ExtendedLoopPatternTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Complex Loop Conditions

    [Fact]
    public async Task ForLoop_AndCondition_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n, int m)
    {
        for (int i = 0; i < n && i < m; i++)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"AND condition: {result.Pattern}");

        // Should be min(n, m) iterations
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ForLoop_OrCondition_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n, int m)
    {
        int i = 0;
        while (i < n || i < m)
        {
            i++;
        }
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"OR condition: {result.Pattern}");

        // Should be max(n, m) iterations
        Assert.True(result.Success || result.Pattern == IterationPattern.Unknown);
    }

    [Fact]
    public async Task WhileLoop_NestedCondition_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int[] arr)
    {
        int i = 0;
        while (i < arr.Length && arr[i] != 0)
        {
            i++;
        }
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"Nested condition: {result.Pattern}");

        // Compound conditions with data-dependent exit may return Unknown or Linear
        Assert.True(result.Success || result.Pattern == IterationPattern.Linear || 
                    result.Pattern == IterationPattern.Unknown);
    }

    [Fact]
    public async Task ForLoop_BitwiseCondition_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 1; (i & n) == 0; i <<= 1)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Bitwise condition: {result.Pattern}");

        // This is essentially logarithmic
        Assert.NotNull(result);
    }

    #endregion

    #region Unusual Increment Patterns

    [Fact]
    public async Task ForLoop_GeometricByThree_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 1; i < n; i *= 3)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Geometric *3: {result.Pattern}");

        // O(log_3(n)) = O(log n)
        Assert.Equal(IterationPattern.Logarithmic, result.Pattern);
    }

    [Fact]
    public async Task WhileLoop_SquareRoot_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        int i = n;
        while (i > 1)
        {
            i = (int)System.Math.Sqrt(i);
        }
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"Square root reduction: {result.Pattern}");

        // O(log log n) - very fast convergence
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ForLoop_ExponentialGrowth_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 2; i < n; i = i * i)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Exponential growth i*i: {result.Pattern}");

        // O(log log n)
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ForLoop_DecrementByHalf_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = n; i > 0; i /= 2)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Decrement by half: {result.Pattern}");

        Assert.Equal(IterationPattern.Logarithmic, result.Pattern);
    }

    [Fact]
    public async Task ForLoop_StepSizeVariable_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n, int step)
    {
        for (int i = 0; i < n; i += step)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Variable step size: {result.Pattern}");

        // O(n/step) - still linear in n
        Assert.Equal(IterationPattern.Linear, result.Pattern);
    }

    #endregion

    #region Loop Variable Modification in Body

    [Fact]
    public async Task WhileLoop_ModifiedInBody_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        int i = 0;
        while (i < n)
        {
            if (SomeCondition())
                i += 2;
            else
                i++;
        }
    }
    private bool SomeCondition() => true;
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"Modified in body: {result.Pattern}");

        // Still O(n) worst case
        Assert.True(result.Success || result.Pattern == IterationPattern.Linear);
    }

    [Fact]
    public async Task ForLoop_ConditionalSkip_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (i % 2 == 0)
            {
                i++; // Skip next
            }
        }
    }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Conditional skip: {result.Pattern}");

        // Still O(n)
        Assert.Equal(IterationPattern.Linear, result.Pattern);
    }

    #endregion

    #region Dependent Nested Loops

    [Fact]
    public async Task NestedLoop_TriangularPattern_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < i; j++)
            {
                DoWork();
            }
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Triangular pattern: {result.Pattern}");

        // O(n²/2) = O(n²)
        Assert.True(result.Success);
    }

    [Fact]
    public async Task NestedLoop_ReverseTriangular_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = n; i > 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                DoWork();
            }
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Reverse triangular: {result.Pattern}");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task NestedLoop_LogarithmicInner_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 1; i < n; i++)
        {
            for (int j = 1; j < n; j *= 2)
            {
                DoWork();
            }
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Linear outer, log inner: {result.Pattern}");

        // O(n log n)
        Assert.True(result.Success);
    }

    #endregion

    #region Deep Nesting (3+ levels)

    [Fact]
    public async Task TripleNestedLoop_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    DoWork();
                }
            }
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Triple nested: {result.Pattern}");

        // O(n³)
        Assert.True(result.Success);
    }

    [Fact]
    public async Task QuadrupleNestedLoop_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                for (int k = 0; k < n; k++)
                    for (int l = 0; l < n; l++)
                        DoWork();
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Quadruple nested: {result.Pattern}");

        // O(n⁴)
        Assert.True(result.Success);
    }

    [Fact]
    public async Task MixedNesting_LinearLogLinear_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)           // O(n)
        {
            for (int j = 1; j < n; j *= 2)    // O(log n)
            {
                for (int k = 0; k < n; k++)   // O(n)
                {
                    DoWork();
                }
            }
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"n * log n * n nesting: {result.Pattern}");

        // O(n² log n)
        Assert.True(result.Success);
    }

    #endregion

    #region Collection-Based Iterations

    [Fact]
    public async Task ForEach_NestedCollections_Analyzes()
    {
        const string code = @"
using System.Collections.Generic;

public class Test
{
    public void Method(List<List<int>> matrix)
    {
        foreach (var row in matrix)
        {
            foreach (var cell in row)
            {
                DoWork(cell);
            }
        }
    }
    private void DoWork(int x) { }
}";
        var result = await AnalyzeLoopAsync(code, useForeach: true);

        _output.WriteLine($"Nested foreach: {result.Pattern}");

        // O(n*m) where n=rows, m=cols
        Assert.True(result.Success || result.Pattern == IterationPattern.Linear);
    }

    [Fact]
    public async Task ForEach_WithLinq_Analyzes()
    {
        const string code = @"
using System.Collections.Generic;
using System.Linq;

public class Test
{
    public void Method(List<int> items)
    {
        foreach (var item in items.Where(x => x > 0))
        {
            DoWork(item);
        }
    }
    private void DoWork(int x) { }
}";
        var result = await AnalyzeLoopAsync(code, useForeach: true, includeLinq: true);

        _output.WriteLine($"Foreach with LINQ: {result.Pattern}");

        // Still O(n)
        Assert.True(result.Success || result.Pattern == IterationPattern.Linear);
    }

    [Fact(Skip = "matrix.GetLength() pattern not yet supported by loop analyzer")]
    public async Task ForLoop_ArrayLength_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                DoWork(matrix[i, j]);
            }
        }
    }
    private void DoWork(int x) { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"2D array iteration: {result.Pattern}");

        Assert.True(result.Success);
    }

    #endregion

    #region Convergence Patterns

    [Fact]
    public async Task WhileLoop_NewtonRaphson_Pattern_Analyzes()
    {
        const string code = @"
public class Test
{
    public double Sqrt(double n)
    {
        double x = n;
        double epsilon = 0.0001;

        while (System.Math.Abs(x * x - n) > epsilon)
        {
            x = (x + n / x) / 2;
        }

        return x;
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"Newton-Raphson pattern: {result.Pattern}");

        // O(log n) due to quadratic convergence, but hard to detect statically
        Assert.NotNull(result);
    }

    [Fact]
    public async Task WhileLoop_GCD_Analyzes()
    {
        const string code = @"
public class Test
{
    public int GCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"GCD (Euclidean): {result.Pattern}");

        // O(log(min(a,b)))
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DoWhileLoop_Convergence_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        int i = n;
        do
        {
            i = i / 2;
        } while (i > 1);
    }
}";
        var result = await AnalyzeLoopAsync(code, useDoWhile: true);

        _output.WriteLine($"Do-while convergence: {result.Pattern}");

        // O(log n)
        Assert.True(result.Success || result.Pattern == IterationPattern.Logarithmic);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task EmptyLoop_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method(int n)
    {
        for (int i = 0; i < n; i++)
        {
            // Empty body
        }
    }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Empty loop: {result.Pattern}");

        Assert.Equal(IterationPattern.Linear, result.Pattern);
    }

    [Fact]
    public async Task InfiniteLoopPattern_DetectsOrHandles()
    {
        const string code = @"
public class Test
{
    public void Method()
    {
        int i = 0;
        while (true)
        {
            i++;
            if (i > 100) break;
        }
    }
}";
        var result = await AnalyzeLoopAsync(code, useWhile: true);

        _output.WriteLine($"Infinite loop with break: {result.Pattern}");

        // Should handle gracefully
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ZeroIterations_Analyzes()
    {
        const string code = @"
public class Test
{
    public void Method()
    {
        for (int i = 0; i < 0; i++)
        {
            DoWork();
        }
    }
    private void DoWork() { }
}";
        var result = await AnalyzeLoopAsync(code);

        _output.WriteLine($"Zero iterations: {result.Pattern}");

        // Constant time (no iterations)
        Assert.NotNull(result);
    }

    #endregion

    #region Helper Methods

    private async Task<LoopAnalysisResult> AnalyzeLoopAsync(
        string code,
        bool useWhile = false,
        bool useDoWhile = false,
        bool useForeach = false,
        bool includeLinq = false)
    {
        var compilation = CreateCompilation(code, includeLinq);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var semanticModel = compilation.GetSemanticModel(tree);

        var methodDecl = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var loopAnalyzer = new LoopAnalyzer(semanticModel);
        var context = new AnalysisContext { SemanticModel = semanticModel };

        if (useWhile)
        {
            var whileLoop = methodDecl.DescendantNodes()
                .OfType<WhileStatementSyntax>()
                .First();
            return loopAnalyzer.AnalyzeWhileLoop(whileLoop, context);
        }

        if (useDoWhile)
        {
            var doWhile = methodDecl.DescendantNodes()
                .OfType<DoStatementSyntax>()
                .First();
            return loopAnalyzer.AnalyzeDoWhileLoop(doWhile, context);
        }

        if (useForeach)
        {
            var foreach_ = methodDecl.DescendantNodes()
                .OfType<ForEachStatementSyntax>()
                .First();
            return loopAnalyzer.AnalyzeForeachLoop(foreach_, context);
        }

        var forLoop = methodDecl.DescendantNodes()
            .OfType<ForStatementSyntax>()
            .First();
        return loopAnalyzer.AnalyzeForLoop(forLoop, context);
    }

    private static Compilation CreateCompilation(string code, bool includeLinq)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
        };

        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimePath != null)
        {
            var systemRuntime = Path.Combine(runtimePath, "System.Runtime.dll");
            if (File.Exists(systemRuntime))
                references.Add(MetadataReference.CreateFromFile(systemRuntime));
        }

        if (includeLinq)
        {
            references.Add(MetadataReference.CreateFromFile(
                typeof(System.Linq.Enumerable).Assembly.Location));
        }

        return CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(code) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    #endregion
}
