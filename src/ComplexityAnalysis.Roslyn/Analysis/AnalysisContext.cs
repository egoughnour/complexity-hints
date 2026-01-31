using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Context for complexity analysis, providing access to semantic model and scope information.
/// </summary>
public sealed record AnalysisContext
{
    /// <summary>
    /// The semantic model for the current syntax tree.
    /// </summary>
    public required SemanticModel SemanticModel { get; init; }

    /// <summary>
    /// The current method being analyzed (if any).
    /// </summary>
    public IMethodSymbol? CurrentMethod { get; init; }

    /// <summary>
    /// Variables in scope with their complexity interpretations.
    /// </summary>
    public ImmutableDictionary<ISymbol, Variable> VariableMap { get; init; }
        = ImmutableDictionary<ISymbol, Variable>.Empty.WithComparers(SymbolEqualityComparer.Default);

    /// <summary>
    /// Known loop variables and their bounds.
    /// </summary>
    public ImmutableDictionary<ISymbol, LoopBound> LoopBounds { get; init; }
        = ImmutableDictionary<ISymbol, LoopBound>.Empty.WithComparers(SymbolEqualityComparer.Default);

    /// <summary>
    /// Call graph for inter-procedural analysis.
    /// </summary>
    public CallGraph? CallGraph { get; init; }

    /// <summary>
    /// Whether to analyze recursion.
    /// </summary>
    public bool AnalyzeRecursion { get; init; } = true;

    /// <summary>
    /// Maximum recursion depth for inter-procedural analysis.
    /// </summary>
    public int MaxCallDepth { get; init; } = 10;

    /// <summary>
    /// Counter for generating canonical variable names (n, m, k, ...).
    /// </summary>
    private int CanonicalVarCounter { get; init; } = 0;

    /// <summary>
    /// Canonical variable name sequence for clean Big-O notation.
    /// </summary>
    private static readonly string[] CanonicalNames = ["n", "m", "k", "p", "q"];

    /// <summary>
    /// Gets the next canonical variable name.
    /// </summary>
    private string GetNextCanonicalName() =>
        CanonicalVarCounter < CanonicalNames.Length
            ? CanonicalNames[CanonicalVarCounter]
            : $"x{CanonicalVarCounter - CanonicalNames.Length + 1}";

    /// <summary>
    /// Creates a child context for a nested scope.
    /// </summary>
    public AnalysisContext WithMethod(IMethodSymbol method) =>
        this with { CurrentMethod = method, CanonicalVarCounter = 0 };

    /// <summary>
    /// Adds a variable to the context.
    /// </summary>
    public AnalysisContext WithVariable(ISymbol symbol, Variable variable) =>
        this with { VariableMap = VariableMap.SetItem(symbol, variable) };

    /// <summary>
    /// Adds a loop bound to the context.
    /// </summary>
    public AnalysisContext WithLoopBound(ISymbol variable, LoopBound bound) =>
        this with { LoopBounds = LoopBounds.SetItem(variable, bound) };

    /// <summary>
    /// Gets the complexity variable for a symbol, if known.
    /// </summary>
    public Variable? GetVariable(ISymbol symbol) =>
        VariableMap.TryGetValue(symbol, out var variable) ? variable : null;

    /// <summary>
    /// Gets the loop bound for a variable, if known.
    /// </summary>
    public LoopBound? GetLoopBound(ISymbol variable) =>
        LoopBounds.TryGetValue(variable, out var bound) ? bound : null;

    /// <summary>
    /// Infers the complexity variable for a parameter.
    /// Uses canonical variable names (n, m, etc.) for cleaner Big-O notation.
    /// Returns a tuple of (Variable, UpdatedContext) to track name allocation.
    /// </summary>
    public (Variable Variable, AnalysisContext UpdatedContext) InferParameterVariableWithContext(IParameterSymbol parameter)
    {
        var type = parameter.Type;
        var canonicalName = GetNextCanonicalName();
        var updatedContext = this with { CanonicalVarCounter = CanonicalVarCounter + 1 };

        // Use canonical names for complexity variables.
        // The first input size parameter gets "n", secondary gets "m", etc.
        // This produces cleaner Big-O notation: O(n²) instead of O(array · array)

        // Check for collection types
        if (IsCollectionType(type))
        {
            return (new Variable(canonicalName, VariableType.DataCount)
            {
                Description = $"Size of {parameter.Name}"
            }, updatedContext);
        }

        // Check for string type
        if (type.SpecialType == SpecialType.System_String)
        {
            return (new Variable(canonicalName, VariableType.StringLength)
            {
                Description = $"Length of {parameter.Name}"
            }, updatedContext);
        }

        // Check for array type
        if (type is IArrayTypeSymbol)
        {
            return (new Variable(canonicalName, VariableType.DataCount)
            {
                Description = $"Length of {parameter.Name}"
            }, updatedContext);
        }

        // Default to generic input size
        return (new Variable(canonicalName, VariableType.InputSize)
        {
            Description = $"Size of {parameter.Name}"
        }, updatedContext);
    }

    /// <summary>
    /// Infers the complexity variable for a parameter.
    /// Uses canonical variable names (n, m, etc.) for cleaner Big-O notation.
    /// Note: This method doesn't track which names have been used; prefer InferParameterVariableWithContext.
    /// </summary>
    public Variable InferParameterVariable(IParameterSymbol parameter)
    {
        var (variable, _) = InferParameterVariableWithContext(parameter);
        return variable;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            var ns = namedType.ContainingNamespace?.ToDisplayString() ?? "";
            var name = namedType.Name;

            return ns.StartsWith("System.Collections") ||
                   name is "List" or "Dictionary" or "HashSet" or "Queue"
                       or "Stack" or "LinkedList" or "SortedSet" or "SortedDictionary"
                       or "IEnumerable" or "ICollection" or "IList" or "ISet";
        }

        return false;
    }
}

/// <summary>
/// Represents a loop iteration bound.
/// </summary>
public record LoopBound
{
    /// <summary>
    /// The lower bound expression.
    /// </summary>
    public required ComplexityExpression LowerBound { get; init; }

    /// <summary>
    /// The upper bound expression.
    /// </summary>
    public required ComplexityExpression UpperBound { get; init; }

    /// <summary>
    /// The step (increment/decrement) per iteration.
    /// </summary>
    public ComplexityExpression Step { get; init; } = ConstantComplexity.One;

    /// <summary>
    /// Whether the bound is exact or an estimate.
    /// </summary>
    public bool IsExact { get; init; } = true;

    /// <summary>
    /// The type of iteration pattern.
    /// </summary>
    public IterationPattern Pattern { get; init; } = IterationPattern.Linear;

    /// <summary>
    /// Computes the number of iterations.
    /// </summary>
    public ComplexityExpression IterationCount =>
        Pattern switch
        {
            IterationPattern.Linear => new BinaryOperationComplexity(
                new BinaryOperationComplexity(UpperBound, BinaryOp.Plus,
                    new BinaryOperationComplexity(LowerBound, BinaryOp.Multiply, new ConstantComplexity(-1))),
                BinaryOp.Multiply,
                new PowerComplexity(Step, -1)),

            IterationPattern.Logarithmic => new LogOfComplexity(UpperBound),

            IterationPattern.Quadratic => new BinaryOperationComplexity(
                new PolynomialComplexity(
                    System.Collections.Immutable.ImmutableDictionary<int, double>.Empty.Add(2, 0.5),
                    ((VariableComplexity)UpperBound).Var),
                BinaryOp.Plus,
                new BinaryOperationComplexity(UpperBound, BinaryOp.Multiply, new ConstantComplexity(0.5))),

            _ => UpperBound // Conservative: assume upper bound iterations
        };

    /// <summary>
    /// Creates a simple 0 to n bound.
    /// </summary>
    public static LoopBound ZeroToN(Variable n) => new()
    {
        LowerBound = ConstantComplexity.Zero,
        UpperBound = new VariableComplexity(n),
        Step = ConstantComplexity.One,
        Pattern = IterationPattern.Linear
    };

    /// <summary>
    /// Creates a logarithmic bound (i *= 2 or i /= 2).
    /// </summary>
    public static LoopBound Logarithmic(Variable n) => new()
    {
        LowerBound = ConstantComplexity.One,
        UpperBound = new VariableComplexity(n),
        Step = new ConstantComplexity(2),
        Pattern = IterationPattern.Logarithmic
    };
}

/// <summary>
/// Types of iteration patterns.
/// </summary>
public enum IterationPattern
{
    /// <summary>
    /// Linear iteration: i++, i--, i += k.
    /// </summary>
    Linear,

    /// <summary>
    /// Logarithmic iteration: i *= k, i /= k.
    /// </summary>
    Logarithmic,

    /// <summary>
    /// Quadratic iteration: dependent on another loop.
    /// </summary>
    Quadratic,

    /// <summary>
    /// Unknown pattern.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents a call graph for inter-procedural analysis.
/// </summary>
public class CallGraph
{
    private readonly Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> _callees = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> _callers = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<IMethodSymbol, ComplexityExpression?> _complexities = new(SymbolEqualityComparer.Default);
    private readonly HashSet<IMethodSymbol> _allMethods = new(SymbolEqualityComparer.Default);

    /// <summary>
    /// Registers a method in the call graph (even if it has no calls).
    /// </summary>
    public void AddMethod(IMethodSymbol method)
    {
        _allMethods.Add(method);
    }

    /// <summary>
    /// Adds a call edge from caller to callee.
    /// </summary>
    public void AddCall(IMethodSymbol caller, IMethodSymbol callee)
    {
        _allMethods.Add(caller);
        _allMethods.Add(callee);

        if (!_callees.ContainsKey(caller))
            _callees[caller] = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        _callees[caller].Add(callee);

        if (!_callers.ContainsKey(callee))
            _callers[callee] = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        _callers[callee].Add(caller);
    }

    /// <summary>
    /// Gets all methods called by the given method.
    /// </summary>
    public IReadOnlySet<IMethodSymbol> GetCallees(IMethodSymbol method) =>
        _callees.TryGetValue(method, out var callees) ? callees : new HashSet<IMethodSymbol>();

    /// <summary>
    /// Gets all methods that call the given method.
    /// </summary>
    public IReadOnlySet<IMethodSymbol> GetCallers(IMethodSymbol method) =>
        _callers.TryGetValue(method, out var callers) ? callers : new HashSet<IMethodSymbol>();

    /// <summary>
    /// Checks if the method is recursive (directly or indirectly).
    /// </summary>
    public bool IsRecursive(IMethodSymbol method) => IsReachable(method, method, new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default));

    /// <summary>
    /// Checks if there's a path from source to target.
    /// </summary>
    public bool IsReachable(IMethodSymbol source, IMethodSymbol target, HashSet<IMethodSymbol> visited)
    {
        if (!visited.Add(source)) return false;

        foreach (var callee in GetCallees(source))
        {
            if (SymbolEqualityComparer.Default.Equals(callee, target))
                return true;
            if (IsReachable(callee, target, visited))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the computed complexity for a method.
    /// </summary>
    public void SetComplexity(IMethodSymbol method, ComplexityExpression complexity) =>
        _complexities[method] = complexity;

    /// <summary>
    /// Gets the computed complexity for a method, if available.
    /// </summary>
    public ComplexityExpression? GetComplexity(IMethodSymbol method) =>
        _complexities.TryGetValue(method, out var complexity) ? complexity : null;

    /// <summary>
    /// Gets all methods in the call graph.
    /// </summary>
    public IEnumerable<IMethodSymbol> AllMethods => _allMethods;

    /// <summary>
    /// Gets methods in topological order (callees before callers).
    /// Returns null if there's a cycle.
    /// </summary>
    public IReadOnlyList<IMethodSymbol>? TopologicalSort()
    {
        var result = new List<IMethodSymbol>();
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var inProgress = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        foreach (var method in AllMethods)
        {
            if (!Visit(method, visited, inProgress, result))
                return null; // Cycle detected
        }

        // Don't reverse - we want callees before callers (post-order gives callees first)
        return result;
    }

    private bool Visit(IMethodSymbol method, HashSet<IMethodSymbol> visited,
        HashSet<IMethodSymbol> inProgress, List<IMethodSymbol> result)
    {
        if (inProgress.Contains(method)) return false; // Cycle
        if (visited.Contains(method)) return true;

        inProgress.Add(method);

        foreach (var callee in GetCallees(method))
        {
            if (!Visit(callee, visited, inProgress, result))
                return false;
        }

        inProgress.Remove(method);
        visited.Add(method);
        result.Add(method);

        return true;
    }

    /// <summary>
    /// Finds all cycles (strongly connected components) in the call graph.
    /// TODO: Implement using Tarjan's or Kosaraju's algorithm.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<IMethodSymbol>> FindCycles()
    {
        // Stub for TDD tests - not yet implemented
        throw new NotImplementedException("Cycle detection not yet implemented");
    }
}
