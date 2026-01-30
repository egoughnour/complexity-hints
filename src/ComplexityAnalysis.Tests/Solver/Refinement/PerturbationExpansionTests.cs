using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver.Refinement;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver.Refinement;

public class PerturbationExpansionTests
{
    private readonly PerturbationExpansion _perturbation = PerturbationExpansion.Instance;

    [Fact]
    public void DetectBoundary_NearCase1To2_DetectsCorrectly()
    {
        // Arrange: T(n) = 2T(n/2) + n^0.9 (f is slightly below n^1)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolyLogComplexity.Polynomial(0.9, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case1,
            2, 2, 1,
            new ExpressionClassification(ExpressionForm.Polynomial, 0.9, null, 1.0),
            PolyLogComplexity.Polynomial(1, Variable.N));

        // Act
        var boundary = _perturbation.DetectBoundary(recurrence, theoremResult);

        // Assert
        Assert.NotNull(boundary);
        Assert.Equal(BoundaryCaseType.MasterTheoremCase1To2, boundary.Type);
    }

    [Fact]
    public void DetectBoundary_NearCase2To3_DetectsCorrectly()
    {
        // Arrange: T(n) = 2T(n/2) + n^1.05 (f is slightly above n^1)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolyLogComplexity.Polynomial(1.05, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case3,
            2, 2, 1,
            new ExpressionClassification(ExpressionForm.Polynomial, 1.05, null, 1.0),
            PolyLogComplexity.Polynomial(1.05, Variable.N));

        // Act
        var boundary = _perturbation.DetectBoundary(recurrence, theoremResult);

        // Assert
        Assert.NotNull(boundary);
        Assert.Equal(BoundaryCaseType.MasterTheoremCase2To3, boundary.Type);
    }

    [Fact]
    public void DetectBoundary_AkraBazziNearInteger_DetectsCorrectly()
    {
        // Arrange: Akra-Bazzi with p ≈ 1.02
        var recurrence = new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(1, 0.5), new RecurrenceRelationTerm(1, 0.5) },
            Variable.N,
            new LinearComplexity(1, Variable.N));

        var theoremResult = new AkraBazziApplicable(
            1.02, // Near integer 1
            PolyLogComplexity.Polynomial(1.02, Variable.N));

        // Act
        var boundary = _perturbation.DetectBoundary(recurrence, theoremResult);

        // Assert
        Assert.NotNull(boundary);
        Assert.Equal(BoundaryCaseType.AkraBazziIntegerExponent, boundary.Type);
    }

    [Fact]
    public void DetectBoundary_NotNearBoundary_ReturnsNull()
    {
        // Arrange: T(n) = 2T(n/2) + n^2 (clearly Case 3)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolynomialComplexity.OfDegree(2, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case3,
            2, 2, 1,
            new ExpressionClassification(ExpressionForm.Polynomial, 2, null, 1.0),
            PolynomialComplexity.OfDegree(2, Variable.N));

        // Act
        var boundary = _perturbation.DetectBoundary(recurrence, theoremResult);

        // Assert
        Assert.Null(boundary);
    }

    [Fact]
    public void ExpandNearBoundary_Case1To2_ProducesSolution()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolyLogComplexity.Polynomial(0.95, Variable.N), Variable.N);

        var boundary = new BoundaryCase
        {
            Type = BoundaryCaseType.MasterTheoremCase1To2,
            LogBA = 1.0,
            FDegree = 0.95,
            Delta = -0.05
        };

        // Act
        var result = _perturbation.ExpandNearBoundary(recurrence, boundary);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
        Assert.True(result.Confidence > 0.7);
    }

    [Fact]
    public void ExpandNearBoundary_Case2To3_ProducesSolution()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolyLogComplexity.Polynomial(1.05, Variable.N), Variable.N);

        var boundary = new BoundaryCase
        {
            Type = BoundaryCaseType.MasterTheoremCase2To3,
            LogBA = 1.0,
            FDegree = 1.05,
            Delta = 0.05
        };

        // Act
        var result = _perturbation.ExpandNearBoundary(recurrence, boundary);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
    }

    [Fact]
    public void TaylorExpandIntegral_NearSingularity_ProducesTerms()
    {
        // Arrange: g(n) = n, p near 1
        var g = new VariableComplexity(Variable.N);
        var p = 1.01;

        // Act
        var result = _perturbation.TaylorExpandIntegral(g, Variable.N, p, 1.0);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Terms.Count > 0);
    }

    [Fact]
    public void TaylorExpandIntegral_AtSingularity_ProducesLogTerm()
    {
        // Arrange: g(n) = n, p = 1 exactly
        var g = new VariableComplexity(Variable.N);
        var p = 1.0;

        // Act
        var result = _perturbation.TaylorExpandIntegral(g, Variable.N, p, 1.0);

        // Assert
        Assert.True(result.Success);
        // Should have logarithmic leading term
        Assert.True(result.Terms.Any(t => t.Expression is LogarithmicComplexity));
    }

    [Fact]
    public void ExpandNearBoundary_AkraBazziInteger_ProducesPolyLog()
    {
        // Arrange
        var recurrence = new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(1, 0.5), new RecurrenceRelationTerm(1, 0.5) },
            Variable.N,
            new LinearComplexity(1, Variable.N));

        var boundary = new BoundaryCase
        {
            Type = BoundaryCaseType.AkraBazziIntegerExponent,
            CriticalExponent = 1.005,
            NearestInteger = 1,
            Delta = 0.005
        };

        // Act
        var result = _perturbation.ExpandNearBoundary(recurrence, boundary);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Solution);
        // Should be polylog form
        Assert.True(result.Solution is PolyLogComplexity);
    }
}
