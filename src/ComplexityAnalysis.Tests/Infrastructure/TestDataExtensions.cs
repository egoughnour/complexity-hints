using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Tests.Infrastructure;

/// <summary>
/// Strongly-typed test data structures and extension methods for data-driven testing.
/// Provides a clean abstraction layer for parameterized tests.
/// </summary>
public static class TestDataExtensions
{
    #region Recurrence Test Data

    /// <summary>
    /// Represents a test case for recurrence relation solving with expected results.
    /// </summary>
    public record RecurrenceTestCase(
        string Name,
        int Subproblems,            // a in T(n) = aT(n/b) + f(n)
        int DivisionFactor,         // b
        string MergeWorkType,       // "constant", "linear", "quadratic", "polylog", "custom"
        int MergeWorkDegree,        // for polylog: polynomial degree
        int MergeWorkLogExponent,   // for polylog: log exponent
        string ExpectedTheorem,     // "master", "akra-bazzi", "linear", "none"
        string ExpectedCase,        // "case1", "case2", "case3", "gap", or critical exponent
        double ExpectedPolyDegree,  // expected solution polynomial degree
        double ExpectedLogExponent, // expected solution log exponent
        double Tolerance = 0.01)
    {
        public ComplexityExpression GetMergeWork(Variable var) => MergeWorkType.ToLower() switch
        {
            "constant" => new ConstantComplexity(1),
            "linear" => new LinearComplexity(1.0, var),
            "quadratic" => PolyLogComplexity.Polynomial(2, var),
            "cubic" => PolyLogComplexity.Polynomial(3, var),
            "polylog" => new PolyLogComplexity(MergeWorkDegree, MergeWorkLogExponent, var),
            "nlogn" => PolyLogComplexity.NLogN(var),
            "logn" => new LogarithmicComplexity(1, var),
            _ => new ConstantComplexity(1)
        };

        public RecurrenceRelation ToRecurrence(Variable var) =>
            RecurrenceRelation.DivideAndConquer(Subproblems, DivisionFactor, GetMergeWork(var), var);
    }

    /// <summary>
    /// Represents a multi-term Akra-Bazzi test case.
    /// </summary>
    public record AkraBazziTestCase(
        string Name,
        (double Coef, double Scale)[] Terms,
        string MergeWorkType,
        int MergeWorkDegree,
        int MergeWorkLogExponent,
        double ExpectedCriticalExponent,
        double ExpectedPolyDegree,
        double ExpectedLogExponent,
        double Tolerance = 0.02)
    {
        public ComplexityExpression GetMergeWork(Variable var) => MergeWorkType.ToLower() switch
        {
            "constant" => new ConstantComplexity(1),
            "linear" => new LinearComplexity(1.0, var),
            "quadratic" => PolyLogComplexity.Polynomial(2, var),
            "polylog" => new PolyLogComplexity(MergeWorkDegree, MergeWorkLogExponent, var),
            "nlogn" => PolyLogComplexity.NLogN(var),
            "logn" => new LogarithmicComplexity(1, var),
            _ => new ConstantComplexity(1)
        };

        public RecurrenceRelation ToRecurrence(Variable var) =>
            new RecurrenceRelation(
                Terms.Select(t => new RecurrenceRelationTerm(t.Coef, t.Scale)).ToList(),
                var,
                GetMergeWork(var));
    }

    #endregion

    #region Critical Exponent Test Data

    /// <summary>
    /// Test data for critical exponent solver validation.
    /// </summary>
    public record CriticalExponentTestCase(
        string Name,
        (double Coef, double Scale)[] Terms,
        double ExpectedExponent,
        double Tolerance = 1e-6)
    {
        public IReadOnlyList<(double Coefficient, double ScaleFactor)> ToTermList() =>
            Terms.Select(t => (t.Coef, t.Scale)).ToList();
    }

    #endregion

    #region Expression Classification Test Data

    /// <summary>
    /// Test data for expression classification.
    /// </summary>
    public record ClassificationTestCase(
        string Name,
        Func<Variable, ComplexityExpression> ExpressionFactory,
        ExpressionForm ExpectedForm,
        double ExpectedPrimaryParameter,
        double ExpectedLogExponent = 0,
        double Tolerance = 0.001);

    #endregion

    #region BCL Complexity Test Data

    /// <summary>
    /// Test data for BCL method complexity verification.
    /// </summary>
    public record BCLComplexityTestCase(
        string TypeName,
        string MethodName,
        string ExpectedComplexity,  // "O(1)", "O(n)", "O(log n)", "O(n log n)", etc.
        bool IsAmortized = false,
        string SourceType = "Documented");

    #endregion

    #region Expression Evaluation Test Data

    /// <summary>
    /// Test data for expression evaluation.
    /// </summary>
    public record EvaluationTestCase(
        string Name,
        Func<Variable, ComplexityExpression> ExpressionFactory,
        double InputValue,
        double ExpectedResult,
        double Tolerance = 0.001);

    #endregion

    #region Extension Methods for Test Context

    /// <summary>
    /// Converts inline test data to strongly typed RecurrenceTestCase.
    /// Usage: [Theory] [MemberData(nameof(MasterTheoremCases))]
    /// </summary>
    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<RecurrenceTestCase> cases)
        => cases.Select(c => new object[] { c });

    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<AkraBazziTestCase> cases)
        => cases.Select(c => new object[] { c });

    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<CriticalExponentTestCase> cases)
        => cases.Select(c => new object[] { c });

    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<ClassificationTestCase> cases)
        => cases.Select(c => new object[] { c });

    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<BCLComplexityTestCase> cases)
        => cases.Select(c => new object[] { c });

    public static IEnumerable<object[]> ToTheoryData(this IEnumerable<EvaluationTestCase> cases)
        => cases.Select(c => new object[] { c });

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Asserts that a PolyLogComplexity matches expected degrees within tolerance.
    /// </summary>
    public static void AssertPolyLogEquals(
        ComplexityExpression? actual,
        double expectedPolyDegree,
        double expectedLogExponent,
        double tolerance = 0.01)
    {
        if (actual is null)
            throw new Xunit.Sdk.XunitException("Expected PolyLogComplexity but got null");

        if (actual is PolyLogComplexity polyLog)
        {
            Xunit.Assert.Equal(expectedPolyDegree, polyLog.PolyDegree, precision: (int)Math.Ceiling(-Math.Log10(tolerance)));
            Xunit.Assert.Equal(expectedLogExponent, polyLog.LogExponent, precision: (int)Math.Ceiling(-Math.Log10(tolerance)));
        }
        else if (actual is LogarithmicComplexity log && expectedPolyDegree == 0 && expectedLogExponent == 1)
        {
            // O(log n) equivalent
            return;
        }
        else if (actual is LinearComplexity lin && expectedPolyDegree == 1 && expectedLogExponent == 0)
        {
            // O(n) equivalent
            return;
        }
        else if (actual is ConstantComplexity && expectedPolyDegree == 0 && expectedLogExponent == 0)
        {
            // O(1) equivalent
            return;
        }
        else
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected PolyLogComplexity({expectedPolyDegree}, {expectedLogExponent}) but got {actual.GetType().Name}: {actual.ToBigONotation()}");
        }
    }

    /// <summary>
    /// Computes the expected critical exponent analytically for validation.
    /// For single-term: a * b^p = 1 → p = log_b(a) = log(a)/log(1/b)
    /// </summary>
    public static double ComputeAnalyticalCriticalExponent(int a, int b)
        => Math.Log(a) / Math.Log(b);

    /// <summary>
    /// Verifies a critical exponent by evaluating the sum equation.
    /// </summary>
    public static double EvaluateCriticalExponentSum(IEnumerable<(double Coef, double Scale)> terms, double p)
        => terms.Sum(t => t.Coef * Math.Pow(t.Scale, p));

    #endregion
}

/// <summary>
/// Standard test data sets for reuse across test classes.
/// </summary>
public static class StandardTestData
{
    /// <summary>
    /// Comprehensive Master Theorem test cases covering all cases and edge scenarios.
    /// </summary>
    public static IEnumerable<TestDataExtensions.RecurrenceTestCase> MasterTheoremCases => new[]
    {
        // Case 1: f(n) polynomially smaller than n^(log_b a)
        new TestDataExtensions.RecurrenceTestCase(
            "BinaryDC_Constant", 2, 2, "constant", 0, 0,
            "master", "case1", 1.0, 0.0),  // T(n)=2T(n/2)+O(1) → Θ(n)

        new TestDataExtensions.RecurrenceTestCase(
            "QuadDC_Linear", 4, 2, "linear", 0, 0,
            "master", "case1", 2.0, 0.0),  // T(n)=4T(n/2)+O(n) → Θ(n²)

        new TestDataExtensions.RecurrenceTestCase(
            "Strassen_Quadratic", 7, 2, "quadratic", 0, 0,
            "master", "case1", 2.807, 0.0, 0.01),  // T(n)=7T(n/2)+O(n²) → Θ(n^2.807)

        new TestDataExtensions.RecurrenceTestCase(
            "OctaDC_Linear", 8, 2, "linear", 0, 0,
            "master", "case1", 3.0, 0.0),  // T(n)=8T(n/2)+O(n) → Θ(n³)

        // Case 2: f(n) = Θ(n^d · log^k n) where d = log_b(a)
        new TestDataExtensions.RecurrenceTestCase(
            "MergeSort", 2, 2, "linear", 0, 0,
            "master", "case2", 1.0, 1.0),  // T(n)=2T(n/2)+O(n) → Θ(n log n)

        new TestDataExtensions.RecurrenceTestCase(
            "MergeSort_NLogN", 2, 2, "nlogn", 1, 1,
            "master", "case2", 1.0, 2.0),  // T(n)=2T(n/2)+O(n log n) → Θ(n log² n)

        new TestDataExtensions.RecurrenceTestCase(
            "BinarySearch", 1, 2, "constant", 0, 0,
            "master", "case2", 0.0, 1.0),  // T(n)=T(n/2)+O(1) → Θ(log n)

        new TestDataExtensions.RecurrenceTestCase(
            "QuadDC_Quadratic", 4, 2, "quadratic", 0, 0,
            "master", "case2", 2.0, 1.0),  // T(n)=4T(n/2)+O(n²) → Θ(n² log n)

        // Case 3: f(n) polynomially dominates n^(log_b a)
        new TestDataExtensions.RecurrenceTestCase(
            "BinarySearch_Linear", 1, 2, "linear", 0, 0,
            "master", "case3", 1.0, 0.0),  // T(n)=T(n/2)+O(n) → Θ(n)

        new TestDataExtensions.RecurrenceTestCase(
            "SingleTerm_Quadratic", 1, 2, "quadratic", 0, 0,
            "master", "case3", 2.0, 0.0),  // T(n)=T(n/2)+O(n²) → Θ(n²)

        new TestDataExtensions.RecurrenceTestCase(
            "BinaryDC_Cubic", 2, 2, "cubic", 0, 0,
            "master", "case3", 3.0, 0.0),  // T(n)=2T(n/2)+O(n³) → Θ(n³)

        // Classic algorithms
        new TestDataExtensions.RecurrenceTestCase(
            "Karatsuba", 3, 2, "linear", 0, 0,
            "master", "case1", 1.585, 0.0, 0.01),  // Θ(n^log₂3)

        new TestDataExtensions.RecurrenceTestCase(
            "MatrixMultiply_Naive", 8, 2, "quadratic", 0, 0,
            "master", "case1", 3.0, 0.0),  // Θ(n³)

        // Ternary division
        new TestDataExtensions.RecurrenceTestCase(
            "TernaryDC_Linear", 3, 3, "linear", 0, 0,
            "master", "case2", 1.0, 1.0),  // T(n)=3T(n/3)+O(n) → Θ(n log n)

        new TestDataExtensions.RecurrenceTestCase(
            "TernarySearch", 1, 3, "constant", 0, 0,
            "master", "case2", 0.0, 1.0),  // T(n)=T(n/3)+O(1) → Θ(log n)
    };

    /// <summary>
    /// Akra-Bazzi specific test cases (multi-term recurrences).
    /// </summary>
    public static IEnumerable<TestDataExtensions.AkraBazziTestCase> AkraBazziCases => new[]
    {
        new TestDataExtensions.AkraBazziTestCase(
            "UnbalancedPartition",
            new[] { (1.0, 1.0/3), (1.0, 2.0/3) },
            "linear", 0, 0, 1.0, 1.0, 1.0),  // T(n)=T(n/3)+T(2n/3)+n → Θ(n log n)

        new TestDataExtensions.AkraBazziTestCase(
            "ThreeWayPartition",
            new[] { (1.0, 0.25), (1.0, 0.25), (1.0, 0.5) },
            "linear", 0, 0, 1.0, 1.0, 1.0),  // Multiple terms

        new TestDataExtensions.AkraBazziTestCase(
            "AsymmetricDC",
            new[] { (2.0, 0.25), (1.0, 0.5) },
            "linear", 0, 0, 1.0, 1.0, 1.0),  // 2T(n/4)+T(n/2)+n

        new TestDataExtensions.AkraBazziTestCase(
            "WeightedSplit",
            new[] { (0.5, 0.5), (0.5, 0.5) },
            "linear", 0, 0, 0.0, 1.0, 0.0),  // Coefficients < 1

        new TestDataExtensions.AkraBazziTestCase(
            "QuarterSplit",
            new[] { (1.0, 0.25), (1.0, 0.25), (1.0, 0.25), (1.0, 0.25) },
            "linear", 0, 0, 1.0, 1.0, 1.0),  // 4 × T(n/4) + n
    };

    /// <summary>
    /// Critical exponent solver test cases with analytically computed expected values.
    /// </summary>
    public static IEnumerable<TestDataExtensions.CriticalExponentTestCase> CriticalExponentCases => new[]
    {
        // Single term: a·b^p = 1 → p = log(a)/log(1/b)
        new TestDataExtensions.CriticalExponentTestCase("Binary", new[] { (2.0, 0.5) }, 1.0),
        new TestDataExtensions.CriticalExponentTestCase("BinarySearch", new[] { (1.0, 0.5) }, 0.0),
        new TestDataExtensions.CriticalExponentTestCase("Karatsuba", new[] { (3.0, 0.5) }, Math.Log(3)/Math.Log(2)),
        new TestDataExtensions.CriticalExponentTestCase("Strassen", new[] { (7.0, 0.5) }, Math.Log(7)/Math.Log(2)),
        new TestDataExtensions.CriticalExponentTestCase("QuadDC", new[] { (4.0, 0.5) }, 2.0),
        new TestDataExtensions.CriticalExponentTestCase("OctaDC", new[] { (8.0, 0.5) }, 3.0),
        new TestDataExtensions.CriticalExponentTestCase("Ternary", new[] { (3.0, 1.0/3) }, 1.0),
        new TestDataExtensions.CriticalExponentTestCase("TernarySearch", new[] { (1.0, 1.0/3) }, 0.0),

        // Multi-term: need numerical solution
        new TestDataExtensions.CriticalExponentTestCase(
            "UnbalancedPartition",
            new[] { (1.0, 1.0/3), (1.0, 2.0/3) }, 1.0),  // (1/3)^1 + (2/3)^1 = 1

        new TestDataExtensions.CriticalExponentTestCase(
            "HalfQuarter",
            new[] { (2.0, 0.25), (1.0, 0.5) }, 1.0),  // 2·(1/4)^1 + 1·(1/2)^1 = 0.5 + 0.5 = 1
    };

    /// <summary>
    /// Expression classification test cases.
    /// </summary>
    public static IEnumerable<TestDataExtensions.ClassificationTestCase> ClassificationCases => new[]
    {
        new TestDataExtensions.ClassificationTestCase("Constant", v => new ConstantComplexity(5), ExpressionForm.Constant, 0),
        new TestDataExtensions.ClassificationTestCase("Linear", v => new LinearComplexity(1, v), ExpressionForm.Polynomial, 1),
        new TestDataExtensions.ClassificationTestCase("Quadratic", v => PolyLogComplexity.Polynomial(2, v), ExpressionForm.Polynomial, 2),
        new TestDataExtensions.ClassificationTestCase("Cubic", v => PolyLogComplexity.Polynomial(3, v), ExpressionForm.Polynomial, 3),
        new TestDataExtensions.ClassificationTestCase("Logarithmic", v => new LogarithmicComplexity(1, v), ExpressionForm.Logarithmic, 0, 1),
        new TestDataExtensions.ClassificationTestCase("NLogN", v => PolyLogComplexity.NLogN(v), ExpressionForm.PolyLog, 1, 1),
        new TestDataExtensions.ClassificationTestCase("NLogSquaredN", v => new PolyLogComplexity(1, 2, v), ExpressionForm.PolyLog, 1, 2),
        new TestDataExtensions.ClassificationTestCase("Exponential", v => new ExponentialComplexity(2, v), ExpressionForm.Exponential, 2),
        new TestDataExtensions.ClassificationTestCase("Factorial", v => new FactorialComplexity(v), ExpressionForm.Factorial, 0),
    };

    /// <summary>
    /// BCL complexity verification test cases.
    /// </summary>
    public static IEnumerable<TestDataExtensions.BCLComplexityTestCase> BCLComplexityCases => new[]
    {
        // List<T>
        new TestDataExtensions.BCLComplexityTestCase("List`1", "get_Count", "O(1)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "get_Item", "O(1)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "Add", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "Contains", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "IndexOf", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "Insert", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "Remove", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "Sort", "O(n log n)"),
        new TestDataExtensions.BCLComplexityTestCase("List`1", "BinarySearch", "O(log n)"),

        // Dictionary<K,V>
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "get_Count", "O(1)"),
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "Add", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "ContainsKey", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "TryGetValue", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "ContainsValue", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("Dictionary`2", "Clear", "O(n)"),

        // HashSet<T>
        new TestDataExtensions.BCLComplexityTestCase("HashSet`1", "Add", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("HashSet`1", "Contains", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("HashSet`1", "Remove", "O(1)", true),

        // SortedSet<T>
        new TestDataExtensions.BCLComplexityTestCase("SortedSet`1", "Add", "O(log n)"),
        new TestDataExtensions.BCLComplexityTestCase("SortedSet`1", "Contains", "O(log n)"),
        new TestDataExtensions.BCLComplexityTestCase("SortedSet`1", "Remove", "O(log n)"),

        // Queue/Stack
        new TestDataExtensions.BCLComplexityTestCase("Queue`1", "Enqueue", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("Queue`1", "Dequeue", "O(1)"),
        new TestDataExtensions.BCLComplexityTestCase("Stack`1", "Push", "O(1)", true),
        new TestDataExtensions.BCLComplexityTestCase("Stack`1", "Pop", "O(1)"),

        // String
        new TestDataExtensions.BCLComplexityTestCase("String", "IndexOf", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("String", "Contains", "O(n)"),
        new TestDataExtensions.BCLComplexityTestCase("String", "Substring", "O(n)"),

        // Array
        new TestDataExtensions.BCLComplexityTestCase("Array", "Sort", "O(n log n)"),
        new TestDataExtensions.BCLComplexityTestCase("Array", "BinarySearch", "O(log n)"),
    };

    /// <summary>
    /// Expression evaluation test cases for numeric verification.
    /// </summary>
    public static IEnumerable<TestDataExtensions.EvaluationTestCase> EvaluationCases => new[]
    {
        new TestDataExtensions.EvaluationTestCase("Constant", _ => new ConstantComplexity(42), 100, 42),
        new TestDataExtensions.EvaluationTestCase("Linear_n=100", v => new LinearComplexity(1, v), 100, 100),
        new TestDataExtensions.EvaluationTestCase("Linear_n=1000", v => new LinearComplexity(2, v), 1000, 2000),
        new TestDataExtensions.EvaluationTestCase("Quadratic_n=10", v => PolyLogComplexity.Polynomial(2, v), 10, 100),
        new TestDataExtensions.EvaluationTestCase("Log_n=1024", v => new LogarithmicComplexity(1, v), 1024, 10),  // log₂(1024)
        new TestDataExtensions.EvaluationTestCase("NLogN_n=100", v => PolyLogComplexity.NLogN(v), 100, 100 * Math.Log2(100), 0.001),
        new TestDataExtensions.EvaluationTestCase("Exponential_n=10", v => new ExponentialComplexity(2, v), 10, 1024),
    };
}
