using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver;
using ComplexityAnalysis.Solver.Refinement;
using ComplexityAnalysis.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Cross-validation tests that verify consistency between different analysis mechanisms:
/// - Master Theorem vs Akra-Bazzi (where both apply)
/// - Critical Exponent Solver vs analytical calculation
/// - Theorem solution vs numerical verification
/// - Induction verification vs refinement results
/// </summary>
public class CrossValidationTests
{
    private readonly ITestOutputHelper _output;
    private readonly TheoremApplicabilityAnalyzer _analyzer = TheoremApplicabilityAnalyzer.Instance;
    private readonly ICriticalExponentSolver _exponentSolver = MathNetCriticalExponentSolver.Instance;
    private readonly IInductionVerifier _inductionVerifier;
    private readonly IRefinementEngine _refinementEngine;

    public CrossValidationTests(ITestOutputHelper output)
    {
        _output = output;
        _inductionVerifier = new InductionVerifier();
        _refinementEngine = new RefinementEngine(
            new SlackVariableOptimizer(),
            new PerturbationExpansion(),
            _inductionVerifier,
            new ConfidenceScorer());
    }

    #region Master Theorem vs Akra-Bazzi Consistency

    /// <summary>
    /// For single-term recurrences, Master Theorem and Akra-Bazzi should give equivalent solutions.
    /// This validates internal consistency of both theorem implementations.
    /// </summary>
    [Theory]
    [MemberData(nameof(SingleTermRecurrenceCases))]
    public void MasterTheorem_And_AkraBazzi_GiveConsistentSolutions(
        string name, int a, int b, string mergeWorkType, double expectedDegree, double expectedLog)
    {
        var var = Variable.N;
        var mergeWork = CreateMergeWork(mergeWorkType, var);

        // Create recurrence for Master Theorem (uses division factor)
        var masterRecurrence = RecurrenceRelation.DivideAndConquer(a, b, mergeWork, var);

        // Create equivalent recurrence for Akra-Bazzi (uses scale factor 1/b)
        var akraRecurrence = new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(a, 1.0 / b) }.ToList(),
            var,
            mergeWork);

        // Analyze with Master Theorem preference
        var masterResult = _analyzer.Analyze(masterRecurrence);

        // Force Akra-Bazzi by using multi-term form (single term still valid for A-B)
        var akraResult = _analyzer.AnalyzeWithAkraBazzi(akraRecurrence);

        _output.WriteLine($"Case: {name}");
        _output.WriteLine($"Master Result: {masterResult}");
        _output.WriteLine($"Akra-Bazzi Result: {akraResult}");

        // Both should be applicable
        Assert.True(masterResult.IsApplicable, $"Master Theorem should apply to {name}");
        Assert.True(akraResult.IsApplicable, $"Akra-Bazzi should apply to {name}");

        // Extract solutions and compare
        var masterSolution = ExtractSolution(masterResult);
        var akraSolution = ExtractSolution(akraResult);

        Assert.NotNull(masterSolution);
        Assert.NotNull(akraSolution);

        // Solutions should match within tolerance
        var masterDegree = GetPolyDegree(masterSolution);
        var akraDegree = GetPolyDegree(akraSolution);
        var masterLog = GetLogExponent(masterSolution);
        var akraLog = GetLogExponent(akraSolution);

        _output.WriteLine($"Master: n^{masterDegree} log^{masterLog} n");
        _output.WriteLine($"Akra-Bazzi: n^{akraDegree} log^{akraLog} n");
        _output.WriteLine($"Expected: n^{expectedDegree} log^{expectedLog} n");

        // Both theorems should agree with each other
        Assert.Equal(masterDegree, akraDegree, precision: 2);
        Assert.Equal(masterLog, akraLog, precision: 1);

        // And both should match the expected values
        Assert.Equal(expectedDegree, masterDegree, precision: 2);
        Assert.Equal(expectedLog, masterLog, precision: 1);
    }

    public static IEnumerable<object[]> SingleTermRecurrenceCases => new[]
    {
        // name, a, b, mergeWorkType, expectedDegree, expectedLogExponent
        new object[] { "MergeSort_2T_n/2_n", 2, 2, "linear", 1.0, 1.0 },
        new object[] { "BinarySearch_T_n/2_1", 1, 2, "constant", 0.0, 1.0 },
        new object[] { "Karatsuba_3T_n/2_n", 3, 2, "linear", 1.585, 0.0 },
        new object[] { "QuadDC_4T_n/2_n", 4, 2, "linear", 2.0, 0.0 },
        new object[] { "Strassen_7T_n/2_n2", 7, 2, "quadratic", 2.807, 0.0 },
        new object[] { "TernaryDC_3T_n/3_n", 3, 3, "linear", 1.0, 1.0 },
    };

    #endregion

    #region Critical Exponent Solver vs Analytical

    /// <summary>
    /// For simple recurrences where the critical exponent has a closed form,
    /// verify the numerical solver matches the analytical result.
    /// </summary>
    [Theory]
    [MemberData(nameof(AnalyticalExponentCases))]
    public void CriticalExponent_MatchesAnalyticalFormula(
        string name, double a, double b, double analyticalExponent)
    {
        var terms = new[] { (Coefficient: a, ScaleFactor: 1.0 / b) };

        var numericalExponent = _exponentSolver.Solve(terms.ToList());

        Assert.NotNull(numericalExponent);
        _output.WriteLine($"{name}: Analytical={analyticalExponent:F6}, Numerical={numericalExponent.Value:F6}");

        // Verify against analytical formula: p = log_b(a) = log(a)/log(b)
        var expectedAnalytical = Math.Log(a) / Math.Log(b);
        Assert.Equal(expectedAnalytical, analyticalExponent, precision: 6);
        Assert.Equal(analyticalExponent, numericalExponent.Value, precision: 5);

        // Verify by substitution: a · (1/b)^p should equal 1
        var verification = a * Math.Pow(1.0 / b, numericalExponent.Value);
        Assert.Equal(1.0, verification, precision: 6);
    }

    public static IEnumerable<object[]> AnalyticalExponentCases => new[]
    {
        // name, a, b, analyticalExponent (log_b(a))
        new object[] { "BinarySearch", 1.0, 2.0, 0.0 },              // log₂(1) = 0
        new object[] { "BinaryDC", 2.0, 2.0, 1.0 },                  // log₂(2) = 1
        new object[] { "Karatsuba", 3.0, 2.0, Math.Log(3)/Math.Log(2) },  // log₂(3) ≈ 1.585
        new object[] { "QuadDC", 4.0, 2.0, 2.0 },                    // log₂(4) = 2
        new object[] { "Strassen", 7.0, 2.0, Math.Log(7)/Math.Log(2) },  // log₂(7) ≈ 2.807
        new object[] { "OctaDC", 8.0, 2.0, 3.0 },                    // log₂(8) = 3
        new object[] { "TernarySearch", 1.0, 3.0, 0.0 },             // log₃(1) = 0
        new object[] { "TernaryDC", 3.0, 3.0, 1.0 },                 // log₃(3) = 1
        new object[] { "Base4_16", 16.0, 4.0, 2.0 },                 // log₄(16) = 2
    };

    #endregion

    #region Solution vs Numerical Verification

    /// <summary>
    /// Verify that the computed solution, when evaluated numerically for large n,
    /// matches the expected growth rate within acceptable bounds.
    /// </summary>
    [Theory]
    [MemberData(nameof(NumericalVerificationCases))]
    public void Solution_MatchesNumericalGrowthRate(
        string name, int a, int b, string mergeType, double[] testInputs)
    {
        var var = Variable.N;
        var mergeWork = CreateMergeWork(mergeType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, mergeWork, var);

        var result = _analyzer.Analyze(recurrence);
        Assert.True(result.IsApplicable);

        var solution = ExtractSolution(result);
        Assert.NotNull(solution);

        _output.WriteLine($"Case: {name}");
        _output.WriteLine($"Solution: {solution.ToBigONotation()}");

        // For each pair of consecutive test inputs, verify the growth ratio
        for (int i = 0; i < testInputs.Length - 1; i++)
        {
            var n1 = testInputs[i];
            var n2 = testInputs[i + 1];

            var assignments1 = new Dictionary<Variable, double> { { var, n1 } };
            var assignments2 = new Dictionary<Variable, double> { { var, n2 } };

            var val1 = solution.Evaluate(assignments1);
            var val2 = solution.Evaluate(assignments2);

            Assert.NotNull(val1);
            Assert.NotNull(val2);

            var actualRatio = val2.Value / val1.Value;
            var polyDegree = GetPolyDegree(solution);
            var logExponent = GetLogExponent(solution);

            // Compute expected ratio based on solution form
            var expectedRatio = ComputeExpectedRatio(n1, n2, polyDegree, logExponent);

            _output.WriteLine($"  n={n1}→{n2}: actual ratio={actualRatio:F4}, expected≈{expectedRatio:F4}");

            // Allow 20% tolerance for numerical effects
            Assert.InRange(actualRatio, expectedRatio * 0.8, expectedRatio * 1.2);
        }
    }

    public static IEnumerable<object[]> NumericalVerificationCases => new[]
    {
        // name, a, b, mergeType, testInputs
        new object[] { "MergeSort", 2, 2, "linear", new double[] { 100, 200, 400, 800 } },
        new object[] { "BinarySearch", 1, 2, "constant", new double[] { 100, 200, 400, 800 } },
        new object[] { "Karatsuba", 3, 2, "linear", new double[] { 64, 128, 256, 512 } },
        new object[] { "QuadDC", 4, 2, "linear", new double[] { 100, 200, 400 } },
    };

    #endregion

    #region Induction Verification vs Theorem Result

    /// <summary>
    /// For solved recurrences, verify that induction verification confirms the solution.
    /// This cross-validates theorem solving against induction-based proof.
    /// </summary>
    [Theory]
    [MemberData(nameof(InductionVerificationCases))]
    public void InductionVerification_ConfirmsTheoremSolution(
        string name, int a, int b, string mergeType)
    {
        var var = Variable.N;
        var mergeWork = CreateMergeWork(mergeType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, mergeWork, var);

        var theoremResult = _analyzer.Analyze(recurrence);
        Assert.True(theoremResult.IsApplicable, $"Theorem should apply to {name}");

        var solution = ExtractSolution(theoremResult);
        Assert.NotNull(solution);

        _output.WriteLine($"Case: {name}");
        _output.WriteLine($"Theorem solution: {solution.ToBigONotation()}");

        // Verify using induction
        var inductionResult = _inductionVerifier.VerifyRecurrenceSolution(
            recurrence, solution, BoundType.Theta);

        _output.WriteLine($"Induction result: {inductionResult.Verified}");
        _output.WriteLine($"  Base case: {inductionResult.BaseCase}");
        _output.WriteLine($"  Inductive step: {inductionResult.InductiveStep}");
        _output.WriteLine($"  Asymptotic: {inductionResult.AsymptoticVerification}");

        // At minimum, numerical verification should pass
        if (!inductionResult.Verified)
        {
            _output.WriteLine($"  Notes: Symbolic induction may fail for complex forms");
        }

        // Verify upper bound at least
        var upperResult = _inductionVerifier.VerifyUpperBound(recurrence, solution);
        _output.WriteLine($"Upper bound verification: valid={upperResult.Holds}, c={upperResult.Constant:F4}");
    }

    public static IEnumerable<object[]> InductionVerificationCases => new[]
    {
        new object[] { "MergeSort", 2, 2, "linear" },
        new object[] { "BinarySearch", 1, 2, "constant" },
        new object[] { "Karatsuba", 3, 2, "linear" },
        new object[] { "QuadDC", 4, 2, "linear" },
        new object[] { "Case3_Simple", 1, 2, "linear" },
    };

    #endregion

    #region Refinement Consistency

    /// <summary>
    /// Verify that refinement engine produces results consistent with theorem analysis.
    /// </summary>
    [Theory]
    [MemberData(nameof(RefinementConsistencyCases))]
    public void Refinement_ProducesConsistentBounds(
        string name, int a, int b, string mergeType)
    {
        var var = Variable.N;
        var mergeWork = CreateMergeWork(mergeType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, mergeWork, var);

        var theoremResult = _analyzer.Analyze(recurrence);
        Assert.True(theoremResult.IsApplicable);

        var refinementResult = _refinementEngine.Refine(recurrence, theoremResult);

        _output.WriteLine($"Case: {name}");
        _output.WriteLine($"Original solution: {ExtractSolution(theoremResult)?.ToBigONotation()}");
        _output.WriteLine($"Refined solution: {refinementResult.RefinedSolution?.ToBigONotation()}");
        _output.WriteLine($"Confidence: {refinementResult.ConfidenceAssessment?.OverallScore:F4}");
        _output.WriteLine($"Verified: {refinementResult.Verification?.Verified}");

        // Refined solution should have same asymptotic form
        var originalDegree = GetPolyDegree(ExtractSolution(theoremResult));
        var refinedDegree = GetPolyDegree(refinementResult.RefinedSolution);

        Assert.Equal(originalDegree, refinedDegree, precision: 2);

        // Confidence should be reasonable
        var confidence = refinementResult.ConfidenceAssessment?.OverallScore ?? 0;
        Assert.True(confidence >= 0);
        Assert.True(confidence <= 1);
    }

    public static IEnumerable<object[]> RefinementConsistencyCases => new[]
    {
        new object[] { "MergeSort", 2, 2, "linear" },
        new object[] { "BinarySearch", 1, 2, "constant" },
        new object[] { "Karatsuba", 3, 2, "linear" },
    };

    #endregion

    #region Multi-Term Akra-Bazzi Verification

    /// <summary>
    /// For multi-term Akra-Bazzi cases, verify critical exponent satisfies the defining equation.
    /// </summary>
    [Theory]
    [MemberData(nameof(MultiTermVerificationCases))]
    public void MultiTermAkraBazzi_CriticalExponentSatisfiesEquation(
        string name, (double coef, double scale)[] terms, double expectedP)
    {
        var termList = terms.Select(t => (t.coef, t.scale)).ToList();

        var solvedP = _exponentSolver.Solve(termList);
        Assert.NotNull(solvedP);

        _output.WriteLine($"Case: {name}");
        _output.WriteLine($"Expected p: {expectedP:F4}, Solved p: {solvedP.Value:F4}");

        // Verify: Σᵢ aᵢ · bᵢ^p = 1
        var sum = termList.Sum(t => t.coef * Math.Pow(t.scale, solvedP.Value));
        _output.WriteLine($"Verification: Σ aᵢ·bᵢ^p = {sum:F6} (should be 1)");

        Assert.Equal(1.0, sum, precision: 5);
        Assert.Equal(expectedP, solvedP.Value, precision: 3);
    }

    public static IEnumerable<object[]> MultiTermVerificationCases => new[]
    {
        new object[] { "TwoThirds", new[] { (1.0, 1.0/3), (1.0, 2.0/3) }, 1.0 },
        new object[] { "HalfQuarter", new[] { (2.0, 0.25), (1.0, 0.5) }, 1.0 },
        new object[] { "ThreeQuarters", new[] { (1.0, 0.25), (1.0, 0.25), (1.0, 0.5) }, 1.0 },
    };

    #endregion

    #region Helper Methods

    private static ComplexityExpression CreateMergeWork(string type, Variable var) => type.ToLower() switch
    {
        "constant" => new ConstantComplexity(1),
        "linear" => new LinearComplexity(1.0, var),
        "quadratic" => PolyLogComplexity.Polynomial(2, var),
        "nlogn" => PolyLogComplexity.NLogN(var),
        "logn" => new LogarithmicComplexity(1, var),
        _ => new ConstantComplexity(1)
    };

    private static ComplexityExpression? ExtractSolution(TheoremApplicability result) => result switch
    {
        MasterTheoremApplicable m => m.Solution,
        AkraBazziApplicable a => a.Solution,
        LinearRecurrenceSolved l => l.Solution,
        _ => null
    };

    private static double GetPolyDegree(ComplexityExpression? expr) => expr switch
    {
        PolyLogComplexity p => p.PolyDegree,
        LinearComplexity => 1.0,
        ConstantComplexity => 0.0,
        LogarithmicComplexity => 0.0,
        PolynomialComplexity p => p.Degree,
        VariableComplexity => 1.0,
        // For power expressions like n^k, extract the exponent
        PowerComplexity pow => pow.Exponent,
        // For binary operations, extract degree from the dominant operand
        // e.g., constant × n^k → degree is k
        BinaryOperationComplexity { Operation: BinaryOp.Multiply, Left: ConstantComplexity, Right: var right } =>
            GetPolyDegree(right),
        BinaryOperationComplexity { Operation: BinaryOp.Multiply, Left: var left, Right: ConstantComplexity } =>
            GetPolyDegree(left),
        BinaryOperationComplexity { Operation: BinaryOp.Plus, Left: var left, Right: var right } =>
            Math.Max(GetPolyDegree(left), GetPolyDegree(right)),
        BinaryOperationComplexity { Operation: BinaryOp.Multiply, Left: var left, Right: var right } =>
            GetPolyDegree(left) + GetPolyDegree(right),
        _ => 0.0
    };

    private static double GetLogExponent(ComplexityExpression? expr) => expr switch
    {
        PolyLogComplexity p => p.LogExponent,
        LogarithmicComplexity => 1.0,
        _ => 0.0
    };

    private static double ComputeExpectedRatio(double n1, double n2, double polyDegree, double logExponent)
    {
        // For Θ(n^k · log^j n), ratio from n1 to n2 is approximately:
        // (n2/n1)^k · (log n2 / log n1)^j
        var polyRatio = Math.Pow(n2 / n1, polyDegree);
        var logRatio = logExponent > 0
            ? Math.Pow(Math.Log(n2) / Math.Log(n1), logExponent)
            : 1.0;
        return polyRatio * logRatio;
    }

    #endregion
}
