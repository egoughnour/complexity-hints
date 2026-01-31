using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Memory;

/// <summary>
/// Represents space/memory complexity analysis result.
/// 
/// Space complexity measures memory usage as a function of input size.
/// Components:
/// - Stack space: Recursion depth, local variables
/// - Heap space: Allocated objects, collections
/// - Auxiliary space: Extra space beyond input
/// </summary>
public sealed record MemoryComplexity : ComplexityExpression
{
    /// <summary>
    /// Total space complexity (dominant term).
    /// </summary>
    public required ComplexityExpression TotalSpace { get; init; }

    /// <summary>
    /// Stack space complexity (recursion depth).
    /// </summary>
    public ComplexityExpression StackSpace { get; init; } = ConstantComplexity.One;

    /// <summary>
    /// Heap space complexity (allocated objects).
    /// </summary>
    public ComplexityExpression HeapSpace { get; init; } = ConstantComplexity.Zero;

    /// <summary>
    /// Auxiliary space (extra space beyond input).
    /// </summary>
    public ComplexityExpression AuxiliarySpace { get; init; } = ConstantComplexity.Zero;

    /// <summary>
    /// Whether the algorithm is in-place (O(1) auxiliary space).
    /// </summary>
    public bool IsInPlace { get; init; }

    /// <summary>
    /// Whether tail-call optimization can reduce stack space.
    /// </summary>
    public bool IsTailRecursive { get; init; }

    /// <summary>
    /// Description of memory usage pattern.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Breakdown of memory allocations by source.
    /// </summary>
    public ImmutableList<AllocationInfo> Allocations { get; init; } = ImmutableList<AllocationInfo>.Empty;

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is IMemoryComplexityVisitor<T> memoryVisitor
            ? memoryVisitor.VisitMemory(this)
            : TotalSpace.Accept(visitor);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        this with
        {
            TotalSpace = TotalSpace.Substitute(variable, replacement),
            StackSpace = StackSpace.Substitute(variable, replacement),
            HeapSpace = HeapSpace.Substitute(variable, replacement),
            AuxiliarySpace = AuxiliarySpace.Substitute(variable, replacement)
        };

    public override ImmutableHashSet<Variable> FreeVariables =>
        TotalSpace.FreeVariables
            .Union(StackSpace.FreeVariables)
            .Union(HeapSpace.FreeVariables)
            .Union(AuxiliarySpace.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        TotalSpace.Evaluate(assignments);

    public override string ToBigONotation()
    {
        var total = TotalSpace.ToBigONotation();
        var parts = new List<string> { $"Space: {total}" };

        if (IsInPlace)
            parts.Add("in-place");
        if (IsTailRecursive)
            parts.Add("tail-recursive");

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Creates O(1) constant space complexity (in-place).
    /// </summary>
    public static MemoryComplexity Constant() =>
        new()
        {
            TotalSpace = ConstantComplexity.One,
            StackSpace = ConstantComplexity.One,
            HeapSpace = ConstantComplexity.Zero,
            AuxiliarySpace = ConstantComplexity.Zero,
            IsInPlace = true,
            Description = "Constant space - in-place algorithm"
        };

    /// <summary>
    /// Creates O(n) linear space complexity.
    /// </summary>
    public static MemoryComplexity Linear(Variable var, MemorySource source = MemorySource.Heap) =>
        new()
        {
            TotalSpace = new LinearComplexity(1.0, var),
            StackSpace = source == MemorySource.Stack
                ? new LinearComplexity(1.0, var)
                : ConstantComplexity.One,
            HeapSpace = source == MemorySource.Heap
                ? new LinearComplexity(1.0, var)
                : ConstantComplexity.Zero,
            AuxiliarySpace = new LinearComplexity(1.0, var),
            IsInPlace = false,
            Description = source == MemorySource.Stack
                ? "Linear stack space - recursion depth O(n)"
                : "Linear heap space - O(n) allocations"
        };

    /// <summary>
    /// Creates O(log n) logarithmic space complexity (typical for recursion).
    /// </summary>
    public static MemoryComplexity Logarithmic(Variable var) =>
        new()
        {
            TotalSpace = new LogarithmicComplexity(1.0, var),
            StackSpace = new LogarithmicComplexity(1.0, var),
            HeapSpace = ConstantComplexity.Zero,
            AuxiliarySpace = new LogarithmicComplexity(1.0, var),
            IsInPlace = false,
            IsTailRecursive = false,
            Description = "Logarithmic stack space - balanced recursion"
        };

    /// <summary>
    /// Creates O(n²) quadratic space complexity.
    /// </summary>
    public static MemoryComplexity Quadratic(Variable var) =>
        new()
        {
            TotalSpace = PolynomialComplexity.OfDegree(2, var),
            HeapSpace = PolynomialComplexity.OfDegree(2, var),
            AuxiliarySpace = PolynomialComplexity.OfDegree(2, var),
            IsInPlace = false,
            Description = "Quadratic space - O(n²) allocations"
        };

    /// <summary>
    /// Creates memory complexity from recursion pattern.
    /// </summary>
    public static MemoryComplexity FromRecursion(
        int recursionDepth,
        ComplexityExpression depthComplexity,
        ComplexityExpression perCallSpace,
        bool isTailRecursive = false)
    {
        ComplexityExpression stackSpace = isTailRecursive
            ? ConstantComplexity.One
            : depthComplexity;

        ComplexityExpression heapPerLevel = perCallSpace is ConstantComplexity c && c.Value == 0
            ? ConstantComplexity.Zero
            : new BinaryOperationComplexity(depthComplexity, BinaryOp.Multiply, perCallSpace);

        ComplexityExpression totalSpace = isTailRecursive 
            ? perCallSpace 
            : new BinaryOperationComplexity(stackSpace, BinaryOp.Plus, heapPerLevel);

        return new()
        {
            TotalSpace = totalSpace,
            StackSpace = stackSpace,
            HeapSpace = heapPerLevel,
            AuxiliarySpace = new BinaryOperationComplexity(stackSpace, BinaryOp.Plus, heapPerLevel),
            IsTailRecursive = isTailRecursive,
            Description = isTailRecursive
                ? "Tail-recursive - constant stack space"
                : $"Recursive with depth {depthComplexity.ToBigONotation()}"
        };
    }
}

/// <summary>
/// Where memory is allocated.
/// </summary>
public enum MemorySource
{
    /// <summary>
    /// Stack allocation (local variables, recursion frames).
    /// </summary>
    Stack,

    /// <summary>
    /// Heap allocation (new objects, collections).
    /// </summary>
    Heap,

    /// <summary>
    /// Both stack and heap.
    /// </summary>
    Both
}

/// <summary>
/// Information about a specific memory allocation.
/// </summary>
public sealed record AllocationInfo
{
    /// <summary>
    /// Description of what is being allocated.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The size complexity of this allocation.
    /// </summary>
    public required ComplexityExpression Size { get; init; }

    /// <summary>
    /// Where the memory is allocated.
    /// </summary>
    public MemorySource Source { get; init; } = MemorySource.Heap;

    /// <summary>
    /// The type being allocated (if known).
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>
    /// How many times this allocation occurs.
    /// </summary>
    public ComplexityExpression Count { get; init; } = ConstantComplexity.One;

    /// <summary>
    /// Total memory from this allocation.
    /// </summary>
    public ComplexityExpression TotalSize =>
        Count is ConstantComplexity { Value: 1 }
            ? Size
            : new BinaryOperationComplexity(Count, BinaryOp.Multiply, Size);

    public static AllocationInfo Array(Variable var, string elementType = "T") =>
        new()
        {
            Description = $"Array allocation",
            Size = new VariableComplexity(var),
            TypeName = $"{elementType}[]",
            Source = MemorySource.Heap
        };

    public static AllocationInfo Collection(Variable var, string collectionType = "List") =>
        new()
        {
            Description = $"{collectionType} allocation",
            Size = new VariableComplexity(var),
            TypeName = collectionType,
            Source = MemorySource.Heap
        };

    public static AllocationInfo RecursionFrame(ComplexityExpression depth, int frameSize = 1) =>
        new()
        {
            Description = "Recursion stack frames",
            Size = new ConstantComplexity(frameSize),
            Count = depth,
            Source = MemorySource.Stack
        };

    public static AllocationInfo Matrix(Variable rows, Variable cols) =>
        new()
        {
            Description = "2D array/matrix allocation",
            Size = new BinaryOperationComplexity(
                new VariableComplexity(rows),
                BinaryOp.Multiply,
                new VariableComplexity(cols)),
            Source = MemorySource.Heap
        };
}

/// <summary>
/// Combined time and space complexity result.
/// </summary>
public sealed record ComplexityAnalysisResult
{
    /// <summary>
    /// Time complexity of the algorithm.
    /// </summary>
    public required ComplexityExpression TimeComplexity { get; init; }

    /// <summary>
    /// Space/memory complexity of the algorithm.
    /// </summary>
    public required MemoryComplexity SpaceComplexity { get; init; }

    /// <summary>
    /// The method or algorithm name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Whether time-space tradeoff is possible.
    /// </summary>
    public bool HasTimeSpaceTradeoff { get; init; }

    /// <summary>
    /// Notes about the analysis.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Confidence in the analysis (0-1).
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    public override string ToString() =>
        $"Time: {TimeComplexity.ToBigONotation()}, {SpaceComplexity.ToBigONotation()}";

    /// <summary>
    /// Common algorithms with their time/space complexities.
    /// </summary>
    public static class CommonAlgorithms
    {
        public static ComplexityAnalysisResult BinarySearch() => new()
        {
            TimeComplexity = new LogarithmicComplexity(1.0, Variable.N),
            SpaceComplexity = MemoryComplexity.Constant(),
            Name = "Binary Search (iterative)",
            Notes = "O(log n) time, O(1) space"
        };

        public static ComplexityAnalysisResult BinarySearchRecursive() => new()
        {
            TimeComplexity = new LogarithmicComplexity(1.0, Variable.N),
            SpaceComplexity = MemoryComplexity.Logarithmic(Variable.N),
            Name = "Binary Search (recursive)",
            Notes = "O(log n) time, O(log n) stack space"
        };

        public static ComplexityAnalysisResult MergeSort() => new()
        {
            TimeComplexity = PolyLogComplexity.NLogN(Variable.N),
            SpaceComplexity = MemoryComplexity.Linear(Variable.N),
            Name = "Merge Sort",
            Notes = "O(n log n) time, O(n) auxiliary space"
        };

        public static ComplexityAnalysisResult QuickSort() => new()
        {
            TimeComplexity = PolyLogComplexity.NLogN(Variable.N),
            SpaceComplexity = MemoryComplexity.Logarithmic(Variable.N),
            Name = "QuickSort",
            Notes = "O(n log n) average time, O(log n) stack space"
        };

        public static ComplexityAnalysisResult HeapSort() => new()
        {
            TimeComplexity = PolyLogComplexity.NLogN(Variable.N),
            SpaceComplexity = MemoryComplexity.Constant(),
            Name = "HeapSort",
            Notes = "O(n log n) time, O(1) in-place"
        };

        public static ComplexityAnalysisResult DFS() => new()
        {
            TimeComplexity = new BinaryOperationComplexity(
                new VariableComplexity(Variable.V),
                BinaryOp.Plus,
                new VariableComplexity(Variable.E)),
            SpaceComplexity = MemoryComplexity.Linear(Variable.V, MemorySource.Stack),
            Name = "Depth-First Search",
            Notes = "O(V+E) time, O(V) stack space for recursion"
        };

        public static ComplexityAnalysisResult BFS() => new()
        {
            TimeComplexity = new BinaryOperationComplexity(
                new VariableComplexity(Variable.V),
                BinaryOp.Plus,
                new VariableComplexity(Variable.E)),
            SpaceComplexity = MemoryComplexity.Linear(Variable.V, MemorySource.Heap),
            Name = "Breadth-First Search",
            Notes = "O(V+E) time, O(V) queue space"
        };

        public static ComplexityAnalysisResult DynamicProgramming2D() => new()
        {
            TimeComplexity = PolynomialComplexity.OfDegree(2, Variable.N),
            SpaceComplexity = MemoryComplexity.Quadratic(Variable.N),
            Name = "2D Dynamic Programming",
            HasTimeSpaceTradeoff = true,
            Notes = "O(n²) time and space, can often optimize to O(n) space"
        };
    }
}

/// <summary>
/// Extended visitor interface for memory complexity types.
/// </summary>
public interface IMemoryComplexityVisitor<T> : IComplexityVisitor<T>
{
    T VisitMemory(MemoryComplexity memory);
}

/// <summary>
/// Categories of space complexity.
/// </summary>
public enum SpaceComplexityClass
{
    /// <summary>
    /// O(1) - Constant space.
    /// </summary>
    Constant,

    /// <summary>
    /// O(log n) - Logarithmic space.
    /// </summary>
    Logarithmic,

    /// <summary>
    /// O(n) - Linear space.
    /// </summary>
    Linear,

    /// <summary>
    /// O(n log n) - Linearithmic space.
    /// </summary>
    Linearithmic,

    /// <summary>
    /// O(n²) - Quadratic space.
    /// </summary>
    Quadratic,

    /// <summary>
    /// O(n³) - Cubic space.
    /// </summary>
    Cubic,

    /// <summary>
    /// O(2^n) - Exponential space.
    /// </summary>
    Exponential,

    /// <summary>
    /// Unknown space complexity.
    /// </summary>
    Unknown
}

/// <summary>
/// Utility methods for space complexity classification.
/// </summary>
public static class SpaceComplexityClassifier
{
    /// <summary>
    /// Classifies a complexity expression into a space complexity class.
    /// </summary>
    public static SpaceComplexityClass Classify(ComplexityExpression expr)
    {
        return expr switch
        {
            ConstantComplexity => SpaceComplexityClass.Constant,
            LogarithmicComplexity => SpaceComplexityClass.Logarithmic,
            VariableComplexity => SpaceComplexityClass.Linear,
            LinearComplexity => SpaceComplexityClass.Linear,
            PolyLogComplexity pl when pl.PolyDegree == 1 && pl.LogExponent > 0 => SpaceComplexityClass.Linearithmic,
            PolyLogComplexity pl when pl.PolyDegree == 1 && pl.LogExponent == 0 => SpaceComplexityClass.Linear,
            PolynomialComplexity p when p.Degree == 2 => SpaceComplexityClass.Quadratic,
            PolynomialComplexity p when p.Degree == 3 => SpaceComplexityClass.Cubic,
            PolynomialComplexity p when p.Degree == 1 => SpaceComplexityClass.Linear,
            ExponentialComplexity => SpaceComplexityClass.Exponential,
            MemoryComplexity m => Classify(m.TotalSpace),
            BinaryOperationComplexity bin when bin.Operation == BinaryOp.Max =>
                (SpaceComplexityClass)Math.Max((int)Classify(bin.Left), (int)Classify(bin.Right)),
            _ => SpaceComplexityClass.Unknown
        };
    }

    /// <summary>
    /// Determines if one space complexity class is better (lower) than another.
    /// </summary>
    public static bool IsBetterThan(SpaceComplexityClass a, SpaceComplexityClass b) =>
        a != SpaceComplexityClass.Unknown && b != SpaceComplexityClass.Unknown && (int)a < (int)b;

    /// <summary>
    /// Gets a human-readable description of the space complexity class.
    /// </summary>
    public static string GetDescription(SpaceComplexityClass spaceClass) =>
        spaceClass switch
        {
            SpaceComplexityClass.Constant => "O(1) - Constant space, in-place",
            SpaceComplexityClass.Logarithmic => "O(log n) - Logarithmic space, balanced recursion",
            SpaceComplexityClass.Linear => "O(n) - Linear space",
            SpaceComplexityClass.Linearithmic => "O(n log n) - Linearithmic space",
            SpaceComplexityClass.Quadratic => "O(n²) - Quadratic space",
            SpaceComplexityClass.Cubic => "O(n³) - Cubic space",
            SpaceComplexityClass.Exponential => "O(2^n) - Exponential space",
            _ => "Unknown space complexity"
        };
}
