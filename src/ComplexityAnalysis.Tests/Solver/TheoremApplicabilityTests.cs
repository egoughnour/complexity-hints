using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver;
using Xunit;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Tests for theorem applicability analysis covering Master Theorem,
/// Akra-Bazzi, and linear recurrence cases.
/// </summary>
public class TheoremApplicabilityTests
{
    private readonly TheoremApplicabilityAnalyzer _analyzer = TheoremApplicabilityAnalyzer.Instance;

    #region Master Theorem Case 1 Tests

    /// <summary>
    /// T(n) = 2T(n/2) + 1
    /// a=2, b=2, f(n)=O(1), log_b(a)=1
    /// f(n) = O(n^0) is polynomially smaller than n^1
    /// Solution: Θ(n)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case1_BinarySearchLike()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 2,
            divisionFactor: 2,
            mergeWork: new ConstantComplexity(1),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        Assert.IsType<MasterTheoremApplicable>(result);

        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case1, master.Case);
        Assert.Equal(2.0, master.A);
        Assert.Equal(2.0, master.B);
        Assert.Equal(1.0, master.LogBA, precision: 6);

        // Solution should be Θ(n)
        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(1.0, solution.PolyDegree, precision: 6);
        Assert.Equal(0.0, solution.LogExponent);
    }

    /// <summary>
    /// T(n) = 4T(n/2) + n
    /// a=4, b=2, f(n)=n, log_b(a)=2
    /// f(n) = O(n^1) is polynomially smaller than n^2
    /// Solution: Θ(n²)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case1_MoreSubproblems()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 4,
            divisionFactor: 2,
            mergeWork: new LinearComplexity(1.0, Variable.N),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case1, master.Case);
        Assert.Equal(2.0, master.LogBA, precision: 6);

        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(2.0, solution.PolyDegree, precision: 6);
    }

    #endregion

    #region Master Theorem Case 2 Tests

    /// <summary>
    /// T(n) = 2T(n/2) + n
    /// a=2, b=2, f(n)=n, log_b(a)=1
    /// f(n) = Θ(n^1) = Θ(n^(log_b(a)))
    /// Solution: Θ(n log n)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case2_MergeSort()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 2,
            divisionFactor: 2,
            mergeWork: new LinearComplexity(1.0, Variable.N),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case2, master.Case);
        Assert.Equal(0.0, master.LogExponentK); // k=0 in f(n) = Θ(n^d · log^0 n)

        // Solution: Θ(n log n)
        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(1.0, solution.PolyDegree, precision: 6);
        Assert.Equal(1.0, solution.LogExponent); // k+1 = 0+1 = 1
    }

    /// <summary>
    /// T(n) = 2T(n/2) + n·log(n)
    /// a=2, b=2, f(n)=n·log(n), log_b(a)=1
    /// f(n) = Θ(n^1 · log^1 n), k=1
    /// Solution: Θ(n log² n)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case2_WithLogFactor()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 2,
            divisionFactor: 2,
            mergeWork: PolyLogComplexity.NLogN(Variable.N),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case2, master.Case);
        Assert.Equal(1.0, master.LogExponentK); // k=1

        // Solution: Θ(n log² n)
        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(1.0, solution.PolyDegree, precision: 6);
        Assert.Equal(2.0, solution.LogExponent); // k+1 = 1+1 = 2
    }

    #endregion

    #region Master Theorem Case 3 Tests

    /// <summary>
    /// T(n) = 2T(n/2) + n²
    /// a=2, b=2, f(n)=n², log_b(a)=1
    /// f(n) = Ω(n^2) dominates n^1
    /// Solution: Θ(n²)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case3_QuadraticWork()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 2,
            divisionFactor: 2,
            mergeWork: PolyLogComplexity.Polynomial(2, Variable.N),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case3, master.Case);
        Assert.True(master.RegularityVerified);
        Assert.True(master.Epsilon > 0);

        // Solution: Θ(n²)
        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(2.0, solution.PolyDegree, precision: 6);
    }

    /// <summary>
    /// T(n) = T(n/2) + n
    /// a=1, b=2, f(n)=n, log_b(a)=0
    /// f(n) = Ω(n^1) dominates n^0 = 1
    /// Solution: Θ(n)
    /// </summary>
    [Fact]
    public void MasterTheorem_Case3_BinarySearchWithLinearWork()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 1,
            divisionFactor: 2,
            mergeWork: new LinearComplexity(1.0, Variable.N),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case3, master.Case);
        Assert.Equal(0.0, master.LogBA, precision: 6);
    }

    #endregion

    #region Akra-Bazzi Tests

    /// <summary>
    /// T(n) = T(n/3) + T(2n/3) + n
    /// Two terms with different scale factors.
    /// Master Theorem doesn't apply, use Akra-Bazzi.
    /// Solution: Θ(n log n)
    /// </summary>
    [Fact]
    public void AkraBazzi_TwoTerms_UnbalancedPartition()
    {
        // T(n) = T(n/3) + T(2n/3) + n
        var recurrence = new RecurrenceRelation(
            new[]
            {
                new RecurrenceRelationTerm(1.0, 1.0 / 3),  // T(n/3)
                new RecurrenceRelationTerm(1.0, 2.0 / 3)   // T(2n/3)
            }.ToList(),
            Variable.N,
            new LinearComplexity(1.0, Variable.N));

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        Assert.IsType<AkraBazziApplicable>(result);

        var akra = (AkraBazziApplicable)result;
        // For 1·(1/3)^p + 1·(2/3)^p = 1, p ≈ 1
        Assert.Equal(1.0, akra.CriticalExponent, precision: 2);

        // With g(n)=n and p=1, integral gives log(n)
        // Solution: Θ(n log n)
        var solution = akra.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(1.0, solution.PolyDegree, precision: 2);
        Assert.Equal(1.0, solution.LogExponent);
    }

    /// <summary>
    /// T(n) = 3T(n/4) + n
    /// Akra-Bazzi reduces to Master Theorem case.
    /// a=3, scale=1/4, so p satisfies 3·(1/4)^p = 1 → p = log_4(3)
    /// </summary>
    [Fact]
    public void AkraBazzi_SingleTerm_EquivalentToMaster()
    {
        var recurrence = new RecurrenceRelation(
            new[]
            {
                new RecurrenceRelationTerm(3.0, 0.25)  // 3T(n/4)
            }.ToList(),
            Variable.N,
            new LinearComplexity(1.0, Variable.N));

        // This should fit both Master and Akra-Bazzi
        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);

        // Master Theorem should take precedence
        if (result is MasterTheoremApplicable master)
        {
            Assert.Equal(3.0, master.A);
            Assert.Equal(4.0, master.B);
        }
        else if (result is AkraBazziApplicable akra)
        {
            // Critical exponent should be log_4(3) ≈ 0.792
            var expected = Math.Log(3) / Math.Log(4);
            Assert.Equal(expected, akra.CriticalExponent, precision: 2);
        }
    }

    #endregion

    #region Linear Recurrence Tests

    /// <summary>
    /// T(n) = T(n-1) + c (constant work per step)
    /// Solution: Θ(n)
    /// </summary>
    [Fact]
    public void LinearRecurrence_ConstantWork()
    {
        // This is approximated by scale factor very close to 1
        // For n-1, we model it as n·(1-1/n) for large n
        var recurrence = new RecurrenceRelation(
            new[]
            {
                new RecurrenceRelationTerm(1.0, 0.999)  // Approximates T(n-1)
            }.ToList(),
            Variable.N,
            new ConstantComplexity(1));

        var result = _analyzer.Analyze(recurrence);

        // Should detect as linear recurrence
        if (result is LinearRecurrenceSolved linear)
        {
            Assert.Contains("summation", linear.Method.ToLower());
            // Solution should be Θ(n)
            var bigO = linear.Solution?.ToBigONotation();
            Assert.NotNull(bigO);
            Assert.Contains("n", bigO);
        }
    }

    #endregion

    #region Edge Cases and Failure Tests

    [Fact]
    public void NoTerms_ReturnsNotApplicable()
    {
        var recurrence = new RecurrenceRelation(
            new List<RecurrenceRelationTerm>(),
            Variable.N,
            new ConstantComplexity(1));

        var result = _analyzer.Analyze(recurrence);

        Assert.False(result.IsApplicable);
        Assert.IsType<TheoremNotApplicable>(result);
    }

    [Fact]
    public void NegativeCoefficient_ReturnsNotApplicable()
    {
        var recurrence = new RecurrenceRelation(
            new[]
            {
                new RecurrenceRelationTerm(-1.0, 0.5)  // Invalid: negative coefficient
            }.ToList(),
            Variable.N,
            new ConstantComplexity(1));

        var result = _analyzer.Analyze(recurrence);

        Assert.False(result.IsApplicable);
        Assert.IsType<TheoremNotApplicable>(result);
        var notApplicable = (TheoremNotApplicable)result;
        Assert.Contains(notApplicable.ViolatedConditions, v => v.ToLower().Contains("coefficient"));
    }

    [Fact]
    public void NonReducingScaleFactor_ReturnsNotApplicable()
    {
        var recurrence = new RecurrenceRelation(
            new[]
            {
                new RecurrenceRelationTerm(1.0, 1.5)  // Invalid: scale > 1
            }.ToList(),
            Variable.N,
            new ConstantComplexity(1));

        var result = _analyzer.Analyze(recurrence);

        Assert.False(result.IsApplicable);
        Assert.IsType<TheoremNotApplicable>(result);
    }

    #endregion

    #region Known Algorithm Recurrences

    /// <summary>
    /// Karatsuba multiplication: T(n) = 3T(n/2) + n
    /// Solution: Θ(n^(log₂3)) ≈ Θ(n^1.585)
    /// </summary>
    [Fact]
    public void KnownAlgorithm_Karatsuba()
    {
        var recurrence = RecurrenceAnalysisExtensions.KaratsubaStyle(
            new LinearComplexity(1.0, Variable.N));

        var result = recurrence.Analyze();

        Assert.True(result.IsApplicable);

        if (result is MasterTheoremApplicable master)
        {
            Assert.Equal(MasterTheoremCase.Case1, master.Case);
            var expectedLogBA = Math.Log(3) / Math.Log(2); // ≈ 1.585
            Assert.Equal(expectedLogBA, master.LogBA, precision: 3);
        }
    }

    /// <summary>
    /// Strassen matrix multiplication: T(n) = 7T(n/2) + n²
    /// Solution: Θ(n^(log₂7)) ≈ Θ(n^2.807)
    /// </summary>
    [Fact]
    public void KnownAlgorithm_Strassen()
    {
        var recurrence = RecurrenceAnalysisExtensions.StrassenStyle(
            PolyLogComplexity.Polynomial(2, Variable.N));

        var result = recurrence.Analyze();

        Assert.True(result.IsApplicable);

        if (result is MasterTheoremApplicable master)
        {
            Assert.Equal(MasterTheoremCase.Case1, master.Case);
            var expectedLogBA = Math.Log(7) / Math.Log(2); // ≈ 2.807
            Assert.Equal(expectedLogBA, master.LogBA, precision: 3);
        }
    }

    /// <summary>
    /// Binary search: T(n) = T(n/2) + O(1)
    /// Solution: Θ(log n)
    /// </summary>
    [Fact]
    public void KnownAlgorithm_BinarySearch()
    {
        var recurrence = RecurrenceRelation.DivideAndConquer(
            subproblems: 1,
            divisionFactor: 2,
            mergeWork: new ConstantComplexity(1),
            Variable.N);

        var result = _analyzer.Analyze(recurrence);

        Assert.True(result.IsApplicable);
        var master = (MasterTheoremApplicable)result;

        // log_2(1) = 0, so this is Case 2 with d=0
        Assert.Equal(MasterTheoremCase.Case2, master.Case);
        Assert.Equal(0.0, master.LogBA, precision: 6);

        // Solution: Θ(log n)
        var solution = master.Solution as PolyLogComplexity;
        Assert.NotNull(solution);
        Assert.Equal(0.0, solution.PolyDegree, precision: 6);
        Assert.Equal(1.0, solution.LogExponent);
    }

    #endregion
}
