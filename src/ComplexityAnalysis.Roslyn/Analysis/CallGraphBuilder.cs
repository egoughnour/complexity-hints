using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Progress;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Builds a call graph from Roslyn compilation for inter-procedural analysis.
/// </summary>
public sealed class CallGraphBuilder
{
    private readonly Compilation _compilation;
    private readonly IAnalysisProgress? _progress;
    private readonly CallGraph _callGraph = new();

    public CallGraphBuilder(Compilation compilation, IAnalysisProgress? progress = null)
    {
        _compilation = compilation;
        _progress = progress;
    }

    /// <summary>
    /// Builds the complete call graph from the compilation.
    /// </summary>
    public CallGraph Build()
    {
        var syntaxTrees = _compilation.SyntaxTrees.ToList();
        var totalTrees = syntaxTrees.Count;
        var processedTrees = 0;

        foreach (var tree in syntaxTrees)
        {
            var semanticModel = _compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var walker = new CallGraphWalker(semanticModel, _callGraph);
            walker.Visit(root);

            processedTrees++;
            _progress?.OnProgressUpdated(
                (double)processedTrees / totalTrees * 100,
                $"Building call graph: {processedTrees}/{totalTrees} files");
        }

        return _callGraph;
    }

    /// <summary>
    /// Builds a call graph for a single method and its transitive callees.
    /// </summary>
    public CallGraph BuildForMethod(IMethodSymbol method)
    {
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        BuildTransitiveGraph(method, visited);
        return _callGraph;
    }

    private void BuildTransitiveGraph(IMethodSymbol method, HashSet<IMethodSymbol> visited)
    {
        if (!visited.Add(method))
            return;

        // Find the syntax reference for this method
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is MethodDeclarationSyntax methodDecl)
            {
                var tree = syntax.SyntaxTree;
                var semanticModel = _compilation.GetSemanticModel(tree);

                var walker = new MethodCallWalker(semanticModel, method);
                walker.Visit(methodDecl);

                foreach (var callee in walker.CalledMethods)
                {
                    _callGraph.AddCall(method, callee);
                    BuildTransitiveGraph(callee, visited);
                }
            }
        }
    }

    /// <summary>
    /// Detects strongly connected components (SCCs) for handling mutual recursion.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<IMethodSymbol>> FindStronglyConnectedComponents()
    {
        var methods = _callGraph.AllMethods.ToList();
        var index = 0;
        var stack = new Stack<IMethodSymbol>();
        var indices = new Dictionary<IMethodSymbol, int>(SymbolEqualityComparer.Default);
        var lowLinks = new Dictionary<IMethodSymbol, int>(SymbolEqualityComparer.Default);
        var onStack = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var sccs = new List<IReadOnlyList<IMethodSymbol>>();

        foreach (var method in methods)
        {
            if (!indices.ContainsKey(method))
            {
                Tarjan(method, ref index, stack, indices, lowLinks, onStack, sccs);
            }
        }

        return sccs;
    }

    private void Tarjan(
        IMethodSymbol method,
        ref int index,
        Stack<IMethodSymbol> stack,
        Dictionary<IMethodSymbol, int> indices,
        Dictionary<IMethodSymbol, int> lowLinks,
        HashSet<IMethodSymbol> onStack,
        List<IReadOnlyList<IMethodSymbol>> sccs)
    {
        indices[method] = index;
        lowLinks[method] = index;
        index++;
        stack.Push(method);
        onStack.Add(method);

        foreach (var callee in _callGraph.GetCallees(method))
        {
            if (!indices.ContainsKey(callee))
            {
                Tarjan(callee, ref index, stack, indices, lowLinks, onStack, sccs);
                lowLinks[method] = Math.Min(lowLinks[method], lowLinks[callee]);
            }
            else if (onStack.Contains(callee))
            {
                lowLinks[method] = Math.Min(lowLinks[method], indices[callee]);
            }
        }

        if (lowLinks[method] == indices[method])
        {
            var scc = new List<IMethodSymbol>();
            IMethodSymbol? w;
            do
            {
                w = stack.Pop();
                onStack.Remove(w);
                scc.Add(w);
            } while (!SymbolEqualityComparer.Default.Equals(w, method));

            sccs.Add(scc);
        }
    }

    /// <summary>
    /// Walker that builds the complete call graph.
    /// </summary>
    private class CallGraphWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly CallGraph _callGraph;
        private IMethodSymbol? _currentMethod;

        public CallGraphWalker(SemanticModel semanticModel, CallGraph callGraph)
        {
            _semanticModel = semanticModel;
            _callGraph = callGraph;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var previousMethod = _currentMethod;
            _currentMethod = _semanticModel.GetDeclaredSymbol(node);
            if (_currentMethod is not null)
            {
                _callGraph.AddMethod(_currentMethod);
            }
            base.VisitMethodDeclaration(node);
            _currentMethod = previousMethod;
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var previousMethod = _currentMethod;
            _currentMethod = _semanticModel.GetDeclaredSymbol(node);
            if (_currentMethod is not null)
            {
                _callGraph.AddMethod(_currentMethod);
            }
            base.VisitConstructorDeclaration(node);
            _currentMethod = previousMethod;
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var previousMethod = _currentMethod;
            _currentMethod = _semanticModel.GetDeclaredSymbol(node) as IMethodSymbol;
            if (_currentMethod is not null)
            {
                _callGraph.AddMethod(_currentMethod);
            }
            base.VisitLocalFunctionStatement(node);
            _currentMethod = previousMethod;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (_currentMethod is not null)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IMethodSymbol callee)
                {
                    // Get the original definition for generic methods
                    var targetMethod = callee.OriginalDefinition;
                    _callGraph.AddCall(_currentMethod, targetMethod);
                }
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (_currentMethod is not null)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IMethodSymbol constructor)
                {
                    _callGraph.AddCall(_currentMethod, constructor);
                }
            }
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // Handle property accesses that may have side effects
            if (_currentMethod is not null)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol is IPropertySymbol property)
                {
                    // Add call to getter/setter if it exists
                    if (property.GetMethod is not null)
                    {
                        _callGraph.AddCall(_currentMethod, property.GetMethod);
                    }
                }
            }
            base.VisitMemberAccessExpression(node);
        }
    }

    /// <summary>
    /// Walker that finds all methods called from a specific method.
    /// </summary>
    private class MethodCallWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly IMethodSymbol _method;
        private readonly HashSet<IMethodSymbol> _calledMethods = new(SymbolEqualityComparer.Default);

        public IReadOnlySet<IMethodSymbol> CalledMethods => _calledMethods;

        public MethodCallWalker(SemanticModel semanticModel, IMethodSymbol method)
        {
            _semanticModel = semanticModel;
            _method = method;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol callee)
            {
                _calledMethods.Add(callee.OriginalDefinition);
            }
            base.VisitInvocationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol constructor)
            {
                _calledMethods.Add(constructor);
            }
            base.VisitObjectCreationExpression(node);
        }
    }
}

/// <summary>
/// Analysis result for a method including its call context.
/// </summary>
public record MethodCallInfo
{
    /// <summary>
    /// The method being called.
    /// </summary>
    public required IMethodSymbol Method { get; init; }

    /// <summary>
    /// The invocation syntax.
    /// </summary>
    public InvocationExpressionSyntax? Invocation { get; init; }

    /// <summary>
    /// Arguments passed to the method.
    /// </summary>
    public IReadOnlyList<ArgumentInfo> Arguments { get; init; } = Array.Empty<ArgumentInfo>();

    /// <summary>
    /// Whether this is a recursive call.
    /// </summary>
    public bool IsRecursive { get; init; }

    /// <summary>
    /// The containing method.
    /// </summary>
    public IMethodSymbol? Caller { get; init; }
}

/// <summary>
/// Information about a method argument.
/// </summary>
public record ArgumentInfo
{
    /// <summary>
    /// The parameter this argument corresponds to.
    /// </summary>
    public required IParameterSymbol Parameter { get; init; }

    /// <summary>
    /// The argument expression.
    /// </summary>
    public required ExpressionSyntax Expression { get; init; }

    /// <summary>
    /// The complexity variable associated with this argument (if known).
    /// </summary>
    public Variable? ComplexityVariable { get; init; }

    /// <summary>
    /// How the argument relates to the caller's parameter (if derivable).
    /// </summary>
    public ArgumentRelation Relation { get; init; } = ArgumentRelation.Unknown;

    /// <summary>
    /// The scale factor if this is a scaled argument (e.g., n/2 has scale 0.5).
    /// </summary>
    public double? ScaleFactor { get; init; }
}

/// <summary>
/// Relationship between caller's parameter and callee's argument.
/// </summary>
public enum ArgumentRelation
{
    /// <summary>
    /// Unknown relationship.
    /// </summary>
    Unknown,

    /// <summary>
    /// Direct pass-through (same variable).
    /// </summary>
    Direct,

    /// <summary>
    /// Scaled version (e.g., n/2, n-1).
    /// </summary>
    Scaled,

    /// <summary>
    /// Derived from multiple variables.
    /// </summary>
    Derived,

    /// <summary>
    /// Constant value.
    /// </summary>
    Constant
}

/// <summary>
/// Extension methods for call graph analysis.
/// </summary>
public static class CallGraphExtensions
{
    /// <summary>
    /// Finds all recursive methods in the call graph.
    /// </summary>
    public static IEnumerable<IMethodSymbol> FindRecursiveMethods(this CallGraph callGraph)
    {
        return callGraph.AllMethods.Where(callGraph.IsRecursive);
    }

    /// <summary>
    /// Finds the longest call chain from a method.
    /// </summary>
    public static int FindMaxCallDepth(this CallGraph callGraph, IMethodSymbol method)
    {
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        return FindMaxDepthInternal(callGraph, method, visited);
    }

    private static int FindMaxDepthInternal(
        CallGraph callGraph, IMethodSymbol method, HashSet<IMethodSymbol> visited)
    {
        if (!visited.Add(method))
            return 0; // Cycle or already visited

        var maxChildDepth = 0;
        foreach (var callee in callGraph.GetCallees(method))
        {
            var childDepth = FindMaxDepthInternal(callGraph, callee, visited);
            maxChildDepth = Math.Max(maxChildDepth, childDepth);
        }

        visited.Remove(method);
        return maxChildDepth + 1;
    }

    /// <summary>
    /// Gets methods that have no callers (entry points).
    /// </summary>
    public static IEnumerable<IMethodSymbol> FindEntryPoints(this CallGraph callGraph)
    {
        return callGraph.AllMethods.Where(m => !callGraph.GetCallers(m).Any());
    }

    /// <summary>
    /// Gets methods that have no callees (leaf methods).
    /// </summary>
    public static IEnumerable<IMethodSymbol> FindLeafMethods(this CallGraph callGraph)
    {
        return callGraph.AllMethods.Where(m => !callGraph.GetCallees(m).Any());
    }
}
