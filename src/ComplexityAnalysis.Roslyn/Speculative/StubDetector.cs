using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Result of stub detection.
/// </summary>
public sealed record StubDetectionResult
{
    public bool IsStub { get; init; }
    public IReadOnlyList<CodePattern> Patterns { get; init; } = Array.Empty<CodePattern>();
    public string? Explanation { get; init; }
}

/// <summary>
/// Detects stub implementations:
/// - Returns default/null/empty
/// - Counter-only implementations (mocks)
/// - Returns constant value with no logic
/// </summary>
public sealed class StubDetector
{
    /// <summary>
    /// Detects if a method is a stub implementation.
    /// </summary>
    public StubDetectionResult Detect(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        var patterns = new List<CodePattern>();
        var explanations = new List<string>();
        bool isStub = false;

        // Check body content
        if (method.Body is not null)
        {
            isStub = AnalyzeBody(method.Body, patterns, explanations);
        }
        else if (method.ExpressionBody is not null)
        {
            isStub = AnalyzeExpressionBody(method.ExpressionBody, patterns, explanations);
        }

        return new StubDetectionResult
        {
            IsStub = isStub,
            Patterns = patterns,
            Explanation = explanations.Count > 0
                ? $"Stub: {string.Join(", ", explanations)}"
                : null
        };
    }

    private bool AnalyzeBody(BlockSyntax body, List<CodePattern> patterns, List<string> explanations)
    {
        var statements = body.Statements;

        // Empty body is a stub
        if (statements.Count == 0)
        {
            patterns.Add(CodePattern.EmptyBody);
            explanations.Add("empty body");
            return true;
        }

        // Single return statement with default/null
        if (statements.Count == 1 && statements[0] is ReturnStatementSyntax ret)
        {
            if (ret.Expression is null)
            {
                patterns.Add(CodePattern.ReturnsDefault);
                explanations.Add("returns void immediately");
                return true;
            }

            if (IsDefaultOrNullExpression(ret.Expression))
            {
                patterns.Add(CodePattern.ReturnsDefault);
                explanations.Add("returns default/null");
                return true;
            }

            if (IsConstantExpression(ret.Expression))
            {
                patterns.Add(CodePattern.ReturnsConstant);
                explanations.Add("returns constant");
                return true;
            }
        }

        // Counter-only pattern (common in mocks)
        if (IsCounterOnlyImplementation(statements))
        {
            patterns.Add(CodePattern.CounterOnly);
            explanations.Add("counter-only implementation");
            return true;
        }

        return false;
    }

    private bool AnalyzeExpressionBody(ArrowExpressionClauseSyntax exprBody, 
        List<CodePattern> patterns, List<string> explanations)
    {
        var expr = exprBody.Expression;

        if (IsDefaultOrNullExpression(expr))
        {
            patterns.Add(CodePattern.ReturnsDefault);
            explanations.Add("returns default/null");
            return true;
        }

        if (IsConstantExpression(expr))
        {
            patterns.Add(CodePattern.ReturnsConstant);
            explanations.Add("returns constant");
            return true;
        }

        return false;
    }

    private static bool IsDefaultOrNullExpression(ExpressionSyntax expr)
    {
        return expr switch
        {
            LiteralExpressionSyntax lit when 
                lit.IsKind(SyntaxKind.NullLiteralExpression) ||
                lit.IsKind(SyntaxKind.DefaultLiteralExpression) => true,
            DefaultExpressionSyntax => true,
            // default(T)
            InvocationExpressionSyntax inv when 
                inv.Expression.ToString() == "default" => true,
            _ => false
        };
    }

    private static bool IsConstantExpression(ExpressionSyntax expr)
    {
        return expr switch
        {
            LiteralExpressionSyntax lit when
                lit.IsKind(SyntaxKind.NumericLiteralExpression) ||
                lit.IsKind(SyntaxKind.StringLiteralExpression) ||
                lit.IsKind(SyntaxKind.TrueLiteralExpression) ||
                lit.IsKind(SyntaxKind.FalseLiteralExpression) => true,
            _ => false
        };
    }

    private static bool IsCounterOnlyImplementation(SyntaxList<StatementSyntax> statements)
    {
        // Pattern: _counter++; or _callCount++; etc.
        if (statements.Count == 1 || statements.Count == 2)
        {
            var hasIncrement = false;
            var hasReturn = false;

            foreach (var stmt in statements)
            {
                if (stmt is ExpressionStatementSyntax exprStmt)
                {
                    var expr = exprStmt.Expression;
                    if (expr is PostfixUnaryExpressionSyntax postfix &&
                        (postfix.IsKind(SyntaxKind.PostIncrementExpression) ||
                         postfix.IsKind(SyntaxKind.PostDecrementExpression)))
                    {
                        // Check if it's accessing a field like _counter
                        var target = postfix.Operand.ToString();
                        if (target.StartsWith("_") || target.Contains("count", StringComparison.OrdinalIgnoreCase))
                        {
                            hasIncrement = true;
                        }
                    }
                    else if (expr is PrefixUnaryExpressionSyntax prefix &&
                             (prefix.IsKind(SyntaxKind.PreIncrementExpression) ||
                              prefix.IsKind(SyntaxKind.PreDecrementExpression)))
                    {
                        hasIncrement = true;
                    }
                }
                else if (stmt is ReturnStatementSyntax)
                {
                    hasReturn = true;
                }
            }

            return hasIncrement && (statements.Count == 1 || hasReturn);
        }

        return false;
    }
}
