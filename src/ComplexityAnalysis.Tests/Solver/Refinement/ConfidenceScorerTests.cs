using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver.Refinement;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver.Refinement;

public class ConfidenceScorerTests
{
    private readonly ConfidenceScorer _scorer = ConfidenceScorer.Instance;

    [Fact]
    public void ComputeConfidence_TheoreticalExact_HighConfidence()
    {
        // Arrange
        var expression = PolyLogComplexity.NLogN(Variable.N);
        var context = new AnalysisContext
        {
            Source = AnalysisSource.TheoreticalExact,
            Verification = VerificationStatus.Proven
        };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        Assert.True(result.OverallScore > 0.8);
        Assert.Equal(ConfidenceLevel.VeryHigh, result.Level);
    }

    [Fact]
    public void ComputeConfidence_Heuristic_LowerConfidence()
    {
        // Arrange
        var expression = PolynomialComplexity.OfDegree(2, Variable.N);
        var context = new AnalysisContext
        {
            Source = AnalysisSource.Heuristic,
            Verification = VerificationStatus.NotVerified
        };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        Assert.True(result.OverallScore < 0.8);
        Assert.Contains("heuristics", result.Warnings.FirstOrDefault() ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComputeConfidence_FailedVerification_LowConfidence()
    {
        // Arrange
        var expression = new LinearComplexity(1, Variable.N);
        var context = new AnalysisContext
        {
            Source = AnalysisSource.TheoreticalApproximate,
            Verification = VerificationStatus.Failed
        };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        Assert.True(result.OverallScore < 0.7);
        Assert.Contains("verification", result.Warnings.FirstOrDefault() ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ComputeTheoremConfidence_MasterTheorem_HighConfidence()
    {
        // Arrange - Note: For Case2, Epsilon isn't used but should be set to avoid
        // the "near boundary" penalty in the confidence scorer
        var masterResult = new MasterTheoremApplicable(
            MasterTheoremCase.Case2,
            2, 2, 1,
            new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 1 },
            PolyLogComplexity.NLogN(Variable.N))
        {
            Epsilon = 1.0,  // Set to avoid "near boundary" penalty
            LogExponentK = 0  // Case2 with k=0
        };

        // Act
        var confidence = _scorer.ComputeTheoremConfidence(masterResult);

        // Assert
        Assert.True(confidence > 0.9);
    }

    [Fact]
    public void ComputeTheoremConfidence_AkraBazzi_HighConfidence()
    {
        // Arrange - Must set Terms to get proper confidence calculation
        var akraResult = new AkraBazziApplicable(
            1.58, // log_2(3) ≈ 1.58
            PolyLogComplexity.Polynomial(1.58, Variable.N))
        {
            Terms = ImmutableList.Create((3.0, 0.5))  // Single term: 3T(n/2)
        };

        // Act
        var confidence = _scorer.ComputeTheoremConfidence(akraResult);

        // Assert
        Assert.True(confidence > 0.85);
    }

    [Fact]
    public void ComputeTheoremConfidence_NotApplicable_LowConfidence()
    {
        // Arrange
        var notApplicable = new TheoremNotApplicable("Test reason", ImmutableList<string>.Empty);

        // Act
        var confidence = _scorer.ComputeTheoremConfidence(notApplicable);

        // Assert
        Assert.True(confidence < 0.5);
    }

    [Fact]
    public void ComputeRefinementConfidence_SuccessfulRefinement_ModerateConfidence()
    {
        // Arrange
        var result = new RefinementResult
        {
            Success = true,
            OriginalExpression = new LinearComplexity(1, Variable.N),
            RefinedExpression = PolyLogComplexity.NLogN(Variable.N),
            ConfidenceScore = 0.85,
            Method = "Slack variable optimization via numerical fitting"
        };

        // Act
        var confidence = _scorer.ComputeRefinementConfidence(result);

        // Assert
        Assert.True(confidence > 0.6);
    }

    [Fact]
    public void ComputeRefinementConfidence_FailedRefinement_ZeroConfidence()
    {
        // Arrange
        var result = new RefinementResult
        {
            Success = false,
            ErrorMessage = "Failed to refine"
        };

        // Act
        var confidence = _scorer.ComputeRefinementConfidence(result);

        // Assert
        Assert.Equal(0.0, confidence);
    }

    [Fact]
    public void ComputeConsensusConfidence_AllAgree_BoostedConfidence()
    {
        // Arrange
        var confidences = new List<double> { 0.8, 0.82, 0.79, 0.81 };

        // Act
        var consensus = _scorer.ComputeConsensusConfidence(confidences);

        // Assert
        Assert.True(consensus >= 0.79); // At least min
        Assert.True(consensus <= 1.0);
    }

    [Fact]
    public void ComputeConsensusConfidence_Disagreement_NoBoost()
    {
        // Arrange
        var confidences = new List<double> { 0.9, 0.5, 0.3 };

        // Act
        var consensus = _scorer.ComputeConsensusConfidence(confidences);

        // Assert
        Assert.True(consensus >= 0.3); // At least min
        Assert.True(consensus < 0.9); // Less than max
    }

    [Fact]
    public void ComputeConsensusConfidence_SingleValue_ReturnsSame()
    {
        // Arrange
        var confidences = new List<double> { 0.75 };

        // Act
        var consensus = _scorer.ComputeConsensusConfidence(confidences);

        // Assert
        Assert.Equal(0.75, consensus);
    }

    [Fact]
    public void ComputeConfidence_IncludesAllFactors()
    {
        // Arrange
        var expression = PolynomialComplexity.OfDegree(2, Variable.N);
        var context = new AnalysisContext
        {
            Source = AnalysisSource.TheoreticalExact,
            Verification = VerificationStatus.NumericallyVerified,
            TheoremResult = new MasterTheoremApplicable(
                MasterTheoremCase.Case3, 2, 2, 1,
                new ExpressionClassification { Form = ExpressionForm.Polynomial, Variable = Variable.N, PrimaryParameter = 2 },
                PolynomialComplexity.OfDegree(2, Variable.N)),
            NumericalSamples = new NumericalFitData
            {
                RSquared = 0.99,
                StandardError = 0.01,
                SampleCount = 10
            }
        };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        Assert.True(result.Factors.Count >= 4); // Source, Expression, Verification, Numerical, Theorem
        Assert.All(result.Factors, f => Assert.True(f.Score > 0 && f.Score <= 1));
    }

    [Fact]
    public void ComputeConfidence_GeneratesRecommendation()
    {
        // Arrange
        var expression = new LinearComplexity(1, Variable.N);
        var context = new AnalysisContext
        {
            Source = AnalysisSource.Unknown,
            Verification = VerificationStatus.NotVerified
        };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        Assert.NotNull(result.Recommendation);
        Assert.NotEmpty(result.Recommendation);
    }

    [Fact]
    public void ComputeConfidence_SimpleExpression_HighExpressionScore()
    {
        // Arrange
        var expression = ConstantComplexity.One;
        var context = new AnalysisContext { Source = AnalysisSource.TheoreticalExact };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        var expressionFactor = result.Factors.FirstOrDefault(f => f.Name == "Expression Simplicity");
        Assert.NotNull(expressionFactor);
        Assert.Equal(1.0, expressionFactor.Score);
    }

    [Fact]
    public void ComputeConfidence_ComplexExpression_LowerExpressionScore()
    {
        // Arrange
        var expression = new RecurrenceComplexity(
            ImmutableList.Create(new RecurrenceTerm(2, new VariableComplexity(Variable.N), 0.5)),
            Variable.N,
            new LinearComplexity(1, Variable.N),
            ConstantComplexity.One);
        var context = new AnalysisContext { Source = AnalysisSource.Unknown };

        // Act
        var result = _scorer.ComputeConfidence(expression, context);

        // Assert
        var expressionFactor = result.Factors.FirstOrDefault(f => f.Name == "Expression Simplicity");
        Assert.NotNull(expressionFactor);
        Assert.True(expressionFactor.Score < 0.8);
    }
}
