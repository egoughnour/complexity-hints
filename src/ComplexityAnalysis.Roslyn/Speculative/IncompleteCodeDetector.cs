using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Result of incomplete code detection.
/// </summary>
public sealed record IncompleteCodeResult
{
    public bool IsDefinitelyIncomplete { get; init; }
    public bool IsLikelyIncomplete { get; init; }
    public bool HasTodoMarker { get; init; }
    public IReadOnlyList<CodePattern> Patterns { get; init; } = Array.Empty<CodePattern>();
    public string? Explanation { get; init; }
}

/// <summary>
/// Detects incomplete code patterns:
/// - throw new NotImplementedException()
/// - throw new NotSupportedException()
/// - TODO/FIXME/HACK comments
/// - Empty method bodies
/// </summary>
public sealed class IncompleteCodeDetector
{
    private static readonly string[] TodoMarkers = { "TODO", "FIXME", "HACK", "XXX", "UNDONE" };

    /// <summary>
    /// Detects incomplete code patterns in a method.
    /// </summary>
    public IncompleteCodeResult Detect(MethodDeclarationSyntax method)
    {
        var patterns = new List<CodePattern>();
        var explanations = new List<string>();
        bool hasTodoMarker = false;
        bool isDefinitelyIncomplete = false;
        bool isLikelyIncomplete = false;

        // Check for throw statements
        var throwStatements = method.DescendantNodes()
            .OfType<ThrowStatementSyntax>()
            .ToList();

        foreach (var throwStmt in throwStatements)
        {
            if (IsNotImplementedException(throwStmt))
            {
                patterns.Add(CodePattern.ThrowsNotImplementedException);
                isDefinitelyIncomplete = true;
                explanations.Add("throws NotImplementedException");
            }
            else if (IsNotSupportedException(throwStmt))
            {
                patterns.Add(CodePattern.ThrowsNotSupportedException);
                isLikelyIncomplete = true;
                explanations.Add("throws NotSupportedException");
            }
        }

        // Check for throw expressions (C# 7+)
        var throwExpressions = method.DescendantNodes()
            .OfType<ThrowExpressionSyntax>()
            .ToList();

        foreach (var throwExpr in throwExpressions)
        {
            if (IsNotImplementedException(throwExpr))
            {
                patterns.Add(CodePattern.ThrowsNotImplementedException);
                isDefinitelyIncomplete = true;
                explanations.Add("throws NotImplementedException");
            }
        }

        // Check for TODO/FIXME comments
        var trivia = method.DescendantTrivia()
            .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                       t.IsKind(SyntaxKind.MultiLineCommentTrivia))
            .ToList();

        foreach (var comment in trivia)
        {
            var text = comment.ToString().ToUpperInvariant();
            if (TodoMarkers.Any(marker => text.Contains(marker)))
            {
                patterns.Add(CodePattern.HasTodoComment);
                hasTodoMarker = true;
                isLikelyIncomplete = true;
                explanations.Add("contains TODO/FIXME comment");
                break;
            }
        }

        // Check for empty body
        if (method.Body is not null)
        {
            var statements = method.Body.Statements;
            if (statements.Count == 0)
            {
                patterns.Add(CodePattern.EmptyBody);
                isLikelyIncomplete = true;
                explanations.Add("empty method body");
            }
            else if (statements.Count == 1 && statements[0] is ReturnStatementSyntax ret && ret.Expression is null)
            {
                // void method with just "return;"
                patterns.Add(CodePattern.EmptyBody);
                isLikelyIncomplete = true;
                explanations.Add("method only contains return statement");
            }
        }
        else if (method.ExpressionBody is not null)
        {
            // Expression-bodied member - check if it's throwing
            if (method.ExpressionBody.Expression is ThrowExpressionSyntax throwExpr)
            {
                if (IsNotImplementedException(throwExpr))
                {
                    patterns.Add(CodePattern.ThrowsNotImplementedException);
                    isDefinitelyIncomplete = true;
                    explanations.Add("throws NotImplementedException");
                }
            }
        }

        return new IncompleteCodeResult
        {
            IsDefinitelyIncomplete = isDefinitelyIncomplete,
            IsLikelyIncomplete = isLikelyIncomplete,
            HasTodoMarker = hasTodoMarker,
            Patterns = patterns,
            Explanation = explanations.Count > 0
                ? $"Incomplete: {string.Join(", ", explanations)}"
                : null
        };
    }

    private static bool IsNotImplementedException(ThrowStatementSyntax throwStmt)
    {
        if (throwStmt.Expression is ObjectCreationExpressionSyntax creation)
        {
            return IsNotImplementedExceptionType(creation.Type);
        }
        return false;
    }

    private static bool IsNotImplementedException(ThrowExpressionSyntax throwExpr)
    {
        if (throwExpr.Expression is ObjectCreationExpressionSyntax creation)
        {
            return IsNotImplementedExceptionType(creation.Type);
        }
        return false;
    }

    private static bool IsNotSupportedException(ThrowStatementSyntax throwStmt)
    {
        if (throwStmt.Expression is ObjectCreationExpressionSyntax creation)
        {
            return IsNotSupportedExceptionType(creation.Type);
        }
        return false;
    }

    private static bool IsNotImplementedExceptionType(TypeSyntax type)
    {
        var typeName = type.ToString();
        return typeName is "NotImplementedException"
            or "System.NotImplementedException"
            or "global::System.NotImplementedException";
    }

    private static bool IsNotSupportedExceptionType(TypeSyntax type)
    {
        var typeName = type.ToString();
        return typeName is "NotSupportedException"
            or "System.NotSupportedException"
            or "global::System.NotSupportedException";
    }
}
