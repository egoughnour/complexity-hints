using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Analyzes loop constructs to extract iteration bounds and patterns.
/// </summary>
public sealed class LoopAnalyzer
{
    private readonly SemanticModel _semanticModel;

    public LoopAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Analyzes a for loop to extract its iteration bound.
    /// </summary>
    public LoopAnalysisResult AnalyzeForLoop(ForStatementSyntax forLoop, AnalysisContext context)
    {
        // Extract loop variable
        var loopVariable = ExtractLoopVariable(forLoop);
        if (loopVariable is null)
        {
            return LoopAnalysisResult.Unknown("Could not identify loop variable");
        }

        // Extract initial value
        var initialValue = ExtractInitialValue(forLoop, loopVariable);

        // Extract bound from condition
        var (bound, boundType) = ExtractConditionBound(forLoop.Condition, loopVariable, context);

        // Extract increment pattern
        var (step, pattern) = ExtractIncrementPattern(forLoop.Incrementors, loopVariable);

        if (bound is null)
        {
            return LoopAnalysisResult.Unknown("Could not determine loop bound");
        }

        var loopBound = new LoopBound
        {
            LowerBound = initialValue ?? ConstantComplexity.Zero,
            UpperBound = bound,
            Step = step,
            Pattern = pattern,
            IsExact = boundType == BoundType.Exact
        };

        return new LoopAnalysisResult
        {
            Success = true,
            LoopVariable = loopVariable,
            Bound = loopBound,
            IterationCount = ComputeIterationCount(loopBound, pattern),
            Pattern = pattern
        };
    }

    /// <summary>
    /// Analyzes a while loop to extract its iteration bound.
    /// </summary>
    public LoopAnalysisResult AnalyzeWhileLoop(WhileStatementSyntax whileLoop, AnalysisContext context)
    {
        // While loops are harder to analyze - look for common patterns
        var condition = whileLoop.Condition;

        // Pattern: while (i < n) with i++ in body
        if (condition is BinaryExpressionSyntax binary &&
            IsComparisonOperator(binary.Kind()))
        {
            var (bound, boundType) = ExtractBinaryConditionBound(binary, context);
            var loopVar = ExtractVariableFromExpression(binary.Left);

            if (bound is not null && loopVar is not null)
            {
                // Check body for increment pattern
                var (step, pattern) = AnalyzeWhileBodyForIncrement(whileLoop.Statement, loopVar);

                return new LoopAnalysisResult
                {
                    Success = true,
                    LoopVariable = loopVar,
                    Bound = new LoopBound
                    {
                        LowerBound = ConstantComplexity.Zero,
                        UpperBound = bound,
                        Step = step,
                        Pattern = pattern,
                        IsExact = boundType == BoundType.Exact
                    },
                    IterationCount = pattern == IterationPattern.Logarithmic
                        ? new LogOfComplexity(bound)
                        : bound,
                    Pattern = pattern
                };
            }
        }

        return LoopAnalysisResult.Unknown("Could not analyze while loop condition");
    }

    /// <summary>
    /// Analyzes a foreach loop to extract its iteration bound.
    /// </summary>
    public LoopAnalysisResult AnalyzeForeachLoop(ForEachStatementSyntax foreachLoop, AnalysisContext context)
    {
        var collection = foreachLoop.Expression;
        var collectionType = _semanticModel.GetTypeInfo(collection).Type;

        // Get the size variable for the collection
        var sizeExpression = GetCollectionSizeExpression(collection, collectionType, context);

        if (sizeExpression is not null)
        {
            return new LoopAnalysisResult
            {
                Success = true,
                LoopVariable = null, // foreach doesn't have a traditional loop variable
                Bound = new LoopBound
                {
                    LowerBound = ConstantComplexity.Zero,
                    UpperBound = sizeExpression,
                    Step = ConstantComplexity.One,
                    Pattern = IterationPattern.Linear,
                    IsExact = true
                },
                IterationCount = sizeExpression,
                Pattern = IterationPattern.Linear
            };
        }

        // Fallback: use generic 'n' variable
        return new LoopAnalysisResult
        {
            Success = true,
            LoopVariable = null,
            Bound = LoopBound.ZeroToN(Variable.N),
            IterationCount = new VariableComplexity(Variable.N),
            Pattern = IterationPattern.Linear,
            Notes = "Collection size unknown, using generic n"
        };
    }

    /// <summary>
    /// Analyzes a do-while loop to extract its iteration bound.
    /// </summary>
    public LoopAnalysisResult AnalyzeDoWhileLoop(DoStatementSyntax doWhile, AnalysisContext context)
    {
        // Do-while is similar to while but executes at least once
        var condition = doWhile.Condition;

        if (condition is BinaryExpressionSyntax binary &&
            IsComparisonOperator(binary.Kind()))
        {
            var (bound, boundType) = ExtractBinaryConditionBound(binary, context);
            var loopVar = ExtractVariableFromExpression(binary.Left);

            if (bound is not null && loopVar is not null)
            {
                var (step, pattern) = AnalyzeWhileBodyForIncrement(doWhile.Statement, loopVar);

                return new LoopAnalysisResult
                {
                    Success = true,
                    LoopVariable = loopVar,
                    Bound = new LoopBound
                    {
                        LowerBound = ConstantComplexity.Zero,
                        UpperBound = bound,
                        Step = step,
                        Pattern = pattern,
                        IsExact = boundType == BoundType.Exact
                    },
                    IterationCount = pattern == IterationPattern.Logarithmic
                        ? new LogOfComplexity(bound)
                        : bound,
                    Pattern = pattern,
                    Notes = "Do-while: executes at least once"
                };
            }
        }

        return LoopAnalysisResult.Unknown("Could not analyze do-while condition");
    }

    /// <summary>
    /// Uses DFA to trace a local variable back to its definition and extract complexity.
    /// </summary>
    private ComplexityExpression? TraceLocalVariableDefinition(ILocalSymbol local, AnalysisContext context)
    {
        // Find the declarator syntax for this local
        foreach (var syntaxRef in local.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is VariableDeclaratorSyntax declarator &&
                declarator.Initializer?.Value is { } initializer)
            {
                // Check if initialized from a parameter
                if (initializer is IdentifierNameSyntax initId)
                {
                    var initSymbol = _semanticModel.GetSymbolInfo(initId).Symbol;
                    if (initSymbol is IParameterSymbol param)
                    {
                        var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                        return new VariableComplexity(variable);
                    }
                }

                // Check if initialized from .Length or .Count
                if (initializer is MemberAccessExpressionSyntax memberAccess)
                {
                    var memberName = memberAccess.Name.Identifier.Text;
                    if (memberName is "Count" or "Length" or "Size")
                    {
                        var targetSymbol = _semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                        if (targetSymbol is IParameterSymbol param)
                        {
                            var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                            return new VariableComplexity(variable);
                        }
                        // Use generic N if we can't determine the source
                        return new VariableComplexity(Variable.N);
                    }
                }

                // Check for literal initialization
                if (initializer is LiteralExpressionSyntax literal)
                {
                    if (literal.Token.Value is int intValue)
                        return new ConstantComplexity(intValue);
                    if (literal.Token.Value is double doubleValue)
                        return new ConstantComplexity(doubleValue);
                }
            }
        }

        return null;
    }

    private ISymbol? ExtractLoopVariable(ForStatementSyntax forLoop)
    {
        if (forLoop.Declaration?.Variables.Count > 0)
        {
            var varDecl = forLoop.Declaration.Variables[0];
            return _semanticModel.GetDeclaredSymbol(varDecl);
        }

        return null;
    }

    private ComplexityExpression? ExtractInitialValue(ForStatementSyntax forLoop, ISymbol loopVariable)
    {
        if (forLoop.Declaration?.Variables.Count > 0)
        {
            var varDecl = forLoop.Declaration.Variables[0];
            if (varDecl.Initializer?.Value is LiteralExpressionSyntax literal)
            {
                if (literal.Token.Value is int intValue)
                    return new ConstantComplexity(intValue);
                if (literal.Token.Value is double doubleValue)
                    return new ConstantComplexity(doubleValue);
            }
        }

        return ConstantComplexity.Zero;
    }

    private (ComplexityExpression? bound, BoundType type) ExtractConditionBound(
        ExpressionSyntax? condition, ISymbol loopVariable, AnalysisContext context)
    {
        if (condition is BinaryExpressionSyntax binary)
        {
            return ExtractBinaryConditionBound(binary, context);
        }

        return (null, BoundType.Unknown);
    }

    private (ComplexityExpression? bound, BoundType type) ExtractBinaryConditionBound(
        BinaryExpressionSyntax binary, AnalysisContext context)
    {
        var right = binary.Right;

        // Check if right side is a known variable
        if (right is IdentifierNameSyntax identifier)
        {
            var symbol = _semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol is IParameterSymbol param)
            {
                var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                return (new VariableComplexity(variable), BoundType.Exact);
            }
            if (symbol is ILocalSymbol local)
            {
                var variable = context.GetVariable(local);
                if (variable is not null)
                    return (new VariableComplexity(variable), BoundType.Exact);

                // DFA: trace back to local variable's definition
                var tracedBound = TraceLocalVariableDefinition(local, context);
                if (tracedBound is not null)
                    return (tracedBound, BoundType.Exact);
            }
        }

        // Check for property access like list.Count or array.Length
        if (right is MemberAccessExpressionSyntax memberAccess)
        {
            var memberName = memberAccess.Name.Identifier.Text;
            if (memberName is "Count" or "Length" or "Size")
            {
                var targetType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                var targetSymbol = _semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;

                if (targetSymbol is IParameterSymbol param)
                {
                    var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                    return (new VariableComplexity(variable), BoundType.Exact);
                }

                // Use generic variable based on collection
                return (new VariableComplexity(Variable.N), BoundType.Estimated);
            }
        }

        // Check for literal
        if (right is LiteralExpressionSyntax literal)
        {
            if (literal.Token.Value is int intValue)
                return (new ConstantComplexity(intValue), BoundType.Exact);
            if (literal.Token.Value is double doubleValue)
                return (new ConstantComplexity(doubleValue), BoundType.Exact);
        }

        // Check for binary expressions (e.g., n - 1, n / 2)
        if (right is BinaryExpressionSyntax nestedBinary)
        {
            // Extract the bound from the left side of the nested binary
            // e.g., for "i < n - 1", nestedBinary is "n - 1", extract bound from "n"
            var leftExpr = nestedBinary.Left;
            ComplexityExpression? leftBound = null;
            
            // Try to get the variable from the left side directly
            if (leftExpr is IdentifierNameSyntax leftId)
            {
                var symbol = _semanticModel.GetSymbolInfo(leftId).Symbol;
                if (symbol is IParameterSymbol param)
                {
                    var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                    leftBound = new VariableComplexity(variable);
                }
                if (symbol is ILocalSymbol local)
                {
                    var localVar = context.GetVariable(local);
                    if (localVar is not null)
                        leftBound = new VariableComplexity(localVar);
                    else
                    {
                        // DFA: trace back to local variable's definition
                        leftBound = TraceLocalVariableDefinition(local, context);
                    }
                }
            }
            else if (leftExpr is MemberAccessExpressionSyntax leftMember)
            {
                var memberName = leftMember.Name.Identifier.Text;
                if (memberName is "Count" or "Length" or "Size")
                {
                    var targetSymbol = _semanticModel.GetSymbolInfo(leftMember.Expression).Symbol;
                    if (targetSymbol is IParameterSymbol param)
                    {
                        var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                        leftBound = new VariableComplexity(variable);
                    }
                    else
                    {
                        leftBound = new VariableComplexity(Variable.N);
                    }
                }
            }

            if (leftBound is not null)
            {
                return nestedBinary.Kind() switch
                {
                    SyntaxKind.SubtractExpression => (leftBound, BoundType.Exact), // n - k is still O(n)
                    SyntaxKind.DivideExpression => (leftBound, BoundType.Exact),   // n / k is still O(n)
                    _ => (leftBound, BoundType.Estimated)
                };
            }
        }

        return (null, BoundType.Unknown);
    }

    private (ComplexityExpression step, IterationPattern pattern) ExtractIncrementPattern(
        SeparatedSyntaxList<ExpressionSyntax> incrementors, ISymbol loopVariable)
    {
        if (incrementors.Count == 0)
            return (ConstantComplexity.One, IterationPattern.Unknown);

        var incrementor = incrementors[0];

        // i++ or ++i
        if (incrementor is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PostIncrementExpression } ||
            incrementor is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PreIncrementExpression })
        {
            return (ConstantComplexity.One, IterationPattern.Linear);
        }

        // i-- or --i
        if (incrementor is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PostDecrementExpression } ||
            incrementor is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.PreDecrementExpression })
        {
            return (ConstantComplexity.One, IterationPattern.Linear);
        }

        // i += k
        if (incrementor is AssignmentExpressionSyntax assignment)
        {
            if (assignment.Kind() == SyntaxKind.AddAssignmentExpression ||
                assignment.Kind() == SyntaxKind.SubtractAssignmentExpression)
            {
                if (assignment.Right is LiteralExpressionSyntax literal &&
                    literal.Token.Value is int step)
                {
                    return (new ConstantComplexity(Math.Abs(step)), IterationPattern.Linear);
                }
                return (ConstantComplexity.One, IterationPattern.Linear);
            }

            // i *= 2 or i /= 2 (logarithmic)
            if (assignment.Kind() == SyntaxKind.MultiplyAssignmentExpression ||
                assignment.Kind() == SyntaxKind.DivideAssignmentExpression)
            {
                if (assignment.Right is LiteralExpressionSyntax literal &&
                    literal.Token.Value is int factor && factor > 1)
                {
                    return (new ConstantComplexity(factor), IterationPattern.Logarithmic);
                }
                return (new ConstantComplexity(2), IterationPattern.Logarithmic);
            }
        }

        // i = i * 2 or i = i / 2
        if (incrementor is AssignmentExpressionSyntax simpleAssign &&
            simpleAssign.Kind() == SyntaxKind.SimpleAssignmentExpression &&
            simpleAssign.Right is BinaryExpressionSyntax binExpr)
        {
            if (binExpr.Kind() == SyntaxKind.MultiplyExpression ||
                binExpr.Kind() == SyntaxKind.DivideExpression)
            {
                return (new ConstantComplexity(2), IterationPattern.Logarithmic);
            }
        }

        return (ConstantComplexity.One, IterationPattern.Unknown);
    }

    private (ComplexityExpression step, IterationPattern pattern) AnalyzeWhileBodyForIncrement(
        StatementSyntax body, ISymbol loopVariable)
    {
        // Look for increment statements in the body
        var incrementFinder = new IncrementFinder(loopVariable, _semanticModel);
        incrementFinder.Visit(body);

        return (incrementFinder.Step, incrementFinder.Pattern);
    }

    private ComplexityExpression? GetCollectionSizeExpression(
        ExpressionSyntax collection, ITypeSymbol? collectionType, AnalysisContext context)
    {
        // Check if collection is a parameter
        if (collection is IdentifierNameSyntax identifier)
        {
            var symbol = _semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol is IParameterSymbol param)
            {
                var variable = context.GetVariable(param) ?? context.InferParameterVariable(param);
                return new VariableComplexity(variable);
            }
            if (symbol is ILocalSymbol local)
            {
                var variable = context.GetVariable(local);
                if (variable is not null)
                    return new VariableComplexity(variable);

                // DFA: trace back to local variable's definition
                var tracedBound = TraceLocalVariableDefinition(local, context);
                if (tracedBound is not null)
                    return tracedBound;
            }
        }

        // Check for member access (this.items, etc.)
        if (collection is MemberAccessExpressionSyntax memberAccess)
        {
            var memberSymbol = _semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (memberSymbol is IFieldSymbol or IPropertySymbol)
            {
                // Use a derived variable
                return new VariableComplexity(
                    new Variable(memberSymbol.Name, VariableType.DataCount)
                    {
                        Description = $"Size of {memberSymbol.Name}"
                    });
            }
        }

        return null;
    }

    private ISymbol? ExtractVariableFromExpression(ExpressionSyntax expression)
    {
        if (expression is IdentifierNameSyntax identifier)
        {
            return _semanticModel.GetSymbolInfo(identifier).Symbol;
        }

        return null;
    }

    private ComplexityExpression ComputeIterationCount(LoopBound bound, IterationPattern pattern)
    {
        return pattern switch
        {
            IterationPattern.Linear => bound.UpperBound,
            IterationPattern.Logarithmic => new LogOfComplexity(bound.UpperBound),
            IterationPattern.Quadratic => new PolynomialComplexity(
                ImmutableDictionary<int, double>.Empty.Add(2, 0.5),
                Variable.N),
            _ => bound.UpperBound
        };
    }

    private static bool IsComparisonOperator(SyntaxKind kind) =>
        kind is SyntaxKind.LessThanExpression
            or SyntaxKind.LessThanOrEqualExpression
            or SyntaxKind.GreaterThanExpression
            or SyntaxKind.GreaterThanOrEqualExpression
            or SyntaxKind.NotEqualsExpression;

    /// <summary>
    /// Helper walker to find increment patterns in while/do-while bodies.
    /// </summary>
    private class IncrementFinder : CSharpSyntaxWalker
    {
        private readonly ISymbol _targetVariable;
        private readonly SemanticModel _semanticModel;

        public ComplexityExpression Step { get; private set; } = ConstantComplexity.One;
        public IterationPattern Pattern { get; private set; } = IterationPattern.Linear;

        public IncrementFinder(ISymbol targetVariable, SemanticModel semanticModel)
        {
            _targetVariable = targetVariable;
            _semanticModel = semanticModel;
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            if (IsTargetVariable(node.Operand))
            {
                Pattern = IterationPattern.Linear;
                Step = ConstantComplexity.One;
            }
            base.VisitPostfixUnaryExpression(node);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (IsTargetVariable(node.Operand))
            {
                Pattern = IterationPattern.Linear;
                Step = ConstantComplexity.One;
            }
            base.VisitPrefixUnaryExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (IsTargetVariable(node.Left))
            {
                switch (node.Kind())
                {
                    case SyntaxKind.AddAssignmentExpression:
                    case SyntaxKind.SubtractAssignmentExpression:
                        Pattern = IterationPattern.Linear;
                        if (node.Right is LiteralExpressionSyntax lit && lit.Token.Value is int step)
                            Step = new ConstantComplexity(Math.Abs(step));
                        break;

                    case SyntaxKind.MultiplyAssignmentExpression:
                    case SyntaxKind.DivideAssignmentExpression:
                        Pattern = IterationPattern.Logarithmic;
                        if (node.Right is LiteralExpressionSyntax factor && factor.Token.Value is int f)
                            Step = new ConstantComplexity(f);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression when node.Right is BinaryExpressionSyntax binary:
                        if (binary.Kind() is SyntaxKind.MultiplyExpression or SyntaxKind.DivideExpression)
                        {
                            Pattern = IterationPattern.Logarithmic;
                            Step = new ConstantComplexity(2);
                        }
                        break;
                }
            }
            base.VisitAssignmentExpression(node);
        }

        private bool IsTargetVariable(ExpressionSyntax expression)
        {
            if (expression is IdentifierNameSyntax identifier)
            {
                var symbol = _semanticModel.GetSymbolInfo(identifier).Symbol;
                return symbol is not null && SymbolEqualityComparer.Default.Equals(symbol, _targetVariable);
            }
            return false;
        }
    }
}

/// <summary>
/// Result of loop analysis.
/// </summary>
public record LoopAnalysisResult
{
    /// <summary>
    /// Whether the analysis was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The loop variable symbol (if identified).
    /// </summary>
    public ISymbol? LoopVariable { get; init; }

    /// <summary>
    /// The computed loop bound.
    /// </summary>
    public LoopBound? Bound { get; init; }

    /// <summary>
    /// The number of iterations as a complexity expression.
    /// </summary>
    public ComplexityExpression? IterationCount { get; init; }

    /// <summary>
    /// The iteration pattern detected.
    /// </summary>
    public IterationPattern Pattern { get; init; }

    /// <summary>
    /// Additional notes about the analysis.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates an unknown/failed result.
    /// </summary>
    public static LoopAnalysisResult Unknown(string reason) => new()
    {
        Success = false,
        Pattern = IterationPattern.Unknown,
        ErrorMessage = reason
    };
}

/// <summary>
/// Type of bound determined from analysis.
/// </summary>
public enum BoundType
{
    /// <summary>
    /// Exact bound known.
    /// </summary>
    Exact,

    /// <summary>
    /// Estimated bound (conservative).
    /// </summary>
    Estimated,

    /// <summary>
    /// Unknown bound.
    /// </summary>
    Unknown
}
