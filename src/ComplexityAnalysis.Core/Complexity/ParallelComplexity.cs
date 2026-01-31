using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents complexity of parallel/concurrent algorithms.
/// 
/// Parallel complexity considers:
/// - Work: Total operations across all processors (sequential equivalent)
/// - Span/Depth: Longest chain of dependent operations (critical path)
/// - Parallelism: Work / Span ratio (how parallelizable the algorithm is)
/// 
/// Examples:
/// - Parallel.For over n items: Work O(n), Span O(1) if independent
/// - Parallel merge sort: Work O(n log n), Span O(log² n)
/// - Parallel prefix sum: Work O(n), Span O(log n)
/// </summary>
public sealed record ParallelComplexity : ComplexityExpression
{
    /// <summary>
    /// Total work across all processors (sequential time complexity).
    /// </summary>
    public required ComplexityExpression Work { get; init; }

    /// <summary>
    /// Span/depth - the longest chain of dependent operations.
    /// Also known as critical path length.
    /// </summary>
    public required ComplexityExpression Span { get; init; }

    /// <summary>
    /// Number of processors/cores assumed.
    /// Use Variable.P for parameterized, or a constant for fixed.
    /// </summary>
    public ComplexityExpression ProcessorCount { get; init; } = new VariableComplexity(Variable.P);

    /// <summary>
    /// The type of parallel pattern detected.
    /// </summary>
    public ParallelPatternType PatternType { get; init; } = ParallelPatternType.Generic;

    /// <summary>
    /// Whether the parallelism is task-based (async/await, Task.Run).
    /// </summary>
    public bool IsTaskBased { get; init; }

    /// <summary>
    /// Whether the parallel operations have synchronization overhead.
    /// </summary>
    public bool HasSynchronizationOverhead { get; init; }

    /// <summary>
    /// Description of the parallel pattern.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the parallelism (Work / Span ratio).
    /// Higher values indicate better parallelizability.
    /// </summary>
    public ComplexityExpression Parallelism =>
        new BinaryOperationComplexity(Work, BinaryOp.Multiply,
            new PowerComplexity(Span, -1)); // Work / Span

    /// <summary>
    /// Gets the parallel time (with p processors): max(Work/p, Span).
    /// By Brent's theorem: T_p ≤ (Work - Span)/p + Span
    /// </summary>
    public ComplexityExpression ParallelTime =>
        new BinaryOperationComplexity(
            new BinaryOperationComplexity(Work, BinaryOp.Multiply,
                new PowerComplexity(ProcessorCount, -1)), // Work/p
            BinaryOp.Max,
            Span);

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is IParallelComplexityVisitor<T> parallelVisitor
            ? parallelVisitor.VisitParallel(this)
            : Work.Accept(visitor); // Fall back to work complexity

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        this with
        {
            Work = Work.Substitute(variable, replacement),
            Span = Span.Substitute(variable, replacement),
            ProcessorCount = ProcessorCount.Substitute(variable, replacement)
        };

    public override ImmutableHashSet<Variable> FreeVariables =>
        Work.FreeVariables
            .Union(Span.FreeVariables)
            .Union(ProcessorCount.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var work = Work.Evaluate(assignments);
        var span = Span.Evaluate(assignments);
        var p = ProcessorCount.Evaluate(assignments) ?? 1;

        if (work == null || span == null) return null;

        // Parallel time: max(Work/p, Span)
        return Math.Max(work.Value / p, span.Value);
    }

    public override string ToBigONotation()
    {
        var workStr = Work.ToBigONotation();
        var spanStr = Span.ToBigONotation();
        return $"Work: {workStr}, Span: {spanStr}";
    }

    /// <summary>
    /// Creates a parallel complexity for embarrassingly parallel work.
    /// Work = O(n), Span = O(1).
    /// </summary>
    public static ParallelComplexity EmbarrassinglyParallel(Variable var) =>
        new()
        {
            Work = new VariableComplexity(var),
            Span = ConstantComplexity.One,
            PatternType = ParallelPatternType.ParallelFor,
            Description = "Embarrassingly parallel - independent iterations"
        };

    /// <summary>
    /// Creates parallel complexity for reduction/aggregation patterns.
    /// Work = O(n), Span = O(log n).
    /// </summary>
    public static ParallelComplexity Reduction(Variable var) =>
        new()
        {
            Work = new VariableComplexity(var),
            Span = new LogarithmicComplexity(1.0, var),
            PatternType = ParallelPatternType.Reduction,
            Description = "Parallel reduction - tree-structured aggregation"
        };

    /// <summary>
    /// Creates parallel complexity for divide-and-conquer patterns.
    /// Work = O(n log n), Span = O(log² n).
    /// </summary>
    public static ParallelComplexity DivideAndConquer(Variable var) =>
        new()
        {
            Work = PolyLogComplexity.NLogN(var),
            Span = new BinaryOperationComplexity(
                new LogarithmicComplexity(1.0, var),
                BinaryOp.Multiply,
                new LogarithmicComplexity(1.0, var)),
            PatternType = ParallelPatternType.DivideAndConquer,
            Description = "Parallel divide-and-conquer (e.g., merge sort)"
        };

    /// <summary>
    /// Creates parallel complexity for prefix/scan operations.
    /// Work = O(n), Span = O(log n).
    /// </summary>
    public static ParallelComplexity PrefixScan(Variable var) =>
        new()
        {
            Work = new VariableComplexity(var),
            Span = new LogarithmicComplexity(1.0, var),
            PatternType = ParallelPatternType.Scan,
            Description = "Parallel prefix scan"
        };

    /// <summary>
    /// Creates parallel complexity for pipeline patterns.
    /// Work = O(n × stages), Span = O(n + stages).
    /// </summary>
    public static ParallelComplexity Pipeline(Variable itemCount, int stages) =>
        new()
        {
            Work = new BinaryOperationComplexity(
                new VariableComplexity(itemCount),
                BinaryOp.Multiply,
                new ConstantComplexity(stages)),
            Span = new BinaryOperationComplexity(
                new VariableComplexity(itemCount),
                BinaryOp.Plus,
                new ConstantComplexity(stages)),
            PatternType = ParallelPatternType.Pipeline,
            Description = $"Pipeline with {stages} stages"
        };

    /// <summary>
    /// Creates complexity for async/await task-based concurrency.
    /// </summary>
    public static ParallelComplexity TaskBased(
        ComplexityExpression work,
        ComplexityExpression span,
        string? description = null) =>
        new()
        {
            Work = work,
            Span = span,
            IsTaskBased = true,
            PatternType = ParallelPatternType.TaskBased,
            Description = description ?? "Task-based async/await concurrency"
        };
}

/// <summary>
/// Types of parallel patterns.
/// </summary>
public enum ParallelPatternType
{
    /// <summary>
    /// Generic parallel pattern.
    /// </summary>
    Generic,

    /// <summary>
    /// Parallel.For / Parallel.ForEach - data parallelism.
    /// </summary>
    ParallelFor,

    /// <summary>
    /// PLINQ - parallel LINQ.
    /// </summary>
    PLINQ,

    /// <summary>
    /// Task.Run / Task.WhenAll - task parallelism.
    /// </summary>
    TaskBased,

    /// <summary>
    /// async/await patterns.
    /// </summary>
    AsyncAwait,

    /// <summary>
    /// Parallel reduction/aggregation.
    /// </summary>
    Reduction,

    /// <summary>
    /// Parallel prefix scan.
    /// </summary>
    Scan,

    /// <summary>
    /// Divide-and-conquer parallelism.
    /// </summary>
    DivideAndConquer,

    /// <summary>
    /// Pipeline parallelism.
    /// </summary>
    Pipeline,

    /// <summary>
    /// Fork-join pattern.
    /// </summary>
    ForkJoin,

    /// <summary>
    /// Producer-consumer pattern.
    /// </summary>
    ProducerConsumer
}

/// <summary>
/// Variable for processor count.
/// </summary>
public static class ParallelVariables
{
    /// <summary>
    /// Number of processors (p).
    /// </summary>
    public static Variable P => Variable.P;

    /// <summary>
    /// Creates a processor count variable with a specific value.
    /// </summary>
    public static ComplexityExpression Processors(int count) =>
        new ConstantComplexity(count);

    /// <summary>
    /// Infinite processors (theoretical analysis).
    /// </summary>
    public static ComplexityExpression InfiniteProcessors =>
        new VariableComplexity(new Variable("∞", VariableType.Custom));
}

/// <summary>
/// Extended visitor interface for parallel complexity.
/// </summary>
public interface IParallelComplexityVisitor<T> : IComplexityVisitor<T>
{
    T VisitParallel(ParallelComplexity parallel);
}

/// <summary>
/// Analysis result for parallel patterns.
/// </summary>
public sealed record ParallelAnalysisResult
{
    /// <summary>
    /// The detected parallel complexity.
    /// </summary>
    public required ParallelComplexity Complexity { get; init; }

    /// <summary>
    /// Speedup factor: T_1 / T_p (sequential time / parallel time).
    /// </summary>
    public double? Speedup { get; init; }

    /// <summary>
    /// Efficiency: Speedup / p (how well processors are utilized).
    /// </summary>
    public double? Efficiency { get; init; }

    /// <summary>
    /// Whether the pattern has good scalability.
    /// </summary>
    public bool IsScalable { get; init; }

    /// <summary>
    /// Potential issues or warnings.
    /// </summary>
    public ImmutableList<string> Warnings { get; init; } = ImmutableList<string>.Empty;

    /// <summary>
    /// Recommendations for improving parallelism.
    /// </summary>
    public ImmutableList<string> Recommendations { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Common parallel algorithm complexities.
/// </summary>
public static class ParallelAlgorithms
{
    /// <summary>
    /// Parallel sum/reduction: Work O(n), Span O(log n).
    /// </summary>
    public static ParallelComplexity ParallelSum() =>
        ParallelComplexity.Reduction(Variable.N);

    /// <summary>
    /// Parallel merge sort: Work O(n log n), Span O(log² n).
    /// </summary>
    public static ParallelComplexity ParallelMergeSort() =>
        ParallelComplexity.DivideAndConquer(Variable.N);

    /// <summary>
    /// Parallel matrix multiply (naive): Work O(n³), Span O(log n).
    /// </summary>
    public static ParallelComplexity ParallelMatrixMultiply() =>
        new()
        {
            Work = PolynomialComplexity.OfDegree(3, Variable.N),
            Span = new LogarithmicComplexity(1.0, Variable.N),
            PatternType = ParallelPatternType.ParallelFor,
            Description = "Parallel matrix multiplication"
        };

    /// <summary>
    /// Parallel quick sort: Work O(n log n), Span O(log² n) expected.
    /// </summary>
    public static ParallelComplexity ParallelQuickSort() =>
        new()
        {
            Work = PolyLogComplexity.NLogN(Variable.N),
            Span = new BinaryOperationComplexity(
                new LogarithmicComplexity(1.0, Variable.N),
                BinaryOp.Multiply,
                new LogarithmicComplexity(1.0, Variable.N)),
            PatternType = ParallelPatternType.DivideAndConquer,
            Description = "Parallel quicksort"
        };

    /// <summary>
    /// Parallel BFS: Work O(V + E), Span O(diameter × log V).
    /// </summary>
    public static ParallelComplexity ParallelBFS() =>
        new()
        {
            Work = new BinaryOperationComplexity(
                new VariableComplexity(Variable.V),
                BinaryOp.Plus,
                new VariableComplexity(Variable.E)),
            Span = new BinaryOperationComplexity(
                new VariableComplexity(new Variable("d", VariableType.Custom) { Description = "Graph diameter" }),
                BinaryOp.Multiply,
                new LogarithmicComplexity(1.0, Variable.V)),
            PatternType = ParallelPatternType.Generic,
            Description = "Parallel breadth-first search"
        };

    /// <summary>
    /// PLINQ Where/Select: Work O(n), Span O(n/p + log p).
    /// </summary>
    public static ParallelComplexity PLINQFilter() =>
        new()
        {
            Work = new VariableComplexity(Variable.N),
            Span = new BinaryOperationComplexity(
                new BinaryOperationComplexity(
                    new VariableComplexity(Variable.N),
                    BinaryOp.Multiply,
                    new PowerComplexity(new VariableComplexity(Variable.P), -1)),
                BinaryOp.Plus,
                new LogarithmicComplexity(1.0, Variable.P)),
            PatternType = ParallelPatternType.PLINQ,
            Description = "PLINQ filter/projection"
        };
}
