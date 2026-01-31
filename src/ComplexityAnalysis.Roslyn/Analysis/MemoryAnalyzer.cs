using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Memory;
using ComplexityAnalysis.Roslyn.BCL;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Analyzes code to determine memory/space complexity.
/// 
/// Detects:
/// - Stack space from recursion depth
/// - Heap allocations (arrays, collections, objects)
/// - Auxiliary space usage
/// - In-place algorithms
/// - Tail recursion optimization potential
/// </summary>
public sealed class MemoryAnalyzer
{
    private readonly SemanticModel _semanticModel;
    private readonly BCLComplexityMappings _bclMappings;

    public MemoryAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
        _bclMappings = BCLComplexityMappings.Instance;
    }

    /// <summary>
    /// Analyzes a method's memory complexity.
    /// </summary>
    public MemoryComplexity AnalyzeMethod(MethodDeclarationSyntax method, AnalysisContext context)
    {
        var allocations = new List<AllocationInfo>();
        var stackSpace = ConstantComplexity.One as ComplexityExpression;
        var heapSpace = ConstantComplexity.Zero as ComplexityExpression;
        bool isTailRecursive = false;

        // Analyze recursion
        var recursionResult = AnalyzeRecursion(method, context);
        if (recursionResult.IsRecursive)
        {
            stackSpace = recursionResult.DepthComplexity;
            isTailRecursive = recursionResult.IsTailRecursive;

            if (isTailRecursive)
            {
                stackSpace = ConstantComplexity.One; // TCO reduces to O(1)
            }

            allocations.Add(AllocationInfo.RecursionFrame(recursionResult.DepthComplexity));
        }

        // Analyze allocations in method body
        var allocationAnalysis = AnalyzeAllocations(method, context);
        allocations.AddRange(allocationAnalysis.Allocations);
        heapSpace = allocationAnalysis.TotalHeapSpace;

        // Calculate total space
        ComplexityExpression totalSpace = new BinaryOperationComplexity(stackSpace, BinaryOp.Plus, heapSpace);

        // Determine if in-place
        var isInPlace = IsInPlaceAlgorithm(heapSpace, allocations);

        // Simplify total space
        totalSpace = ComplexitySimplifier.Instance.NormalizeForm(totalSpace);

        return new MemoryComplexity
        {
            TotalSpace = totalSpace,
            StackSpace = stackSpace,
            HeapSpace = heapSpace,
            AuxiliarySpace = totalSpace, // For now, auxiliary = total (excluding input)
            IsInPlace = isInPlace,
            IsTailRecursive = isTailRecursive,
            Allocations = allocations.ToImmutableList(),
            Description = GenerateDescription(totalSpace, isInPlace, isTailRecursive)
        };
    }

    /// <summary>
    /// Analyzes recursion depth and patterns.
    /// </summary>
    public RecursionAnalysisResult AnalyzeRecursion(MethodDeclarationSyntax method, AnalysisContext context)
    {
        var methodSymbol = _semanticModel.GetDeclaredSymbol(method);
        if (methodSymbol == null)
            return RecursionAnalysisResult.NotRecursive;

        var finder = new RecursiveCallAnalyzer(_semanticModel, methodSymbol);
        finder.Visit(method);

        if (!finder.IsRecursive)
            return RecursionAnalysisResult.NotRecursive;

        // Analyze recursion depth
        var depthComplexity = AnalyzeRecursionDepth(finder.RecursiveCalls, method, context);

        // Check for tail recursion
        var isTailRecursive = CheckTailRecursion(finder.RecursiveCalls, method);

        return new RecursionAnalysisResult
        {
            IsRecursive = true,
            DepthComplexity = depthComplexity,
            IsTailRecursive = isTailRecursive,
            RecursiveCallCount = finder.RecursiveCalls.Count,
            Pattern = DetermineRecursionPattern(finder.RecursiveCalls, method, context)
        };
    }

    /// <summary>
    /// Analyzes heap allocations in a method.
    /// </summary>
    public AllocationAnalysisResult AnalyzeAllocations(MethodDeclarationSyntax method, AnalysisContext context)
    {
        var allocations = new List<AllocationInfo>();
        var walker = new AllocationWalker(_semanticModel, context, _bclMappings);
        walker.Visit(method);

        allocations.AddRange(walker.Allocations);

        // Calculate total heap space
        var totalHeap = allocations
            .Where(a => a.Source == MemorySource.Heap)
            .Select(a => a.TotalSize)
            .DefaultIfEmpty(ConstantComplexity.Zero)
            .Aggregate((a, b) => new BinaryOperationComplexity(a, BinaryOp.Plus, b));

        return new AllocationAnalysisResult
        {
            Allocations = allocations.ToImmutableList(),
            TotalHeapSpace = ComplexitySimplifier.Instance.NormalizeForm(totalHeap)
        };
    }

    #region Recursion Analysis

    private ComplexityExpression AnalyzeRecursionDepth(
        IReadOnlyList<InvocationExpressionSyntax> recursiveCalls,
        MethodDeclarationSyntax method,
        AnalysisContext context)
    {
        if (recursiveCalls.Count == 0)
            return ConstantComplexity.Zero;

        // Analyze the first recursive call's argument transformation
        var firstCall = recursiveCalls[0];
        if (firstCall.ArgumentList.Arguments.Count > 0)
        {
            var firstArg = firstCall.ArgumentList.Arguments[0].Expression;
            return AnalyzeArgumentTransformation(firstArg, context);
        }

        // Default: assume O(n) depth
        return new VariableComplexity(Variable.N);
    }

    private ComplexityExpression AnalyzeArgumentTransformation(ExpressionSyntax arg, AnalysisContext context)
    {
        // n / 2 -> O(log n) depth
        if (arg is BinaryExpressionSyntax binary)
        {
            if (binary.Kind() == SyntaxKind.DivideExpression)
            {
                if (binary.Right is LiteralExpressionSyntax lit && lit.Token.Value is int divisor && divisor > 1)
                {
                    return new LogarithmicComplexity(1.0, Variable.N, divisor);
                }
                return new LogarithmicComplexity(1.0, Variable.N);
            }

            // n - 1, n - k -> O(n) depth
            if (binary.Kind() == SyntaxKind.SubtractExpression)
            {
                return new VariableComplexity(Variable.N);
            }

            // n + n, n * 2 at each level -> exponential is unusual, assume linear
            if (binary.Kind() == SyntaxKind.MultiplyExpression || binary.Kind() == SyntaxKind.AddExpression)
            {
                return new VariableComplexity(Variable.N);
            }
        }

        // Default: linear depth
        return new VariableComplexity(Variable.N);
    }

    private bool CheckTailRecursion(IReadOnlyList<InvocationExpressionSyntax> calls, MethodDeclarationSyntax method)
    {
        if (calls.Count != 1)
            return false; // Multiple recursive calls can't be tail-recursive

        var call = calls[0];

        // Check if the call is in a return statement
        var parent = call.Parent;
        while (parent != null)
        {
            if (parent is ReturnStatementSyntax)
                return true;

            // Check for expression-bodied member: int Fac(int n) => n <= 1 ? 1 : n * Fac(n-1)
            if (parent is ArrowExpressionClauseSyntax)
            {
                // Not tail-recursive if there's an operation after the call
                // e.g., n * Fac(n-1) is NOT tail recursive
                if (call.Parent is BinaryExpressionSyntax)
                    return false;
                return true;
            }

            // If we hit a binary expression before return, it's not tail recursive
            // e.g., return n * Factorial(n - 1) is NOT tail recursive
            if (parent is BinaryExpressionSyntax)
                return false;

            if (parent is BlockSyntax || parent is MethodDeclarationSyntax)
                break;

            parent = parent.Parent;
        }

        return false;
    }

    private RecursionPattern DetermineRecursionPattern(
        IReadOnlyList<InvocationExpressionSyntax> calls,
        MethodDeclarationSyntax method,
        AnalysisContext context)
    {
        if (calls.Count == 0)
            return RecursionPattern.None;

        if (calls.Count == 1)
        {
            var arg = calls[0].ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (arg is BinaryExpressionSyntax binary)
            {
                if (binary.Kind() == SyntaxKind.DivideExpression)
                    return RecursionPattern.DivideByConstant;
                if (binary.Kind() == SyntaxKind.SubtractExpression)
                    return RecursionPattern.DecrementByConstant;
            }
            return RecursionPattern.Linear;
        }

        if (calls.Count == 2)
        {
            // Check for divide-and-conquer pattern (like merge sort)
            var hasHalving = calls.All(c =>
            {
                var arg = c.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                return arg is BinaryExpressionSyntax b && b.Kind() == SyntaxKind.DivideExpression;
            });

            if (hasHalving)
                return RecursionPattern.DivideAndConquer;

            // Check for tree recursion (like Fibonacci)
            return RecursionPattern.TreeRecursion;
        }

        return RecursionPattern.Multiple;
    }

    #endregion

    #region Helper Methods

    private static bool IsInPlaceAlgorithm(ComplexityExpression heapSpace, IReadOnlyList<AllocationInfo> allocations)
    {
        // In-place if no significant heap allocations
        if (heapSpace is ConstantComplexity c && c.Value <= 10) // Allow small constant allocations
            return true;

        // Check if all allocations are constant
        return allocations.All(a =>
            a.Source != MemorySource.Heap ||
            a.TotalSize is ConstantComplexity);
    }

    private static string GenerateDescription(
        ComplexityExpression totalSpace,
        bool isInPlace,
        bool isTailRecursive)
    {
        var parts = new List<string>();

        var spaceClass = SpaceComplexityClassifier.Classify(totalSpace);
        parts.Add(SpaceComplexityClassifier.GetDescription(spaceClass));

        if (isInPlace)
            parts.Add("In-place algorithm");
        if (isTailRecursive)
            parts.Add("Tail-recursive (TCO applicable)");

        return string.Join(". ", parts);
    }

    #endregion

    #region Nested Types

    private class RecursiveCallAnalyzer : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly IMethodSymbol _targetMethod;
        public List<InvocationExpressionSyntax> RecursiveCalls { get; } = new();
        public bool IsRecursive => RecursiveCalls.Count > 0;

        public RecursiveCallAnalyzer(SemanticModel semanticModel, IMethodSymbol targetMethod)
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

    private class AllocationWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly AnalysisContext _context;
        private readonly BCLComplexityMappings _bclMappings;
        public List<AllocationInfo> Allocations { get; } = new();

        // Track current loop context for allocation counting
        private readonly Stack<ComplexityExpression> _loopIterations = new();

        public AllocationWalker(SemanticModel semanticModel, AnalysisContext context, BCLComplexityMappings bclMappings)
        {
            _semanticModel = semanticModel;
            _context = context;
            _bclMappings = bclMappings;
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            // Estimate loop iterations
            var iterations = EstimateLoopIterations(node);
            _loopIterations.Push(iterations);
            base.VisitForStatement(node);
            _loopIterations.Pop();
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            var iterations = new VariableComplexity(Variable.N);
            _loopIterations.Push(iterations);
            base.VisitForEachStatement(node);
            _loopIterations.Pop();
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            var iterations = new VariableComplexity(Variable.N);
            _loopIterations.Push(iterations);
            base.VisitWhileStatement(node);
            _loopIterations.Pop();
        }

        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            var size = AnalyzeArraySize(node);
            var count = GetCurrentIterationCount();

            Allocations.Add(new AllocationInfo
            {
                Description = "Array allocation",
                Size = size,
                Count = count,
                Source = MemorySource.Heap,
                TypeName = node.Type.ToString()
            });

            base.VisitArrayCreationExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = _semanticModel.GetTypeInfo(node);
            var typeName = typeInfo.Type?.ToDisplayString() ?? "object";

            // Check for collection types
            if (IsCollectionType(typeInfo.Type))
            {
                var size = EstimateCollectionSize(node, typeInfo.Type);
                var count = GetCurrentIterationCount();

                Allocations.Add(new AllocationInfo
                {
                    Description = $"{GetSimpleTypeName(typeName)} allocation",
                    Size = size,
                    Count = count,
                    Source = MemorySource.Heap,
                    TypeName = typeName
                });
            }
            else
            {
                // Regular object allocation
                var count = GetCurrentIterationCount();

                Allocations.Add(new AllocationInfo
                {
                    Description = "Object allocation",
                    Size = ConstantComplexity.One,
                    Count = count,
                    Source = MemorySource.Heap,
                    TypeName = typeName
                });
            }

            base.VisitObjectCreationExpression(node);
        }

        public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            var initCount = node.Initializer.Expressions.Count;

            Allocations.Add(new AllocationInfo
            {
                Description = "Implicit array allocation",
                Size = new ConstantComplexity(initCount),
                Count = GetCurrentIterationCount(),
                Source = MemorySource.Heap
            });

            base.VisitImplicitArrayCreationExpression(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Check for methods that allocate (like ToList(), ToArray(), Clone())
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is IMethodSymbol method)
            {
                var methodName = method.Name;
                var containingType = method.ContainingType?.MetadataName ?? "";

                if (IsAllocatingMethod(methodName))
                {
                    var size = EstimateMethodAllocationSize(method, node);
                    var count = GetCurrentIterationCount();

                    Allocations.Add(new AllocationInfo
                    {
                        Description = $"{methodName}() allocation",
                        Size = size,
                        Count = count,
                        Source = MemorySource.Heap,
                        TypeName = method.ReturnType?.ToDisplayString()
                    });
                }

                // Check BCL for space complexity hints
                var bclMapping = _bclMappings.GetComplexity(containingType, methodName, method.Parameters.Length);
                if (bclMapping?.SpaceComplexity != null)
                {
                    Allocations.Add(new AllocationInfo
                    {
                        Description = $"{methodName}() internal allocation",
                        Size = bclMapping.SpaceComplexity,
                        Count = GetCurrentIterationCount(),
                        Source = MemorySource.Heap
                    });
                }
            }

            base.VisitInvocationExpression(node);
        }

        private ComplexityExpression AnalyzeArraySize(ArrayCreationExpressionSyntax node)
        {
            // Check for explicit size: new int[n]
            if (node.Type.RankSpecifiers.Count > 0)
            {
                var firstRank = node.Type.RankSpecifiers[0];
                if (firstRank.Sizes.Count > 0)
                {
                    var sizeExpr = firstRank.Sizes[0];
                    return AnalyzeSizeExpression(sizeExpr);
                }
            }

            // Check for initializer: new int[] { 1, 2, 3 }
            if (node.Initializer != null)
            {
                return new ConstantComplexity(node.Initializer.Expressions.Count);
            }

            return new VariableComplexity(Variable.N);
        }

        private ComplexityExpression AnalyzeSizeExpression(ExpressionSyntax expr)
        {
            // Literal: new int[10]
            if (expr is LiteralExpressionSyntax literal)
            {
                if (literal.Token.Value is int intVal)
                    return new ConstantComplexity(intVal);
            }

            // Variable: new int[n]
            if (expr is IdentifierNameSyntax identifier)
            {
                var symbol = _semanticModel.GetSymbolInfo(identifier).Symbol;
                if (symbol is IParameterSymbol param)
                {
                    var variable = _context.GetVariable(param) ?? _context.InferParameterVariable(param);
                    return new VariableComplexity(variable);
                }
            }

            // Member access: new int[array.Length]
            if (expr is MemberAccessExpressionSyntax memberAccess)
            {
                var memberName = memberAccess.Name.Identifier.Text;
                if (memberName is "Length" or "Count" or "Size")
                {
                    return new VariableComplexity(Variable.N);
                }
            }

            // Binary: new int[n * m] or new int[n / 2]
            if (expr is BinaryExpressionSyntax binary)
            {
                var left = AnalyzeSizeExpression(binary.Left);
                var right = AnalyzeSizeExpression(binary.Right);

                return binary.Kind() switch
                {
                    SyntaxKind.MultiplyExpression => new BinaryOperationComplexity(left, BinaryOp.Multiply, right),
                    SyntaxKind.AddExpression => new BinaryOperationComplexity(left, BinaryOp.Plus, right),
                    _ => left
                };
            }

            return new VariableComplexity(Variable.N);
        }

        private ComplexityExpression EstimateLoopIterations(ForStatementSyntax forLoop)
        {
            // Simple heuristic: check condition for bound
            if (forLoop.Condition is BinaryExpressionSyntax binary)
            {
                var right = binary.Right;
                if (right is IdentifierNameSyntax id)
                {
                    var symbol = _semanticModel.GetSymbolInfo(id).Symbol;
                    if (symbol is IParameterSymbol param)
                    {
                        var variable = _context.GetVariable(param) ?? _context.InferParameterVariable(param);
                        return new VariableComplexity(variable);
                    }
                }
                if (right is MemberAccessExpressionSyntax member &&
                    member.Name.Identifier.Text is "Length" or "Count")
                {
                    return new VariableComplexity(Variable.N);
                }
                if (right is LiteralExpressionSyntax lit && lit.Token.Value is int val)
                {
                    return new ConstantComplexity(val);
                }
            }
            return new VariableComplexity(Variable.N);
        }

        private ComplexityExpression GetCurrentIterationCount()
        {
            if (_loopIterations.Count == 0)
                return ConstantComplexity.One;

            // Multiply all nested loop iterations
            return _loopIterations.Aggregate(
                ConstantComplexity.One as ComplexityExpression,
                (acc, iter) => new BinaryOperationComplexity(acc, BinaryOp.Multiply, iter));
        }

        private ComplexityExpression EstimateCollectionSize(ObjectCreationExpressionSyntax node, ITypeSymbol? type)
        {
            // Check for capacity argument: new List<int>(n)
            if (node.ArgumentList?.Arguments.Count > 0)
            {
                var firstArg = node.ArgumentList.Arguments[0].Expression;

                // Check if it's a collection being copied
                var argType = _semanticModel.GetTypeInfo(firstArg).Type;
                if (argType != null && IsCollectionType(argType))
                {
                    return new VariableComplexity(Variable.N);
                }

                // Check for capacity
                return AnalyzeSizeExpression(firstArg);
            }

            // Check for initializer: new List<int> { 1, 2, 3 }
            if (node.Initializer != null)
            {
                return new ConstantComplexity(node.Initializer.Expressions.Count);
            }

            // Default: unknown size, could grow to O(n)
            return ConstantComplexity.One;
        }

        private static bool IsCollectionType(ITypeSymbol? type)
        {
            if (type == null) return false;

            var typeName = type.ToDisplayString();
            return typeName.StartsWith("System.Collections") ||
                   typeName.Contains("List<") ||
                   typeName.Contains("Dictionary<") ||
                   typeName.Contains("HashSet<") ||
                   typeName.Contains("Queue<") ||
                   typeName.Contains("Stack<") ||
                   typeName.Contains("[]");
        }

        private static bool IsAllocatingMethod(string methodName)
        {
            return methodName is
                "ToList" or "ToArray" or "ToDictionary" or "ToHashSet" or
                "Clone" or "Copy" or "CopyTo" or
                "Concat" or "Union" or "Intersect" or "Except" or
                "Select" or "Where" or "OrderBy" or "GroupBy" or
                "Reverse" or "Take" or "Skip" or "Distinct" or
                "Split" or "Substring" or "ToString" or
                "GetRange" or "Slice";
        }

        private ComplexityExpression EstimateMethodAllocationSize(IMethodSymbol method, InvocationExpressionSyntax node)
        {
            var methodName = method.Name;

            // ToList/ToArray typically creates copy of input
            if (methodName is "ToList" or "ToArray" or "ToDictionary" or "ToHashSet")
            {
                return new VariableComplexity(Variable.N);
            }

            // Clone creates a copy
            if (methodName is "Clone" or "Copy")
            {
                return new VariableComplexity(Variable.N);
            }

            // String operations
            if (methodName is "Split")
            {
                return new VariableComplexity(Variable.N);
            }

            if (methodName is "Substring" or "ToString")
            {
                return new VariableComplexity(Variable.N);
            }

            // LINQ operations that materialize
            if (methodName is "Concat" or "Union" or "Intersect" or "Except")
            {
                return new VariableComplexity(Variable.N);
            }

            if (methodName is "Select" or "Where" or "OrderBy" or "GroupBy")
            {
                // Deferred execution, but when materialized...
                return new VariableComplexity(Variable.N);
            }

            return ConstantComplexity.One;
        }

        private static string GetSimpleTypeName(string fullTypeName)
        {
            var lastDot = fullTypeName.LastIndexOf('.');
            if (lastDot >= 0)
                return fullTypeName.Substring(lastDot + 1);

            var genericIndex = fullTypeName.IndexOf('<');
            if (genericIndex >= 0)
                return fullTypeName.Substring(0, genericIndex);

            return fullTypeName;
        }
    }

    #endregion
}

/// <summary>
/// Result of recursion analysis.
/// </summary>
public sealed record RecursionAnalysisResult
{
    public bool IsRecursive { get; init; }
    public ComplexityExpression DepthComplexity { get; init; } = ConstantComplexity.Zero;
    public bool IsTailRecursive { get; init; }
    public int RecursiveCallCount { get; init; }
    public RecursionPattern Pattern { get; init; } = RecursionPattern.None;

    public static RecursionAnalysisResult NotRecursive => new()
    {
        IsRecursive = false,
        DepthComplexity = ConstantComplexity.Zero,
        Pattern = RecursionPattern.None
    };
}

/// <summary>
/// Patterns of recursion.
/// </summary>
public enum RecursionPattern
{
    /// <summary>
    /// No recursion.
    /// </summary>
    None,

    /// <summary>
    /// Single recursive call with n-1 or similar.
    /// </summary>
    Linear,

    /// <summary>
    /// Single recursive call with n/k.
    /// </summary>
    DivideByConstant,

    /// <summary>
    /// Single recursive call decrementing by constant.
    /// </summary>
    DecrementByConstant,

    /// <summary>
    /// Two calls with halving (like merge sort).
    /// </summary>
    DivideAndConquer,

    /// <summary>
    /// Two calls without halving (like Fibonacci).
    /// </summary>
    TreeRecursion,

    /// <summary>
    /// More than two recursive calls.
    /// </summary>
    Multiple
}

/// <summary>
/// Result of allocation analysis.
/// </summary>
public sealed record AllocationAnalysisResult
{
    public ImmutableList<AllocationInfo> Allocations { get; init; } = ImmutableList<AllocationInfo>.Empty;
    public ComplexityExpression TotalHeapSpace { get; init; } = ConstantComplexity.Zero;
}

/// <summary>
/// Extension methods for memory analysis.
/// </summary>
public static class MemoryAnalysisExtensions
{
    /// <summary>
    /// Analyzes a method for both time and space complexity.
    /// </summary>
    public static ComplexityAnalysisResult AnalyzeComplete(
        this RoslynComplexityExtractor extractor,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel)
    {
        var timeComplexity = extractor.AnalyzeMethod(method);

        var memoryAnalyzer = new MemoryAnalyzer(semanticModel);
        var context = new AnalysisContext { SemanticModel = semanticModel };
        var spaceComplexity = memoryAnalyzer.AnalyzeMethod(method, context);

        return new ComplexityAnalysisResult
        {
            TimeComplexity = timeComplexity,
            SpaceComplexity = spaceComplexity,
            Name = method.Identifier.Text,
            Confidence = ComputeConfidence(timeComplexity, spaceComplexity)
        };
    }

    private static double ComputeConfidence(ComplexityExpression time, MemoryComplexity space)
    {
        var timeConfidence = time switch
        {
            ConstantComplexity => 1.0,
            VariableComplexity => 0.95,
            LinearComplexity => 0.9,
            LogarithmicComplexity => 0.9,
            PolynomialComplexity => 0.85,
            PolyLogComplexity => 0.85,
            _ => 0.7
        };

        var spaceConfidence = space.TotalSpace switch
        {
            ConstantComplexity => 1.0,
            VariableComplexity => 0.9,
            LinearComplexity => 0.85,
            LogarithmicComplexity => 0.9,
            _ => 0.7
        };

        return (timeConfidence + spaceConfidence) / 2;
    }
}
