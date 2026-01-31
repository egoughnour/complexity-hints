using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Analyzes code patterns to detect amortized complexity scenarios.
/// 
/// Detects patterns like:
/// - Dynamic array resizing (doubling strategy)
/// - Hash table rehashing
/// - Binary counter increment
/// - Stack with multipop
/// - Union-Find with path compression
/// </summary>
public sealed class AmortizedAnalyzer
{
    private readonly SemanticModel _semanticModel;

    public AmortizedAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Analyzes a method for amortized complexity patterns.
    /// Returns an AmortizedComplexity if an amortized pattern is detected,
    /// or null if the complexity should be treated as worst-case.
    /// </summary>
    public AmortizedComplexity? AnalyzeMethod(MethodDeclarationSyntax method)
    {
        var patterns = new List<AmortizedPattern>();

        // Check for doubling resize pattern
        var doublingPattern = DetectDoublingResizePattern(method);
        if (doublingPattern != null)
            patterns.Add(doublingPattern);

        // Check for rehash pattern
        var rehashPattern = DetectRehashPattern(method);
        if (rehashPattern != null)
            patterns.Add(rehashPattern);

        // Check for binary counter pattern
        var counterPattern = DetectBinaryCounterPattern(method);
        if (counterPattern != null)
            patterns.Add(counterPattern);

        // Check for Union-Find pattern
        var unionFindPattern = DetectUnionFindPattern(method);
        if (unionFindPattern != null)
            patterns.Add(unionFindPattern);

        // Check for multipop pattern
        var multipopPattern = DetectMultipopPattern(method);
        if (multipopPattern != null)
            patterns.Add(multipopPattern);

        // Return the most significant pattern found
        return patterns.OrderByDescending(p => p.Priority).FirstOrDefault()?.ToAmortizedComplexity();
    }

    /// <summary>
    /// Analyzes a sequence of operations for aggregate amortized complexity.
    /// </summary>
    public AmortizedComplexity? AnalyzeOperationSequence(
        IReadOnlyList<(MethodDeclarationSyntax Method, int Count)> operations)
    {
        var individualCosts = new List<(ComplexityExpression Amortized, ComplexityExpression Worst, int Count)>();

        foreach (var (method, count) in operations)
        {
            var amortized = AnalyzeMethod(method);
            if (amortized != null)
            {
                individualCosts.Add((amortized.AmortizedCost, amortized.WorstCaseCost, count));
            }
        }

        if (individualCosts.Count == 0)
            return null;

        // Aggregate: total amortized cost
        ComplexityExpression totalAmortized = ConstantComplexity.Zero;
        ComplexityExpression totalWorst = ConstantComplexity.Zero;

        foreach (var (amortized, worst, count) in individualCosts)
        {
            var countExpr = new ConstantComplexity(count);
            totalAmortized = new BinaryOperationComplexity(
                totalAmortized,
                BinaryOp.Plus,
                new BinaryOperationComplexity(countExpr, BinaryOp.Multiply, amortized));
            totalWorst = new BinaryOperationComplexity(
                totalWorst,
                BinaryOp.Plus,
                new BinaryOperationComplexity(countExpr, BinaryOp.Multiply, worst));
        }

        return new AmortizedComplexity
        {
            AmortizedCost = totalAmortized,
            WorstCaseCost = totalWorst,
            Method = AmortizationMethod.Aggregate,
            Description = "Aggregate analysis over operation sequence"
        };
    }

    #region Pattern Detection

    /// <summary>
    /// Detects the doubling resize pattern common in dynamic arrays.
    /// Pattern: if (count == capacity) resize to capacity * 2
    /// </summary>
    private AmortizedPattern? DetectDoublingResizePattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        // Look for resize condition: count == length or count >= capacity
        var ifStatements = body.DescendantNodes().OfType<IfStatementSyntax>();

        foreach (var ifStmt in ifStatements)
        {
            var condition = ifStmt.Condition.ToString().ToLowerInvariant();

            // Check for resize condition patterns
            var isResizeCondition =
                (condition.Contains("count") || condition.Contains("_count") || condition.Contains("size")) &&
                (condition.Contains("length") || condition.Contains("capacity") || condition.Contains("_data"));

            if (!isResizeCondition) continue;

            // Check for array creation with doubling
            var arrayCreations = ifStmt.Statement.DescendantNodes().OfType<ArrayCreationExpressionSyntax>();
            foreach (var creation in arrayCreations)
            {
                var sizeExpr = creation.ToString().ToLowerInvariant();
                if (sizeExpr.Contains("* 2") || sizeExpr.Contains("*2") ||
                    sizeExpr.Contains("<< 1") || sizeExpr.Contains("<<1"))
                {
                    return new AmortizedPattern
                    {
                        PatternType = AmortizedPatternType.DoublingResize,
                        AmortizedCost = ConstantComplexity.One,
                        WorstCaseCost = new LinearComplexity(1.0, Variable.N),
                        Potential = PotentialFunction.Common.DynamicArray,
                        Priority = 10,
                        Description = "Dynamic array with doubling resize strategy"
                    };
                }
            }

            // Also check for List/Array copy in resize
            var invocations = ifStmt.Statement.DescendantNodes().OfType<InvocationExpressionSyntax>();
            foreach (var invocation in invocations)
            {
                var methodName = invocation.ToString().ToLowerInvariant();
                if (methodName.Contains("copy") || methodName.Contains("resize"))
                {
                    return new AmortizedPattern
                    {
                        PatternType = AmortizedPatternType.DoublingResize,
                        AmortizedCost = ConstantComplexity.One,
                        WorstCaseCost = new LinearComplexity(1.0, Variable.N),
                        Potential = PotentialFunction.Common.DynamicArray,
                        Priority = 10,
                        Description = "Dynamic array with resize"
                    };
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Detects hash table rehash pattern.
    /// Pattern: if (load > threshold) rehash to larger table
    /// </summary>
    private AmortizedPattern? DetectRehashPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var ifStatements = body.DescendantNodes().OfType<IfStatementSyntax>();

        foreach (var ifStmt in ifStatements)
        {
            var condition = ifStmt.Condition.ToString().ToLowerInvariant();

            // Check for load factor condition
            var isLoadFactorCondition =
                (condition.Contains("load") || condition.Contains("count") || condition.Contains("_count")) &&
                (condition.Contains("bucket") || condition.Contains("capacity") || condition.Contains("length"));

            if (!isLoadFactorCondition) continue;

            // Check for rehash call or bucket reallocation
            var statementText = ifStmt.Statement.ToString().ToLowerInvariant();
            if (statementText.Contains("rehash") ||
                (statementText.Contains("bucket") && statementText.Contains("new")))
            {
                return new AmortizedPattern
                {
                    PatternType = AmortizedPatternType.HashRehash,
                    AmortizedCost = ConstantComplexity.One,
                    WorstCaseCost = new LinearComplexity(1.0, Variable.N),
                    Potential = PotentialFunction.Common.HashTable,
                    Priority = 10,
                    Description = "Hash table with load factor and rehashing"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Detects binary counter increment pattern.
    /// Pattern: while (bit[i] == 1) flip to 0; flip next to 1
    /// </summary>
    private AmortizedPattern? DetectBinaryCounterPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var whileStatements = body.DescendantNodes().OfType<WhileStatementSyntax>();

        foreach (var whileStmt in whileStatements)
        {
            var condition = whileStmt.Condition.ToString().ToLowerInvariant();
            var bodyText = whileStmt.Statement.ToString().ToLowerInvariant();

            // Pattern: while (bits[i] is true/1) { set to false/0; i++ }
            if ((condition.Contains("bit") || condition.Contains("[i]") || condition.Contains("[j]")) &&
                (bodyText.Contains("false") || bodyText.Contains("= 0") || bodyText.Contains("^") || bodyText.Contains("flip")))
            {
                return new AmortizedPattern
                {
                    PatternType = AmortizedPatternType.BinaryCounter,
                    AmortizedCost = ConstantComplexity.One,
                    WorstCaseCost = new LogarithmicComplexity(1.0, Variable.N), // O(log n) bits
                    Potential = PotentialFunction.Common.BinaryCounter,
                    Priority = 8,
                    Description = "Binary counter increment"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Detects Union-Find pattern with path compression.
    /// Pattern: recursive Find with _parent[x] = Find(_parent[x])
    /// </summary>
    private AmortizedPattern? DetectUnionFindPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var methodName = method.Identifier.Text.ToLowerInvariant();
        var bodyText = body.ToString().ToLowerInvariant();

        // Check for Find with path compression
        if (methodName == "find" || methodName == "findroot" || methodName == "getroot")
        {
            // Path compression pattern: parent[x] = Find(parent[x])
            if (bodyText.Contains("parent") && bodyText.Contains("find") &&
                (bodyText.Contains("parent[") || bodyText.Contains("_parent[")))
            {
                return new AmortizedPattern
                {
                    PatternType = AmortizedPatternType.UnionFind,
                    AmortizedCost = new InverseAckermannComplexity(Variable.N),
                    WorstCaseCost = new LogarithmicComplexity(1.0, Variable.N),
                    Potential = PotentialFunction.Common.UnionFind,
                    Priority = 9,
                    Description = "Union-Find with path compression"
                };
            }
        }

        // Check for Union with rank/size heuristic
        if (methodName == "union" || methodName == "merge")
        {
            if (bodyText.Contains("rank") || bodyText.Contains("size"))
            {
                return new AmortizedPattern
                {
                    PatternType = AmortizedPatternType.UnionFind,
                    AmortizedCost = new InverseAckermannComplexity(Variable.N),
                    WorstCaseCost = new LogarithmicComplexity(1.0, Variable.N),
                    Potential = PotentialFunction.Common.UnionFind,
                    Priority = 9,
                    Description = "Union-Find with union by rank/size"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Detects multipop stack pattern.
    /// Pattern: pop k items in a loop
    /// </summary>
    private AmortizedPattern? DetectMultipopPattern(MethodDeclarationSyntax method)
    {
        var body = method.Body ?? (SyntaxNode?)method.ExpressionBody;
        if (body == null) return null;

        var methodName = method.Identifier.Text.ToLowerInvariant();

        if (methodName.Contains("multipop") || methodName.Contains("popall") || methodName.Contains("clear"))
        {
            // Check for pop loop
            var forLoops = body.DescendantNodes().OfType<ForStatementSyntax>();
            var whileLoops = body.DescendantNodes().OfType<WhileStatementSyntax>();

            foreach (var loop in forLoops.Cast<StatementSyntax>().Concat(whileLoops))
            {
                var loopText = loop.ToString().ToLowerInvariant();
                if (loopText.Contains("pop") || loopText.Contains("dequeue") || loopText.Contains("remove"))
                {
                    return new AmortizedPattern
                    {
                        PatternType = AmortizedPatternType.MultipopStack,
                        AmortizedCost = ConstantComplexity.One,
                        WorstCaseCost = new LinearComplexity(1.0, Variable.K), // O(k) for k pops
                        Potential = PotentialFunction.Common.MultipopStack,
                        Priority = 7,
                        Description = "Multipop: amortized O(1) per push/pop"
                    };
                }
            }
        }

        return null;
    }

    #endregion

    #region Helper Types

    private sealed record AmortizedPattern
    {
        public required AmortizedPatternType PatternType { get; init; }
        public required ComplexityExpression AmortizedCost { get; init; }
        public required ComplexityExpression WorstCaseCost { get; init; }
        public PotentialFunction? Potential { get; init; }
        public int Priority { get; init; }
        public string? Description { get; init; }

        public AmortizedComplexity ToAmortizedComplexity() =>
            new()
            {
                AmortizedCost = AmortizedCost,
                WorstCaseCost = WorstCaseCost,
                Method = Potential != null ? AmortizationMethod.Potential : AmortizationMethod.Aggregate,
                Potential = Potential,
                Description = Description
            };
    }

    private enum AmortizedPatternType
    {
        DoublingResize,
        HashRehash,
        BinaryCounter,
        UnionFind,
        MultipopStack,
        SplayTree
    }

    #endregion
}

/// <summary>
/// Extends RoslynComplexityExtractor with amortized analysis capability.
/// </summary>
public static class AmortizedAnalysisExtensions
{
    /// <summary>
    /// Analyzes a method with amortized complexity detection.
    /// Returns AmortizedComplexity if a pattern is detected, otherwise falls back to worst-case.
    /// </summary>
    public static ComplexityExpression AnalyzeWithAmortization(
        this RoslynComplexityExtractor extractor,
        MethodDeclarationSyntax method,
        SemanticModel semanticModel)
    {
        var amortizedAnalyzer = new AmortizedAnalyzer(semanticModel);
        var amortized = amortizedAnalyzer.AnalyzeMethod(method);

        if (amortized != null)
        {
            return amortized;
        }

        // Fall back to regular analysis
        return extractor.AnalyzeMethod(method);
    }

    /// <summary>
    /// Analyzes a loop containing BCL calls with amortized complexity.
    /// </summary>
    public static ComplexityExpression AnalyzeLoopWithAmortization(
        ComplexityExpression loopBodyComplexity,
        ComplexityExpression iterationCount,
        bool bodyContainsAmortizedOperations)
    {
        if (!bodyContainsAmortizedOperations)
        {
            // Regular multiplication: iterations × body
            return new BinaryOperationComplexity(iterationCount, BinaryOp.Multiply, loopBodyComplexity);
        }

        // For amortized operations in a loop, the total is still iterations × amortized cost
        // because the amortized bound holds over any sequence of operations
        if (loopBodyComplexity is AmortizedComplexity amortized)
        {
            return new AmortizedComplexity
            {
                AmortizedCost = new BinaryOperationComplexity(iterationCount, BinaryOp.Multiply, amortized.AmortizedCost),
                WorstCaseCost = new BinaryOperationComplexity(iterationCount, BinaryOp.Multiply, amortized.WorstCaseCost),
                Method = amortized.Method,
                Potential = amortized.Potential,
                Description = $"Loop of {iterationCount.ToBigONotation()} iterations"
            };
        }

        return new BinaryOperationComplexity(iterationCount, BinaryOp.Multiply, loopBodyComplexity);
    }
}
