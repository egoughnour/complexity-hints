using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Solver;
using ComplexityAnalysis.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Solver;

/// <summary>
/// Comprehensive data-driven tests for theorem applicability analysis.
/// Tests a wide range of recurrence patterns with known analytical solutions.
/// </summary>
public class DataDrivenTheoremTests
{
    private readonly ITestOutputHelper _output;
    private readonly TheoremApplicabilityAnalyzer _analyzer = TheoremApplicabilityAnalyzer.Instance;
    private readonly ICriticalExponentSolver _exponentSolver = MathNetCriticalExponentSolver.Instance;

    public DataDrivenTheoremTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Master Theorem - Comprehensive Case Coverage

    /// <summary>
    /// Master Theorem Case 1: f(n) = O(n^c) where c < log_b(a).
    /// Work is dominated by leaves of recursion tree.
    /// </summary>
    [Theory]
    [MemberData(nameof(MasterCase1Data))]
    public void MasterTheorem_Case1_LeafDominated(
        string name, int a, int b, string fType, double expectedDegree)
    {
        var var = Variable.N;
        var f = CreateWork(fType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, f, var);

        var result = _analyzer.Analyze(recurrence);

        LogResult(name, result, expectedDegree, 0);

        Assert.True(result.IsApplicable);
        Assert.IsType<MasterTheoremApplicable>(result);

        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case1, master.Case);

        // Solution should be Θ(n^(log_b(a)))
        var logBA = Math.Log(a) / Math.Log(b);
        AssertSolutionDegree(master.Solution, logBA);
    }

    public static IEnumerable<object[]> MasterCase1Data => new[]
    {
        // name, a, b, fType, expectedDegree
        new object[] { "2T(n/2)+1", 2, 2, "O(1)", 1.0 },
        new object[] { "4T(n/2)+n", 4, 2, "O(n)", 2.0 },
        new object[] { "8T(n/2)+n", 8, 2, "O(n)", 3.0 },
        new object[] { "8T(n/2)+n^2", 8, 2, "O(n^2)", 3.0 },
        new object[] { "9T(n/3)+n", 9, 3, "O(n)", 2.0 },
        new object[] { "4T(n/2)+1", 4, 2, "O(1)", 2.0 },
        new object[] { "16T(n/4)+n", 16, 4, "O(n)", 2.0 },
        new object[] { "7T(n/2)+n^2", 7, 2, "O(n^2)", 2.807 },  // Strassen
        new object[] { "3T(n/2)+n", 3, 2, "O(n)", 1.585 },       // Karatsuba
    };

    /// <summary>
    /// Master Theorem Case 2: f(n) = Θ(n^(log_b(a)) * log^k(n)).
    /// Work at each level is balanced.
    /// </summary>
    [Theory]
    [MemberData(nameof(MasterCase2Data))]
    public void MasterTheorem_Case2_BalancedWork(
        string name, int a, int b, string fType, double expectedDegree, double expectedLog)
    {
        var var = Variable.N;
        var f = CreateWork(fType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, f, var);

        var result = _analyzer.Analyze(recurrence);

        LogResult(name, result, expectedDegree, expectedLog);

        Assert.True(result.IsApplicable);
        Assert.IsType<MasterTheoremApplicable>(result);

        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case2, master.Case);

        // Solution should be Θ(n^d * log^(k+1)(n))
        AssertSolutionForm(master.Solution, expectedDegree, expectedLog);
    }

    public static IEnumerable<object[]> MasterCase2Data => new[]
    {
        // name, a, b, fType, expectedDegree, expectedLogExponent
        new object[] { "MergeSort: 2T(n/2)+n", 2, 2, "O(n)", 1.0, 1.0 },
        new object[] { "BinarySearch: T(n/2)+1", 1, 2, "O(1)", 0.0, 1.0 },
        new object[] { "TernarySearch: T(n/3)+1", 1, 3, "O(1)", 0.0, 1.0 },
        new object[] { "3T(n/3)+n", 3, 3, "O(n)", 1.0, 1.0 },
        new object[] { "4T(n/2)+n^2", 4, 2, "O(n^2)", 2.0, 1.0 },
        new object[] { "2T(n/2)+nlogn", 2, 2, "O(nlogn)", 1.0, 2.0 },
        new object[] { "9T(n/3)+n^2", 9, 3, "O(n^2)", 2.0, 1.0 },
    };

    /// <summary>
    /// Master Theorem Case 3: f(n) = Ω(n^c) where c > log_b(a).
    /// Work dominated by root (merge/combine step).
    /// </summary>
    [Theory]
    [MemberData(nameof(MasterCase3Data))]
    public void MasterTheorem_Case3_RootDominated(
        string name, int a, int b, string fType, double expectedDegree)
    {
        var var = Variable.N;
        var f = CreateWork(fType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, f, var);

        var result = _analyzer.Analyze(recurrence);

        LogResult(name, result, expectedDegree, 0);

        Assert.True(result.IsApplicable);
        Assert.IsType<MasterTheoremApplicable>(result);

        var master = (MasterTheoremApplicable)result;
        Assert.Equal(MasterTheoremCase.Case3, master.Case);

        // Regularity condition should be verified
        Assert.True(master.RegularityVerified == true || master.Epsilon > 0);

        // Solution should be Θ(f(n))
        AssertSolutionDegree(master.Solution, expectedDegree);
    }

    public static IEnumerable<object[]> MasterCase3Data => new[]
    {
        // name, a, b, fType, expectedDegree
        new object[] { "T(n/2)+n", 1, 2, "O(n)", 1.0 },
        new object[] { "T(n/2)+n^2", 1, 2, "O(n^2)", 2.0 },
        new object[] { "2T(n/2)+n^2", 2, 2, "O(n^2)", 2.0 },
        new object[] { "2T(n/2)+n^3", 2, 2, "O(n^3)", 3.0 },
        new object[] { "T(n/3)+n", 1, 3, "O(n)", 1.0 },
        new object[] { "3T(n/3)+n^2", 3, 3, "O(n^2)", 2.0 },
    };

    #endregion

    #region Akra-Bazzi - Multi-Term Recurrences

    /// <summary>
    /// Akra-Bazzi theorem for unbalanced partition recurrences.
    /// </summary>
    [Theory]
    [MemberData(nameof(AkraBazziData))]
    public void AkraBazzi_MultiTermRecurrences(
        string name, (double coef, double scale)[] terms, string fType,
        double expectedCritExp, double expectedDegree, double expectedLog)
    {
        var var = Variable.N;
        var f = CreateWork(fType, var);
        var recurrence = new RecurrenceRelation(
            terms.Select(t => new RecurrenceRelationTerm(t.coef, t.scale)).ToList(),
            var, f);

        var result = _analyzer.Analyze(recurrence);

        _output.WriteLine($"=== {name} ===");
        _output.WriteLine($"Recurrence: T(n) = {string.Join(" + ", terms.Select(t => $"{t.coef}T({t.scale}n)"))} + {fType}");
        _output.WriteLine($"Result: {result}");

        Assert.True(result.IsApplicable);

        if (result is AkraBazziApplicable akra)
        {
            _output.WriteLine($"Critical exponent p = {akra.CriticalExponent:F4}");

            // Verify critical exponent
            Assert.Equal(expectedCritExp, akra.CriticalExponent, precision: 2);

            // Verify solution form
            AssertSolutionForm(akra.Solution, expectedDegree, expectedLog);
        }
        else if (result is MasterTheoremApplicable master)
        {
            // Single-term might be handled by Master Theorem
            _output.WriteLine($"Handled by Master Theorem Case {master.Case}");
        }
    }

    public static IEnumerable<object[]> AkraBazziData => new[]
    {
        // Unbalanced partition: T(n) = T(n/3) + T(2n/3) + n
        new object[] {
            "Unbalanced_1/3_2/3",
            new[] { (1.0, 1.0/3), (1.0, 2.0/3) },
            "O(n)", 1.0, 1.0, 1.0 },

        // Three-way partition: T(n) = T(n/4) + T(n/4) + T(n/2) + n
        new object[] {
            "ThreeWay_1/4_1/4_1/2",
            new[] { (1.0, 0.25), (1.0, 0.25), (1.0, 0.5) },
            "O(n)", 1.0, 1.0, 1.0 },

        // Asymmetric: T(n) = 2T(n/4) + T(n/2) + n
        new object[] {
            "Asymmetric_2x1/4_1x1/2",
            new[] { (2.0, 0.25), (1.0, 0.5) },
            "O(n)", 1.0, 1.0, 1.0 },

        // Weighted: T(n) = 0.5T(n/2) + 0.5T(n/2) + n (same as T(n/2)+n)
        new object[] {
            "Weighted_0.5_0.5",
            new[] { (0.5, 0.5), (0.5, 0.5) },
            "O(n)", 0.0, 1.0, 0.0 },
    };

    #endregion

    #region Critical Exponent Solver - Extensive Verification

    /// <summary>
    /// Verify critical exponent solver against known closed-form solutions.
    /// </summary>
    [Theory]
    [MemberData(nameof(CriticalExponentData))]
    public void CriticalExponent_SolvesCorrectly(
        string name, (double coef, double scale)[] terms, double expectedP)
    {
        var termList = terms.Select(t => (t.coef, t.scale)).ToList();

        var p = _exponentSolver.Solve(termList);

        Assert.NotNull(p);
        _output.WriteLine($"{name}: p = {p.Value:F6} (expected {expectedP:F6})");

        // Verify solution
        var sum = termList.Sum(t => t.coef * Math.Pow(t.scale, p.Value));
        _output.WriteLine($"Verification: Σ aᵢbᵢ^p = {sum:F10}");

        Assert.Equal(1.0, sum, precision: 6);
        Assert.Equal(expectedP, p.Value, precision: 4);
    }

    public static IEnumerable<object[]> CriticalExponentData => new[]
    {
        // Single term: a·b^p = 1 → p = log(a)/log(1/b) = log(a)/(-log(b)) = -log(a)/log(b)
        new object[] { "Binary", new[] { (2.0, 0.5) }, 1.0 },
        new object[] { "BinarySearch", new[] { (1.0, 0.5) }, 0.0 },
        new object[] { "Karatsuba", new[] { (3.0, 0.5) }, Math.Log(3) / Math.Log(2) },
        new object[] { "Strassen", new[] { (7.0, 0.5) }, Math.Log(7) / Math.Log(2) },
        new object[] { "QuadDC", new[] { (4.0, 0.5) }, 2.0 },
        new object[] { "OctaDC", new[] { (8.0, 0.5) }, 3.0 },
        new object[] { "Ternary", new[] { (3.0, 1.0/3) }, 1.0 },

        // Multi-term
        new object[] { "TwoThirds", new[] { (1.0, 1.0/3), (1.0, 2.0/3) }, 1.0 },
        new object[] { "HalfQuarter", new[] { (2.0, 0.25), (1.0, 0.5) }, 1.0 },
    };

    #endregion

    #region Edge Cases and Boundary Conditions

    /// <summary>
    /// Test recurrences at Master Theorem case boundaries.
    /// </summary>
    [Theory]
    [MemberData(nameof(BoundaryData))]
    public void Theorem_HandlesBoundaryConditions(
        string name, int a, int b, string fType, string expectedHandling)
    {
        var var = Variable.N;
        var f = CreateWork(fType, var);
        var recurrence = RecurrenceRelation.DivideAndConquer(a, b, f, var);

        var result = _analyzer.Analyze(recurrence);

        _output.WriteLine($"=== Boundary: {name} ===");
        _output.WriteLine($"Result: {result}");
        _output.WriteLine($"Expected handling: {expectedHandling}");

        // Verify appropriate handling
        switch (expectedHandling)
        {
            case "Case2":
                if (result is MasterTheoremApplicable m2)
                    Assert.Equal(MasterTheoremCase.Case2, m2.Case);
                break;
            case "Gap":
                // Gap cases should fall back to Akra-Bazzi or be marked as Gap
                Assert.True(result.IsApplicable);
                break;
            case "NotApplicable":
                Assert.False(result.IsApplicable);
                break;
        }
    }

    public static IEnumerable<object[]> BoundaryData => new[]
    {
        // Exact Case 2 boundary
        new object[] { "Exact_Case2", 2, 2, "O(n)", "Case2" },
        new object[] { "Exact_Case2_log", 2, 2, "O(nlogn)", "Case2" },

        // Near boundary (gap cases)
        new object[] { "Near_Case1_Case2", 2, 2, "O(n^0.99)", "Gap" },
        new object[] { "Near_Case2_Case3", 2, 2, "O(n^1.01)", "Gap" },
    };

    /// <summary>
    /// Test invalid recurrences.
    /// </summary>
    [Theory]
    [MemberData(nameof(InvalidRecurrenceData))]
    public void Theorem_RejectsInvalidRecurrences(
        string name, double a, double scale, string expectedReason)
    {
        var var = Variable.N;
        var recurrence = new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(a, scale) }.ToList(),
            var,
            new ConstantComplexity(1));

        var result = _analyzer.Analyze(recurrence);

        _output.WriteLine($"=== Invalid: {name} ===");
        _output.WriteLine($"Result: {result}");

        Assert.False(result.IsApplicable);
        Assert.IsType<TheoremNotApplicable>(result);

        var notApplicable = (TheoremNotApplicable)result;
        _output.WriteLine($"Reason: {notApplicable.Reason}");

        Assert.Contains(notApplicable.ViolatedConditions,
            v => v.ToLower().Contains(expectedReason.ToLower()));
    }

    public static IEnumerable<object[]> InvalidRecurrenceData => new[]
    {
        // Invalid coefficient - messages contain "a ≥" or "aᵢ >"
        new object[] { "NegativeCoef", -1.0, 0.5, "a" },
        new object[] { "ZeroCoef", 0.0, 0.5, "theorem" },  // Gets generic "does not fit" message

        // Invalid scale factor - messages contain "bᵢ" or "0 < b"
        new object[] { "ScaleGreaterThan1", 2.0, 1.5, "b" },
        new object[] { "ScaleEqualTo1", 2.0, 1.0, "b" },
        new object[] { "NegativeScale", 2.0, -0.5, "theorem" },  // Gets generic "does not fit" message
        new object[] { "ZeroScale", 2.0, 0.0, "theorem" },  // Gets generic "does not fit" message
    };

    #endregion

    #region Helper Methods

    private ComplexityExpression CreateWork(string type, Variable var) => type switch
    {
        "O(1)" => new ConstantComplexity(1),
        "O(n)" => new LinearComplexity(1, var),
        "O(n^2)" => PolyLogComplexity.Polynomial(2, var),
        "O(n^3)" => PolyLogComplexity.Polynomial(3, var),
        "O(n^0.99)" => new PolyLogComplexity(0.99, 0, var),
        "O(n^1.01)" => new PolyLogComplexity(1.01, 0, var),
        "O(nlogn)" => PolyLogComplexity.NLogN(var),
        "O(logn)" => new LogarithmicComplexity(1, var),
        _ => new ConstantComplexity(1)
    };

    private void LogResult(string name, TheoremApplicability result, double expectedDegree, double expectedLog)
    {
        _output.WriteLine($"=== {name} ===");
        _output.WriteLine($"Result: {result}");
        _output.WriteLine($"Expected: O(n^{expectedDegree}" + (expectedLog > 0 ? $" log^{expectedLog} n)" : ")"));

        if (result is MasterTheoremApplicable m)
        {
            _output.WriteLine($"Case: {m.Case}, LogBA: {m.LogBA:F4}, Solution: {m.Solution?.ToBigONotation()}");
        }
        else if (result is AkraBazziApplicable a)
        {
            _output.WriteLine($"Critical Exponent: {a.CriticalExponent:F4}, Solution: {a.Solution?.ToBigONotation()}");
        }
    }

    private void AssertSolutionDegree(ComplexityExpression? solution, double expectedDegree)
    {
        Assert.NotNull(solution);

        var actualDegree = solution switch
        {
            PolyLogComplexity p => p.PolyDegree,
            LinearComplexity => 1.0,
            ConstantComplexity => 0.0,
            _ => 0.0
        };

        Assert.Equal(expectedDegree, actualDegree, precision: 2);
    }

    private void AssertSolutionForm(ComplexityExpression? solution, double expectedDegree, double expectedLog)
    {
        Assert.NotNull(solution);

        double actualDegree, actualLog;

        if (solution is PolyLogComplexity p)
        {
            actualDegree = p.PolyDegree;
            actualLog = p.LogExponent;
        }
        else if (solution is LogarithmicComplexity)
        {
            actualDegree = 0;
            actualLog = 1;
        }
        else if (solution is LinearComplexity)
        {
            actualDegree = 1;
            actualLog = 0;
        }
        else
        {
            actualDegree = 0;
            actualLog = 0;
        }

        Assert.Equal(expectedDegree, actualDegree, precision: 1);
        Assert.Equal(expectedLog, actualLog, precision: 1);
    }

    #endregion
}
