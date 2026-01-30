using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Progress;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Roslyn.BCL;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Extracts complexity expressions from C# source code using Roslyn.
/// </summary>
public sealed class RoslynComplexityExtractor : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly LoopAnalyzer _loopAnalyzer;
    private readonly ControlFlowAnalysis _cfgAnalyzer;
    private readonly BCLComplexityMappings _bclMappings;
    private readonly IAnalysisProgress? _progress;

    private AnalysisContext _context;
    private ComplexityExpression _currentComplexity = ConstantComplexity.Zero;
    private readonly Stack<ComplexityExpression> _complexityStack = new();
    private readonly List<MethodComplexityResult> _methodResults = new();
    private readonly Dictionary<IMethodSymbol, ComplexityExpression> _methodComplexities = new(SymbolEqualityComparer.Default);

    public RoslynComplexityExtractor(
        SemanticModel semanticModel,
        CallGraph? callGraph = null,
        IAnalysisProgress? progress = null)
    {
        _semanticModel = semanticModel;
        _loopAnalyzer = new LoopAnalyzer(semanticModel);
        _cfgAnalyzer = new ControlFlowAnalysis(semanticModel);
        _bclMappings = BCLComplexityMappings.Instance;
        _progress = progress;

        _context = new AnalysisContext
        {
            SemanticModel = semanticModel,
            CallGraph = callGraph
        };
    }

    /// <summary>
    /// Gets the results of method analysis.
    /// </summary>
    public IReadOnlyList<MethodComplexityResult> MethodResults => _methodResults;

    /// <summary>
    /// Gets computed complexities for methods.
    /// </summary>
    public IReadOnlyDictionary<IMethodSymbol, ComplexityExpression> MethodComplexities => _methodComplexities;

    /// <summary>
    /// Analyzes a single method and returns its complexity.
    /// </summary>
    public ComplexityExpression AnalyzeMethod(MethodDeclarationSyntax method)
    {
        var methodSymbol = _semanticModel.GetDeclaredSymbol(method);
        if (methodSymbol is null)
            return ConstantComplexity.One;

        // Set up context with parameters
        _context = _context.WithMethod(methodSymbol);
        foreach (var param in methodSymbol.Parameters)
        {
            var variable = _context.InferParameterVariable(param);
            _context = _context.WithVariable(param, variable);
        }

        // Reset state
        _currentComplexity = ConstantComplexity.Zero;
        _complexityStack.Clear();

        // Analyze the method body
        if (method.Body is not null)
        {
            Visit(method.Body);
        }
        else if (method.ExpressionBody is not null)
        {
            Visit(method.ExpressionBody);
        }

        // Simplify the accumulated complexity expression
        var complexity = ComplexitySimplifier.Instance.Simplify(_currentComplexity);

        // Check for recursion
        if (_context.CallGraph?.IsRecursive(methodSymbol) == true)
        {
            complexity = DetectRecurrence(method, methodSymbol, complexity);
        }

        // Record result
        _methodComplexities[methodSymbol] = complexity;
        _methodResults.Add(new MethodComplexityResult
        {
            MethodName = methodSymbol.ToDisplayString(),
            FilePath = method.SyntaxTree.FilePath,
            LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            TimeComplexity = complexity,
            Confidence = ComputeConfidence(complexity),
            RequiresReview = RequiresReview(complexity)
        });

        _progress?.OnMethodAnalyzed(_methodResults[^1]);

        return complexity;
    }

    #region Statement Visitors

    public override void VisitBlock(BlockSyntax node)
    {
        // Sequential composition: sum of all statements
        var blockComplexity = ConstantComplexity.Zero as ComplexityExpression;

        foreach (var statement in node.Statements)
        {
            _currentComplexity = ConstantComplexity.Zero;
            Visit(statement);

            blockComplexity = new BinaryOperationComplexity(
                blockComplexity,
                BinaryOp.Plus,
                _currentComplexity);
        }

        _currentComplexity = blockComplexity;
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        var loopResult = _loopAnalyzer.AnalyzeForLoop(node, _context);

        if (loopResult.Success && loopResult.IterationCount is not null)
        {
            // Update context with loop bound
            if (loopResult.LoopVariable is not null && loopResult.Bound is not null)
            {
                _context = _context.WithLoopBound(loopResult.LoopVariable, loopResult.Bound);
            }

            // Analyze loop body
            _complexityStack.Push(_currentComplexity);
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Statement);
            var bodyComplexity = _currentComplexity;

            // Loop complexity = iterations × body
            _currentComplexity = new BinaryOperationComplexity(
                loopResult.IterationCount,
                BinaryOp.Multiply,
                bodyComplexity);

            // Add to parent
            var parent = _complexityStack.Pop();
            _currentComplexity = new BinaryOperationComplexity(
                parent,
                BinaryOp.Plus,
                _currentComplexity);
        }
        else
        {
            // Conservative: O(n) iterations with unknown body
            base.VisitForStatement(node);
        }
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        var loopResult = _loopAnalyzer.AnalyzeForeachLoop(node, _context);

        if (loopResult.Success && loopResult.IterationCount is not null)
        {
            // Analyze loop body
            _complexityStack.Push(_currentComplexity);
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Statement);
            var bodyComplexity = _currentComplexity;

            // Loop complexity = iterations × body
            _currentComplexity = new BinaryOperationComplexity(
                loopResult.IterationCount,
                BinaryOp.Multiply,
                bodyComplexity);

            // Add to parent
            var parent = _complexityStack.Pop();
            _currentComplexity = new BinaryOperationComplexity(
                parent,
                BinaryOp.Plus,
                _currentComplexity);
        }
        else
        {
            base.VisitForEachStatement(node);
        }
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        var loopResult = _loopAnalyzer.AnalyzeWhileLoop(node, _context);

        if (loopResult.Success && loopResult.IterationCount is not null)
        {
            // Analyze loop body
            _complexityStack.Push(_currentComplexity);
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Statement);
            var bodyComplexity = _currentComplexity;

            // Loop complexity = iterations × body
            _currentComplexity = new BinaryOperationComplexity(
                loopResult.IterationCount,
                BinaryOp.Multiply,
                bodyComplexity);

            // Add to parent
            var parent = _complexityStack.Pop();
            _currentComplexity = new BinaryOperationComplexity(
                parent,
                BinaryOp.Plus,
                _currentComplexity);
        }
        else
        {
            // Conservative: unknown iterations
            _progress?.OnWarning(new AnalysisWarning
            {
                Code = "CA0001",
                Message = "Could not determine while loop bound",
                Severity = WarningSeverity.Warning,
                Location = node.GetLocation().ToString()
            });
            base.VisitWhileStatement(node);
        }
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        var loopResult = _loopAnalyzer.AnalyzeDoWhileLoop(node, _context);

        if (loopResult.Success && loopResult.IterationCount is not null)
        {
            _complexityStack.Push(_currentComplexity);
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Statement);
            var bodyComplexity = _currentComplexity;

            _currentComplexity = new BinaryOperationComplexity(
                loopResult.IterationCount,
                BinaryOp.Multiply,
                bodyComplexity);

            var parent = _complexityStack.Pop();
            _currentComplexity = new BinaryOperationComplexity(
                parent,
                BinaryOp.Plus,
                _currentComplexity);
        }
        else
        {
            base.VisitDoStatement(node);
        }
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        // Branching: max of both branches
        _complexityStack.Push(_currentComplexity);

        _currentComplexity = ConstantComplexity.Zero;
        Visit(node.Statement);
        var trueBranch = _currentComplexity;

        var falseBranch = ConstantComplexity.Zero as ComplexityExpression;
        if (node.Else is not null)
        {
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Else.Statement);
            falseBranch = _currentComplexity;
        }

        // max(trueBranch, falseBranch)
        var branchComplexity = new BinaryOperationComplexity(
            trueBranch,
            BinaryOp.Max,
            falseBranch);

        var parent = _complexityStack.Pop();
        _currentComplexity = new BinaryOperationComplexity(
            parent,
            BinaryOp.Plus,
            branchComplexity);
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        // Switch: max of all cases
        _complexityStack.Push(_currentComplexity);

        var maxCaseComplexity = ConstantComplexity.Zero as ComplexityExpression;

        foreach (var section in node.Sections)
        {
            _currentComplexity = ConstantComplexity.Zero;
            foreach (var statement in section.Statements)
            {
                Visit(statement);
            }

            maxCaseComplexity = new BinaryOperationComplexity(
                maxCaseComplexity,
                BinaryOp.Max,
                _currentComplexity);
        }

        var parent = _complexityStack.Pop();
        _currentComplexity = new BinaryOperationComplexity(
            parent,
            BinaryOp.Plus,
            maxCaseComplexity);
    }

    public override void VisitTryStatement(TryStatementSyntax node)
    {
        // Try: max of try block and all catch blocks
        _complexityStack.Push(_currentComplexity);

        _currentComplexity = ConstantComplexity.Zero;
        Visit(node.Block);
        var tryComplexity = _currentComplexity;

        var catchComplexity = ConstantComplexity.Zero as ComplexityExpression;
        foreach (var catchClause in node.Catches)
        {
            _currentComplexity = ConstantComplexity.Zero;
            if (catchClause.Block is not null)
                Visit(catchClause.Block);

            catchComplexity = new BinaryOperationComplexity(
                catchComplexity,
                BinaryOp.Max,
                _currentComplexity);
        }

        var finallyComplexity = ConstantComplexity.Zero as ComplexityExpression;
        if (node.Finally is not null)
        {
            _currentComplexity = ConstantComplexity.Zero;
            Visit(node.Finally.Block);
            finallyComplexity = _currentComplexity;
        }

        // Total: max(try, catches) + finally
        var total = new BinaryOperationComplexity(
            new BinaryOperationComplexity(tryComplexity, BinaryOp.Max, catchComplexity),
            BinaryOp.Plus,
            finallyComplexity);

        var parent = _complexityStack.Pop();
        _currentComplexity = new BinaryOperationComplexity(parent, BinaryOp.Plus, total);
    }

    #endregion

    #region Expression Visitors

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            var callComplexity = GetMethodCallComplexity(method, node, _context);

            _currentComplexity = new BinaryOperationComplexity(
                _currentComplexity,
                BinaryOp.Plus,
                callComplexity);
        }

        base.VisitInvocationExpression(node);
    }

    public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol is IMethodSymbol constructor)
        {
            var complexity = GetConstructorComplexity(constructor, node);
            _currentComplexity = new BinaryOperationComplexity(
                _currentComplexity,
                BinaryOp.Plus,
                complexity);
        }

        base.VisitObjectCreationExpression(node);
    }

    public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
    {
        // Array/dictionary access is typically O(1)
        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);

        base.VisitElementAccessExpression(node);
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        // Assignment itself is O(1), but RHS may have complexity
        base.VisitAssignmentExpression(node);

        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
    {
        // Postfix operations like i++, i-- are O(1)
        base.VisitPostfixUnaryExpression(node);

        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
    {
        // Prefix operations like ++i, --i, !flag are O(1)
        base.VisitPrefixUnaryExpression(node);

        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        // Visit the return expression first (may have complexity)
        base.VisitReturnStatement(node);

        // Return itself is O(1)
        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    public override void VisitBreakStatement(BreakStatementSyntax node)
    {
        // Break is O(1)
        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    public override void VisitContinueStatement(ContinueStatementSyntax node)
    {
        // Continue is O(1)
        _currentComplexity = new BinaryOperationComplexity(
            _currentComplexity,
            BinaryOp.Plus,
            ConstantComplexity.One);
    }

    #endregion

    #region Helper Methods

    private ComplexityExpression GetMethodCallComplexity(
        IMethodSymbol method, InvocationExpressionSyntax invocation, AnalysisContext context)
    {
        // Check BCL mappings first
        var containingType = method.ContainingType?.ToDisplayString() ?? "";
        var methodName = method.Name;

        var bclMapping = _bclMappings.GetComplexity(containingType, methodName, method.Parameters.Length);
        if (bclMapping is not null)
        {
            // Substitute argument sizes into the complexity expression
            return SubstituteArguments(bclMapping.Complexity, method, invocation, context);
        }

        // Check if we have a computed complexity for this method
        if (_methodComplexities.TryGetValue(method.OriginalDefinition, out var computed))
        {
            return SubstituteArguments(computed, method, invocation, context);
        }

        // Check call graph
        if (context.CallGraph is not null)
        {
            var graphComplexity = context.CallGraph.GetComplexity(method.OriginalDefinition);
            if (graphComplexity is not null)
            {
                return SubstituteArguments(graphComplexity, method, invocation, context);
            }
        }

        // Check if this is a recursive call
        if (context.CurrentMethod is not null &&
            SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, context.CurrentMethod.OriginalDefinition))
        {
            // Return a placeholder for recursion - will be resolved later
            return new VariableComplexity(new Variable("T", VariableType.Custom)
            {
                Description = "Recursive call complexity"
            });
        }

        // Conservative fallback
        return GetConservativeComplexity(method);
    }

    private ComplexityExpression SubstituteArguments(
        ComplexityExpression complexity,
        IMethodSymbol method,
        InvocationExpressionSyntax invocation,
        AnalysisContext context)
    {
        var result = complexity;

        for (int i = 0; i < method.Parameters.Length && i < invocation.ArgumentList.Arguments.Count; i++)
        {
            var param = method.Parameters[i];
            var arg = invocation.ArgumentList.Arguments[i].Expression;

            var argComplexity = GetArgumentComplexity(arg, context);
            var paramVar = new Variable(param.Name, VariableType.InputSize);

            result = result.Substitute(paramVar, argComplexity);

            // Also substitute common variable names
            if (i == 0)
            {
                result = result.Substitute(Variable.N, argComplexity);
            }
        }

        return result;
    }

    private ComplexityExpression GetArgumentComplexity(ExpressionSyntax arg, AnalysisContext context)
    {
        // Check if argument is a known variable
        if (arg is IdentifierNameSyntax identifier)
        {
            var symbol = _semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol is not null)
            {
                var variable = context.GetVariable(symbol);
                if (variable is not null)
                    return new VariableComplexity(variable);

                if (symbol is IParameterSymbol param)
                {
                    var inferred = context.InferParameterVariable(param);
                    return new VariableComplexity(inferred);
                }
            }
        }

        // Check for member access (e.g., list.Count)
        if (arg is MemberAccessExpressionSyntax memberAccess)
        {
            var memberName = memberAccess.Name.Identifier.Text;
            if (memberName is "Count" or "Length" or "Size")
            {
                // Return the collection's size
                var target = memberAccess.Expression;
                return GetArgumentComplexity(target, context);
            }
        }

        // Check for binary expressions (e.g., n / 2, n - 1)
        if (arg is BinaryExpressionSyntax binary)
        {
            var left = GetArgumentComplexity(binary.Left, context);
            var right = GetArgumentComplexity(binary.Right, context);

            return binary.Kind() switch
            {
                SyntaxKind.DivideExpression => new BinaryOperationComplexity(left, BinaryOp.Multiply, right),
                SyntaxKind.SubtractExpression => left, // n - k is still O(n)
                SyntaxKind.MultiplyExpression => new BinaryOperationComplexity(left, BinaryOp.Multiply, right),
                SyntaxKind.AddExpression => new BinaryOperationComplexity(left, BinaryOp.Plus, right),
                _ => left
            };
        }

        // Literal values
        if (arg is LiteralExpressionSyntax literal)
        {
            if (literal.Token.Value is int intVal)
                return new ConstantComplexity(intVal);
            if (literal.Token.Value is double dblVal)
                return new ConstantComplexity(dblVal);
        }

        // Default to n
        return new VariableComplexity(Variable.N);
    }

    private ComplexityExpression GetConstructorComplexity(
        IMethodSymbol constructor, ObjectCreationExpressionSyntax creation)
    {
        var typeName = constructor.ContainingType?.ToDisplayString() ?? "";

        // Check BCL mappings for common collection constructors
        var mapping = _bclMappings.GetComplexity(typeName, ".ctor", constructor.Parameters.Length);
        if (mapping is not null)
            return mapping.Complexity;

        // Most constructors are O(1) unless they take a collection to copy
        if (constructor.Parameters.Any(p => IsCollectionType(p.Type)))
        {
            return new VariableComplexity(Variable.N); // O(n) for copying
        }

        return ConstantComplexity.One;
    }

    private ComplexityExpression GetConservativeComplexity(IMethodSymbol method)
    {
        // Conservative heuristics for unknown methods
        var containingType = method.ContainingType;

        // Sorting methods
        if (method.Name.Contains("Sort", StringComparison.OrdinalIgnoreCase))
        {
            return PolyLogComplexity.NLogN(Variable.N);
        }

        // Search methods
        if (method.Name.Contains("Find", StringComparison.OrdinalIgnoreCase) ||
            method.Name.Contains("Search", StringComparison.OrdinalIgnoreCase) ||
            method.Name.Contains("Contains", StringComparison.OrdinalIgnoreCase))
        {
            return new VariableComplexity(Variable.N);
        }

        // Methods on collections are often O(n) in the worst case
        if (containingType is not null && IsCollectionType(containingType))
        {
            return new VariableComplexity(Variable.N);
        }

        // Default: O(1)
        return ConstantComplexity.One;
    }

    private ComplexityExpression DetectRecurrence(
        MethodDeclarationSyntax method, IMethodSymbol methodSymbol, ComplexityExpression bodyComplexity)
    {
        // Find recursive calls and their arguments
        var recursiveCalls = FindRecursiveCalls(method, methodSymbol);

        if (recursiveCalls.Count == 0)
            return bodyComplexity;

        var terms = new List<RecurrenceTerm>();

        foreach (var call in recursiveCalls)
        {
            var (coefficient, scaleFactor) = AnalyzeRecursiveCall(call, methodSymbol);
            if (scaleFactor > 0 && scaleFactor < 1)
            {
                terms.Add(new RecurrenceTerm(
                    coefficient,
                    new VariableComplexity(Variable.N),
                    scaleFactor));
            }
        }

        if (terms.Count == 0)
        {
            // Linear recursion T(n) = T(n-1) + f(n)
            return PolyLogComplexity.NLogN(Variable.N); // Conservative
        }

        // Create recurrence expression
        var recurrence = new RecurrenceComplexity(
            terms.ToImmutableList(),
            Variable.N,
            bodyComplexity,
            ConstantComplexity.One);

        _progress?.OnRecurrenceDetected(new RecurrenceDetectionResult
        {
            MethodName = methodSymbol.ToDisplayString(),
            Recurrence = recurrence,
            Type = terms.Count == 1 ? RecurrenceType.DivideAndConquer : RecurrenceType.MultiTerm,
            IsSolvable = recurrence.FitsMasterTheorem || recurrence.FitsAkraBazzi,
            RecommendedApproach = recurrence.FitsMasterTheorem
                ? SolvingApproach.MasterTheorem
                : SolvingApproach.AkraBazzi
        });

        return recurrence;
    }

    private List<InvocationExpressionSyntax> FindRecursiveCalls(
        MethodDeclarationSyntax method, IMethodSymbol methodSymbol)
    {
        var calls = new List<InvocationExpressionSyntax>();
        var walker = new RecursiveCallFinder(_semanticModel, methodSymbol);
        walker.Visit(method);
        return walker.RecursiveCalls;
    }

    private (double coefficient, double scaleFactor) AnalyzeRecursiveCall(
        InvocationExpressionSyntax call, IMethodSymbol method)
    {
        // Count how many times this call appears (coefficient)
        double coefficient = 1.0;

        // Analyze arguments to determine scale factor
        if (call.ArgumentList.Arguments.Count > 0)
        {
            var firstArg = call.ArgumentList.Arguments[0].Expression;
            var scaleFactor = ExtractScaleFactor(firstArg, method);
            return (coefficient, scaleFactor);
        }

        return (1.0, 0.5); // Default: T(n/2)
    }

    private double ExtractScaleFactor(ExpressionSyntax arg, IMethodSymbol method)
    {
        // n / 2, n / 3, etc.
        if (arg is BinaryExpressionSyntax binary && binary.Kind() == SyntaxKind.DivideExpression)
        {
            if (binary.Right is LiteralExpressionSyntax literal && literal.Token.Value is int divisor)
            {
                return 1.0 / divisor;
            }
        }

        // n - 1, n - k (linear recursion)
        if (arg is BinaryExpressionSyntax sub && sub.Kind() == SyntaxKind.SubtractExpression)
        {
            return 0.99; // Approximation for linear recursion
        }

        return 0.5; // Default assumption
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            var ns = namedType.ContainingNamespace?.ToDisplayString() ?? "";
            return ns.StartsWith("System.Collections") ||
                   namedType.AllInterfaces.Any(i =>
                       i.ToDisplayString().StartsWith("System.Collections.Generic.IEnumerable"));
        }
        return false;
    }

    private double ComputeConfidence(ComplexityExpression complexity)
    {
        // Higher confidence for simpler expressions
        return complexity switch
        {
            ConstantComplexity => 1.0,
            VariableComplexity => 0.95,
            LinearComplexity => 0.9,
            PolynomialComplexity => 0.85,
            LogarithmicComplexity => 0.9,
            PolyLogComplexity => 0.85,
            RecurrenceComplexity => 0.7,
            _ => 0.6
        };
    }

    private bool RequiresReview(ComplexityExpression complexity)
    {
        return complexity switch
        {
            RecurrenceComplexity => true,
            ExponentialComplexity => true,
            FactorialComplexity => true,
            BinaryOperationComplexity bin => RequiresReview(bin.Left) || RequiresReview(bin.Right),
            _ => false
        };
    }

    private class RecursiveCallFinder : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly IMethodSymbol _targetMethod;
        public List<InvocationExpressionSyntax> RecursiveCalls { get; } = new();

        public RecursiveCallFinder(SemanticModel semanticModel, IMethodSymbol targetMethod)
        {
            _semanticModel = semanticModel;
            _targetMethod = targetMethod;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol is IMethodSymbol method &&
                SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, _targetMethod.OriginalDefinition))
            {
                RecursiveCalls.Add(node);
            }
            base.VisitInvocationExpression(node);
        }
    }

    #endregion
}

/// <summary>
/// Extension methods for the complexity extractor.
/// </summary>
public static class RoslynComplexityExtractorExtensions
{
    /// <summary>
    /// Analyzes all methods in a syntax tree.
    /// </summary>
    public static IReadOnlyList<MethodComplexityResult> AnalyzeAllMethods(
        this RoslynComplexityExtractor extractor,
        SyntaxNode root)
    {
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToList();

        foreach (var method in methods)
        {
            extractor.AnalyzeMethod(method);
        }

        return extractor.MethodResults;
    }

    /// <summary>
    /// Analyzes methods in topological order based on call graph.
    /// </summary>
    public static IReadOnlyList<MethodComplexityResult> AnalyzeInTopologicalOrder(
        this RoslynComplexityExtractor extractor,
        SyntaxNode root,
        CallGraph callGraph)
    {
        var sortedMethods = callGraph.TopologicalSort();

        if (sortedMethods is null)
        {
            // Cycle detected, fall back to normal analysis
            return extractor.AnalyzeAllMethods(root);
        }

        var methodSyntax = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToDictionary(
                m => m,
                m => extractor.MethodComplexities.Keys
                    .FirstOrDefault(k => k.Name == GetMethodSymbolName(m)));

        foreach (var methodSymbol in sortedMethods)
        {
            var syntax = methodSyntax.FirstOrDefault(kvp =>
                kvp.Value is not null &&
                SymbolEqualityComparer.Default.Equals(kvp.Value, methodSymbol));

            if (syntax.Key is not null)
            {
                extractor.AnalyzeMethod(syntax.Key);
            }
        }

        return extractor.MethodResults;
    }

    private static string GetMethodSymbolName(MethodDeclarationSyntax method) =>
        method.Identifier.Text;
}
