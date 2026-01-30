using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Recurrence;

/// <summary>
/// Base type for theorem applicability results.
/// Captures which theorem applies (or not) and all relevant parameters.
/// </summary>
public abstract record TheoremApplicability
{
    /// <summary>Whether any theorem successfully applies.</summary>
    public abstract bool IsApplicable { get; }

    /// <summary>The recommended solution if applicable.</summary>
    public abstract ComplexityExpression? Solution { get; }

    /// <summary>Human-readable explanation of the result.</summary>
    public abstract string Explanation { get; }
}

/// <summary>
/// Master Theorem applies successfully.
/// </summary>
public sealed record MasterTheoremApplicable(
    /// <summary>Which case of the Master Theorem applies.</summary>
    MasterTheoremCase Case,

    /// <summary>Number of subproblems: a in T(n) = aT(n/b) + f(n).</summary>
    double A,

    /// <summary>Division factor: b in T(n) = aT(n/b) + f(n).</summary>
    double B,

    /// <summary>Critical exponent: log_b(a).</summary>
    double LogBA,

    /// <summary>Classification of f(n).</summary>
    ExpressionClassification FClassification,

    /// <summary>The closed-form solution.</summary>
    ComplexityExpression SolutionExpr) : TheoremApplicability
{
    public override bool IsApplicable => true;
    public override ComplexityExpression? Solution => SolutionExpr;

    /// <summary>
    /// For Case 1: the ε such that f(n) = O(n^(log_b(a) - ε)).
    /// For Case 3: the ε such that f(n) = Ω(n^(log_b(a) + ε)).
    /// For Case 2: 0 (exact match).
    /// </summary>
    public double Epsilon { get; init; }

    /// <summary>For Case 2: the k in f(n) = Θ(n^d · log^k n).</summary>
    public double? LogExponentK { get; init; }

    /// <summary>For Case 3: whether the regularity condition was verified.</summary>
    public bool? RegularityVerified { get; init; }

    public override string Explanation => Case switch
    {
        MasterTheoremCase.Case1 =>
            $"Master Theorem Case 1: f(n) = O(n^{LogBA - Epsilon:F2}) is polynomially smaller than n^{LogBA:F2}. " +
            $"Solution: Θ(n^{LogBA:F2})",

        MasterTheoremCase.Case2 =>
            $"Master Theorem Case 2: f(n) = Θ(n^{LogBA:F2} · log^{LogExponentK ?? 0} n). " +
            $"Solution: Θ(n^{LogBA:F2} · log^{(LogExponentK ?? 0) + 1} n)",

        MasterTheoremCase.Case3 =>
            $"Master Theorem Case 3: f(n) = Ω(n^{LogBA + Epsilon:F2}) dominates n^{LogBA:F2}" +
            (RegularityVerified == true ? " and regularity holds" : "") +
            $". Solution: Θ(f(n))",

        _ => "Master Theorem applies (case unknown)"
    };
}

/// <summary>
/// The three cases of the Master Theorem.
/// </summary>
public enum MasterTheoremCase
{
    /// <summary>
    /// f(n) = O(n^(log_b(a) - ε)) for some ε > 0.
    /// Work at leaves dominates. Solution: Θ(n^(log_b a)).
    /// </summary>
    Case1,

    /// <summary>
    /// f(n) = Θ(n^(log_b a) · log^k n) for some k ≥ 0.
    /// Work balanced across levels. Solution: Θ(n^(log_b a) · log^(k+1) n).
    /// </summary>
    Case2,

    /// <summary>
    /// f(n) = Ω(n^(log_b(a) + ε)) for some ε > 0, AND regularity holds.
    /// Work at root dominates. Solution: Θ(f(n)).
    /// </summary>
    Case3,

    /// <summary>
    /// Falls between cases (Master Theorem gap).
    /// Use Akra-Bazzi or other methods.
    /// </summary>
    Gap
}

/// <summary>
/// Akra-Bazzi Theorem applies successfully.
/// </summary>
public sealed record AkraBazziApplicable(
    /// <summary>
    /// Critical exponent p satisfying Σᵢ aᵢ · bᵢ^p = 1.
    /// </summary>
    double CriticalExponent,

    /// <summary>
    /// The closed-form solution.
    /// </summary>
    ComplexityExpression SolutionExpr,

    /// <summary>
    /// The evaluated integral term from Akra-Bazzi formula.
    /// </summary>
    ComplexityExpression? IntegralTerm = null) : TheoremApplicability
{
    public override bool IsApplicable => true;
    public override ComplexityExpression? Solution => SolutionExpr;

    /// <summary>The recurrence terms used.</summary>
    public ImmutableList<(double Coefficient, double ScaleFactor)> Terms { get; init; } =
        ImmutableList<(double, double)>.Empty;

    /// <summary>Classification of g(n).</summary>
    public ExpressionClassification? GClassification { get; init; }

    public override string Explanation =>
        $"Akra-Bazzi Theorem: p = {CriticalExponent:F4} satisfies Σᵢ aᵢ · bᵢ^p = 1. " +
        $"Solution: Θ(n^{CriticalExponent:F2}" +
        (IntegralTerm != null ? $" · (1 + integral)" : "") + ")";
}

/// <summary>
/// Linear recurrence T(n) = T(n-1) + f(n) solved directly.
/// </summary>
public sealed record LinearRecurrenceSolved(
    /// <summary>The closed-form solution.</summary>
    ComplexityExpression SolutionExpr,

    /// <summary>Explanation of how it was solved.</summary>
    string Method) : TheoremApplicability
{
    public override bool IsApplicable => true;
    public override ComplexityExpression? Solution => SolutionExpr;

    public override string Explanation =>
        $"Linear recurrence solved by {Method}. Solution: {SolutionExpr.ToBigONotation()}";
}

/// <summary>
/// No standard theorem applies.
/// </summary>
public sealed record TheoremNotApplicable(
    /// <summary>Primary reason why no theorem applies.</summary>
    string Reason,

    /// <summary>Specific conditions that were violated.</summary>
    ImmutableList<string> ViolatedConditions) : TheoremApplicability
{
    public override bool IsApplicable => false;
    public override ComplexityExpression? Solution => null;

    /// <summary>Suggested alternative approaches.</summary>
    public ImmutableList<string> Suggestions { get; init; } = ImmutableList<string>.Empty;

    public override string Explanation =>
        $"No standard theorem applies: {Reason}. " +
        $"Violated: {string.Join(", ", ViolatedConditions)}";

    #region Factory Methods

    public static TheoremNotApplicable MultipleTermsNoAkraBazzi(ImmutableList<string> issues) =>
        new("Multiple recursive terms but Akra-Bazzi conditions not met", issues)
        {
            Suggestions = ImmutableList.Create(
                "Try numerical evaluation",
                "Check if terms can be combined",
                "Consider substitution method")
        };

    public static TheoremNotApplicable MasterTheoremGap(double logBA, double fDegree) =>
        new($"f(n) degree ({fDegree:F2}) falls in Master Theorem gap near log_b(a) = {logBA:F2}",
            ImmutableList.Create(
                $"f(n) is neither O(n^{logBA - 0.01:F2}) nor Ω(n^{logBA + 0.01:F2})",
                "Regularity condition may not hold"))
        {
            Suggestions = ImmutableList.Create(
                "Use Akra-Bazzi theorem instead",
                "Try perturbation analysis",
                "Use numerical methods for tight bound")
        };

    public static TheoremNotApplicable NonReducingRecurrence() =>
        new("Recurrence is not reducing (subproblem size ≥ original)",
            ImmutableList.Create("All terms must have 0 < bᵢ < 1"))
        {
            Suggestions = ImmutableList.Create(
                "Check for off-by-one errors in recursion",
                "May be infinite recursion or different pattern")
        };

    public static TheoremNotApplicable NegativeCoefficients() =>
        new("Negative coefficients in recurrence",
            ImmutableList.Create("Master Theorem requires a ≥ 1", "Akra-Bazzi requires aᵢ > 0"))
        {
            Suggestions = ImmutableList.Create(
                "Check recurrence formulation",
                "May need different analysis approach")
        };

    public static TheoremNotApplicable UnclassifiableF() =>
        new("Cannot classify f(n) into standard form",
            ImmutableList.Create("f(n) must be recognizable as polynomial, polylog, or similar"))
        {
            Suggestions = ImmutableList.Create(
                "Try simplifying f(n) expression",
                "Use numerical evaluation",
                "May need custom analysis")
        };

    #endregion
}

/// <summary>
/// Analyzer that determines which theorem applies to a recurrence.
/// </summary>
public interface ITheoremApplicabilityAnalyzer
{
    /// <summary>
    /// Analyzes a recurrence and determines which theorem applies.
    /// </summary>
    TheoremApplicability Analyze(RecurrenceRelation recurrence);

    /// <summary>
    /// Specifically checks Master Theorem applicability.
    /// </summary>
    TheoremApplicability CheckMasterTheorem(RecurrenceRelation recurrence);

    /// <summary>
    /// Specifically checks Akra-Bazzi applicability.
    /// </summary>
    TheoremApplicability CheckAkraBazzi(RecurrenceRelation recurrence);
}

/// <summary>
/// Extension methods for working with theorem applicability.
/// </summary>
public static class TheoremApplicabilityExtensions
{
    /// <summary>
    /// Tries Master Theorem first, then Akra-Bazzi, then reports failure.
    /// </summary>
    public static TheoremApplicability AnalyzeRecurrence(
        this RecurrenceComplexity recurrence,
        ITheoremApplicabilityAnalyzer analyzer)
    {
        var relation = RecurrenceRelation.FromComplexity(recurrence);
        return analyzer.Analyze(relation);
    }
}
