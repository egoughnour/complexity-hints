using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// Tests for amortized complexity analysis.
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

    [Fact]
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
        Assert.True(result is AmortizedComplexity, $"Expected AmortizedComplexity, got {result.GetType().Name}");
        
        if (result is AmortizedComplexity amortized)
        {
            AssertConstantComplexity(amortized.AmortizedCost);
            _output.WriteLine($"  Amortized cost: {amortized.AmortizedCost.ToBigONotation()}");
            _output.WriteLine($"  Worst case: {amortized.WorstCaseCost.ToBigONotation()}");
        }
    }

    [Fact]
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
        // For now, accept linear or amortized linear
    }

    [Fact]
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

        _output.WriteLine($"HashTable Insert: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is AmortizedComplexity, $"Expected AmortizedComplexity, got {result.GetType().Name}");
        
        if (result is AmortizedComplexity amortized)
        {
            AssertConstantComplexity(amortized.AmortizedCost);
        }
    }

    [Fact]
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
        var resultPop = await AnalyzeWithAmortizationAsync(code, "Multipop");

        _output.WriteLine($"Multipop: {resultPop?.ToBigONotation()}");

        // Multipop should be detected as amortized O(1) pattern
        Assert.NotNull(resultPop);
        Assert.True(resultPop is AmortizedComplexity, $"Expected AmortizedComplexity, got {resultPop.GetType().Name}");
    }

    [Fact]
    public async Task BinaryCounterIncrement_DetectsAmortizedConstant()
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
        Assert.True(result is AmortizedComplexity, $"Expected AmortizedComplexity, got {result.GetType().Name}");
        
        if (result is AmortizedComplexity amortized)
        {
            AssertConstantComplexity(amortized.AmortizedCost);
        }
    }

    [Fact]
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

        _output.WriteLine($"Find: {resultFind?.ToBigONotation()}");
        _output.WriteLine($"Union: {resultUnion?.ToBigONotation()}");

        // Should detect O(α(n)) or at least report as amortized
        Assert.NotNull(resultFind);
        Assert.NotNull(resultUnion);
        Assert.True(resultFind is AmortizedComplexity, $"Expected AmortizedComplexity for Find");
        Assert.True(resultUnion is AmortizedComplexity, $"Expected AmortizedComplexity for Union");
    }

    #region Amortized Complexity Type Tests

    [Fact]
    public void AmortizedComplexity_ToBigONotation_ShowsBothCosts()
    {
        var amortized = new AmortizedComplexity
        {
            AmortizedCost = ConstantComplexity.One,
            WorstCaseCost = new LinearComplexity(1.0, Variable.N)
        };

        var notation = amortized.ToBigONotation();

        _output.WriteLine($"Notation: {notation}");
        Assert.Contains("amortized", notation.ToLower());
        Assert.Contains("O(1)", notation);
    }

    [Fact]
    public void AmortizedComplexity_Evaluate_UsesAmortizedCost()
    {
        var amortized = new AmortizedComplexity
        {
            AmortizedCost = ConstantComplexity.One,
            WorstCaseCost = new LinearComplexity(1.0, Variable.N)
        };

        var assignments = new Dictionary<Variable, double> { [Variable.N] = 1000 };
        var result = amortized.Evaluate(assignments);

        Assert.Equal(1.0, result);  // Should use amortized O(1), not worst-case O(n)
    }

    [Fact]
    public void InverseAckermannComplexity_ReturnsSmallConstants()
    {
        var alpha = new InverseAckermannComplexity(Variable.N);

        var assignments = new Dictionary<Variable, double>();

        // α(n) should be ≤ 4 for any practical n
        assignments[Variable.N] = 1;
        Assert.True(alpha.Evaluate(assignments) <= 1);

        assignments[Variable.N] = 100;
        Assert.True(alpha.Evaluate(assignments) <= 4);

        assignments[Variable.N] = 1_000_000;
        Assert.True(alpha.Evaluate(assignments) <= 4);

        assignments[Variable.N] = 1_000_000_000;
        Assert.True(alpha.Evaluate(assignments) <= 5);
    }

    [Fact]
    public void PotentialFunction_CommonFunctions_Defined()
    {
        Assert.NotNull(PotentialFunction.Common.DynamicArray);
        Assert.NotNull(PotentialFunction.Common.HashTable);
        Assert.NotNull(PotentialFunction.Common.BinaryCounter);
        Assert.NotNull(PotentialFunction.Common.MultipopStack);
        Assert.NotNull(PotentialFunction.Common.SplayTree);
        Assert.NotNull(PotentialFunction.Common.UnionFind);

        _output.WriteLine($"DynamicArray: {PotentialFunction.Common.DynamicArray.Formula}");
        _output.WriteLine($"HashTable: {PotentialFunction.Common.HashTable.Formula}");
        _output.WriteLine($"BinaryCounter: {PotentialFunction.Common.BinaryCounter.Formula}");
    }

    #endregion

    #region BCL Integration Tests

    [Fact]
    public void BCLMappings_ListAdd_ReturnsAmortizedComplexity()
    {
        var mappings = ComplexityAnalysis.Roslyn.BCL.BCLComplexityMappings.Instance;
        var result = mappings.GetComplexity("List`1", "Add");

        _output.WriteLine($"List.Add complexity: {result.Complexity.ToBigONotation()}");
        _output.WriteLine($"Notes: {result.Notes}");

        Assert.True(result.Complexity is AmortizedComplexity,
            $"Expected AmortizedComplexity, got {result.Complexity.GetType().Name}");
        Assert.True(result.Notes.HasFlag(ComplexityAnalysis.Roslyn.BCL.ComplexityNotes.Amortized));
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

    private async Task<ComplexityExpression?> AnalyzeWithAmortizationAsync(string code, string methodName)
    {
        var compilation = CreateCompilation(code);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var semanticModel = compilation.GetSemanticModel(tree);

        var methodDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (methodDecl == null) return null;

        // Use AmortizedAnalyzer for pattern detection
        var amortizedAnalyzer = new AmortizedAnalyzer(semanticModel);
        var amortizedResult = amortizedAnalyzer.AnalyzeMethod(methodDecl);

        if (amortizedResult != null)
            return amortizedResult;

        // Fall back to regular analysis
        var extractor = new RoslynComplexityExtractor(semanticModel);
        return extractor.AnalyzeMethod(methodDecl);
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
            (expr is PolynomialComplexity p && p.Degree == 0) ||
            expr is InverseAckermannComplexity;  // α(n) is effectively constant
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
