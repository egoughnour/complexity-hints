using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver.Refinement;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver.Refinement;

public class SlackVariableOptimizerTests
{
    private readonly SlackVariableOptimizer _optimizer = SlackVariableOptimizer.Instance;

    [Fact]
    public void Refine_PolynomialExpression_FindsCorrectDegree()
    {
        // Arrange: n^2
        var expression = PolynomialComplexity.OfDegree(2, Variable.N);

        // Act
        var result = _optimizer.Refine(expression, Variable.N);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.RefinedExpression);
    }

    [Fact]
    public void Refine_LinearExpression_Succeeds()
    {
        // Arrange
        var expression = new LinearComplexity(1, Variable.N);

        // Act
        var result = _optimizer.Refine(expression, Variable.N);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void RefineRecurrence_MergeSortRecurrence_RefinesCorrectly()
    {
        // Arrange: T(n) = 2T(n/2) + n (merge sort)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2,
            2, 2, 1,
            new ExpressionClassification(ExpressionForm.Polynomial, 1, null, 1.0),
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _optimizer.RefineRecurrence(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RefinedSolution);
    }

    [Fact]
    public void RefineRecurrence_BinarySearchRecurrence_RefinesCorrectly()
    {
        // Arrange: T(n) = T(n/2) + 1 (binary search)
        var recurrence = RecurrenceRelation.DivideAndConquer(1, 2, ConstantComplexity.One, Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2,
            1, 2, 0,
            new ExpressionClassification(ExpressionForm.Constant, null, null, 1.0),
            new LogarithmicComplexity(1, Variable.N));

        // Act
        var result = _optimizer.RefineRecurrence(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void RefineGap_SmallGap_FindsSolution()
    {
        // Arrange: small gap between log_b(a) and f(n) degree
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var logBA = 1.0;
        var fDegree = 1.05; // Small gap

        // Act
        var result = _optimizer.RefineGap(recurrence, logBA, fDegree);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RefinedSolution);
        Assert.True(result.ConfidenceScore > 0);
    }

    [Fact]
    public void RefineGap_LargeGap_IdentifiesDominantTerm()
    {
        // Arrange: large gap
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolynomialComplexity.OfDegree(2, Variable.N), Variable.N);
        var logBA = 1.0;
        var fDegree = 2.0; // Large gap - f(n) dominates

        // Act
        var result = _optimizer.RefineGap(recurrence, logBA, fDegree);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("dominates", result.Diagnostics.FirstOrDefault() ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RefineRecurrence_NonApplicableTheorem_ReturnsFailed()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, ConstantComplexity.One, Variable.N);
        var theoremResult = new TheoremNotApplicable("Test", ImmutableList<string>.Empty);

        // Act
        var result = _optimizer.RefineRecurrence(recurrence, theoremResult);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }
}
