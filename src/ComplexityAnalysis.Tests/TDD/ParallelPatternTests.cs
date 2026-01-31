using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// TDD tests for parallel and concurrent pattern analysis.
/// These tests are EXPECTED TO FAIL until the feature is implemented.
///
/// Covers: Parallel.For, Parallel.ForEach, Task patterns, PLINQ, async/await
/// </summary>
public class ParallelPatternTests
{
    private readonly ITestOutputHelper _output;

    public ParallelPatternTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task ParallelFor_DetectsParallelization()
    {
        const string code = @"
using System.Threading.Tasks;

public class ParallelOps
{
    public void ProcessParallel(int[] arr)
    {
        Parallel.For(0, arr.Length, i =>
        {
            arr[i] = arr[i] * 2;
        });
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessParallel");

        _output.WriteLine($"Parallel.For: {result?.ToBigONotation()}");

        // Should report O(n/p) where p is processor count, or O(n) work / O(n/p) span
        Assert.NotNull(result);
        // Check for parallel annotation
        Assert.True(result is ParallelComplexity || HasParallelAnnotation(result));
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task ParallelForEach_DetectsParallelization()
    {
        const string code = @"
using System.Collections.Generic;
using System.Threading.Tasks;

public class ParallelOps
{
    public void ProcessItems(List<int> items)
    {
        Parallel.ForEach(items, item =>
        {
            DoWork(item);
        });
    }

    private void DoWork(int x) { }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessItems");

        _output.WriteLine($"Parallel.ForEach: {result?.ToBigONotation()}");

        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task PLINQ_DetectsParallelization()
    {
        const string code = @"
using System.Collections.Generic;
using System.Linq;

public class PLINQOps
{
    public List<int> ProcessWithPLINQ(List<int> items)
    {
        return items
            .AsParallel()
            .Where(x => x > 0)
            .Select(x => x * 2)
            .ToList();
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessWithPLINQ");

        _output.WriteLine($"PLINQ: {result?.ToBigONotation()}");

        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task TaskWhenAll_DetectsParallelExecution()
    {
        const string code = @"
using System.Threading.Tasks;

public class TaskOps
{
    public async Task ProcessConcurrently(int[] arr)
    {
        var tasks = new Task[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => arr[index] = arr[index] * 2);
        }
        await Task.WhenAll(tasks);
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessConcurrently");

        _output.WriteLine($"Task.WhenAll: {result?.ToBigONotation()}");

        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task NestedParallelFor_DetectsCorrectly()
    {
        const string code = @"
using System.Threading.Tasks;

public class NestedParallel
{
    public void MatrixOp(int[,] matrix, int n)
    {
        Parallel.For(0, n, i =>
        {
            Parallel.For(0, n, j =>
            {
                matrix[i, j] = i + j;
            });
        });
    }
}";
        var result = await AnalyzeMethodAsync(code, "MatrixOp");

        _output.WriteLine($"Nested Parallel.For: {result?.ToBigONotation()}");

        // Work: O(n²), Span: O(1) with enough parallelism
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task ParallelWithReduction_DetectsCorrectly()
    {
        const string code = @"
using System.Threading;
using System.Threading.Tasks;

public class ParallelReduction
{
    public long ParallelSum(int[] arr)
    {
        long total = 0;
        Parallel.For(0, arr.Length,
            () => 0L,  // Local init
            (i, state, local) => local + arr[i],  // Body
            local => Interlocked.Add(ref total, local)  // Final
        );
        return total;
    }
}";
        var result = await AnalyzeMethodAsync(code, "ParallelSum");

        _output.WriteLine($"Parallel reduction: {result?.ToBigONotation()}");

        // Work: O(n), Span: O(log n) for reduction
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task AsyncAwait_ChainedCalls_AnalyzesCorrectly()
    {
        const string code = @"
using System.Threading.Tasks;

public class AsyncOps
{
    public async Task<int> ProcessAsync(int[] arr)
    {
        var result1 = await ComputeAsync(arr);
        var result2 = await ComputeAsync(arr);
        return result1 + result2;
    }

    private async Task<int> ComputeAsync(int[] arr)
    {
        await Task.Delay(1);
        int sum = 0;
        for (int i = 0; i < arr.Length; i++) sum += arr[i];
        return sum;
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessAsync");

        _output.WriteLine($"Async chain: {result?.ToBigONotation()}");

        // Sequential async: O(n) + O(n) = O(n)
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task AsyncAwait_ConcurrentCalls_AnalyzesCorrectly()
    {
        const string code = @"
using System.Threading.Tasks;

public class AsyncOps
{
    public async Task<int> ProcessConcurrentAsync(int[] arr)
    {
        var task1 = ComputeAsync(arr);
        var task2 = ComputeAsync(arr);
        await Task.WhenAll(task1, task2);
        return task1.Result + task2.Result;
    }

    private async Task<int> ComputeAsync(int[] arr)
    {
        await Task.Delay(1);
        int sum = 0;
        for (int i = 0; i < arr.Length; i++) sum += arr[i];
        return sum;
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessConcurrentAsync");

        _output.WriteLine($"Concurrent async: {result?.ToBigONotation()}");

        // Concurrent async: max(O(n), O(n)) = O(n) span
        Assert.NotNull(result);
    }

    [Fact(Skip = "TDD: Parallel pattern analysis not yet implemented")]
    public async Task DataflowBlock_AnalyzesCorrectly()
    {
        const string code = @"
using System.Threading.Tasks.Dataflow;

public class DataflowOps
{
    public async Task ProcessWithDataflow(int[] arr)
    {
        var block = new ActionBlock<int>(
            x => DoWork(x),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 }
        );

        foreach (var item in arr)
            block.Post(item);

        block.Complete();
        await block.Completion;
    }

    private void DoWork(int x) { }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessWithDataflow");

        _output.WriteLine($"Dataflow: {result?.ToBigONotation()}");

        Assert.NotNull(result);
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
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Parallel).Assembly.Location),
        };

        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimePath != null)
        {
            var systemRuntime = Path.Combine(runtimePath, "System.Runtime.dll");
            if (File.Exists(systemRuntime))
                references.Add(MetadataReference.CreateFromFile(systemRuntime));

            var threading = Path.Combine(runtimePath, "System.Threading.dll");
            if (File.Exists(threading))
                references.Add(MetadataReference.CreateFromFile(threading));

            var threadingTasks = Path.Combine(runtimePath, "System.Threading.Tasks.Parallel.dll");
            if (File.Exists(threadingTasks))
                references.Add(MetadataReference.CreateFromFile(threadingTasks));
        }

        return CSharpCompilation.Create("TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(code) },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static bool HasParallelAnnotation(ComplexityExpression? expr)
    {
        // TODO: Check if expression has parallel metadata
        return false;
    }

    #endregion
}

/// <summary>
/// Placeholder for parallel complexity type.
/// Would represent work/span model or similar.
/// </summary>
public class ParallelComplexity : ComplexityExpression
{
    public ComplexityExpression Work { get; }
    public ComplexityExpression Span { get; }

    public ParallelComplexity(ComplexityExpression work, ComplexityExpression span)
    {
        Work = work;
        Span = span;
    }

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
        => Span.Evaluate(assignments);

    public override string ToBigONotation()
        => $"Work: {Work.ToBigONotation()}, Span: {Span.ToBigONotation()}";

    public override T Accept<T>(IComplexityVisitor<T> visitor)
        => throw new NotImplementedException();
}
