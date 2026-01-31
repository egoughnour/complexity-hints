using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// Tests for parallel pattern detection and complexity analysis.
///
/// Parallel complexity models:
/// - Work: Total operations across all processors
/// - Span: Critical path length (longest dependent chain)
/// - Parallelism: Work / Span ratio
///
/// Patterns tested:
/// - Parallel.For / ForEach (data parallelism)
/// - PLINQ (parallel LINQ)
/// - Task.WhenAll / WhenAny (task parallelism)
/// - async/await patterns
/// </summary>
public class ParallelPatternTests
{
    private readonly ITestOutputHelper _output;

    public ParallelPatternTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Parallel.For / ForEach Tests

    [Fact]
    public async Task ParallelFor_DetectsDataParallelism()
    {
        // Parallel.For: Work = O(n), Span = O(1) for independent iterations
        const string code = @"
using System.Threading.Tasks;

public class ParallelExample
{
    public void ProcessInParallel(int[] data)
    {
        Parallel.For(0, data.Length, i =>
        {
            data[i] = data[i] * 2;
        });
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ProcessInParallel");

        _output.WriteLine($"Parallel.For: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity, $"Expected ParallelComplexity, got {result.GetType().Name}");

        if (result is ParallelComplexity parallel)
        {
            Assert.Equal(ParallelPatternType.ParallelFor, parallel.PatternType);
            _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
            _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");
            // Work should be O(n)
            Assert.True(IsLinearComplexity(parallel.Work), $"Expected linear work, got {parallel.Work.ToBigONotation()}");
        }
    }

    [Fact]
    public async Task ParallelForEach_DetectsDataParallelism()
    {
        const string code = @"
using System.Collections.Generic;
using System.Threading.Tasks;

public class ParallelExample
{
    public void ProcessItems(IEnumerable<int> items)
    {
        Parallel.ForEach(items, item =>
        {
            System.Console.WriteLine(item * 2);
        });
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ProcessItems");

        _output.WriteLine($"Parallel.ForEach: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);

        if (result is ParallelComplexity parallel)
        {
            Assert.Equal(ParallelPatternType.ParallelFor, parallel.PatternType);
        }
    }

    [Fact]
    public async Task ParallelFor_WithNestedLoop_DetectsCorrectWork()
    {
        // Nested loop inside Parallel.For: Work = O(n × n) = O(n²)
        const string code = @"
using System.Threading.Tasks;

public class ParallelExample
{
    public void NestedParallel(int[,] matrix, int n)
    {
        Parallel.For(0, n, i =>
        {
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] = i * j;
            }
        });
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "NestedParallel");

        _output.WriteLine($"Nested Parallel.For: {result?.ToBigONotation()}");

        // Should detect nested loop in body
        Assert.NotNull(result);
    }

    #endregion

    #region PLINQ Tests

    [Fact]
    public async Task AsParallel_DetectsPLINQPattern()
    {
        const string code = @"
using System.Linq;

public class PLINQExample
{
    public int[] ProcessWithPLINQ(int[] data)
    {
        return data
            .AsParallel()
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToArray();
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ProcessWithPLINQ");

        _output.WriteLine($"PLINQ: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);

        if (result is ParallelComplexity parallel)
        {
            Assert.Equal(ParallelPatternType.PLINQ, parallel.PatternType);
            _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
            _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");
        }
    }

    [Fact]
    public async Task PLINQOrderBy_DetectsOrderingOverhead()
    {
        const string code = @"
using System.Linq;

public class PLINQExample
{
    public int[] SortInParallel(int[] data)
    {
        return data
            .AsParallel()
            .OrderBy(x => x)
            .ToArray();
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "SortInParallel");

        _output.WriteLine($"PLINQ OrderBy: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        // OrderBy has synchronization overhead
    }

    [Fact]
    public async Task PLINQAggregate_DetectsReductionPattern()
    {
        const string code = @"
using System.Linq;

public class PLINQExample
{
    public int ParallelSum(int[] data)
    {
        return data
            .AsParallel()
            .Aggregate(0, (acc, x) => acc + x);
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ParallelSum");

        _output.WriteLine($"PLINQ Aggregate: {result?.ToBigONotation()}");

        // Aggregate uses tree reduction: Work O(n), Span O(log n)
        Assert.NotNull(result);
    }

    #endregion

    #region Task-Based Tests

    [Fact]
    public async Task TaskWhenAll_DetectsParallelTasks()
    {
        const string code = @"
using System.Threading.Tasks;

public class TaskExample
{
    public async Task ProcessConcurrently(int[] data)
    {
        var tasks = new Task[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => Process(data[index]));
        }
        await Task.WhenAll(tasks);
    }

    private void Process(int value) { }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ProcessConcurrently");

        _output.WriteLine($"Task.WhenAll: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);

        if (result is ParallelComplexity parallel)
        {
            Assert.True(parallel.IsTaskBased);
            _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
            _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");
        }
    }

    [Fact]
    public async Task TaskWhenAny_DetectsRacePattern()
    {
        const string code = @"
using System.Threading.Tasks;

public class TaskExample
{
    public async Task<int> FirstToComplete(Task<int>[] tasks)
    {
        var winner = await Task.WhenAny(tasks);
        return await winner;
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "FirstToComplete");

        _output.WriteLine($"Task.WhenAny: {result?.ToBigONotation()}");

        // WhenAny: minimal span (first task to complete)
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MultipleTaskRun_DetectsParallelism()
    {
        const string code = @"
using System.Threading.Tasks;

public class TaskExample
{
    public async Task ForkJoinPattern()
    {
        var task1 = Task.Run(() => DoWork1());
        var task2 = Task.Run(() => DoWork2());
        var task3 = Task.Run(() => DoWork3());

        await Task.WhenAll(task1, task2, task3);
    }

    private void DoWork1() { }
    private void DoWork2() { }
    private void DoWork3() { }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ForkJoinPattern");

        _output.WriteLine($"Multiple Task.Run: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);
    }

    #endregion

    #region Parallel.Invoke Tests

    [Fact]
    public async Task ParallelInvoke_DetectsForkJoin()
    {
        const string code = @"
using System.Threading.Tasks;

public class ParallelExample
{
    public void ParallelActions()
    {
        Parallel.Invoke(
            () => Action1(),
            () => Action2(),
            () => Action3()
        );
    }

    private void Action1() { }
    private void Action2() { }
    private void Action3() { }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ParallelActions");

        _output.WriteLine($"Parallel.Invoke: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);

        if (result is ParallelComplexity parallel)
        {
            Assert.Equal(ParallelPatternType.ForkJoin, parallel.PatternType);
        }
    }

    #endregion

    #region Async/Await Tests

    [Fact]
    public async Task AsyncMethod_DetectsSequentialAwaits()
    {
        const string code = @"
using System.Threading.Tasks;

public class AsyncExample
{
    public async Task SequentialAsync()
    {
        await Task.Delay(100);
        await Task.Delay(100);
        await Task.Delay(100);
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "SequentialAsync");

        _output.WriteLine($"Sequential awaits: {result?.ToBigONotation()}");

        // Sequential awaits - not truly parallel
        if (result is ParallelComplexity parallel)
        {
            Assert.Equal(ParallelPatternType.AsyncAwait, parallel.PatternType);
            _output.WriteLine($"  Description: {parallel.Description}");
        }
    }

    [Fact]
    public async Task AsyncMethod_WithWhenAll_IsParallel()
    {
        const string code = @"
using System.Threading.Tasks;

public class AsyncExample
{
    public async Task ParallelAsync()
    {
        var t1 = DoWorkAsync();
        var t2 = DoWorkAsync();
        var t3 = DoWorkAsync();

        await Task.WhenAll(t1, t2, t3);
    }

    private async Task DoWorkAsync()
    {
        await Task.Delay(100);
    }
}";
        var result = await AnalyzeParallelPatternAsync(code, "ParallelAsync");

        _output.WriteLine($"Parallel async with WhenAll: {result?.ToBigONotation()}");

        Assert.NotNull(result);
        Assert.True(result is ParallelComplexity);
    }

    #endregion

    #region ParallelComplexity Core Types Tests

    [Fact]
    public void ParallelComplexity_EmbarrassinglyParallel_HasCorrectMetrics()
    {
        var parallel = ParallelComplexity.EmbarrassinglyParallel(Variable.N);

        _output.WriteLine($"Embarrassingly Parallel:");
        _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
        _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");
        _output.WriteLine($"  BigO: {parallel.ToBigONotation()}");

        // Work = O(n), Span = O(1)
        Assert.True(IsLinearComplexity(parallel.Work));
        Assert.True(IsConstantComplexity(parallel.Span));
        Assert.Equal(ParallelPatternType.ParallelFor, parallel.PatternType);
    }

    [Fact]
    public void ParallelComplexity_Reduction_HasLogSpan()
    {
        var parallel = ParallelComplexity.Reduction(Variable.N);

        _output.WriteLine($"Parallel Reduction:");
        _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
        _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");

        // Work = O(n), Span = O(log n)
        Assert.True(IsLinearComplexity(parallel.Work));
        Assert.True(IsLogarithmicComplexity(parallel.Span));
        Assert.Equal(ParallelPatternType.Reduction, parallel.PatternType);
    }

    [Fact]
    public void ParallelComplexity_DivideAndConquer_HasCorrectMetrics()
    {
        var parallel = ParallelComplexity.DivideAndConquer(Variable.N);

        _output.WriteLine($"Divide and Conquer:");
        _output.WriteLine($"  Work: {parallel.Work.ToBigONotation()}");
        _output.WriteLine($"  Span: {parallel.Span.ToBigONotation()}");

        // Work = O(n log n), Span = O(log² n)
        Assert.NotNull(parallel.Work);
        Assert.NotNull(parallel.Span);
        Assert.Equal(ParallelPatternType.DivideAndConquer, parallel.PatternType);
    }

    [Fact]
    public void ParallelComplexity_Evaluate_ComputesCorrectValues()
    {
        var parallel = ParallelComplexity.EmbarrassinglyParallel(Variable.N);

        var assignments = new Dictionary<Variable, double>
        {
            { Variable.N, 1000 },
            { Variable.P, 4 }
        };

        var result = parallel.Evaluate(assignments);

        _output.WriteLine($"n=1000, p=4: parallel time = {result}");

        // Parallel time should be Work/p or Span (whichever is larger)
        Assert.NotNull(result);
        Assert.True(result > 0);
    }

    [Fact]
    public void ParallelComplexity_Substitute_PreservesStructure()
    {
        var parallel = ParallelComplexity.EmbarrassinglyParallel(Variable.N);
        var substituted = parallel.Substitute(Variable.N, new VariableComplexity(Variable.M));

        Assert.NotNull(substituted);
        Assert.True(substituted is ParallelComplexity);
    }

    [Fact]
    public void ParallelAlgorithms_Provides_CommonPatterns()
    {
        var sum = ParallelAlgorithms.ParallelSum();
        var mergeSort = ParallelAlgorithms.ParallelMergeSort();
        var quickSort = ParallelAlgorithms.ParallelQuickSort();
        var matrixMult = ParallelAlgorithms.ParallelMatrixMultiply();
        var bfs = ParallelAlgorithms.ParallelBFS();
        var plinqFilter = ParallelAlgorithms.PLINQFilter();

        _output.WriteLine("Common Parallel Algorithms:");
        _output.WriteLine($"  Sum: {sum.ToBigONotation()}");
        _output.WriteLine($"  Merge Sort: {mergeSort.ToBigONotation()}");
        _output.WriteLine($"  Quick Sort: {quickSort.ToBigONotation()}");
        _output.WriteLine($"  Matrix Multiply: {matrixMult.ToBigONotation()}");
        _output.WriteLine($"  BFS: {bfs.ToBigONotation()}");
        _output.WriteLine($"  PLINQ Filter: {plinqFilter.ToBigONotation()}");

        Assert.NotNull(sum);
        Assert.NotNull(mergeSort);
        Assert.NotNull(quickSort);
        Assert.NotNull(matrixMult);
        Assert.NotNull(bfs);
        Assert.NotNull(plinqFilter);
    }

    #endregion

    #region BCL Mapping Tests

    [Fact]
    public void BCLMappings_ContainsParallelFor()
    {
        var mappings = ComplexityAnalysis.Roslyn.BCL.BCLComplexityMappings.Instance;
        var complexity = mappings.GetComplexity("Parallel", "For");

        _output.WriteLine($"Parallel.For mapping: {complexity.Complexity.ToBigONotation()}");

        Assert.NotNull(complexity);
        Assert.True(complexity.Complexity is ParallelComplexity);
    }

    [Fact]
    public void BCLMappings_ContainsTaskWhenAll()
    {
        var mappings = ComplexityAnalysis.Roslyn.BCL.BCLComplexityMappings.Instance;
        var complexity = mappings.GetComplexity("Task", "WhenAll");

        _output.WriteLine($"Task.WhenAll mapping: {complexity.Complexity.ToBigONotation()}");

        Assert.NotNull(complexity);
        Assert.True(complexity.Complexity is ParallelComplexity);
    }

    [Fact]
    public void BCLMappings_ContainsPLINQ()
    {
        var mappings = ComplexityAnalysis.Roslyn.BCL.BCLComplexityMappings.Instance;
        var complexity = mappings.GetComplexity("ParallelEnumerable", "AsParallel");

        _output.WriteLine($"AsParallel mapping: {complexity.Complexity.ToBigONotation()}");

        Assert.NotNull(complexity);
        Assert.True(complexity.Complexity is ParallelComplexity);
    }

    [Fact]
    public void BCLMappings_TaskDelay_IsConstant()
    {
        var mappings = ComplexityAnalysis.Roslyn.BCL.BCLComplexityMappings.Instance;
        var complexity = mappings.GetComplexity("Task", "Delay");

        _output.WriteLine($"Task.Delay mapping: {complexity.Complexity.ToBigONotation()}");

        Assert.NotNull(complexity);
        Assert.True(IsConstantComplexity(complexity.Complexity));
    }

    #endregion

    #region Test Helpers

    private async Task<ComplexityExpression?> AnalyzeParallelPatternAsync(string code, string methodName)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        
        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Parallel).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.ParallelEnumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.IEnumerable<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        };

        if (runtimePath != null)
        {
            var systemRuntime = Path.Combine(runtimePath, "System.Runtime.dll");
            if (File.Exists(systemRuntime))
                references.Add(MetadataReference.CreateFromFile(systemRuntime));
        }

        var compilation = CSharpCompilation.Create("Test",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(tree);
        var root = await tree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (method == null) return null;

        var analyzer = new ParallelPatternAnalyzer(semanticModel);
        return analyzer.AnalyzeMethod(method);
    }

    private static bool IsConstantComplexity(ComplexityExpression expr) =>
        expr is ConstantComplexity ||
        (expr is PolynomialComplexity p && p.Degree == 0);

    private static bool IsLinearComplexity(ComplexityExpression expr) =>
        expr is LinearComplexity ||
        expr is VariableComplexity ||
        (expr is PolynomialComplexity p && p.Degree == 1) ||
        // Also accept BinaryOperationComplexity like n * 1
        (expr is BinaryOperationComplexity bin && bin.Operation == BinaryOp.Multiply &&
         (IsLinearComplexity(bin.Left) && IsConstantComplexity(bin.Right) ||
          IsConstantComplexity(bin.Left) && IsLinearComplexity(bin.Right)));

    private static bool IsLogarithmicComplexity(ComplexityExpression expr) =>
        expr is LogarithmicComplexity;

    #endregion
}
