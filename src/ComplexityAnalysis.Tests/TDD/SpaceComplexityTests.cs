using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// TDD tests for space/memory complexity analysis.
/// These tests are EXPECTED TO FAIL until the feature is implemented.
///
/// Covers: Stack depth, heap allocations, collection growth, auxiliary space
/// </summary>
public class SpaceComplexityTests
{
    private readonly ITestOutputHelper _output;

    public SpaceComplexityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Stack Depth Analysis

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task RecursiveFactorial_DetectsLinearStackDepth()
    {
        const string code = @"
public class Algorithms
{
    public long Factorial(int n)
    {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }
}";
        var result = await AnalyzeSpaceAsync(code, "Factorial");

        _output.WriteLine($"Factorial stack depth: {result?.ToBigONotation()}");

        // Stack depth: O(n) due to n recursive calls
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task TailRecursive_DetectsConstantStackDepth()
    {
        const string code = @"
public class Algorithms
{
    public long FactorialTail(int n, long acc = 1)
    {
        if (n <= 1) return acc;
        return FactorialTail(n - 1, n * acc);  // Tail recursive
    }
}";
        var result = await AnalyzeSpaceAsync(code, "FactorialTail");

        _output.WriteLine($"Tail recursive stack: {result?.ToBigONotation()}");

        // With tail call optimization: O(1)
        // Without TCO: O(n) - should detect and report both
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task BinaryTreeRecursion_DetectsLogStackDepth()
    {
        const string code = @"
public class Algorithms
{
    public int BinarySearch(int[] arr, int target, int lo, int hi)
    {
        if (lo > hi) return -1;
        int mid = lo + (hi - lo) / 2;
        if (arr[mid] == target) return mid;
        if (arr[mid] > target)
            return BinarySearch(arr, target, lo, mid - 1);
        else
            return BinarySearch(arr, target, mid + 1, hi);
    }
}";
        var result = await AnalyzeSpaceAsync(code, "BinarySearch");

        _output.WriteLine($"Binary search stack: {result?.ToBigONotation()}");

        // Stack depth: O(log n)
        Assert.NotNull(result);
        AssertLogarithmicComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task FibonacciRecursive_DetectsLinearStackDepth()
    {
        const string code = @"
public class Algorithms
{
    public long Fib(int n)
    {
        if (n <= 1) return n;
        return Fib(n - 1) + Fib(n - 2);
    }
}";
        var result = await AnalyzeSpaceAsync(code, "Fib");

        _output.WriteLine($"Fibonacci stack depth: {result?.ToBigONotation()}");

        // Stack depth: O(n) - max depth of recursion tree
        // (even though time is O(2^n))
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    #endregion

    #region Heap Allocation Analysis

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task ArrayAllocation_DetectsLinearSpace()
    {
        const string code = @"
public class Algorithms
{
    public int[] CreateArray(int n)
    {
        return new int[n];
    }
}";
        var result = await AnalyzeSpaceAsync(code, "CreateArray");

        _output.WriteLine($"Array allocation: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task MatrixAllocation_DetectsQuadraticSpace()
    {
        const string code = @"
public class Algorithms
{
    public int[,] CreateMatrix(int n)
    {
        return new int[n, n];
    }
}";
        var result = await AnalyzeSpaceAsync(code, "CreateMatrix");

        _output.WriteLine($"Matrix allocation: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertQuadraticComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task ListGrowth_DetectsLinearSpace()
    {
        const string code = @"
using System.Collections.Generic;

public class Algorithms
{
    public List<int> BuildList(int n)
    {
        var list = new List<int>();
        for (int i = 0; i < n; i++)
        {
            list.Add(i);
        }
        return list;
    }
}";
        var result = await AnalyzeSpaceAsync(code, "BuildList");

        _output.WriteLine($"List growth: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task NestedListAllocation_DetectsCorrectly()
    {
        const string code = @"
using System.Collections.Generic;

public class Algorithms
{
    public List<List<int>> CreateNestedLists(int n)
    {
        var outer = new List<List<int>>();
        for (int i = 0; i < n; i++)
        {
            var inner = new List<int>();
            for (int j = 0; j < n; j++)
            {
                inner.Add(j);
            }
            outer.Add(inner);
        }
        return outer;
    }
}";
        var result = await AnalyzeSpaceAsync(code, "CreateNestedLists");

        _output.WriteLine($"Nested lists: {result?.ToBigONotation()}");

        // O(n²) space
        Assert.NotNull(result);
        AssertQuadraticComplexity(result);
    }

    #endregion

    #region Auxiliary Space Analysis

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task MergeSort_DetectsLinearAuxiliarySpace()
    {
        const string code = @"
public class Algorithms
{
    public void MergeSort(int[] arr, int lo, int hi)
    {
        if (lo >= hi) return;
        int mid = (lo + hi) / 2;
        MergeSort(arr, lo, mid);
        MergeSort(arr, mid + 1, hi);
        Merge(arr, lo, mid, hi);
    }

    private void Merge(int[] arr, int lo, int mid, int hi)
    {
        int[] temp = new int[hi - lo + 1];  // O(n) auxiliary
        // ... merge logic
    }
}";
        var result = await AnalyzeSpaceAsync(code, "MergeSort");

        _output.WriteLine($"MergeSort auxiliary: {result?.ToBigONotation()}");

        // O(n) auxiliary space for temp array
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task QuickSort_DetectsLogAuxiliarySpace()
    {
        const string code = @"
public class Algorithms
{
    public void QuickSort(int[] arr, int lo, int hi)
    {
        if (lo >= hi) return;
        int pivot = Partition(arr, lo, hi);
        QuickSort(arr, lo, pivot - 1);
        QuickSort(arr, pivot + 1, hi);
    }

    private int Partition(int[] arr, int lo, int hi)
    {
        // In-place partitioning
        int pivot = arr[hi];
        int i = lo - 1;
        for (int j = lo; j < hi; j++)
        {
            if (arr[j] <= pivot)
            {
                i++;
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
        }
        (arr[i + 1], arr[hi]) = (arr[hi], arr[i + 1]);
        return i + 1;
    }
}";
        var result = await AnalyzeSpaceAsync(code, "QuickSort");

        _output.WriteLine($"QuickSort auxiliary: {result?.ToBigONotation()}");

        // O(log n) average case (stack depth), O(n) worst case
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task InPlaceAlgorithm_DetectsConstantSpace()
    {
        const string code = @"
public class Algorithms
{
    public void ReverseInPlace(int[] arr)
    {
        int left = 0, right = arr.Length - 1;
        while (left < right)
        {
            int temp = arr[left];
            arr[left] = arr[right];
            arr[right] = temp;
            left++;
            right--;
        }
    }
}";
        var result = await AnalyzeSpaceAsync(code, "ReverseInPlace");

        _output.WriteLine($"In-place reverse: {result?.ToBigONotation()}");

        // O(1) auxiliary space
        Assert.NotNull(result);
        AssertConstantComplexity(result);
    }

    #endregion

    #region Combined Time/Space Analysis

    [Fact(Skip = "TDD: Space complexity analysis not yet implemented")]
    public async Task DPWithMemoization_DetectsSpaceTimeTradeoff()
    {
        const string code = @"
using System.Collections.Generic;

public class Algorithms
{
    private Dictionary<int, long> _memo = new();

    public long FibMemo(int n)
    {
        if (n <= 1) return n;
        if (_memo.TryGetValue(n, out var cached)) return cached;

        long result = FibMemo(n - 1) + FibMemo(n - 2);
        _memo[n] = result;
        return result;
    }
}";
        var timeResult = await AnalyzeMethodAsync(code, "FibMemo");
        var spaceResult = await AnalyzeSpaceAsync(code, "FibMemo");

        _output.WriteLine($"Memoized Fib time: {timeResult?.ToBigONotation()}");
        _output.WriteLine($"Memoized Fib space: {spaceResult?.ToBigONotation()}");

        // Time: O(n), Space: O(n) for memo table
        Assert.NotNull(timeResult);
        Assert.NotNull(spaceResult);
        AssertLinearComplexity(spaceResult);
    }

    #endregion

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

    private async Task<ComplexityExpression?> AnalyzeSpaceAsync(string code, string methodName)
    {
        // TODO: This should use a space-complexity-aware analyzer
        // For now, returns null to indicate not implemented
        return await Task.FromResult<ComplexityExpression?>(null);
    }

    private static Compilation CreateCompilation(string code)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.Dictionary<,>).Assembly.Location),
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

    private static void AssertConstantComplexity(ComplexityExpression expr)
    {
        var isConstant = expr is ConstantComplexity ||
            (expr is PolynomialComplexity p && p.Degree == 0);
        Assert.True(isConstant, $"Expected O(1), got {expr.ToBigONotation()}");
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

    private static void AssertLogarithmicComplexity(ComplexityExpression expr)
    {
        var isLog = expr is LogarithmicComplexity ||
            (expr is PolyLogComplexity pl && pl.PolyDegree == 0 && pl.LogExponent == 1);
        Assert.True(isLog, $"Expected O(log n), got {expr.ToBigONotation()}");
    }

    private static void AssertQuadraticComplexity(ComplexityExpression expr)
    {
        var isQuadratic = expr is PolynomialComplexity p && Math.Abs(p.Degree - 2) < 0.1;
        Assert.True(isQuadratic, $"Expected O(n²), got {expr.ToBigONotation()}");
    }

    #endregion
}
