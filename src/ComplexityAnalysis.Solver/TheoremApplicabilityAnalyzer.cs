using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Main analyzer that determines which recurrence-solving theorem applies
/// and computes the closed-form solution.
///
/// Analysis order:
/// 1. Check if Master Theorem applies (simpler, more precise)
/// 2. Fall back to Akra-Bazzi for multi-term recurrences
/// 3. Handle linear recurrences T(n) = T(n-1) + f(n) directly
/// 4. Report failure with diagnostics
/// </summary>
public sealed class TheoremApplicabilityAnalyzer : ITheoremApplicabilityAnalyzer
{
    private readonly IExpressionClassifier _classifier;
    private readonly ICriticalExponentSolver _criticalExponentSolver;
    private readonly IRegularityChecker _regularityChecker;
    private readonly IAkraBazziIntegralEvaluator _integralEvaluator;

    /// <summary>Tolerance for numerical comparisons.</summary>
    private const double Epsilon = 1e-9;

    /// <summary>Minimum epsilon for Master Theorem cases 1 and 3.</summary>
    private const double MinEpsilon = 0.01;

    public TheoremApplicabilityAnalyzer(
        IExpressionClassifier? classifier = null,
        ICriticalExponentSolver? criticalExponentSolver = null,
        IRegularityChecker? regularityChecker = null,
        IAkraBazziIntegralEvaluator? integralEvaluator = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
        _criticalExponentSolver = criticalExponentSolver ?? MathNetCriticalExponentSolver.Instance;
        _regularityChecker = regularityChecker ?? NumericalRegularityChecker.Instance;
        _integralEvaluator = integralEvaluator ?? TableDrivenIntegralEvaluator.Instance;
    }

    public static TheoremApplicabilityAnalyzer Instance { get; } = new();

    /// <summary>
    /// Analyzes a recurrence and determines which theorem applies.
    /// Tries Master Theorem first, then Akra-Bazzi, then linear recurrence.
    /// </summary>
    public TheoremApplicability Analyze(RecurrenceRelation recurrence)
    {
        // Validate basic conditions
        var validationResult = ValidateRecurrence(recurrence);
        if (validationResult != null)
            return validationResult;

        // Try Master Theorem first (for single-term divide-and-conquer)
        if (recurrence.FitsMasterTheorem)
        {
            var masterResult = CheckMasterTheorem(recurrence);
            if (masterResult.IsApplicable)
                return masterResult;

            // If Master Theorem has a gap, try Akra-Bazzi
            if (masterResult is TheoremNotApplicable { Reason: var reason } && reason.Contains("gap"))
            {
                var akraResult = CheckAkraBazzi(recurrence);
                if (akraResult.IsApplicable)
                    return akraResult;
            }

            return masterResult;
        }

        // Try Akra-Bazzi for multi-term recurrences
        if (recurrence.FitsAkraBazzi)
        {
            return CheckAkraBazzi(recurrence);
        }

        // Check for linear recurrence T(n) = T(n-1) + f(n)
        var linearResult = CheckLinearRecurrence(recurrence);
        if (linearResult != null)
            return linearResult;

        return TheoremNotApplicable.MultipleTermsNoAkraBazzi(
            ImmutableList.Create(
                "Recurrence does not fit standard theorem forms",
                $"Terms: {recurrence.Terms.Count}",
                $"Master Theorem fit: {recurrence.FitsMasterTheorem}",
                $"Akra-Bazzi fit: {recurrence.FitsAkraBazzi}"));
    }

    /// <summary>
    /// Forces Akra-Bazzi analysis even for single-term recurrences.
    /// Useful for cross-validation testing.
    /// </summary>
    public TheoremApplicability AnalyzeWithAkraBazzi(RecurrenceRelation recurrence)
    {
        // Validate basic conditions
        var validationResult = ValidateRecurrence(recurrence);
        if (validationResult != null)
            return validationResult;

        return CheckAkraBazzi(recurrence);
    }

    /// <summary>
    /// Validates that the recurrence is well-formed.
    /// </summary>
    private TheoremNotApplicable? ValidateRecurrence(RecurrenceRelation recurrence)
    {
        if (recurrence.Terms.Count == 0)
        {
            return new TheoremNotApplicable(
                "No recursive terms in recurrence",
                ImmutableList.Create("At least one T(...) term required"));
        }

        // Check for negative coefficients
        if (recurrence.Terms.Any(t => t.Coefficient < 0))
        {
            return TheoremNotApplicable.NegativeCoefficients();
        }

        // Check for non-reducing scale factors
        if (recurrence.Terms.Any(t => t.ScaleFactor >= 1))
        {
            return TheoremNotApplicable.NonReducingRecurrence();
        }

        return null;
    }

    /// <summary>
    /// Checks Master Theorem applicability for T(n) = a·T(n/b) + f(n).
    /// </summary>
    public TheoremApplicability CheckMasterTheorem(RecurrenceRelation recurrence)
    {
        if (!recurrence.FitsMasterTheorem)
        {
            return new TheoremNotApplicable(
                "Recurrence does not fit Master Theorem form",
                ImmutableList.Create(
                    "Master Theorem requires exactly one term: T(n) = a·T(n/b) + f(n)",
                    $"Actual terms: {recurrence.Terms.Count}"));
        }

        var term = recurrence.Terms[0];
        var a = term.Coefficient;
        var b = 1.0 / term.ScaleFactor; // ScaleFactor is 1/b, so b = 1/ScaleFactor
        var f = recurrence.NonRecursiveWork;
        var variable = recurrence.Variable;

        // Compute log_b(a)
        var logBA = Math.Log(a) / Math.Log(b);

        // Classify f(n)
        var fClassification = _classifier.Classify(f, variable);

        if (fClassification.Form == ExpressionForm.Unknown)
        {
            return TheoremNotApplicable.UnclassifiableF();
        }

        // Extract the polynomial degree of f(n)
        if (!_classifier.TryExtractPolyLogForm(f, variable, out var fDegree, out var fLogExponent))
        {
            // Try just polynomial
            if (!_classifier.TryExtractPolynomialDegree(f, variable, out fDegree))
            {
                // f(n) might be constant
                if (fClassification.Form == ExpressionForm.Constant)
                {
                    fDegree = 0;
                    fLogExponent = 0;
                }
                else
                {
                    return TheoremNotApplicable.UnclassifiableF();
                }
            }
        }

        // Determine which case applies
        var diff = fDegree - logBA;

        // Case 1: f(n) = O(n^(log_b(a) - ε)) for some ε > 0
        // i.e., fDegree < logBA (polynomially smaller)
        if (diff < -MinEpsilon)
        {
            var epsilon = -diff;
            var solution = PolyLogComplexity.Polynomial(logBA, variable);

            return new MasterTheoremApplicable(
                MasterTheoremCase.Case1,
                a, b, logBA,
                fClassification,
                solution)
            {
                Epsilon = epsilon,
                LogExponentK = fLogExponent
            };
        }

        // Case 3: f(n) = Ω(n^(log_b(a) + ε)) for some ε > 0
        // i.e., fDegree > logBA (polynomially larger)
        if (diff > MinEpsilon)
        {
            var epsilon = diff;

            // Must verify regularity condition
            var regularityResult = _regularityChecker.CheckRegularity(a, b, f, variable);

            if (!regularityResult.Holds)
            {
                return TheoremNotApplicable.MasterTheoremGap(logBA, fDegree) with
                {
                    Suggestions = ImmutableList.Create(
                        $"Regularity check failed: {regularityResult.Reasoning}",
                        "Use Akra-Bazzi theorem instead")
                };
            }

            // Solution is Θ(f(n))
            ComplexityExpression solution = fLogExponent > 0
                ? new PolyLogComplexity(fDegree, fLogExponent, variable)
                : PolyLogComplexity.Polynomial(fDegree, variable);

            return new MasterTheoremApplicable(
                MasterTheoremCase.Case3,
                a, b, logBA,
                fClassification,
                solution)
            {
                Epsilon = epsilon,
                LogExponentK = fLogExponent,
                RegularityVerified = true
            };
        }

        // Case 2: f(n) = Θ(n^(log_b(a)) · log^k n) for some k ≥ 0
        // i.e., fDegree ≈ logBA
        if (Math.Abs(diff) <= MinEpsilon)
        {
            // Check if f has additional log factors
            var k = fLogExponent;

            // Solution: Θ(n^(log_b(a)) · log^(k+1) n)
            var solution = PolyLogComplexity.MasterCase2Solution(logBA, k, variable);

            return new MasterTheoremApplicable(
                MasterTheoremCase.Case2,
                a, b, logBA,
                fClassification,
                solution)
            {
                Epsilon = 0,
                LogExponentK = k
            };
        }

        // Falls in the gap (shouldn't reach here due to MinEpsilon threshold)
        return TheoremNotApplicable.MasterTheoremGap(logBA, fDegree);
    }

    /// <summary>
    /// Checks Akra-Bazzi theorem applicability.
    /// </summary>
    public TheoremApplicability CheckAkraBazzi(RecurrenceRelation recurrence)
    {
        if (!recurrence.FitsAkraBazzi)
        {
            var violations = new List<string>();

            foreach (var (term, i) in recurrence.Terms.Select((t, i) => (t, i)))
            {
                if (term.Coefficient <= 0)
                    violations.Add($"Term {i}: coefficient {term.Coefficient} must be > 0");
                if (term.ScaleFactor <= 0 || term.ScaleFactor >= 1)
                    violations.Add($"Term {i}: scale factor {term.ScaleFactor} must be in (0, 1)");
            }

            return TheoremNotApplicable.MultipleTermsNoAkraBazzi(violations.ToImmutableList());
        }

        // Extract terms for critical exponent solver
        var terms = recurrence.Terms
            .Select(t => (t.Coefficient, t.ScaleFactor))
            .ToList();

        // Solve for critical exponent p
        var p = _criticalExponentSolver.Solve(terms);

        if (!p.HasValue)
        {
            return new TheoremNotApplicable(
                "Could not solve for critical exponent p",
                ImmutableList.Create(
                    "Newton's method did not converge",
                    $"Terms: {string.Join(", ", terms.Select(t => $"{t.Coefficient}·T({t.ScaleFactor}n)"))}"))
            {
                Suggestions = ImmutableList.Create(
                    "Try numerical methods with different initial guess",
                    "Check that terms satisfy Σᵢ aᵢ·bᵢ^p = 1 for some p")
            };
        }

        var variable = recurrence.Variable;
        var g = recurrence.NonRecursiveWork;

        // Evaluate the Akra-Bazzi integral
        var integralResult = _integralEvaluator.Evaluate(g, variable, p.Value);

        if (!integralResult.Success || integralResult.FullSolution == null)
        {
            // Fall back to basic solution without integral
            var basicSolution = PolyLogComplexity.Polynomial(p.Value, variable);

            return new AkraBazziApplicable(
                p.Value,
                basicSolution)
            {
                Terms = terms.ToImmutableList(),
                GClassification = _classifier.Classify(g, variable)
            };
        }

        return new AkraBazziApplicable(
            p.Value,
            integralResult.FullSolution,
            integralResult.IntegralTerm)
        {
            Terms = terms.ToImmutableList(),
            GClassification = _classifier.Classify(g, variable)
        };
    }

    /// <summary>
    /// Checks for linear recurrence T(n) = T(n-1) + f(n).
    /// </summary>
    private TheoremApplicability? CheckLinearRecurrence(RecurrenceRelation recurrence)
    {
        // Linear recurrence: single term with argument n-1 (or similar)
        // This doesn't fit the multiplicative scale factor model directly

        if (recurrence.Terms.Count != 1)
            return null;

        var term = recurrence.Terms[0];

        // Check if this looks like T(n-1) rather than T(n/b)
        // Linear recurrence has scale factor that approaches 1 as n grows
        // but argument is n - constant, not n × constant

        // For now, we detect this heuristically based on the argument structure
        // A more robust approach would parse the argument expression

        // If coefficient is 1 and scale factor is very close to 1, might be linear
        if (Math.Abs(term.Coefficient - 1.0) < Epsilon && term.ScaleFactor > 0.99)
        {
            var f = recurrence.NonRecursiveWork;
            var variable = recurrence.Variable;
            var fClassification = _classifier.Classify(f, variable);

            // T(n) = T(n-1) + f(n) has solution T(n) = Σᵢ f(i) + T(0)
            // For f(n) = n^k: sum is Θ(n^(k+1))
            // For f(n) = c: sum is Θ(n)

            ComplexityExpression solution;
            string method;

            if (fClassification.Form == ExpressionForm.Constant)
            {
                solution = new LinearComplexity(1.0, variable);
                method = "summation of constant: Σ c = Θ(n)";
            }
            else if (fClassification.Form == ExpressionForm.Polynomial && fClassification.PrimaryParameter.HasValue)
            {
                var k = fClassification.PrimaryParameter.Value;
                solution = PolyLogComplexity.Polynomial(k + 1, variable);
                method = $"summation of polynomial: Σ n^{k:F1} = Θ(n^{k + 1:F1})";
            }
            else if (fClassification.Form == ExpressionForm.Logarithmic)
            {
                // Σ log(i) ≈ n log n - n = Θ(n log n)
                solution = PolyLogComplexity.NLogN(variable);
                method = "summation of logarithm: Σ log(i) = Θ(n log n)";
            }
            else if (fClassification.Form == ExpressionForm.PolyLog && fClassification.PrimaryParameter.HasValue)
            {
                var k = fClassification.PrimaryParameter.Value;
                var j = fClassification.LogExponent ?? 0;
                // Σ n^k log^j(n) ≈ Θ(n^(k+1) log^j(n))
                solution = new PolyLogComplexity(k + 1, j, variable);
                method = $"summation of polylog: Σ n^{k:F1}·log^{j} n = Θ(n^{k + 1:F1}·log^{j} n)";
            }
            else
            {
                return null; // Can't solve
            }

            return new LinearRecurrenceSolved(solution, method);
        }

        return null;
    }
}

/// <summary>
/// Extension methods for convenient analysis.
/// </summary>
public static class RecurrenceAnalysisExtensions
{
    private static readonly TheoremApplicabilityAnalyzer DefaultAnalyzer = TheoremApplicabilityAnalyzer.Instance;

    /// <summary>
    /// Analyzes a recurrence relation using the default analyzer.
    /// </summary>
    public static TheoremApplicability Analyze(this RecurrenceRelation recurrence) =>
        DefaultAnalyzer.Analyze(recurrence);

    /// <summary>
    /// Analyzes a RecurrenceComplexity using the default analyzer.
    /// </summary>
    public static TheoremApplicability Analyze(this RecurrenceComplexity recurrence)
    {
        var relation = RecurrenceRelation.FromComplexity(recurrence);
        return DefaultAnalyzer.Analyze(relation);
    }

    /// <summary>
    /// Creates a binary divide-and-conquer recurrence T(n) = 2T(n/2) + f(n).
    /// </summary>
    public static RecurrenceRelation BinaryDivideAndConquer(
        ComplexityExpression mergeWork,
        Variable? variable = null)
    {
        var v = variable ?? Variable.N;
        return RecurrenceRelation.DivideAndConquer(2, 2, mergeWork, v);
    }

    /// <summary>
    /// Creates a Karatsuba-style recurrence T(n) = 3T(n/2) + f(n).
    /// </summary>
    public static RecurrenceRelation KaratsubaStyle(
        ComplexityExpression mergeWork,
        Variable? variable = null)
    {
        var v = variable ?? Variable.N;
        return RecurrenceRelation.DivideAndConquer(3, 2, mergeWork, v);
    }

    /// <summary>
    /// Creates a Strassen-style recurrence T(n) = 7T(n/2) + f(n).
    /// </summary>
    public static RecurrenceRelation StrassenStyle(
        ComplexityExpression mergeWork,
        Variable? variable = null)
    {
        var v = variable ?? Variable.N;
        return RecurrenceRelation.DivideAndConquer(7, 2, mergeWork, v);
    }
}
