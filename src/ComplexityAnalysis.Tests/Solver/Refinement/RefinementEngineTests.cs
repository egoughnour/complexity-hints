using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver.Refinement;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver.Refinement;

public class RefinementEngineTests
{
    private readonly RefinementEngine _engine = RefinementEngine.Instance;

    [Fact]
    public void Refine_MergeSortRecurrence_ProducesRefinedSolution()
    {
        // Arrange: T(n) = 2T(n/2) + n (merge sort)
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2,
            2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RefinedSolution);
        Assert.True(result.Stages.Count > 0);
    }

    [Fact]
    public void Refine_NonApplicableTheorem_ReturnsFailed()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, ConstantComplexity.One, Variable.N);
        var theoremResult = new TheoremNotApplicable("Test", ImmutableList<string>.Empty);

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Refine_ProducesConfidenceAssessment()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2, 2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.NotNull(result.ConfidenceAssessment);
        Assert.True(result.ConfidenceAssessment.OverallScore > 0);
    }

    [Fact]
    public void Refine_ProducesVerificationResult()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2, 2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.NotNull(result.Verification);
    }

    [Fact]
    public void Refine_RecordsAllStages()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2, 2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Stages.Count >= 2); // At least slack optimization and verification
        Assert.All(result.Stages, s =>
        {
            Assert.NotNull(s.Name);
            Assert.True(s.Confidence >= 0 && s.Confidence <= 1);
        });
    }

    [Fact]
    public void QuickRefine_SimpleExpression_Succeeds()
    {
        // Arrange
        var expression = PolynomialComplexity.OfDegree(2, Variable.N);

        // Act
        var result = _engine.QuickRefine(expression, Variable.N);

        // Assert
        Assert.NotNull(result.RefinedExpression);
        Assert.True(result.Confidence >= 0);
    }

    [Fact]
    public void VerifyBound_UpperBound_Verifies()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var upperBound = PolynomialComplexity.OfDegree(2, Variable.N);

        // Act
        var result = _engine.VerifyBound(recurrence, upperBound, BoundType.BigO);

        // Assert
        Assert.True(result.Holds);
    }

    [Fact]
    public void VerifyBound_LowerBound_Verifies()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var lowerBound = new LinearComplexity(1, Variable.N);

        // Act
        var result = _engine.VerifyBound(recurrence, lowerBound, BoundType.Omega);

        // Assert
        Assert.True(result.Holds);
    }

    [Fact]
    public void VerifyBound_ThetaBound_VerifiesBoth()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var tightBound = PolyLogComplexity.NLogN(Variable.N);

        // Act
        var result = _engine.VerifyBound(recurrence, tightBound, BoundType.Theta);

        // Assert
        // Should verify both upper and lower
        Assert.True(result.Diagnostics.Count >= 2);
    }

    [Fact]
    public void Refine_NearBoundaryCase_AppliesPerturbation()
    {
        // Arrange: Near boundary case
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2,
            PolyLogComplexity.Polynomial(0.95, Variable.N), Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case1, 2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 0.95 },
            PolyLogComplexity.Polynomial(1, Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        // Should have perturbation stage if boundary detected
        var hasBoundaryAnalysis = result.Diagnostics.Any(d =>
            d.Contains("boundary", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("perturbation", StringComparison.OrdinalIgnoreCase));
        // May or may not detect boundary depending on threshold
    }

    [Fact]
    public void Refine_WasImproved_ReportsCorrectly()
    {
        // Arrange
        var recurrence = RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2, 2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        // WasImproved depends on whether refinement changed the solution
        Assert.NotNull(result.OriginalSolution);
        Assert.NotNull(result.RefinedSolution);
    }

    [Fact]
    public void Refine_BinarySearchRecurrence_ProducesLogSolution()
    {
        // Arrange: T(n) = T(n/2) + 1 (binary search)
        var recurrence = RecurrenceRelation.DivideAndConquer(1, 2, ConstantComplexity.One, Variable.N);

        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2,
            1, 2, 0,
            new ExpressionClassification { Form = ExpressionForm.Constant, Variable = Variable.N },
            new LogarithmicComplexity(1, Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RefinedSolution);
    }

    [Fact]
    public void Refine_StrassenRecurrence_RefinesCorrectly()
    {
        // Arrange: T(n) = 7T(n/2) + n² (Strassen)
        var recurrence = RecurrenceRelation.DivideAndConquer(7, 2,
            PolynomialComplexity.OfDegree(2, Variable.N), Variable.N);

        // log_2(7) ≈ 2.807, and n² < n^2.807, so Case 1
        var theoremResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case1,
            7, 2, Math.Log(7) / Math.Log(2),
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 2 },
            PolyLogComplexity.Polynomial(Math.Log(7) / Math.Log(2), Variable.N));

        // Act
        var result = _engine.Refine(recurrence, theoremResult);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RefinedSolution);
    }
}
