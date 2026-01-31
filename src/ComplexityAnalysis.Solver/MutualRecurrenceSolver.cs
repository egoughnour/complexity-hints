using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Solves mutual recursion systems by converting them to equivalent single recurrences.
/// 
/// Key insight: A mutual recursion cycle A → B → C → A can be "unrolled" to a single
/// recurrence by substitution. If each step reduces by 1:
///   A(n) calls B(n-1), B(n) calls C(n-1), C(n) calls A(n-1)
///   Combined: A(n) = f_A + f_B + f_C + A(n-3)
///   This is T(n) = T(n-k) + g(n) where k = cycle length
/// </summary>
public sealed class MutualRecurrenceSolver
{
    private readonly IExpressionClassifier _classifier;
    private readonly TheoremApplicabilityAnalyzer _theoremAnalyzer;

    public MutualRecurrenceSolver(
        IExpressionClassifier? classifier = null,
        TheoremApplicabilityAnalyzer? theoremAnalyzer = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
        _theoremAnalyzer = theoremAnalyzer ?? TheoremApplicabilityAnalyzer.Instance;
    }

    public static MutualRecurrenceSolver Instance { get; } = new();

    /// <summary>
    /// Solves a mutual recursion system.
    /// </summary>
    public MutualRecurrenceSolution Solve(MutualRecurrenceSystem system)
    {
        if (system.Components.Count == 0)
        {
            return MutualRecurrenceSolution.Failed("Empty mutual recursion system");
        }

        // Convert to equivalent single recurrence
        var equivalentRecurrence = system.ToSingleRecurrence();

        // Determine solving approach based on pattern
        if (system.IsSubtractionPattern)
        {
            return SolveSubtractionPattern(system, equivalentRecurrence);
        }

        if (system.IsDivisionPattern)
        {
            return SolveDivisionPattern(system, equivalentRecurrence);
        }

        // Mixed pattern - try to solve anyway
        return SolveMixedPattern(system, equivalentRecurrence);
    }

    /// <summary>
    /// Solves subtraction-based mutual recursion: A(n) → B(n-1) → C(n-1) → A(n-1)
    /// 
    /// For cycle of length k with each step reducing by 1:
    /// T(n) = T(n-k) + g(n) where g(n) is combined work
    /// 
    /// This sums to: T(n) = Σᵢ g(n - i*k) + T(base) for i from 0 to n/k
    /// Approximately: T(n) = (n/k) * g(n) = Θ(n * g(n) / k) = Θ(n * g(n))
    /// </summary>
    private MutualRecurrenceSolution SolveSubtractionPattern(
        MutualRecurrenceSystem system,
        RecurrenceRelation equivalentRecurrence)
    {
        var cycleLength = system.CycleLength;
        var combinedWork = system.CombinedWork;
        var variable = system.Variable;

        // Classify combined work
        var workClassification = _classifier.Classify(combinedWork, variable);

        ComplexityExpression solution;
        string method;

        switch (workClassification.Form)
        {
            case ExpressionForm.Constant:
                // T(n) = T(n-k) + c → T(n) = Θ(n/k) = Θ(n)
                solution = new LinearComplexity(1.0, variable);
                method = $"Linear summation: T(n) = T(n-{cycleLength}) + O(1) → O(n)";
                break;

            case ExpressionForm.Polynomial:
                // T(n) = T(n-k) + n^d → T(n) = Θ(n^(d+1))
                var degree = workClassification.PrimaryParameter ?? 1;
                solution = PolyLogComplexity.Polynomial(degree + 1, variable);
                method = $"Polynomial summation: T(n) = T(n-{cycleLength}) + O(n^{degree:F1}) → O(n^{degree + 1:F1})";
                break;

            case ExpressionForm.Logarithmic:
                // T(n) = T(n-k) + log(n) → T(n) = Θ(n log n)
                solution = PolyLogComplexity.NLogN(variable);
                method = $"Logarithmic summation: T(n) = T(n-{cycleLength}) + O(log n) → O(n log n)";
                break;

            case ExpressionForm.PolyLog:
                // T(n) = T(n-k) + n^d log^j(n) → T(n) = Θ(n^(d+1) log^j(n))
                var polyDegree = workClassification.PrimaryParameter ?? 1;
                var logExponent = workClassification.LogExponent ?? 1;
                solution = new PolyLogComplexity(polyDegree + 1, logExponent, variable);
                method = $"PolyLog summation: T(n) = T(n-{cycleLength}) + O(n^{polyDegree:F1}·log^{logExponent} n) → O(n^{polyDegree + 1:F1}·log^{logExponent} n)";
                break;

            default:
                // Conservative: multiply by n
                solution = ComplexityComposition.Nested(
                    new VariableComplexity(variable),
                    combinedWork);
                solution = ComplexitySimplifier.Instance.NormalizeForm(solution);
                method = $"Conservative: T(n) = T(n-{cycleLength}) + f(n) → O(n · f(n))";
                break;
        }

        return MutualRecurrenceSolution.Solved(solution, method, equivalentRecurrence);
    }

    /// <summary>
    /// Solves division-based mutual recursion: A(n) → B(n/2) → C(n/2) → A(n/2)
    /// 
    /// For cycle of length k with each step dividing by b:
    /// Combined scale factor: b^k (e.g., if k=3 and b=2, scale = 1/8)
    /// 
    /// Use standard theorem solving on the combined recurrence.
    /// </summary>
    private MutualRecurrenceSolution SolveDivisionPattern(
        MutualRecurrenceSystem system,
        RecurrenceRelation equivalentRecurrence)
    {
        // Use the theorem analyzer on the combined recurrence
        var result = _theoremAnalyzer.Analyze(equivalentRecurrence);

        if (result.IsApplicable && result.Solution != null)
        {
            var method = result switch
            {
                MasterTheoremApplicable mt => $"Master Theorem (Case {(int)mt.Case}) on combined recurrence",
                AkraBazziApplicable ab => $"Akra-Bazzi (p={ab.CriticalExponent:F3}) on combined recurrence",
                LinearRecurrenceSolved lr => $"Linear recurrence: {lr.Method}",
                _ => "Theorem solving on combined recurrence"
            };

            return MutualRecurrenceSolution.Solved(result.Solution, method, equivalentRecurrence);
        }

        // Fall back to heuristic
        return SolveByHeuristic(system, equivalentRecurrence);
    }

    /// <summary>
    /// Handles mixed patterns where methods use different reduction strategies.
    /// </summary>
    private MutualRecurrenceSolution SolveMixedPattern(
        MutualRecurrenceSystem system,
        RecurrenceRelation equivalentRecurrence)
    {
        // Try theorem solving first
        var result = _theoremAnalyzer.Analyze(equivalentRecurrence);

        if (result.IsApplicable && result.Solution != null)
        {
            return MutualRecurrenceSolution.Solved(
                result.Solution,
                "Theorem solving on mixed-pattern combined recurrence",
                equivalentRecurrence);
        }

        return SolveByHeuristic(system, equivalentRecurrence);
    }

    /// <summary>
    /// Heuristic solver when standard theorems don't apply.
    /// </summary>
    private MutualRecurrenceSolution SolveByHeuristic(
        MutualRecurrenceSystem system,
        RecurrenceRelation equivalentRecurrence)
    {
        var cycleLength = system.CycleLength;
        var combinedWork = system.CombinedWork;
        var variable = system.Variable;

        // For most practical mutual recursion, the result is linear
        // A → B → A with constant work → O(n)
        var workClassification = _classifier.Classify(combinedWork, variable);

        if (workClassification.Form == ExpressionForm.Constant)
        {
            return MutualRecurrenceSolution.Solved(
                new LinearComplexity(1.0, variable),
                $"Heuristic: {cycleLength}-way mutual recursion with O(1) work → O(n)",
                equivalentRecurrence);
        }

        // Conservative: O(n * combined_work)
        var solution = ComplexityComposition.Nested(
            new VariableComplexity(variable),
            combinedWork);
        solution = ComplexitySimplifier.Instance.NormalizeForm(solution);

        return MutualRecurrenceSolution.Solved(
            solution,
            $"Heuristic: {cycleLength}-way mutual recursion → O(n · f(n))",
            equivalentRecurrence);
    }
}

/// <summary>
/// Extension methods for mutual recursion solving.
/// </summary>
public static class MutualRecurrenceSolverExtensions
{
    /// <summary>
    /// Solves a mutual recurrence system using the default solver.
    /// </summary>
    public static MutualRecurrenceSolution Solve(this MutualRecurrenceSystem system) =>
        MutualRecurrenceSolver.Instance.Solve(system);
}
