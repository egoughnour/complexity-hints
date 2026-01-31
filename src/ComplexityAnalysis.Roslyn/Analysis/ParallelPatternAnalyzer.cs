using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Analyzes code patterns to detect parallel complexity scenarios.
/// 
/// Detects patterns like:
/// - Parallel.For / Parallel.ForEach (data parallelism)
/// - PLINQ (AsParallel, parallel LINQ)
/// - Task.Run / Task.WhenAll / Task.WhenAny (task parallelism)
/// - async/await patterns
/// - Parallel invoke
/// </summary>
public sealed class ParallelPatternAnalyzer
{
    private readonly SemanticModel _semanticModel;
    private readonly AnalysisContext? _context;

    public ParallelPatternAnalyzer(SemanticModel semanticModel, AnalysisContext? context = null)
    {
        _semanticModel = semanticModel;
        _context = context;
    }

    /// <summary>
    /// Analyzes a method for parallel complexity patterns.
    /// Returns a ParallelComplexity if a parallel pattern is detected,
    /// or null if no parallel pattern is found.
    /// </summary>
    public ParallelComplexity? AnalyzeMethod(MethodDeclarationSyntax method)
    {
        var patterns = new List<DetectedPattern>();

        // Check for Parallel.For / ForEach patterns
        var parallelForPattern = DetectParallelForPattern(method);
        if (parallelForPattern != null)
            patterns.Add(parallelForPattern);

        // Check for PLINQ patterns
        var plinqPattern = DetectPLINQPattern(method);
        if (plinqPattern != null)
            patterns.Add(plinqPattern);

        // Check for Task.WhenAll / WhenAny patterns
        var taskWhenPattern = DetectTaskWhenPattern(method);
        if (taskWhenPattern != null)
            patterns.Add(taskWhenPattern);

        // Check for Task.Run patterns
        var taskRunPattern = DetectTaskRunPattern(method);
        if (taskRunPattern != null)
            patterns.Add(taskRunPattern);

        // Check for Parallel.Invoke patterns
        var parallelInvokePattern = DetectParallelInvokePattern(method);
        if (parallelInvokePattern != null)
            patterns.Add(parallelInvokePattern);

        // Check for async/await patterns
        var asyncPattern = DetectAsyncAwaitPattern(method);
        if (asyncPattern != null)
            patterns.Add(asyncPattern);

        // Return the pattern with highest priority
        var bestPattern = patterns.OrderByDescending(p => p.Priority).FirstOrDefault();
        return bestPattern?.ToParallelComplexity();
    }

    /// <summary>
    /// Analyzes a block of code for parallel patterns.
    /// </summary>
    public ParallelComplexity? AnalyzeBlock(BlockSyntax block)
    {
        var patterns = new List<DetectedPattern>();

        // Create a temporary walker to analyze the block
        foreach (var node in block.DescendantNodes())
        {
            if (node is InvocationExpressionSyntax invocation)
            {
                var pattern = AnalyzeInvocation(invocation);
                if (pattern != null)
                    patterns.Add(pattern);
            }
            else if (node is AwaitExpressionSyntax awaitExpr)
            {
                var pattern = AnalyzeAwait(awaitExpr);
                if (pattern != null)
                    patterns.Add(pattern);
            }
        }

        return patterns.OrderByDescending(p => p.Priority).FirstOrDefault()?.ToParallelComplexity();
    }

    #region Pattern Detection

    /// <summary>
    /// Detects Parallel.For and Parallel.ForEach patterns.
    /// </summary>
    private DetectedPattern? DetectParallelForPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var methodName = memberAccess.Name.Identifier.Text;
            var expression = memberAccess.Expression.ToString();

            // Check for Parallel.For / Parallel.ForEach
            if (expression is "Parallel" or "System.Threading.Tasks.Parallel")
            {
                if (methodName is "For" or "ForEach" or "ForEachAsync")
                {
                    return AnalyzeParallelForInvocation(invocation, methodName);
                }
            }
        }

        return null;
    }

    private DetectedPattern AnalyzeParallelForInvocation(InvocationExpressionSyntax invocation, string methodName)
    {
        var args = invocation.ArgumentList.Arguments;
        
        // Try to determine iteration count from arguments
        Variable iterVar = Variable.N;
        ComplexityExpression iterCount = new VariableComplexity(iterVar);

        // For Parallel.For(fromInclusive, toExclusive, body)
        if (methodName == "For" && args.Count >= 3)
        {
            // Try to infer range
            var fromExpr = args[0].Expression.ToString();
            var toExpr = args[1].Expression.ToString();

            // Common patterns: For(0, n, ...) or For(0, array.Length, ...)
            if (fromExpr == "0" && (toExpr.Contains(".Length") || toExpr.Contains(".Count") || 
                char.IsLetter(toExpr.FirstOrDefault())))
            {
                iterCount = new VariableComplexity(iterVar);
            }
        }
        // For Parallel.ForEach(source, body)
        else if (methodName is "ForEach" or "ForEachAsync" && args.Count >= 2)
        {
            iterCount = new VariableComplexity(iterVar);
        }

        // Analyze the loop body complexity
        var bodyComplexity = AnalyzeParallelLoopBody(invocation);

        // Work = iterations × body complexity
        var work = new BinaryOperationComplexity(iterCount, BinaryOp.Multiply, bodyComplexity);

        // Span depends on whether iterations are independent
        ComplexityExpression span;
        if (DetectDependentIterations(invocation))
        {
            // If iterations have dependencies, span could be O(n)
            span = iterCount;
        }
        else
        {
            // Embarrassingly parallel: span = body complexity
            span = bodyComplexity;
        }

        return new DetectedPattern
        {
            PatternType = ParallelPatternType.ParallelFor,
            Work = work,
            Span = span,
            Priority = 10,
            Description = $"Parallel.{methodName} - data parallelism over {iterCount.ToBigONotation()} items"
        };
    }

    /// <summary>
    /// Detects PLINQ patterns (AsParallel(), parallel LINQ).
    /// </summary>
    private DetectedPattern? DetectPLINQPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

        // Look for .AsParallel()
        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var methodName = memberAccess.Name.Identifier.Text;
            
            if (methodName == "AsParallel")
            {
                // Find the chain of LINQ operations after AsParallel
                var linqChain = AnalyzePLINQChain(invocation);
                return linqChain;
            }
        }

        // Also check for ParallelEnumerable static methods
        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var expression = memberAccess.Expression.ToString();
            if (expression is "ParallelEnumerable" or "System.Linq.ParallelEnumerable")
            {
                return AnalyzeParallelEnumerableCall(invocation, memberAccess.Name.Identifier.Text);
            }
        }

        return null;
    }

    private DetectedPattern AnalyzePLINQChain(InvocationExpressionSyntax asParallelInvocation)
    {
        // Walk up the expression tree to find the full LINQ chain
        var chainOps = new List<string>();
        var current = asParallelInvocation.Parent;

        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Parent is InvocationExpressionSyntax parentInvocation)
            {
                chainOps.Add(memberAccess.Name.Identifier.Text);
                current = parentInvocation.Parent;
            }
            else
            {
                break;
            }
        }

        // Calculate work and span based on operations
        var n = Variable.N;
        ComplexityExpression work = new VariableComplexity(n);
        ComplexityExpression span = ConstantComplexity.One;

        bool hasOrdering = false;
        bool hasAggregation = false;

        foreach (var op in chainOps)
        {
            switch (op)
            {
                case "Where":
                case "Select":
                case "SelectMany":
                    // Filter/project: Work O(n), Span O(1) per element
                    break;

                case "OrderBy":
                case "OrderByDescending":
                case "ThenBy":
                case "ThenByDescending":
                    hasOrdering = true;
                    work = PolyLogComplexity.NLogN(n);
                    span = new BinaryOperationComplexity(
                        new LogarithmicComplexity(1.0, n),
                        BinaryOp.Multiply,
                        new LogarithmicComplexity(1.0, n)); // O(log² n)
                    break;

                case "Aggregate":
                case "Sum":
                case "Average":
                case "Min":
                case "Max":
                case "Count":
                    hasAggregation = true;
                    if (!hasOrdering)
                    {
                        work = new VariableComplexity(n);
                        span = new LogarithmicComplexity(1.0, n); // Tree reduction
                    }
                    break;

                case "GroupBy":
                    work = new VariableComplexity(n);
                    span = new BinaryOperationComplexity(
                        new VariableComplexity(new Variable("g", VariableType.Custom) { Description = "Number of groups" }),
                        BinaryOp.Plus,
                        new LogarithmicComplexity(1.0, n));
                    break;

                case "Join":
                case "GroupJoin":
                    var m = Variable.M;
                    work = new BinaryOperationComplexity(
                        new VariableComplexity(n),
                        BinaryOp.Plus,
                        new VariableComplexity(m));
                    span = new LogarithmicComplexity(1.0, n);
                    break;

                case "Distinct":
                    work = new VariableComplexity(n);
                    span = new LogarithmicComplexity(1.0, n);
                    break;

                case "AsOrdered":
                    // Preserves ordering, increases overhead
                    hasOrdering = true;
                    break;

                case "AsUnordered":
                    // Allows more parallelism
                    hasOrdering = false;
                    break;
            }
        }

        var description = chainOps.Count > 0
            ? $"PLINQ chain: AsParallel().{string.Join("().", chainOps)}()"
            : "PLINQ with AsParallel()";

        return new DetectedPattern
        {
            PatternType = ParallelPatternType.PLINQ,
            Work = work,
            Span = span,
            HasSynchronization = hasOrdering || hasAggregation,
            Priority = 9,
            Description = description
        };
    }

    private DetectedPattern AnalyzeParallelEnumerableCall(InvocationExpressionSyntax invocation, string methodName)
    {
        var n = Variable.N;

        return methodName switch
        {
            "Range" or "Repeat" => new DetectedPattern
            {
                PatternType = ParallelPatternType.PLINQ,
                Work = new VariableComplexity(n),
                Span = ConstantComplexity.One,
                Priority = 8,
                Description = $"ParallelEnumerable.{methodName} - parallel generation"
            },
            "Aggregate" => new DetectedPattern
            {
                PatternType = ParallelPatternType.PLINQ,
                Work = new VariableComplexity(n),
                Span = new LogarithmicComplexity(1.0, n),
                Priority = 9,
                Description = "ParallelEnumerable.Aggregate - parallel reduction"
            },
            _ => new DetectedPattern
            {
                PatternType = ParallelPatternType.PLINQ,
                Work = new VariableComplexity(n),
                Span = ConstantComplexity.One,
                Priority = 7,
                Description = $"ParallelEnumerable.{methodName}"
            }
        };
    }

    /// <summary>
    /// Detects Task.WhenAll / Task.WhenAny patterns.
    /// </summary>
    private DetectedPattern? DetectTaskWhenPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var methodName = memberAccess.Name.Identifier.Text;
            var expression = memberAccess.Expression.ToString();

            if (expression is "Task" or "System.Threading.Tasks.Task")
            {
                if (methodName == "WhenAll")
                {
                    return AnalyzeTaskWhenAll(invocation);
                }
                else if (methodName == "WhenAny")
                {
                    return AnalyzeTaskWhenAny(invocation);
                }
            }
        }

        return null;
    }

    private DetectedPattern AnalyzeTaskWhenAll(InvocationExpressionSyntax invocation)
    {
        var args = invocation.ArgumentList.Arguments;
        
        // Estimate number of tasks
        int taskCount = 1;
        ComplexityExpression taskCountExpr;

        if (args.Count == 1)
        {
            var arg = args[0].Expression;
            if (arg is ArrayCreationExpressionSyntax arrayCreate)
            {
                taskCount = arrayCreate.Initializer?.Expressions.Count ?? 1;
                taskCountExpr = new ConstantComplexity(taskCount);
            }
            else if (arg is ImplicitArrayCreationExpressionSyntax implicitArray)
            {
                taskCount = implicitArray.Initializer.Expressions.Count;
                taskCountExpr = new ConstantComplexity(taskCount);
            }
            else if (arg is IdentifierNameSyntax || arg is MemberAccessExpressionSyntax)
            {
                // Collection of tasks - assume variable count
                taskCountExpr = new VariableComplexity(Variable.N);
            }
            else
            {
                taskCountExpr = new VariableComplexity(Variable.N);
            }
        }
        else
        {
            // Multiple task arguments
            taskCount = args.Count;
            taskCountExpr = new ConstantComplexity(taskCount);
        }

        // For WhenAll: Work = sum of all task work, Span = max task work
        // Simplified: Work = O(n × task_work), Span = O(task_work) if similar tasks
        var taskWork = ConstantComplexity.One; // Assume O(1) per task unless we can infer more

        return new DetectedPattern
        {
            PatternType = ParallelPatternType.TaskBased,
            Work = new BinaryOperationComplexity(taskCountExpr, BinaryOp.Multiply, taskWork),
            Span = taskWork, // Max of all tasks
            IsTaskBased = true,
            Priority = 8,
            Description = $"Task.WhenAll - awaiting {(taskCount > 1 ? $"{taskCount} tasks" : "multiple tasks")} in parallel"
        };
    }

    private DetectedPattern AnalyzeTaskWhenAny(InvocationExpressionSyntax invocation)
    {
        return new DetectedPattern
        {
            PatternType = ParallelPatternType.TaskBased,
            Work = ConstantComplexity.One, // Only one task needs to complete
            Span = ConstantComplexity.One, // Minimum task time
            IsTaskBased = true,
            Priority = 7,
            Description = "Task.WhenAny - awaiting first task to complete"
        };
    }

    /// <summary>
    /// Detects Task.Run patterns.
    /// </summary>
    private DetectedPattern? DetectTaskRunPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var taskRunCalls = new List<InvocationExpressionSyntax>();
        var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var methodName = memberAccess.Name.Identifier.Text;
            var expression = memberAccess.Expression.ToString();

            if (expression is "Task" or "System.Threading.Tasks.Task" &&
                methodName is "Run" or "Factory.StartNew")
            {
                taskRunCalls.Add(invocation);
            }
        }

        if (taskRunCalls.Count == 0) return null;

        // Multiple Task.Run calls indicate parallel execution
        if (taskRunCalls.Count > 1)
        {
            return new DetectedPattern
            {
                PatternType = ParallelPatternType.TaskBased,
                Work = new ConstantComplexity(taskRunCalls.Count), // Total work
                Span = ConstantComplexity.One, // Parallel execution
                IsTaskBased = true,
                Priority = 6,
                Description = $"Multiple Task.Run calls ({taskRunCalls.Count} tasks)"
            };
        }

        return new DetectedPattern
        {
            PatternType = ParallelPatternType.TaskBased,
            Work = ConstantComplexity.One,
            Span = ConstantComplexity.One,
            IsTaskBased = true,
            Priority = 5,
            Description = "Task.Run - offloading work to thread pool"
        };
    }

    /// <summary>
    /// Detects Parallel.Invoke patterns.
    /// </summary>
    private DetectedPattern? DetectParallelInvokePattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>();

        foreach (var invocation in invocations)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess == null) continue;

            var methodName = memberAccess.Name.Identifier.Text;
            var expression = memberAccess.Expression.ToString();

            if (expression is "Parallel" or "System.Threading.Tasks.Parallel" &&
                methodName == "Invoke")
            {
                var actionCount = invocation.ArgumentList.Arguments.Count;

                return new DetectedPattern
                {
                    PatternType = ParallelPatternType.ForkJoin,
                    Work = new ConstantComplexity(actionCount),
                    Span = ConstantComplexity.One, // All run in parallel
                    Priority = 8,
                    Description = $"Parallel.Invoke - fork-join pattern with {actionCount} actions"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Detects async/await patterns in async methods.
    /// </summary>
    private DetectedPattern? DetectAsyncAwaitPattern(MethodDeclarationSyntax method)
    {
        // Check if method is async
        if (!method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
            return null;

        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var awaitExpressions = body.DescendantNodes().OfType<AwaitExpressionSyntax>().ToList();
        
        if (awaitExpressions.Count == 0) return null;

        // Check if awaits are concurrent (Task.WhenAll) or sequential
        bool hasWhenAll = body.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(i => i.ToString().Contains("WhenAll"));

        if (hasWhenAll)
        {
            // Already handled by TaskWhenPattern
            return null;
        }

        // Sequential awaits - not truly parallel, but async
        return new DetectedPattern
        {
            PatternType = ParallelPatternType.AsyncAwait,
            Work = new ConstantComplexity(awaitExpressions.Count),
            Span = new ConstantComplexity(awaitExpressions.Count), // Sequential
            IsTaskBased = true,
            HasSynchronization = true,
            Priority = 3, // Lower priority - sequential async
            Description = $"Async method with {awaitExpressions.Count} sequential await(s)"
        };
    }

    #endregion

    #region Helper Methods

    private DetectedPattern? AnalyzeInvocation(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return null;

        var methodName = memberAccess.Name.Identifier.Text;
        var expression = memberAccess.Expression.ToString();

        // Check for Parallel methods
        if (expression is "Parallel" or "System.Threading.Tasks.Parallel")
        {
            return methodName switch
            {
                "For" or "ForEach" or "ForEachAsync" => AnalyzeParallelForInvocation(invocation, methodName),
                "Invoke" => new DetectedPattern
                {
                    PatternType = ParallelPatternType.ForkJoin,
                    Work = new ConstantComplexity(invocation.ArgumentList.Arguments.Count),
                    Span = ConstantComplexity.One,
                    Priority = 8,
                    Description = "Parallel.Invoke"
                },
                _ => null
            };
        }

        // Check for Task methods
        if (expression is "Task" or "System.Threading.Tasks.Task")
        {
            return methodName switch
            {
                "WhenAll" => AnalyzeTaskWhenAll(invocation),
                "WhenAny" => AnalyzeTaskWhenAny(invocation),
                _ => null
            };
        }

        // Check for PLINQ
        if (methodName == "AsParallel")
        {
            return AnalyzePLINQChain(invocation);
        }

        return null;
    }

    private DetectedPattern? AnalyzeAwait(AwaitExpressionSyntax awaitExpr)
    {
        // Check what is being awaited
        var awaited = awaitExpr.Expression;

        if (awaited is InvocationExpressionSyntax invocation)
        {
            return AnalyzeInvocation(invocation);
        }

        return null;
    }

    private ComplexityExpression AnalyzeParallelLoopBody(InvocationExpressionSyntax parallelCall)
    {
        var args = parallelCall.ArgumentList.Arguments;
        
        // Find the lambda/delegate body
        foreach (var arg in args)
        {
            if (arg.Expression is SimpleLambdaExpressionSyntax simpleLambda)
            {
                return AnalyzeLambdaBody(simpleLambda.Body);
            }
            if (arg.Expression is ParenthesizedLambdaExpressionSyntax parenLambda)
            {
                return AnalyzeLambdaBody(parenLambda.Body);
            }
        }

        // Default to O(1) if we can't determine
        return ConstantComplexity.One;
    }

    private ComplexityExpression AnalyzeLambdaBody(CSharpSyntaxNode body)
    {
        // Simple heuristic: count nested loops
        int loopDepth = 0;
        foreach (var node in body.DescendantNodesAndSelf())
        {
            if (node is ForStatementSyntax or ForEachStatementSyntax or 
                WhileStatementSyntax or DoStatementSyntax)
            {
                loopDepth++;
            }
        }

        if (loopDepth == 0) return ConstantComplexity.One;
        if (loopDepth == 1) return new VariableComplexity(Variable.N);
        
        return PolynomialComplexity.OfDegree(loopDepth, Variable.N);
    }

    private bool DetectDependentIterations(InvocationExpressionSyntax parallelCall)
    {
        var args = parallelCall.ArgumentList.Arguments;

        foreach (var arg in args)
        {
            SyntaxNode? body = null;
            
            if (arg.Expression is SimpleLambdaExpressionSyntax simpleLambda)
                body = simpleLambda.Body;
            else if (arg.Expression is ParenthesizedLambdaExpressionSyntax parenLambda)
                body = parenLambda.Body;

            if (body == null) continue;

            // Look for signs of shared state / dependencies
            var identifiers = body.DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var id in identifiers)
            {
                var name = id.Identifier.Text.ToLowerInvariant();
                // Shared state indicators
                if (name.Contains("lock") || name.Contains("mutex") || 
                    name.Contains("semaphore") || name.Contains("interlocked") ||
                    name.Contains("concurrent"))
                {
                    return true;
                }
            }

            // Check for Interlocked calls
            var invocations = body.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var text = invocation.ToString().ToLowerInvariant();
                if (text.Contains("interlocked") || text.Contains("lock"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Helper Types

    private sealed record DetectedPattern
    {
        public required ParallelPatternType PatternType { get; init; }
        public required ComplexityExpression Work { get; init; }
        public required ComplexityExpression Span { get; init; }
        public bool IsTaskBased { get; init; }
        public bool HasSynchronization { get; init; }
        public int Priority { get; init; }
        public string? Description { get; init; }

        public ParallelComplexity ToParallelComplexity() =>
            new()
            {
                Work = Work,
                Span = Span,
                PatternType = PatternType,
                IsTaskBased = IsTaskBased,
                HasSynchronizationOverhead = HasSynchronization,
                Description = Description
            };
    }

    #endregion
}

/// <summary>
/// Extension methods for parallel pattern analysis.
/// </summary>
public static class ParallelAnalysisExtensions
{
    /// <summary>
    /// Analyzes a method with parallel complexity detection.
    /// Returns ParallelComplexity if a pattern is detected, otherwise falls back to sequential analysis.
    /// </summary>
    public static ComplexityExpression AnalyzeWithParallelism(
        this RoslynComplexityExtractor extractor,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel,
        AnalysisContext? context = null)
    {
        var parallelAnalyzer = new ParallelPatternAnalyzer(semanticModel, context);
        var parallel = parallelAnalyzer.AnalyzeMethod(method);

        if (parallel != null)
        {
            return parallel;
        }

        // Fall back to regular analysis
        return extractor.AnalyzeMethod(method);
    }

    /// <summary>
    /// Determines if a method contains any parallel patterns.
    /// </summary>
    public static bool ContainsParallelPatterns(this MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return false;

        var text = body.ToString().ToLowerInvariant();

        return text.Contains("parallel.") ||
               text.Contains("asparallel") ||
               text.Contains("task.whenall") ||
               text.Contains("task.whenany") ||
               (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)) &&
                text.Contains("await"));
    }

    /// <summary>
    /// Gets a summary of parallel patterns in a method.
    /// </summary>
    public static ParallelPatternSummary GetParallelPatternSummary(
        this MethodDeclarationSyntax method,
        SemanticModel semanticModel)
    {
        var analyzer = new ParallelPatternAnalyzer(semanticModel);
        var complexity = analyzer.AnalyzeMethod(method);

        return new ParallelPatternSummary
        {
            HasParallelPatterns = complexity != null,
            PatternType = complexity?.PatternType ?? ParallelPatternType.Generic,
            Work = complexity?.Work ?? ConstantComplexity.One,
            Span = complexity?.Span ?? ConstantComplexity.One,
            Description = complexity?.Description
        };
    }
}

/// <summary>
/// Summary of parallel patterns in a method.
/// </summary>
public sealed record ParallelPatternSummary
{
    public bool HasParallelPatterns { get; init; }
    public ParallelPatternType PatternType { get; init; }
    public required ComplexityExpression Work { get; init; }
    public required ComplexityExpression Span { get; init; }
    public string? Description { get; init; }
}
