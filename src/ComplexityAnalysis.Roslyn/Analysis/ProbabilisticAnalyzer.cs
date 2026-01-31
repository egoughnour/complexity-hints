// Copyright (c) 2024. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Roslyn.Analysis;

/// <summary>
/// Detects probabilistic patterns in code and produces probabilistic complexity analysis.
/// </summary>
public sealed class ProbabilisticAnalyzer
{
    private readonly SemanticModel _semanticModel;

    public ProbabilisticAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Analyzes a method for probabilistic complexity patterns.
    /// </summary>
    public ProbabilisticAnalysisResult Analyze(MethodDeclarationSyntax method, AnalysisContext context)
    {
        var patterns = new List<ProbabilisticPattern>();

        // Detect all probabilistic patterns in the method
        var walker = new ProbabilisticPatternWalker(_semanticModel, context);
        walker.Visit(method);
        patterns.AddRange(walker.Patterns);

        if (patterns.Count == 0)
        {
            return ProbabilisticAnalysisResult.NoProbabilisticPatterns();
        }

        // Combine patterns into overall probabilistic complexity
        var combinedComplexity = CombinePatterns(patterns, context);

        return new ProbabilisticAnalysisResult
        {
            Success = true,
            ProbabilisticComplexity = combinedComplexity,
            DetectedPatterns = patterns.ToImmutableArray(),
            Notes = GenerateNotes(patterns)
        };
    }

    /// <summary>
    /// Analyzes a specific expression for probabilistic characteristics.
    /// </summary>
    public ProbabilisticPattern? AnalyzeExpression(ExpressionSyntax expression, AnalysisContext context)
    {
        // Check for Random.Next() or similar
        if (expression is InvocationExpressionSyntax invocation)
        {
            return AnalyzeInvocation(invocation, context);
        }

        // Check for hash-based operations
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            return AnalyzeMemberAccess(memberAccess, context);
        }

        return null;
    }

    private ProbabilisticPattern? AnalyzeInvocation(InvocationExpressionSyntax invocation, AnalysisContext context)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol method)
            return null;

        var typeName = method.ContainingType?.Name ?? "";
        var methodName = method.Name;

        // Random number generation
        if (IsRandomMethod(typeName, methodName))
        {
            return new ProbabilisticPattern
            {
                Type = ProbabilisticPatternType.RandomNumberGeneration,
                Source = RandomnessSource.AlgorithmRandomness,
                Distribution = ProbabilityDistribution.Uniform,
                Location = invocation.GetLocation(),
                Description = $"Random number generation: {typeName}.{methodName}"
            };
        }

        // Hash-based operations
        if (IsHashMethod(typeName, methodName))
        {
            return new ProbabilisticPattern
            {
                Type = ProbabilisticPatternType.HashFunction,
                Source = RandomnessSource.HashFunction,
                Distribution = ProbabilityDistribution.Uniform,
                Location = invocation.GetLocation(),
                Description = $"Hash function: {typeName}.{methodName}",
                Assumptions = ImmutableArray.Create("simple uniform hashing assumption")
            };
        }

        // Shuffle operations
        if (IsShuffleMethod(typeName, methodName))
        {
            return new ProbabilisticPattern
            {
                Type = ProbabilisticPatternType.Shuffle,
                Source = RandomnessSource.AlgorithmRandomness,
                Distribution = ProbabilityDistribution.Uniform,
                Location = invocation.GetLocation(),
                Description = $"Shuffle operation: {typeName}.{methodName}",
                ExpectedComplexity = new LinearComplexity(1.0, Variable.N),
                WorstCaseComplexity = new LinearComplexity(1.0, Variable.N)
            };
        }

        // QuickSort-like operations
        if (IsPivotSelectionMethod(typeName, methodName))
        {
            return new ProbabilisticPattern
            {
                Type = ProbabilisticPatternType.PivotSelection,
                Source = RandomnessSource.AlgorithmRandomness,
                Distribution = ProbabilityDistribution.Uniform,
                Location = invocation.GetLocation(),
                Description = "Random pivot selection",
                ExpectedComplexity = PolyLogComplexity.NLogN(Variable.N),
                WorstCaseComplexity = PolynomialComplexity.OfDegree(2, Variable.N)
            };
        }

        // Monte Carlo methods
        if (IsMonteCarloMethod(typeName, methodName))
        {
            return new ProbabilisticPattern
            {
                Type = ProbabilisticPatternType.MonteCarlo,
                Source = RandomnessSource.MonteCarlo,
                Distribution = ProbabilityDistribution.HighProbabilityBound,
                Location = invocation.GetLocation(),
                Description = $"Monte Carlo algorithm: {typeName}.{methodName}"
            };
        }

        return null;
    }

    private ProbabilisticPattern? AnalyzeMemberAccess(MemberAccessExpressionSyntax memberAccess, AnalysisContext context)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(memberAccess);
        
        // Check for dictionary/hashset access patterns
        if (symbolInfo.Symbol is IPropertySymbol property)
        {
            var typeName = property.ContainingType?.Name ?? "";
            
            if (IsHashBasedCollection(typeName))
            {
                return new ProbabilisticPattern
                {
                    Type = ProbabilisticPatternType.HashTableOperation,
                    Source = RandomnessSource.HashFunction,
                    Distribution = ProbabilityDistribution.Uniform,
                    Location = memberAccess.GetLocation(),
                    Description = $"Hash table access: {typeName}",
                    ExpectedComplexity = ConstantComplexity.One,
                    WorstCaseComplexity = new LinearComplexity(1.0, Variable.N),
                    Assumptions = ImmutableArray.Create("simple uniform hashing assumption")
                };
            }
        }

        return null;
    }

    private ProbabilisticComplexity CombinePatterns(List<ProbabilisticPattern> patterns, AnalysisContext context)
    {
        // Find the dominant pattern
        var dominantPattern = patterns
            .OrderByDescending(p => GetPatternPriority(p.Type))
            .First();

        var expectedComplexity = dominantPattern.ExpectedComplexity ?? ConstantComplexity.One;
        var worstCaseComplexity = dominantPattern.WorstCaseComplexity ?? expectedComplexity;

        // Combine sources
        var sources = patterns.Select(p => p.Source).Distinct().ToList();
        var combinedSource = sources.Count == 1 ? sources[0] : RandomnessSource.Mixed;

        // Combine assumptions
        var assumptions = patterns
            .SelectMany(p => p.Assumptions)
            .Distinct()
            .ToImmutableArray();

        return new ProbabilisticComplexity
        {
            ExpectedComplexity = expectedComplexity,
            WorstCaseComplexity = worstCaseComplexity,
            Source = combinedSource,
            Distribution = dominantPattern.Distribution,
            Assumptions = assumptions,
            Description = $"Combined probabilistic analysis from {patterns.Count} pattern(s)"
        };
    }

    private static int GetPatternPriority(ProbabilisticPatternType type) => type switch
    {
        ProbabilisticPatternType.MonteCarlo => 10,
        ProbabilisticPatternType.PivotSelection => 9,
        ProbabilisticPatternType.Shuffle => 8,
        ProbabilisticPatternType.RandomizedSelection => 7,
        ProbabilisticPatternType.SkipList => 6,
        ProbabilisticPatternType.HashTableOperation => 5,
        ProbabilisticPatternType.HashFunction => 4,
        ProbabilisticPatternType.RandomNumberGeneration => 3,
        ProbabilisticPatternType.BloomFilter => 2,
        _ => 0
    };

    private static string GenerateNotes(List<ProbabilisticPattern> patterns)
    {
        var notes = new List<string>();
        
        foreach (var pattern in patterns.DistinctBy(p => p.Type))
        {
            notes.Add(pattern.Description);
        }

        return string.Join("; ", notes);
    }

    private static bool IsRandomMethod(string typeName, string methodName) =>
        (typeName is "Random" or "RandomNumberGenerator") &&
        methodName is "Next" or "NextInt64" or "NextDouble" or "NextSingle" or "NextBytes" or "GetBytes" or "GetInt32" or "GetInt64";

    private static bool IsHashMethod(string typeName, string methodName) =>
        (typeName is "HashCode" && methodName is "Add" or "Combine" or "ToHashCode") ||
        (methodName == "GetHashCode");

    private static bool IsShuffleMethod(string typeName, string methodName) =>
        (typeName is "Random" && methodName == "Shuffle") ||
        (methodName is "Shuffle" or "RandomShuffle" or "FisherYates");

    private static bool IsPivotSelectionMethod(string typeName, string methodName) =>
        methodName.Contains("Pivot", StringComparison.OrdinalIgnoreCase) ||
        methodName.Contains("Partition", StringComparison.OrdinalIgnoreCase);

    private static bool IsMonteCarloMethod(string typeName, string methodName) =>
        methodName.Contains("MonteCarlo", StringComparison.OrdinalIgnoreCase) ||
        methodName.Contains("Probabilistic", StringComparison.OrdinalIgnoreCase) ||
        methodName.Contains("MillerRabin", StringComparison.OrdinalIgnoreCase);

    private static bool IsHashBasedCollection(string typeName) =>
        typeName is "Dictionary" or "HashSet" or "ConcurrentDictionary" or "ConcurrentBag" ||
        typeName.Contains("Hash") ||
        typeName.EndsWith("`2") && typeName.StartsWith("Dictionary") ||
        typeName.EndsWith("`1") && typeName.StartsWith("HashSet");

    /// <summary>
    /// Walker to find probabilistic patterns in code.
    /// </summary>
    private sealed class ProbabilisticPatternWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly AnalysisContext _context;
        private readonly List<ProbabilisticPattern> _patterns = new();

        public IReadOnlyList<ProbabilisticPattern> Patterns => _patterns;

        public ProbabilisticPatternWalker(SemanticModel semanticModel, AnalysisContext context)
        {
            _semanticModel = semanticModel;
            _context = context;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var analyzer = new ProbabilisticAnalyzer(_semanticModel);
            var pattern = analyzer.AnalyzeInvocation(node, _context);
            if (pattern is not null)
            {
                _patterns.Add(pattern);
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            // Check for dictionary/hash table indexer access
            var typeInfo = _semanticModel.GetTypeInfo(node.Expression);
            var typeName = typeInfo.Type?.Name ?? "";

            if (IsHashBasedCollection(typeName))
            {
                _patterns.Add(new ProbabilisticPattern
                {
                    Type = ProbabilisticPatternType.HashTableOperation,
                    Source = RandomnessSource.HashFunction,
                    Distribution = ProbabilityDistribution.Uniform,
                    Location = node.GetLocation(),
                    Description = $"Hash table indexer access: {typeName}",
                    ExpectedComplexity = ConstantComplexity.One,
                    WorstCaseComplexity = new LinearComplexity(1.0, Variable.N),
                    Assumptions = ImmutableArray.Create("simple uniform hashing assumption")
                });
            }

            base.VisitElementAccessExpression(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            // Detect randomized loop patterns (e.g., QuickSort partition)
            DetectRandomizedLoopPattern(node);
            base.VisitForStatement(node);
        }

        private void DetectRandomizedLoopPattern(ForStatementSyntax forLoop)
        {
            // Check if loop body contains random operations that affect loop iteration
            var hasRandomInCondition = forLoop.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv =>
                {
                    var symbol = _semanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                    return symbol?.ContainingType?.Name == "Random";
                });

            if (hasRandomInCondition)
            {
                _patterns.Add(new ProbabilisticPattern
                {
                    Type = ProbabilisticPatternType.RandomizedLoop,
                    Source = RandomnessSource.AlgorithmRandomness,
                    Distribution = ProbabilityDistribution.Geometric,
                    Location = forLoop.GetLocation(),
                    Description = "Randomized loop pattern detected"
                });
            }
        }
    }
}

/// <summary>
/// Result of probabilistic complexity analysis.
/// </summary>
public record ProbabilisticAnalysisResult
{
    /// <summary>
    /// Whether the analysis found probabilistic patterns.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The combined probabilistic complexity.
    /// </summary>
    public ProbabilisticComplexity? ProbabilisticComplexity { get; init; }

    /// <summary>
    /// All detected probabilistic patterns.
    /// </summary>
    public ImmutableArray<ProbabilisticPattern> DetectedPatterns { get; init; } = ImmutableArray<ProbabilisticPattern>.Empty;

    /// <summary>
    /// Additional notes about the analysis.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Error message if analysis failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a result indicating no probabilistic patterns were found.
    /// </summary>
    public static ProbabilisticAnalysisResult NoProbabilisticPatterns() => new()
    {
        Success = false,
        Notes = "No probabilistic patterns detected"
    };
}

/// <summary>
/// A detected probabilistic pattern in code.
/// </summary>
public record ProbabilisticPattern
{
    /// <summary>
    /// The type of probabilistic pattern detected.
    /// </summary>
    public required ProbabilisticPatternType Type { get; init; }

    /// <summary>
    /// The source of randomness in this pattern.
    /// </summary>
    public RandomnessSource Source { get; init; }

    /// <summary>
    /// The probability distribution of this pattern.
    /// </summary>
    public ProbabilityDistribution Distribution { get; init; }

    /// <summary>
    /// The location in code where this pattern was detected.
    /// </summary>
    public Location? Location { get; init; }

    /// <summary>
    /// Description of the pattern.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The expected complexity for this pattern.
    /// </summary>
    public ComplexityExpression? ExpectedComplexity { get; init; }

    /// <summary>
    /// The worst-case complexity for this pattern.
    /// </summary>
    public ComplexityExpression? WorstCaseComplexity { get; init; }

    /// <summary>
    /// Assumptions required for the expected complexity.
    /// </summary>
    public ImmutableArray<string> Assumptions { get; init; } = ImmutableArray<string>.Empty;
}

/// <summary>
/// Types of probabilistic patterns that can be detected.
/// </summary>
public enum ProbabilisticPatternType
{
    /// <summary>
    /// Random number generation (Random.Next, etc.)
    /// </summary>
    RandomNumberGeneration,

    /// <summary>
    /// Hash function computation (GetHashCode, HashCode.Combine)
    /// </summary>
    HashFunction,

    /// <summary>
    /// Hash table operations (Dictionary, HashSet access)
    /// </summary>
    HashTableOperation,

    /// <summary>
    /// Random shuffle operations (Fisher-Yates, etc.)
    /// </summary>
    Shuffle,

    /// <summary>
    /// Random pivot selection (QuickSort-like)
    /// </summary>
    PivotSelection,

    /// <summary>
    /// Randomized selection (Quickselect)
    /// </summary>
    RandomizedSelection,

    /// <summary>
    /// Skip list operations
    /// </summary>
    SkipList,

    /// <summary>
    /// Bloom filter operations
    /// </summary>
    BloomFilter,

    /// <summary>
    /// Monte Carlo algorithm patterns
    /// </summary>
    MonteCarlo,

    /// <summary>
    /// Loop with randomized iteration count
    /// </summary>
    RandomizedLoop,

    /// <summary>
    /// Other probabilistic pattern
    /// </summary>
    Other
}
