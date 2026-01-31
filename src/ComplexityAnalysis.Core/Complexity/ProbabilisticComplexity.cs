// Copyright (c) 2024. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Specifies the source of randomness in a probabilistic algorithm.
/// </summary>
public enum RandomnessSource
{
    /// <summary>
    /// Randomness comes from the input distribution (average-case analysis).
    /// Example: QuickSort with random input permutation.
    /// </summary>
    InputDistribution,

    /// <summary>
    /// Randomness comes from the algorithm itself (Las Vegas algorithms).
    /// Example: Randomized QuickSort with random pivot selection.
    /// </summary>
    AlgorithmRandomness,

    /// <summary>
    /// Monte Carlo algorithms that may produce incorrect results with small probability.
    /// Example: Miller-Rabin primality test.
    /// </summary>
    MonteCarlo,

    /// <summary>
    /// Hash function randomness (universal hashing, expected behavior).
    /// Example: Hash table operations assuming uniform hashing.
    /// </summary>
    HashFunction,

    /// <summary>
    /// Multiple sources of randomness combined.
    /// </summary>
    Mixed
}

/// <summary>
/// Specifies the probability distribution of the complexity.
/// </summary>
public enum ProbabilityDistribution
{
    /// <summary>
    /// Uniform distribution over all inputs.
    /// </summary>
    Uniform,

    /// <summary>
    /// Exponential distribution (common in queueing theory).
    /// </summary>
    Exponential,

    /// <summary>
    /// Geometric distribution (common in randomized algorithms).
    /// </summary>
    Geometric,

    /// <summary>
    /// Bounded/concentrated distribution with high probability guarantees.
    /// </summary>
    HighProbabilityBound,

    /// <summary>
    /// Distribution determined by specific input characteristics.
    /// </summary>
    InputDependent,

    /// <summary>
    /// Unknown or unspecified distribution.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents probabilistic complexity analysis for randomized algorithms.
/// Captures expected (average), best-case, and worst-case complexities along with
/// probability distribution information.
/// </summary>
/// <remarks>
/// This is used for analyzing:
/// - Average-case complexity (QuickSort, hash tables)
/// - Randomized algorithms (randomized QuickSort, randomized selection)
/// - Monte Carlo algorithms (primality testing)
/// - Las Vegas algorithms (randomized algorithms that always produce correct results)
/// </remarks>
public sealed record ProbabilisticComplexity : ComplexityExpression
{
    /// <summary>
    /// Gets the expected (average-case) complexity.
    /// This represents E[T(n)] - the expected running time.
    /// </summary>
    public required ComplexityExpression ExpectedComplexity { get; init; }

    /// <summary>
    /// Gets the worst-case complexity.
    /// This is the upper bound that holds for all inputs/random choices.
    /// </summary>
    public required ComplexityExpression WorstCaseComplexity { get; init; }

    /// <summary>
    /// Gets the best-case complexity.
    /// Optional - when not specified, defaults to constant.
    /// </summary>
    public ComplexityExpression? BestCaseComplexity { get; init; }

    /// <summary>
    /// Gets the source of randomness in the algorithm.
    /// </summary>
    public RandomnessSource Source { get; init; } = RandomnessSource.InputDistribution;

    /// <summary>
    /// Gets the probability distribution of the complexity.
    /// </summary>
    public ProbabilityDistribution Distribution { get; init; } = ProbabilityDistribution.Uniform;

    /// <summary>
    /// Gets the variance of the complexity if known.
    /// Null indicates unknown variance.
    /// </summary>
    public ComplexityExpression? Variance { get; init; }

    /// <summary>
    /// Gets the high-probability bound if applicable.
    /// For algorithms with concentration bounds: Pr[T(n) > bound] ≤ probability.
    /// </summary>
    public HighProbabilityBound? HighProbability { get; init; }

    /// <summary>
    /// Gets any assumptions required for the expected complexity to hold.
    /// Example: "uniform random input permutation", "independent hash function"
    /// </summary>
    public ImmutableArray<string> Assumptions { get; init; } = ImmutableArray<string>.Empty;

    /// <summary>
    /// Gets an optional description of the probabilistic analysis.
    /// </summary>
    public string? Description { get; init; }

    /// <inheritdoc />
    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is IProbabilisticComplexityVisitor<T> probabilisticVisitor
            ? probabilisticVisitor.VisitProbabilistic(this)
            : ExpectedComplexity.Accept(visitor);

    /// <inheritdoc />
    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        this with
        {
            ExpectedComplexity = ExpectedComplexity.Substitute(variable, replacement),
            WorstCaseComplexity = WorstCaseComplexity.Substitute(variable, replacement),
            BestCaseComplexity = BestCaseComplexity?.Substitute(variable, replacement),
            Variance = Variance?.Substitute(variable, replacement),
            HighProbability = HighProbability is not null
                ? HighProbability with { Bound = HighProbability.Bound.Substitute(variable, replacement) }
                : null
        };

    /// <inheritdoc />
    public override ImmutableHashSet<Variable> FreeVariables =>
        ExpectedComplexity.FreeVariables
            .Union(WorstCaseComplexity.FreeVariables)
            .Union(BestCaseComplexity?.FreeVariables ?? ImmutableHashSet<Variable>.Empty)
            .Union(Variance?.FreeVariables ?? ImmutableHashSet<Variable>.Empty)
            .Union(HighProbability?.Bound.FreeVariables ?? ImmutableHashSet<Variable>.Empty);

    /// <inheritdoc />
    public override double? Evaluate(IReadOnlyDictionary<Variable, double> bindings) =>
        ExpectedComplexity.Evaluate(bindings);

    /// <inheritdoc />
    public override string ToBigONotation()
    {
        var expected = ExpectedComplexity.ToBigONotation();
        var worst = WorstCaseComplexity.ToBigONotation();

        if (expected == worst)
        {
            return expected;
        }

        return $"E[{expected}], W[{worst}]";
    }

    /// <summary>
    /// Creates a probabilistic complexity with expected O(n log n) and worst O(n²).
    /// Common for randomized sorting algorithms like QuickSort.
    /// </summary>
    public static ProbabilisticComplexity QuickSortLike(Variable n, RandomnessSource source = RandomnessSource.InputDistribution) =>
        new()
        {
            ExpectedComplexity = PolyLogComplexity.NLogN(n),
            WorstCaseComplexity = PolynomialComplexity.OfDegree(2, n),
            BestCaseComplexity = PolyLogComplexity.NLogN(n),
            Source = source,
            Distribution = ProbabilityDistribution.Uniform,
            Assumptions = source == RandomnessSource.InputDistribution
                ? ImmutableArray.Create("uniform random input permutation")
                : ImmutableArray.Create("random pivot selection"),
            Description = "Randomized QuickSort complexity"
        };

    /// <summary>
    /// Creates a probabilistic complexity for hash table operations.
    /// Expected O(1), worst O(n).
    /// </summary>
    public static ProbabilisticComplexity HashTableLookup(Variable n) =>
        new()
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = new LinearComplexity(1.0, n),
            BestCaseComplexity = ConstantComplexity.One,
            Source = RandomnessSource.HashFunction,
            Distribution = ProbabilityDistribution.Uniform,
            Assumptions = ImmutableArray.Create("simple uniform hashing assumption"),
            Description = "Hash table lookup with expected constant time"
        };

    /// <summary>
    /// Creates a probabilistic complexity for randomized selection (Quickselect).
    /// Expected O(n), worst O(n²).
    /// </summary>
    public static ProbabilisticComplexity RandomizedSelection(Variable n) =>
        new()
        {
            ExpectedComplexity = new LinearComplexity(1.0, n),
            WorstCaseComplexity = PolynomialComplexity.OfDegree(2, n),
            BestCaseComplexity = new LinearComplexity(1.0, n),
            Source = RandomnessSource.AlgorithmRandomness,
            Distribution = ProbabilityDistribution.Geometric,
            Assumptions = ImmutableArray.Create("random pivot selection"),
            Description = "Randomized selection (Quickselect) complexity"
        };

    /// <summary>
    /// Creates a probabilistic complexity for skip list operations.
    /// Expected O(log n), worst O(n).
    /// </summary>
    public static ProbabilisticComplexity SkipListOperation(Variable n) =>
        new()
        {
            ExpectedComplexity = new LogarithmicComplexity(1.0, n),
            WorstCaseComplexity = new LinearComplexity(1.0, n),
            BestCaseComplexity = ConstantComplexity.One,
            Source = RandomnessSource.AlgorithmRandomness,
            Distribution = ProbabilityDistribution.Geometric,
            HighProbability = new HighProbabilityBound
            {
                Bound = new LogarithmicComplexity(1.0, n),
                Probability = 0.99, // High probability for typical n
                ProbabilityExpression = "1 - 1/n"
            },
            Description = "Skip list search/insert/delete"
        };

    /// <summary>
    /// Creates a probabilistic complexity for Bloom filter operations.
    /// O(k) where k is the number of hash functions, with false positive probability.
    /// </summary>
    public static ProbabilisticComplexity BloomFilterLookup(int hashFunctionCount) =>
        new()
        {
            ExpectedComplexity = new ConstantComplexity(hashFunctionCount),
            WorstCaseComplexity = new ConstantComplexity(hashFunctionCount),
            BestCaseComplexity = new ConstantComplexity(hashFunctionCount),
            Source = RandomnessSource.HashFunction,
            Distribution = ProbabilityDistribution.HighProbabilityBound,
            Description = $"Bloom filter with {hashFunctionCount} hash functions"
        };

    /// <summary>
    /// Creates a Monte Carlo complexity where the result may be incorrect with some probability.
    /// </summary>
    public static ProbabilisticComplexity MonteCarlo(
        ComplexityExpression complexity,
        double errorProbability,
        string description) =>
        new()
        {
            ExpectedComplexity = complexity,
            WorstCaseComplexity = complexity,
            Source = RandomnessSource.MonteCarlo,
            Distribution = ProbabilityDistribution.HighProbabilityBound,
            HighProbability = new HighProbabilityBound
            {
                Bound = complexity,
                Probability = 1.0 - errorProbability
            },
            Assumptions = ImmutableArray.Create($"error probability ≤ {errorProbability}"),
            Description = description
        };
}

/// <summary>
/// Represents a high-probability bound: Pr[T(n) ≤ bound] ≥ probability.
/// </summary>
public sealed record HighProbabilityBound
{
    /// <summary>
    /// Gets the complexity bound that holds with high probability.
    /// </summary>
    public required ComplexityExpression Bound { get; init; }

    /// <summary>
    /// Gets the probability that the bound holds.
    /// For "with high probability" bounds, this is typically 1 - 1/n^c for some constant c.
    /// </summary>
    public double Probability { get; init; } = 0.99;

    /// <summary>
    /// Gets an optional expression for the probability as a function of n.
    /// Example: 1 - 1/n for bounds that hold "with high probability".
    /// </summary>
    public string? ProbabilityExpression { get; init; }
}

/// <summary>
/// Extension of IComplexityVisitor for probabilistic complexity.
/// </summary>
public interface IProbabilisticComplexityVisitor<out T> : IComplexityVisitor<T>
{
    /// <summary>
    /// Visits a probabilistic complexity expression.
    /// </summary>
    T VisitProbabilistic(ProbabilisticComplexity complexity);
}
