using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// TDD tests for amortized complexity analysis.
/// These tests are EXPECTED TO FAIL until the feature is implemented.
///
/// Amortized analysis: Average cost over sequence of operations
/// Examples: Dynamic array resizing, hash table rehashing, splay trees
/// </summary>
public class AmortizedAnalysisTests
{
    private readonly ITestOutputHelper _output;

    public AmortizedAnalysisTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task DynamicArrayResize_DetectsAmortizedConstant()
    {
        // ArrayList/List<T> doubling strategy
        // Worst case: O(n) for resize
        // Amortized: O(1) per insertion
        const string code = @"
public class DynamicArray
{
    private int[] _data = new int[1];
    private int _count = 0;

    public void Add(int item)
    {
        if (_count == _data.Length)
        {
            // Resize: O(n) copy
            var newData = new int[_data.Length * 2];
            for (int i = 0; i < _count; i++)
                newData[i] = _data[i];
            _data = newData;
        }
        _data[_count++] = item;
    }
}";
        var result = await AnalyzeWithAmortizationAsync(code, "Add");

        _output.WriteLine($"Add amortized: {result?.ToBigONotation()}");

        // Should report O(1) amortized, not O(n) worst case
        Assert.NotNull(result);
        AssertConstantComplexity(result);
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task MultipleAdds_DetectsLinearTotal()
    {
        // n Add operations should be O(n) total, not O(n²)
        const string code = @"
public class Usage
{
    public void AddMany(System.Collections.Generic.List<int> list, int n)
    {
        for (int i = 0; i < n; i++)
        {
            list.Add(i);  // Each is O(1) amortized
        }
    }
}";
        var result = await AnalyzeMethodAsync(code, "AddMany");

        _output.WriteLine($"AddMany: {result?.ToBigONotation()}");

        // n iterations × O(1) amortized = O(n) total
        Assert.NotNull(result);
        AssertLinearComplexity(result);
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task HashTableRehash_DetectsAmortizedConstant()
    {
        const string code = @"
public class SimpleHashTable
{
    private int[] _buckets = new int[4];
    private int _count = 0;
    private const double LoadFactor = 0.75;

    public void Insert(int key)
    {
        if (_count >= _buckets.Length * LoadFactor)
        {
            Rehash();  // O(n) operation
        }
        _buckets[key % _buckets.Length] = key;
        _count++;
    }

    private void Rehash()
    {
        var oldBuckets = _buckets;
        _buckets = new int[oldBuckets.Length * 2];
        for (int i = 0; i < oldBuckets.Length; i++)
        {
            if (oldBuckets[i] != 0)
                _buckets[oldBuckets[i] % _buckets.Length] = oldBuckets[i];
        }
    }
}";
        var result = await AnalyzeWithAmortizationAsync(code, "Insert");

        _output.WriteLine($"HashTable Insert amortized: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertConstantComplexity(result);
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task StackWithMultipop_DetectsAmortizedConstant()
    {
        // Multipop can pop k items at O(k) cost
        // But amortized over push/pop sequence is O(1)
        const string code = @"
public class MultipopStack
{
    private System.Collections.Generic.Stack<int> _stack = new();

    public void Push(int item) => _stack.Push(item);

    public void Multipop(int k)
    {
        for (int i = 0; i < k && _stack.Count > 0; i++)
        {
            _stack.Pop();
        }
    }
}";
        var resultPush = await AnalyzeWithAmortizationAsync(code, "Push");
        var resultPop = await AnalyzeWithAmortizationAsync(code, "Multipop");

        _output.WriteLine($"Push amortized: {resultPush?.ToBigONotation()}");
        _output.WriteLine($"Multipop amortized: {resultPop?.ToBigONotation()}");

        // Both should be O(1) amortized using accounting/potential method
        Assert.NotNull(resultPush);
        Assert.NotNull(resultPop);
        AssertConstantComplexity(resultPush);
        AssertConstantComplexity(resultPop);
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task IncrementBinaryCounter_DetectsAmortizedConstant()
    {
        // Incrementing binary counter: worst case O(k) bits flip
        // Amortized: O(1) per increment
        const string code = @"
public class BinaryCounter
{
    private bool[] _bits = new bool[32];

    public void Increment()
    {
        int i = 0;
        while (i < _bits.Length && _bits[i])
        {
            _bits[i] = false;  // Flip 1 to 0
            i++;
        }
        if (i < _bits.Length)
            _bits[i] = true;  // Flip 0 to 1
    }
}";
        var result = await AnalyzeWithAmortizationAsync(code, "Increment");

        _output.WriteLine($"Binary counter increment: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        AssertConstantComplexity(result);
    }

    [Fact(Skip = "TDD: Amortized analysis not yet implemented")]
    public async Task UnionFind_DetectsInverseAckermann()
    {
        // Union-Find with path compression + union by rank
        // Amortized: O(α(n)) ≈ O(1) for practical purposes
        const string code = @"
public class UnionFind
{
    private int[] _parent;
    private int[] _rank;

    public UnionFind(int n)
    {
        _parent = new int[n];
        _rank = new int[n];
        for (int i = 0; i < n; i++) _parent[i] = i;
    }

    public int Find(int x)
    {
        if (_parent[x] != x)
            _parent[x] = Find(_parent[x]);  // Path compression
        return _parent[x];
    }

    public void Union(int x, int y)
    {
        int px = Find(x), py = Find(y);
        if (px == py) return;

        // Union by rank
        if (_rank[px] < _rank[py])
            _parent[px] = py;
        else if (_rank[px] > _rank[py])
            _parent[py] = px;
        else
        {
            _parent[py] = px;
            _rank[px]++;
        }
    }
}";
        var resultFind = await AnalyzeWithAmortizationAsync(code, "Find");
        var resultUnion = await AnalyzeWithAmortizationAsync(code, "Union");

        _output.WriteLine($"Find amortized: {resultFind?.ToBigONotation()}");
        _output.WriteLine($"Union amortized: {resultUnion?.ToBigONotation()}");

        // Should detect O(α(n)) or at least report as "effectively constant"
        Assert.NotNull(resultFind);
        Assert.NotNull(resultUnion);
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

    private async Task<ComplexityExpression?> AnalyzeWithAmortizationAsync(string code, string methodName)
    {
        // TODO: This should use an amortization-aware analyzer
        // For now, falls back to regular analysis
        return await AnalyzeMethodAsync(code, methodName);
    }

    private static Compilation CreateCompilation(string code)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.Stack<>).Assembly.Location),
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

    #endregion
}
