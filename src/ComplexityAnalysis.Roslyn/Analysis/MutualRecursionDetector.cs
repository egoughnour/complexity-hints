using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Detects mutual recursion patterns in code using call graph analysis.
/// 
/// Mutual recursion occurs when two or more methods call each other in a cycle:
/// - A() calls B(), B() calls A()
/// - A() calls B(), B() calls C(), C() calls A()
/// 
/// Detection uses Tarjan's algorithm for strongly connected components (SCCs).
/// </summary>
public sealed class MutualRecursionDetector
{
    private readonly SemanticModel _semanticModel;
    private readonly CallGraph _callGraph;

    public MutualRecursionDetector(SemanticModel semanticModel, CallGraph callGraph)
    {
        _semanticModel = semanticModel;
        _callGraph = callGraph;
    }

    /// <summary>
    /// Detects all mutual recursion cycles in the call graph.
    /// </summary>
    public IReadOnlyList<MutualRecursionCycle> DetectCycles()
    {
        var sccs = _callGraph.FindCycles();
        var cycles = new List<MutualRecursionCycle>();

        foreach (var scc in sccs)
        {
            // SCCs with more than one method are mutual recursion
            if (scc.Count > 1)
            {
                var cycle = AnalyzeCycle(scc);
                if (cycle != null)
                {
                    cycles.Add(cycle);
                }
            }
            // Single-method SCCs that call themselves are direct recursion (handled elsewhere)
        }

        return cycles;
    }

    /// <summary>
    /// Checks if a specific method is part of a mutual recursion cycle.
    /// </summary>
    public bool IsInMutualRecursion(IMethodSymbol method)
    {
        var cycles = _callGraph.FindCycles();
        return cycles.Any(scc => 
            scc.Count > 1 && 
            scc.Any(m => SymbolEqualityComparer.Default.Equals(m, method)));
    }

    /// <summary>
    /// Gets the mutual recursion cycle containing a specific method, if any.
    /// </summary>
    public MutualRecursionCycle? GetCycleContaining(IMethodSymbol method)
    {
        var cycles = _callGraph.FindCycles();
        var scc = cycles.FirstOrDefault(c => 
            c.Count > 1 && 
            c.Any(m => SymbolEqualityComparer.Default.Equals(m, method)));

        return scc != null ? AnalyzeCycle(scc) : null;
    }

    /// <summary>
    /// Analyzes a strongly connected component to extract mutual recursion details.
    /// </summary>
    private MutualRecursionCycle? AnalyzeCycle(IReadOnlyList<IMethodSymbol> scc)
    {
        if (scc.Count < 2)
            return null;

        // Order the cycle by call relationships
        var orderedMethods = OrderCycle(scc);
        var methodInfos = new List<MutualRecursionMethodInfo>();

        foreach (var method in orderedMethods)
        {
            var info = AnalyzeMethod(method, orderedMethods);
            methodInfos.Add(info);
        }

        return new MutualRecursionCycle
        {
            Methods = methodInfos.ToImmutableList(),
            CycleOrder = orderedMethods.Select(m => m.Name).ToImmutableList()
        };
    }

    /// <summary>
    /// Orders methods in a cycle by their call relationships.
    /// Returns methods in the order they call each other: A → B → C → A
    /// </summary>
    private IReadOnlyList<IMethodSymbol> OrderCycle(IReadOnlyList<IMethodSymbol> scc)
    {
        if (scc.Count <= 1)
            return scc;

        // Build a subgraph of just this SCC
        var sccSet = new HashSet<IMethodSymbol>(scc, SymbolEqualityComparer.Default);
        
        // Start from the first method and follow the cycle
        var ordered = new List<IMethodSymbol>();
        var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        var current = scc[0];

        while (visited.Add(current) && ordered.Count < scc.Count)
        {
            ordered.Add(current);
            
            // Find the next method in the cycle
            var callees = _callGraph.GetCallees(current);
            var nextInCycle = callees.FirstOrDefault(c => 
                sccSet.Contains(c) && !visited.Contains(c));

            if (nextInCycle == null)
            {
                // If no unvisited callee, we've completed or there's a branch
                break;
            }
            
            current = nextInCycle;
        }

        // If we didn't visit all methods, just return original order
        if (ordered.Count != scc.Count)
        {
            return scc;
        }

        return ordered;
    }

    /// <summary>
    /// Analyzes a single method's contribution to the mutual recursion.
    /// </summary>
    private MutualRecursionMethodInfo AnalyzeMethod(
        IMethodSymbol method,
        IReadOnlyList<IMethodSymbol> cycleMethods)
    {
        var cycleSet = new HashSet<IMethodSymbol>(cycleMethods, SymbolEqualityComparer.Default);
        
        // Find the method's syntax
        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        var methodSyntax = syntaxRef?.GetSyntax() as MethodDeclarationSyntax;

        // Find calls to other methods in the cycle
        var cycleCalls = new List<MutualRecursionCall>();
        ComplexityExpression nonRecursiveWork = ConstantComplexity.One;

        if (methodSyntax != null)
        {
            var analyzer = new MethodBodyAnalyzer(_semanticModel, method, cycleSet);
            analyzer.Visit(methodSyntax);
            
            cycleCalls = analyzer.CycleCalls;
            nonRecursiveWork = analyzer.NonRecursiveWork;
        }

        return new MutualRecursionMethodInfo
        {
            Method = method,
            MethodName = method.Name,
            NonRecursiveWork = nonRecursiveWork,
            CycleCalls = cycleCalls.ToImmutableList()
        };
    }

    /// <summary>
    /// Analyzes method body to find cycle calls and non-recursive work.
    /// </summary>
    private class MethodBodyAnalyzer : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly IMethodSymbol _currentMethod;
        private readonly HashSet<IMethodSymbol> _cycleSet;
        
        public List<MutualRecursionCall> CycleCalls { get; } = new();
        public ComplexityExpression NonRecursiveWork { get; private set; } = ConstantComplexity.One;

        private int _loopNestingDepth = 0;
        private ComplexityExpression _accumulatedWork = ConstantComplexity.Zero;

        public MethodBodyAnalyzer(
            SemanticModel semanticModel,
            IMethodSymbol currentMethod,
            HashSet<IMethodSymbol> cycleSet)
        {
            _semanticModel = semanticModel;
            _currentMethod = currentMethod;
            _cycleSet = cycleSet;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            base.VisitMethodDeclaration(node);
            
            // Finalize non-recursive work
            NonRecursiveWork = _accumulatedWork is ConstantComplexity { Value: 0 }
                ? ConstantComplexity.One
                : ComplexitySimplifier.Instance.Simplify(_accumulatedWork);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol calledMethod)
            {
                var originalMethod = calledMethod.OriginalDefinition;
                
                if (_cycleSet.Contains(originalMethod) && 
                    !SymbolEqualityComparer.Default.Equals(originalMethod, _currentMethod))
                {
                    // This is a call to another method in the cycle
                    var call = AnalyzeCall(node, originalMethod);
                    CycleCalls.Add(call);
                }
                else if (!_cycleSet.Contains(originalMethod))
                {
                    // Non-cycle call contributes to work
                    AddWork(ConstantComplexity.One);
                }
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            _loopNestingDepth++;
            
            // Simple heuristic: loop adds O(n) factor
            var loopFactor = new VariableComplexity(Variable.N);
            var previousWork = _accumulatedWork;
            _accumulatedWork = ConstantComplexity.Zero;
            
            base.VisitForStatement(node);
            
            // Combine loop body work with loop factor
            var bodyWork = _accumulatedWork;
            var loopWork = ComplexityComposition.Nested(loopFactor, bodyWork);
            _accumulatedWork = ComplexityComposition.Sequential(previousWork, loopWork);
            
            _loopNestingDepth--;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            _loopNestingDepth++;
            
            var loopFactor = new VariableComplexity(Variable.N);
            var previousWork = _accumulatedWork;
            _accumulatedWork = ConstantComplexity.Zero;
            
            base.VisitForEachStatement(node);
            
            var bodyWork = _accumulatedWork;
            var loopWork = ComplexityComposition.Nested(loopFactor, bodyWork);
            _accumulatedWork = ComplexityComposition.Sequential(previousWork, loopWork);
            
            _loopNestingDepth--;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            _loopNestingDepth++;
            
            var loopFactor = new VariableComplexity(Variable.N);
            var previousWork = _accumulatedWork;
            _accumulatedWork = ConstantComplexity.Zero;
            
            base.VisitWhileStatement(node);
            
            var bodyWork = _accumulatedWork;
            var loopWork = ComplexityComposition.Nested(loopFactor, bodyWork);
            _accumulatedWork = ComplexityComposition.Sequential(previousWork, loopWork);
            
            _loopNestingDepth--;
        }

        private void AddWork(ComplexityExpression work)
        {
            _accumulatedWork = ComplexityComposition.Sequential(_accumulatedWork, work);
        }

        private MutualRecursionCall AnalyzeCall(
            InvocationExpressionSyntax invocation,
            IMethodSymbol targetMethod)
        {
            var reduction = 1.0;
            var scaleFactor = 0.99; // Default: subtraction pattern

            // Analyze the first argument to determine how the problem shrinks
            if (invocation.ArgumentList.Arguments.Count > 0)
            {
                var firstArg = invocation.ArgumentList.Arguments[0].Expression;
                (reduction, scaleFactor) = AnalyzeArgument(firstArg);
            }

            return new MutualRecursionCall
            {
                TargetMethod = targetMethod,
                TargetMethodName = targetMethod.Name,
                Reduction = reduction,
                ScaleFactor = scaleFactor,
                InvocationSyntax = invocation
            };
        }

        private (double reduction, double scaleFactor) AnalyzeArgument(ExpressionSyntax arg)
        {
            // n - 1, n - 2, etc.
            if (arg is BinaryExpressionSyntax sub && sub.Kind() == SyntaxKind.SubtractExpression)
            {
                if (sub.Right is LiteralExpressionSyntax literal && 
                    literal.Token.Value is int reduction)
                {
                    return (reduction, 1.0 - (reduction * 0.01));
                }
                return (1.0, 0.99);
            }

            // n / 2, n / 3, etc.
            if (arg is BinaryExpressionSyntax div && div.Kind() == SyntaxKind.DivideExpression)
            {
                if (div.Right is LiteralExpressionSyntax literal &&
                    literal.Token.Value is int divisor && divisor > 0)
                {
                    return (0.0, 1.0 / divisor);
                }
                return (0.0, 0.5);
            }

            // Default: subtraction by 1
            return (1.0, 0.99);
        }
    }
}

/// <summary>
/// Represents a detected mutual recursion cycle.
/// </summary>
public sealed record MutualRecursionCycle
{
    /// <summary>
    /// Information about each method in the cycle.
    /// </summary>
    public required ImmutableList<MutualRecursionMethodInfo> Methods { get; init; }

    /// <summary>
    /// The order of methods in the cycle (by name).
    /// </summary>
    public required ImmutableList<string> CycleOrder { get; init; }

    /// <summary>
    /// Number of methods in the cycle.
    /// </summary>
    public int Length => Methods.Count;

    /// <summary>
    /// Converts to a mutual recurrence system for solving.
    /// </summary>
    public MutualRecurrenceSystem ToRecurrenceSystem(Variable variable)
    {
        var components = Methods.Select(m => new MutualRecurrenceComponent
        {
            MethodName = m.MethodName,
            NonRecursiveWork = m.NonRecursiveWork,
            Reduction = m.CycleCalls.FirstOrDefault()?.Reduction ?? 1.0,
            ScaleFactor = m.CycleCalls.FirstOrDefault()?.ScaleFactor ?? 0.99,
            Callees = m.CycleCalls.Select(c => c.TargetMethodName).ToImmutableList()
        }).ToImmutableList();

        return new MutualRecurrenceSystem
        {
            Components = components,
            Variable = variable
        };
    }

    /// <summary>
    /// Gets a human-readable description of the cycle.
    /// </summary>
    public string GetDescription()
    {
        var cycle = string.Join(" → ", CycleOrder);
        return $"{cycle} → {CycleOrder[0]}";
    }
}

/// <summary>
/// Information about a single method in a mutual recursion cycle.
/// </summary>
public sealed record MutualRecursionMethodInfo
{
    /// <summary>
    /// The method symbol.
    /// </summary>
    public required IMethodSymbol Method { get; init; }

    /// <summary>
    /// The method name.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// The non-recursive work done by this method.
    /// </summary>
    public required ComplexityExpression NonRecursiveWork { get; init; }

    /// <summary>
    /// Calls to other methods in the cycle.
    /// </summary>
    public required ImmutableList<MutualRecursionCall> CycleCalls { get; init; }
}

/// <summary>
/// Information about a call to another method in the mutual recursion cycle.
/// </summary>
public sealed record MutualRecursionCall
{
    /// <summary>
    /// The target method being called.
    /// </summary>
    public required IMethodSymbol TargetMethod { get; init; }

    /// <summary>
    /// The target method name.
    /// </summary>
    public required string TargetMethodName { get; init; }

    /// <summary>
    /// How much the problem size is reduced (for subtraction patterns).
    /// </summary>
    public double Reduction { get; init; } = 1.0;

    /// <summary>
    /// Scale factor (for division patterns).
    /// </summary>
    public double ScaleFactor { get; init; } = 0.99;

    /// <summary>
    /// The invocation syntax.
    /// </summary>
    public InvocationExpressionSyntax? InvocationSyntax { get; init; }
}
