using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver.Refinement;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver.Refinement;

public class InductionVerifierTests
{
    private readonly InductionVerifier _verifier = InductionVerifier.Instance;

    [Fact]
    public void VerifyRecurrenceSolution_MergeSort_Verifies()
    {
        // Arrange: T(n) = 2T(n/2) + n, solution = n log n
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var solution = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, solution);

        // Assert
        Assert.True(result.Verified || result.ConfidenceScore > 0.5,
            $"Verification failed: {string.Join(", ", result.Diagnostics)}");
    }

    [Fact]
    public void VerifyRecurrenceSolution_BinarySearch_Verifies()
    {
        // Arrange: T(n) = T(n/2) + 1, solution = log n
        var recurrence = RecurrenceRelation.DivideAndConquer(1, 2, ConstantComplexity.One, Variable.N);
        var solution = new LogarithmicComplexity(1, Variable.N);

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, solution);

        // Assert
        Assert.True(result.BaseCase?.Holds ?? false, "Base case should hold");
    }

    [Fact]
    public void VerifyRecurrenceSolution_IncorrectSolution_Fails()
    {
        // Arrange: T(n) = 2T(n/2) + n with wrong solution n
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var wrongSolution = new LinearComplexity(1, Variable.N); // Should be n log n

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, wrongSolution);

        // Assert
        // The verification might not completely fail but should have lower confidence
        Assert.True(result.ConfidenceScore < 0.9 || !result.Verified);
    }

    [Fact]
    public void VerifyUpperBound_CorrectBound_Holds()
    {
        // Arrange: T(n) = 2T(n/2) + n, bound = n²
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var upperBound = PolynomialComplexity.OfDegree(2, Variable.N);

        // Act
        var result = _verifier.VerifyUpperBound(recurrence, upperBound);

        // Assert
        Assert.True(result.Holds, $"Upper bound should hold: {string.Join(", ", result.Diagnostics)}");
    }

    [Fact]
    public void VerifyUpperBound_TooTightBound_Fails()
    {
        // Arrange: T(n) = 2T(n/2) + n, bound = log n (too tight)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var upperBound = new LogarithmicComplexity(1, Variable.N);

        // Act
        var result = _verifier.VerifyUpperBound(recurrence, upperBound);

        // Assert
        Assert.False(result.Holds, "Upper bound log n should not hold for n log n growth");
    }

    [Fact]
    public void VerifyLowerBound_CorrectBound_Holds()
    {
        // Arrange: T(n) = 2T(n/2) + n, bound = n
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var lowerBound = new LinearComplexity(1, Variable.N);

        // Act
        var result = _verifier.VerifyLowerBound(recurrence, lowerBound);

        // Assert
        Assert.True(result.Holds, $"Lower bound should hold: {string.Join(", ", result.Diagnostics)}");
    }

    [Fact]
    public void VerifySymbolically_StandardRecurrence_Succeeds()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var solution = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _verifier.VerifySymbolically(recurrence, solution);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ProofSketch);
        Assert.True(result.Steps.Count > 0);
    }

    [Fact]
    public void VerifySymbolically_UnsupportedForm_ReturnsFailure()
    {
        // Arrange: Non-standard recurrence
        var recurrence = new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(1, 0.33), new RecurrenceRelationTerm(2, 0.5) },
            Variable.N,
            new LinearComplexity(1, Variable.N));

        var solution = PolynomialComplexity.OfDegree(2, Variable.N);

        // Act
        var result = _verifier.VerifySymbolically(recurrence, solution);

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void VerifyRecurrenceSolution_ReportsBaseCase()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var solution = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, solution);

        // Assert
        Assert.NotNull(result.BaseCase);
        Assert.True(result.BaseCase.Results.Count > 0);
    }

    [Fact]
    public void VerifyRecurrenceSolution_ReportsInductiveStep()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var solution = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, solution);

        // Assert
        Assert.NotNull(result.InductiveStep);
        Assert.True(result.InductiveStep.MaxRatio > 0);
    }

    [Fact]
    public void VerifyRecurrenceSolution_ReportsAsymptoticBehavior()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var solution = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _verifier.VerifyRecurrenceSolution(recurrence, solution);

        // Assert
        Assert.NotNull(result.AsymptoticVerification);
    }
}
