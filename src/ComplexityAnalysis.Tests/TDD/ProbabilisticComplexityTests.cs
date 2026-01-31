// Copyright (c) 2024. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using Xunit;

namespace ComplexityAnalysis.Tests.TDD;

/// <summary>
/// Tests for probabilistic complexity analysis (M17).
/// </summary>
public class ProbabilisticComplexityTests
{
    #region Core ProbabilisticComplexity Type Tests

    [Fact]
    public void ProbabilisticComplexity_QuickSortLike_ReturnsCorrectExpectedAndWorstCase()
    {
        // Arrange & Act
        var complexity = ProbabilisticComplexity.QuickSortLike(Variable.N);

        // Assert
        Assert.NotNull(complexity.ExpectedComplexity);
        Assert.NotNull(complexity.WorstCaseComplexity);
        Assert.Equal(RandomnessSource.InputDistribution, complexity.Source);
        Assert.Equal(ProbabilityDistribution.Uniform, complexity.Distribution);

        // Expected should be O(n log n)
        var expectedNotation = complexity.ExpectedComplexity.ToBigONotation();
        Assert.Contains("log", expectedNotation);

        // Worst case should be O(n²)
        var worstNotation = complexity.WorstCaseComplexity.ToBigONotation();
        Assert.Contains("²", worstNotation);
    }

    [Fact]
    public void ProbabilisticComplexity_HashTableLookup_ExpectedConstantWorstLinear()
    {
        // Arrange & Act
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);

        // Assert
        Assert.Equal(RandomnessSource.HashFunction, complexity.Source);
        Assert.Contains("simple uniform hashing", complexity.Assumptions.FirstOrDefault() ?? "");

        // Expected is O(1)
        Assert.Equal("O(1)", complexity.ExpectedComplexity.ToBigONotation());

        // Worst is O(n)
        Assert.Contains("n", complexity.WorstCaseComplexity.ToBigONotation());
    }

    [Fact]
    public void ProbabilisticComplexity_RandomizedSelection_ExpectedLinearWorstQuadratic()
    {
        // Arrange & Act
        var complexity = ProbabilisticComplexity.RandomizedSelection(Variable.N);

        // Assert
        Assert.Equal(RandomnessSource.AlgorithmRandomness, complexity.Source);
        Assert.Equal(ProbabilityDistribution.Geometric, complexity.Distribution);

        // Expected is O(n)
        var expected = complexity.ExpectedComplexity.ToBigONotation();
        Assert.Contains("n", expected);
        Assert.DoesNotContain("²", expected);

        // Worst is O(n²)
        var worst = complexity.WorstCaseComplexity.ToBigONotation();
        Assert.Contains("²", worst);
    }

    [Fact]
    public void ProbabilisticComplexity_SkipListOperation_ExpectedLogWorstLinear()
    {
        // Arrange & Act
        var complexity = ProbabilisticComplexity.SkipListOperation(Variable.N);

        // Assert
        Assert.NotNull(complexity.HighProbability);
        Assert.Equal(ProbabilityDistribution.Geometric, complexity.Distribution);

        // Expected is O(log n)
        var expected = complexity.ExpectedComplexity.ToBigONotation();
        Assert.Contains("log", expected);

        // High probability bound exists
        Assert.NotNull(complexity.HighProbability);
        Assert.Equal("1 - 1/n", complexity.HighProbability.ProbabilityExpression);
    }

    [Fact]
    public void ProbabilisticComplexity_BloomFilter_ConstantComplexity()
    {
        // Arrange
        const int hashFunctionCount = 5;

        // Act
        var complexity = ProbabilisticComplexity.BloomFilterLookup(hashFunctionCount);

        // Assert
        Assert.Equal(RandomnessSource.HashFunction, complexity.Source);
        Assert.Equal(ProbabilityDistribution.HighProbabilityBound, complexity.Distribution);

        // Both expected and worst are O(k) = O(5)
        var expected = complexity.ExpectedComplexity as ConstantComplexity;
        Assert.NotNull(expected);
        Assert.Equal(hashFunctionCount, expected.Value);
    }

    [Fact]
    public void ProbabilisticComplexity_MonteCarlo_IncludesErrorProbability()
    {
        // Arrange
        var baseComplexity = PolyLogComplexity.NLogN(Variable.N);
        const double errorProbability = 0.01;

        // Act
        var complexity = ProbabilisticComplexity.MonteCarlo(baseComplexity, errorProbability, "Primality test");

        // Assert
        Assert.Equal(RandomnessSource.MonteCarlo, complexity.Source);
        Assert.NotNull(complexity.HighProbability);
        Assert.Equal(0.99, complexity.HighProbability.Probability, 5);
        Assert.Contains("error probability", complexity.Assumptions.FirstOrDefault() ?? "");
    }

    #endregion

    #region ToBigONotation Tests

    [Fact]
    public void ToBigONotation_ExpectedEqualsWorst_ReturnsSimpleNotation()
    {
        // Arrange
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = new LinearComplexity(1.0, Variable.N),
            WorstCaseComplexity = new LinearComplexity(1.0, Variable.N),
            Source = RandomnessSource.InputDistribution
        };

        // Act
        var notation = complexity.ToBigONotation();

        // Assert
        Assert.Equal("O(n)", notation);
    }

    [Fact]
    public void ToBigONotation_ExpectedDiffersFromWorst_ReturnsBothNotations()
    {
        // Arrange
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);

        // Act
        var notation = complexity.ToBigONotation();

        // Assert
        Assert.Contains("E[", notation);
        Assert.Contains("W[", notation);
    }

    #endregion

    #region Visitor Tests

    [Fact]
    public void Accept_WithProbabilisticVisitor_CallsVisitProbabilistic()
    {
        // Arrange
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);
        var visitor = new TestProbabilisticVisitor();

        // Act
        var result = complexity.Accept(visitor);

        // Assert
        Assert.True(result);
        Assert.True(visitor.VisitedProbabilistic);
    }

    [Fact]
    public void Accept_WithNonProbabilisticVisitor_FallsBackToExpectedComplexity()
    {
        // Arrange
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);
        var visitor = new TestBasicVisitor();

        // Act
        var result = complexity.Accept(visitor);

        // Assert
        // Falls back to visiting the expected complexity (ConstantComplexity)
        Assert.Equal("Constant", result);
    }

    private class TestProbabilisticVisitor : IProbabilisticComplexityVisitor<bool>
    {
        public bool VisitedProbabilistic { get; private set; }

        public bool VisitProbabilistic(ProbabilisticComplexity complexity)
        {
            VisitedProbabilistic = true;
            return true;
        }

        public bool Visit(ConstantComplexity c) => false;
        public bool Visit(VariableComplexity c) => false;
        public bool Visit(LinearComplexity c) => false;
        public bool Visit(PolynomialComplexity c) => false;
        public bool Visit(LogarithmicComplexity c) => false;
        public bool Visit(ExponentialComplexity c) => false;
        public bool Visit(FactorialComplexity c) => false;
        public bool Visit(BinaryOperationComplexity c) => false;
        public bool Visit(ConditionalComplexity c) => false;
        public bool Visit(PowerComplexity c) => false;
        public bool Visit(LogOfComplexity c) => false;
        public bool Visit(ExponentialOfComplexity c) => false;
        public bool Visit(FactorialOfComplexity c) => false;
        public bool Visit(PolyLogComplexity c) => false;
        public bool Visit(RecurrenceComplexity c) => false;
        public bool VisitUnknown(ComplexityExpression c) => false;
    }

    private class TestBasicVisitor : IComplexityVisitor<string>
    {
        public string Visit(ConstantComplexity c) => "Constant";
        public string Visit(VariableComplexity c) => "Variable";
        public string Visit(LinearComplexity c) => "Linear";
        public string Visit(PolynomialComplexity c) => "Polynomial";
        public string Visit(LogarithmicComplexity c) => "Logarithmic";
        public string Visit(ExponentialComplexity c) => "Exponential";
        public string Visit(FactorialComplexity c) => "Factorial";
        public string Visit(BinaryOperationComplexity c) => "Binary";
        public string Visit(ConditionalComplexity c) => "Conditional";
        public string Visit(PowerComplexity c) => "Power";
        public string Visit(LogOfComplexity c) => "LogOf";
        public string Visit(ExponentialOfComplexity c) => "ExponentialOf";
        public string Visit(FactorialOfComplexity c) => "FactorialOf";
        public string Visit(PolyLogComplexity c) => "PolyLog";
        public string Visit(RecurrenceComplexity c) => "Recurrence";
        public string VisitUnknown(ComplexityExpression c) => "Unknown";
    }

    #endregion

    #region Substitute Tests

    [Fact]
    public void Substitute_ReplacesVariableInAllComplexities()
    {
        // Arrange
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);
        var replacement = new LinearComplexity(2.0, Variable.M);

        // Act
        var substituted = complexity.Substitute(Variable.N, replacement) as ProbabilisticComplexity;

        // Assert
        Assert.NotNull(substituted);
        // The worst case (linear in N) should now be substituted
        Assert.Contains(Variable.M, substituted.WorstCaseComplexity.FreeVariables);
    }

    #endregion

    #region FreeVariables Tests

    [Fact]
    public void FreeVariables_ContainsAllVariablesFromAllComplexities()
    {
        // Arrange
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = new LinearComplexity(1.0, Variable.N),
            WorstCaseComplexity = new LinearComplexity(1.0, Variable.M),
            BestCaseComplexity = ConstantComplexity.One,
            Source = RandomnessSource.InputDistribution
        };

        // Act
        var freeVars = complexity.FreeVariables;

        // Assert
        Assert.Contains(Variable.N, freeVars);
        Assert.Contains(Variable.M, freeVars);
    }

    #endregion

    #region Evaluate Tests

    [Fact]
    public void Evaluate_ReturnsExpectedComplexityValue()
    {
        // Arrange
        var complexity = ProbabilisticComplexity.HashTableLookup(Variable.N);
        var bindings = new Dictionary<Variable, double> { { Variable.N, 100 } };

        // Act
        var result = complexity.Evaluate(bindings);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Value); // Expected O(1)
    }

    #endregion

    #region Randomness Source Tests

    [Theory]
    [InlineData(RandomnessSource.InputDistribution)]
    [InlineData(RandomnessSource.AlgorithmRandomness)]
    [InlineData(RandomnessSource.MonteCarlo)]
    [InlineData(RandomnessSource.HashFunction)]
    [InlineData(RandomnessSource.Mixed)]
    public void RandomnessSource_AllEnumValuesSupported(RandomnessSource source)
    {
        // Arrange & Act
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = ConstantComplexity.One,
            Source = source
        };

        // Assert
        Assert.Equal(source, complexity.Source);
    }

    #endregion

    #region Probability Distribution Tests

    [Theory]
    [InlineData(ProbabilityDistribution.Uniform)]
    [InlineData(ProbabilityDistribution.Exponential)]
    [InlineData(ProbabilityDistribution.Geometric)]
    [InlineData(ProbabilityDistribution.HighProbabilityBound)]
    [InlineData(ProbabilityDistribution.InputDependent)]
    [InlineData(ProbabilityDistribution.Unknown)]
    public void ProbabilityDistribution_AllEnumValuesSupported(ProbabilityDistribution distribution)
    {
        // Arrange & Act
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = ConstantComplexity.One,
            Distribution = distribution
        };

        // Assert
        Assert.Equal(distribution, complexity.Distribution);
    }

    #endregion

    #region HighProbabilityBound Tests

    [Fact]
    public void HighProbabilityBound_DefaultProbability_Is99Percent()
    {
        // Arrange & Act
        var bound = new HighProbabilityBound
        {
            Bound = new LogarithmicComplexity(1.0, Variable.N)
        };

        // Assert
        Assert.Equal(0.99, bound.Probability);
    }

    [Fact]
    public void HighProbabilityBound_WithCustomProbability_PreservesProbability()
    {
        // Arrange
        var bound = new HighProbabilityBound
        {
            Bound = new LogarithmicComplexity(1.0, Variable.N),
            Probability = 0.9999,
            ProbabilityExpression = "1 - 1/n²"
        };

        // Assert
        Assert.Equal(0.9999, bound.Probability);
        Assert.Equal("1 - 1/n²", bound.ProbabilityExpression);
    }

    #endregion

    #region Integration with Factory Methods

    [Fact]
    public void QuickSortLike_WithAlgorithmRandomness_HasDifferentAssumptions()
    {
        // Arrange & Act
        var inputDistribution = ProbabilisticComplexity.QuickSortLike(Variable.N, RandomnessSource.InputDistribution);
        var algorithmRandom = ProbabilisticComplexity.QuickSortLike(Variable.N, RandomnessSource.AlgorithmRandomness);

        // Assert
        Assert.Contains("input permutation", inputDistribution.Assumptions.FirstOrDefault() ?? "");
        Assert.Contains("pivot selection", algorithmRandom.Assumptions.FirstOrDefault() ?? "");
    }

    #endregion

    #region Complex Scenario Tests

    [Fact]
    public void ProbabilisticComplexity_CompositionOfMultiplePatterns()
    {
        // Scenario: A method that uses both hash table lookups and randomized sorting
        // The overall complexity should consider both patterns

        var hashLookup = ProbabilisticComplexity.HashTableLookup(Variable.N);
        var sort = ProbabilisticComplexity.QuickSortLike(Variable.N);

        // In a real scenario, these would be composed
        // For now, verify both can coexist
        Assert.NotEqual(hashLookup.Source, sort.Source);
        Assert.NotEqual(hashLookup.ExpectedComplexity.ToBigONotation(),
                       sort.ExpectedComplexity.ToBigONotation());
    }

    [Fact]
    public void ProbabilisticComplexity_WithVariance_PreservesVarianceInformation()
    {
        // Arrange
        var variance = new LinearComplexity(1.0, Variable.N);
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = new LinearComplexity(1.0, Variable.N),
            Variance = variance,
            Source = RandomnessSource.HashFunction
        };

        // Assert
        Assert.NotNull(complexity.Variance);
        Assert.Contains(Variable.N, complexity.Variance.FreeVariables);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ProbabilisticComplexity_WithoutOptionalProperties_HasSensibleDefaults()
    {
        // Arrange & Act
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = ConstantComplexity.One
        };

        // Assert
        Assert.Equal(RandomnessSource.InputDistribution, complexity.Source);
        Assert.Equal(ProbabilityDistribution.Uniform, complexity.Distribution);
        Assert.Null(complexity.BestCaseComplexity);
        Assert.Null(complexity.Variance);
        Assert.Null(complexity.HighProbability);
        Assert.Empty(complexity.Assumptions);
        Assert.Null(complexity.Description);
    }

    [Fact]
    public void ProbabilisticComplexity_WithEmptyAssumptions_ReturnsEmptyArray()
    {
        // Arrange
        var complexity = new ProbabilisticComplexity
        {
            ExpectedComplexity = ConstantComplexity.One,
            WorstCaseComplexity = ConstantComplexity.One
        };

        // Assert
        Assert.NotNull(complexity.Assumptions);
        Assert.Empty(complexity.Assumptions);
    }

    #endregion
}
