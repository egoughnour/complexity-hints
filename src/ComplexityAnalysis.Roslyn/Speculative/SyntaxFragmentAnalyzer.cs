using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Analyzes syntax fragments, including incomplete code during active editing.
/// Provides best-effort complexity estimates with confidence values.
/// </summary>
public sealed class SyntaxFragmentAnalyzer
{
    /// <summary>
    /// Analyzes a method, handling incomplete syntax gracefully.
    /// </summary>
    public MethodAnalysisSnapshot AnalyzeMethod(
        MethodDeclarationSyntax method,
        bool isKnownIncomplete = false)
    {
        var hasBody = method.Body is not null || method.ExpressionBody is not null;

        if (!hasBody)
        {
            return new MethodAnalysisSnapshot
            {
                Complexity = null,
                Confidence = 0.0,
                IsComplete = false,
                IncompleteReason = "Method has no body"
            };
        }

        var loops = AnalyzeLoops(method);
        var recursiveCalls = DetectRecursiveCalls(method);
        var (complexity, confidence) = EstimateMethodComplexity(method, loops, recursiveCalls, isKnownIncomplete);

        return new MethodAnalysisSnapshot
        {
            Complexity = complexity,
            Confidence = confidence,
            IsComplete = !isKnownIncomplete,
            IncompleteReason = isKnownIncomplete ? "Code is being edited" : null,
            Loops = loops,
            RecursiveCalls = recursiveCalls
        };
    }

    /// <summary>
    /// Analyzes a single statement, useful for incremental updates.
    /// </summary>
    public StatementAnalysisResult AnalyzeStatement(StatementSyntax statement)
    {
        return statement switch
        {
            ForStatementSyntax forStmt => AnalyzeForStatement(forStmt),
            ForEachStatementSyntax foreachStmt => AnalyzeForEachStatement(foreachStmt),
            WhileStatementSyntax whileStmt => AnalyzeWhileStatement(whileStmt),
            DoStatementSyntax doStmt => AnalyzeDoStatement(doStmt),
            IfStatementSyntax ifStmt => AnalyzeIfStatement(ifStmt),
            BlockSyntax block => AnalyzeBlock(block),
            ReturnStatementSyntax ret => AnalyzeReturn(ret),
            ExpressionStatementSyntax expr => AnalyzeExpressionStatement(expr),
            _ => new StatementAnalysisResult
            {
                Complexity = ConstantComplexity.One,
                Confidence = 0.9,
                IsComplete = true
            }
        };
    }

    #region Loop Analysis

    private IReadOnlyList<LoopSnapshot> AnalyzeLoops(MethodDeclarationSyntax method)
    {
        var loops = new List<LoopSnapshot>();
        var body = (SyntaxNode?)method.Body ?? method.ExpressionBody;

        if (body is null)
            return loops;

        foreach (var node in body.DescendantNodes())
        {
            LoopSnapshot? loop = node switch
            {
                ForStatementSyntax forStmt => AnalyzeForLoop(forStmt),
                ForEachStatementSyntax foreachStmt => AnalyzeForEachLoop(foreachStmt),
                WhileStatementSyntax whileStmt => AnalyzeWhileLoop(whileStmt),
                DoStatementSyntax doStmt => AnalyzeDoWhileLoop(doStmt),
                _ => null
            };

            if (loop is not null)
                loops.Add(loop);
        }

        return loops;
    }

    private LoopSnapshot AnalyzeForLoop(ForStatementSyntax forStmt)
    {
        var iterationCount = InferForLoopIterations(forStmt);
        var bodyComplexity = forStmt.Statement is not null
            ? AnalyzeStatement(forStmt.Statement).Complexity
            : null;

        return new LoopSnapshot
        {
            LoopType = "for",
            IterationCount = iterationCount,
            BodyComplexity = bodyComplexity,
            IsComplete = forStmt.Statement is not EmptyStatementSyntax
        };
    }

    private LoopSnapshot AnalyzeForEachLoop(ForEachStatementSyntax foreachStmt)
    {
        // Collection size typically linear
        var iterationCount = new VariableComplexity(Variable.N);
        var bodyComplexity = foreachStmt.Statement is not null
            ? AnalyzeStatement(foreachStmt.Statement).Complexity
            : null;

        return new LoopSnapshot
        {
            LoopType = "foreach",
            IterationCount = iterationCount,
            BodyComplexity = bodyComplexity,
            IsComplete = foreachStmt.Statement is not EmptyStatementSyntax
        };
    }

    private LoopSnapshot AnalyzeWhileLoop(WhileStatementSyntax whileStmt)
    {
        var iterationCount = InferWhileLoopIterations(whileStmt);
        var bodyComplexity = whileStmt.Statement is not null
            ? AnalyzeStatement(whileStmt.Statement).Complexity
            : null;

        return new LoopSnapshot
        {
            LoopType = "while",
            IterationCount = iterationCount,
            BodyComplexity = bodyComplexity,
            IsComplete = whileStmt.Statement is not EmptyStatementSyntax
        };
    }

    private LoopSnapshot AnalyzeDoWhileLoop(DoStatementSyntax doStmt)
    {
        // Similar to while loop
        var iterationCount = new VariableComplexity(Variable.N);
        var bodyComplexity = doStmt.Statement is not null
            ? AnalyzeStatement(doStmt.Statement).Complexity
            : null;

        return new LoopSnapshot
        {
            LoopType = "do-while",
            IterationCount = iterationCount,
            BodyComplexity = bodyComplexity,
            IsComplete = doStmt.Statement is not EmptyStatementSyntax
        };
    }

    private ComplexityExpression InferForLoopIterations(ForStatementSyntax forStmt)
    {
        // Check incrementor FIRST for logarithmic patterns (i *= 2, i /= 2, etc.)
        if (forStmt.Incrementors.Count > 0)
        {
            var incrementor = forStmt.Incrementors[0];

            // i *= 2, i *= k (logarithmic)
            if (incrementor is AssignmentExpressionSyntax assign &&
                assign.IsKind(SyntaxKind.MultiplyAssignmentExpression))
            {
                return new LogarithmicComplexity(1.0, Variable.N);
            }

            // i = i * 2 (logarithmic)
            if (incrementor is AssignmentExpressionSyntax simpleAssign &&
                simpleAssign.Right is BinaryExpressionSyntax multExpr &&
                multExpr.IsKind(SyntaxKind.MultiplyExpression))
            {
                return new LogarithmicComplexity(1.0, Variable.N);
            }

            // i /= 2 (logarithmic)
            if (incrementor is AssignmentExpressionSyntax divAssign &&
                divAssign.IsKind(SyntaxKind.DivideAssignmentExpression))
            {
                return new LogarithmicComplexity(1.0, Variable.N);
            }
        }

        // Try to parse standard linear patterns like: for (int i = 0; i < n; i++)
        if (forStmt.Condition is BinaryExpressionSyntax binary)
        {
            // Check for i < n, i <= n, etc.
            if (binary.IsKind(SyntaxKind.LessThanExpression) ||
                binary.IsKind(SyntaxKind.LessThanOrEqualExpression))
            {
                var bound = binary.Right;

                // If bound is a simple identifier like 'n' or 'length'
                if (bound is IdentifierNameSyntax id)
                {
                    return new VariableComplexity(new Variable(id.Identifier.Text, VariableType.InputSize));
                }

                // If bound is array.Length or collection.Count
                if (bound is MemberAccessExpressionSyntax memberAccess)
                {
                    var memberName = memberAccess.Name.Identifier.Text;
                    if (memberName is "Length" or "Count")
                    {
                        return new VariableComplexity(Variable.N);
                    }
                }

                // If bound is a numeric literal
                if (bound is LiteralExpressionSyntax literal &&
                    literal.Token.Value is int value)
                {
                    return new ConstantComplexity(value);
                }
            }

            // Check for division patterns like i < n/2 (still linear)
            if (binary.Right is BinaryExpressionSyntax innerBinary &&
                innerBinary.IsKind(SyntaxKind.DivideExpression))
            {
                return new VariableComplexity(Variable.N);
            }
        }

        // Default: assume linear
        return new VariableComplexity(Variable.N);
    }

    private ComplexityExpression InferWhileLoopIterations(WhileStatementSyntax whileStmt)
    {
        // Look for patterns in the condition
        if (whileStmt.Condition is BinaryExpressionSyntax binary)
        {
            // i < n pattern
            if (binary.IsKind(SyntaxKind.LessThanExpression) ||
                binary.IsKind(SyntaxKind.GreaterThanExpression))
            {
                // Check body for logarithmic updates
                if (HasLogarithmicUpdate(whileStmt.Statement))
                {
                    return new LogarithmicComplexity(1.0, Variable.N);
                }
                return new VariableComplexity(Variable.N);
            }

            // i != j (like in GCD - assume logarithmic)
            if (binary.IsKind(SyntaxKind.NotEqualsExpression))
            {
                return new LogarithmicComplexity(1.0, Variable.N);
            }
        }

        return new VariableComplexity(Variable.N);
    }

    private bool HasLogarithmicUpdate(StatementSyntax? statement)
    {
        if (statement is null)
            return false;

        return statement.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(a => a.IsKind(SyntaxKind.DivideAssignmentExpression) ||
                      a.IsKind(SyntaxKind.MultiplyAssignmentExpression) ||
                      a.IsKind(SyntaxKind.RightShiftAssignmentExpression));
    }

    #endregion

    #region Recursion Detection

    private IReadOnlyList<string> DetectRecursiveCalls(MethodDeclarationSyntax method)
    {
        var methodName = method.Identifier.Text;
        var calls = new List<string>();

        var body = (SyntaxNode?)method.Body ?? method.ExpressionBody;
        if (body is null)
            return calls;

        foreach (var invocation in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            string? calledName = invocation.Expression switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                _ => null
            };

            if (calledName == methodName)
            {
                // Format the call for context
                calls.Add(invocation.ToString());
            }
        }

        return calls;
    }

    #endregion

    #region Statement Analysis

    private StatementAnalysisResult AnalyzeForStatement(ForStatementSyntax forStmt)
    {
        var iterations = InferForLoopIterations(forStmt);
        var bodyResult = forStmt.Statement is not null
            ? AnalyzeStatement(forStmt.Statement)
            : new StatementAnalysisResult { Complexity = ConstantComplexity.One, Confidence = 0.5 };

        return new StatementAnalysisResult
        {
            Complexity = MultiplyComplexity(iterations, bodyResult.Complexity),
            Confidence = bodyResult.Confidence * 0.9,
            IsComplete = forStmt.Statement is not EmptyStatementSyntax
        };
    }

    private StatementAnalysisResult AnalyzeForEachStatement(ForEachStatementSyntax foreachStmt)
    {
        var bodyResult = foreachStmt.Statement is not null
            ? AnalyzeStatement(foreachStmt.Statement)
            : new StatementAnalysisResult { Complexity = ConstantComplexity.One, Confidence = 0.5 };

        return new StatementAnalysisResult
        {
            Complexity = MultiplyComplexity(new VariableComplexity(Variable.N), bodyResult.Complexity),
            Confidence = bodyResult.Confidence * 0.85, // Less certain about collection size
            IsComplete = foreachStmt.Statement is not EmptyStatementSyntax
        };
    }

    private StatementAnalysisResult AnalyzeWhileStatement(WhileStatementSyntax whileStmt)
    {
        var iterations = InferWhileLoopIterations(whileStmt);
        var bodyResult = whileStmt.Statement is not null
            ? AnalyzeStatement(whileStmt.Statement)
            : new StatementAnalysisResult { Complexity = ConstantComplexity.One, Confidence = 0.5 };

        return new StatementAnalysisResult
        {
            Complexity = MultiplyComplexity(iterations, bodyResult.Complexity),
            Confidence = bodyResult.Confidence * 0.7, // While loops are less predictable
            IsComplete = whileStmt.Statement is not EmptyStatementSyntax
        };
    }

    private StatementAnalysisResult AnalyzeDoStatement(DoStatementSyntax doStmt)
    {
        var bodyResult = doStmt.Statement is not null
            ? AnalyzeStatement(doStmt.Statement)
            : new StatementAnalysisResult { Complexity = ConstantComplexity.One, Confidence = 0.5 };

        return new StatementAnalysisResult
        {
            Complexity = MultiplyComplexity(new VariableComplexity(Variable.N), bodyResult.Complexity),
            Confidence = bodyResult.Confidence * 0.7,
            IsComplete = doStmt.Statement is not EmptyStatementSyntax
        };
    }

    private StatementAnalysisResult AnalyzeIfStatement(IfStatementSyntax ifStmt)
    {
        var thenResult = AnalyzeStatement(ifStmt.Statement);
        var elseResult = ifStmt.Else?.Statement is not null
            ? AnalyzeStatement(ifStmt.Else.Statement)
            : null;

        // Take max of branches
        var complexity = elseResult is not null
            ? MaxComplexity(thenResult.Complexity, elseResult.Complexity)
            : thenResult.Complexity;

        return new StatementAnalysisResult
        {
            Complexity = complexity,
            Confidence = elseResult is not null
                ? Math.Min(thenResult.Confidence, elseResult.Confidence)
                : thenResult.Confidence,
            IsComplete = true
        };
    }

    private StatementAnalysisResult AnalyzeBlock(BlockSyntax block)
    {
        if (block.Statements.Count == 0)
        {
            return new StatementAnalysisResult
            {
                Complexity = ConstantComplexity.One,
                Confidence = 1.0,
                IsComplete = true
            };
        }

        ComplexityExpression total = ConstantComplexity.One;
        double minConfidence = 1.0;

        foreach (var stmt in block.Statements)
        {
            var result = AnalyzeStatement(stmt);
            total = AddComplexity(total, result.Complexity);
            minConfidence = Math.Min(minConfidence, result.Confidence);
        }

        return new StatementAnalysisResult
        {
            Complexity = total,
            Confidence = minConfidence,
            IsComplete = true
        };
    }

    private StatementAnalysisResult AnalyzeReturn(ReturnStatementSyntax ret)
    {
        // Check for recursive return
        if (ret.Expression is InvocationExpressionSyntax)
        {
            // Might be a recursive call, but we handle that separately
            return new StatementAnalysisResult
            {
                Complexity = ConstantComplexity.One,
                Confidence = 0.8,
                IsComplete = true
            };
        }

        return new StatementAnalysisResult
        {
            Complexity = ConstantComplexity.One,
            Confidence = 1.0,
            IsComplete = true
        };
    }

    private StatementAnalysisResult AnalyzeExpressionStatement(ExpressionStatementSyntax expr)
    {
        // Check for method calls that might have complexity
        if (expr.Expression is InvocationExpressionSyntax invocation)
        {
            var methodName = invocation.Expression switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                _ => null
            };

            // Well-known linear operations
            if (methodName is "Sort" or "Reverse" or "ToList" or "ToArray")
            {
                return new StatementAnalysisResult
                {
                    Complexity = PolyLogComplexity.NLogN(), // n log n for Sort
                    Confidence = 0.9,
                    IsComplete = true
                };
            }

            if (methodName is "Contains" or "IndexOf" or "Find")
            {
                return new StatementAnalysisResult
                {
                    Complexity = new VariableComplexity(Variable.N),
                    Confidence = 0.85,
                    IsComplete = true
                };
            }
        }

        return new StatementAnalysisResult
        {
            Complexity = ConstantComplexity.One,
            Confidence = 0.95,
            IsComplete = true
        };
    }

    #endregion

    #region Complexity Estimation

    private (ComplexityExpression Complexity, double Confidence) EstimateMethodComplexity(
        MethodDeclarationSyntax method,
        IReadOnlyList<LoopSnapshot> loops,
        IReadOnlyList<string> recursiveCalls,
        bool isKnownIncomplete)
    {
        // Start with constant
        ComplexityExpression total = ConstantComplexity.One;
        double confidence = 1.0;

        // If recursive, estimate based on call pattern
        if (recursiveCalls.Count > 0)
        {
            var recursiveComplexity = EstimateRecursiveComplexity(method, recursiveCalls);
            if (recursiveComplexity is not null)
            {
                total = recursiveComplexity;
                confidence = 0.7; // Recursive estimation is less certain
            }
        }
        else
        {
            // Compose loop complexities
            foreach (var loop in loops)
            {
                if (loop.IterationCount is not null)
                {
                    var loopComplexity = MultiplyComplexity(
                        loop.IterationCount,
                        loop.BodyComplexity ?? ConstantComplexity.One);
                    total = MaxComplexity(total, loopComplexity);

                    if (!loop.IsComplete)
                        confidence *= 0.8;
                }
            }

            // If no loops, analyze the body directly
            if (loops.Count == 0)
            {
                var body = method.Body ?? (StatementSyntax?)method.ExpressionBody?.Expression.Parent;
                if (body is StatementSyntax stmt)
                {
                    var result = AnalyzeStatement(stmt);
                    total = result.Complexity;
                    confidence = result.Confidence;
                }
            }
        }

        // Reduce confidence if code is incomplete
        if (isKnownIncomplete)
            confidence *= 0.6;

        return (total, confidence);
    }

    private ComplexityExpression? EstimateRecursiveComplexity(
        MethodDeclarationSyntax method,
        IReadOnlyList<string> recursiveCalls)
    {
        // Count recursive calls
        var callCount = recursiveCalls.Count;

        // Look for divide pattern (n/2, n-1, etc.)
        var hasDivide = recursiveCalls.Any(c => c.Contains('/'));
        var hasDecrement = recursiveCalls.Any(c => c.Contains("- 1") || c.Contains("-1"));

        if (callCount == 1 && hasDivide)
        {
            // Binary search pattern: T(n) = T(n/2) + O(1) → O(log n)
            return new LogarithmicComplexity(1.0, Variable.N);
        }

        if (callCount == 2 && hasDivide)
        {
            // Merge sort pattern: T(n) = 2T(n/2) + O(n) → O(n log n)
            return PolyLogComplexity.NLogN();
        }

        if (callCount == 1 && hasDecrement)
        {
            // Linear recursion: T(n) = T(n-1) + O(1) → O(n)
            return new VariableComplexity(Variable.N);
        }

        if (callCount == 2 && hasDecrement)
        {
            // Fibonacci pattern: T(n) = T(n-1) + T(n-2) → O(2^n)
            return new ExponentialComplexity(2, Variable.N);
        }

        // Default: linear
        return new VariableComplexity(Variable.N);
    }

    #endregion

    #region Complexity Arithmetic

    private static ComplexityExpression MultiplyComplexity(
        ComplexityExpression a,
        ComplexityExpression b)
    {
        // Simplify: anything * O(1) = anything
        if (a is ConstantComplexity)
            return b;
        if (b is ConstantComplexity)
            return a;

        // n * n = n²
        if (a is VariableComplexity && b is VariableComplexity)
            return PolynomialComplexity.OfDegree(2, Variable.N);

        // n * log n = n log n
        if (a is VariableComplexity && b is LogarithmicComplexity)
            return PolyLogComplexity.NLogN();
        if (b is VariableComplexity && a is LogarithmicComplexity)
            return PolyLogComplexity.NLogN();

        // n * n log n = n² log n
        if (a is VariableComplexity && b is PolyLogComplexity pl)
            return PolyLogComplexity.PolyTimesLog(pl.PolyDegree + 1);

        // Default: return the "larger" one (pessimistic)
        return MaxComplexity(a, b);
    }

    private static ComplexityExpression AddComplexity(
        ComplexityExpression a,
        ComplexityExpression b)
    {
        // Addition takes the max (asymptotically)
        return MaxComplexity(a, b);
    }

    private static ComplexityExpression MaxComplexity(
        ComplexityExpression a,
        ComplexityExpression b)
    {
        var degreeA = GetAsymptoticDegree(a);
        var degreeB = GetAsymptoticDegree(b);

        return degreeA >= degreeB ? a : b;
    }

    private static double GetAsymptoticDegree(ComplexityExpression expr)
    {
        return expr switch
        {
            ConstantComplexity => 0,
            LogarithmicComplexity => 0.1,
            VariableComplexity or LinearComplexity => 1,
            PolyLogComplexity pl => pl.PolyDegree + (pl.LogExponent * 0.01),
            PolynomialComplexity p => p.Degree,
            ExponentialComplexity => 100,
            FactorialComplexity => 200,
            _ => 1
        };
    }

    #endregion
}

/// <summary>
/// Result of analyzing a single statement.
/// </summary>
public record StatementAnalysisResult
{
    public ComplexityExpression Complexity { get; init; } = ConstantComplexity.One;
    public double Confidence { get; init; } = 1.0;
    public bool IsComplete { get; init; } = true;
}
