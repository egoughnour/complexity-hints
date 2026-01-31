using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Recurrence;

/// <summary>
/// Represents a system of mutually recursive recurrence relations.
/// 
/// For mutually recursive functions A(n) and B(n):
/// - A(n) = T_A(n-1) + f_A(n) where A calls B
/// - B(n) = T_B(n-1) + f_B(n) where B calls A
/// 
/// This can be combined into a single recurrence by substitution.
/// </summary>
public sealed record MutualRecurrenceSystem
{
    /// <summary>
    /// The methods involved in the mutual recursion cycle.
    /// The order represents the cycle: methods[0] → methods[1] → ... → methods[0]
    /// </summary>
    public required ImmutableList<MutualRecurrenceComponent> Components { get; init; }

    /// <summary>
    /// The recurrence variable (typically n).
    /// </summary>
    public required Variable Variable { get; init; }

    /// <summary>
    /// Number of methods in the cycle.
    /// </summary>
    public int CycleLength => Components.Count;

    /// <summary>
    /// The combined reduction per full cycle through all methods.
    /// For A → B → A with each doing -1, this is -2 (or scale 0.99^2 for divide pattern).
    /// </summary>
    public double CombinedReduction => Components.Sum(c => c.Reduction);

    /// <summary>
    /// The combined non-recursive work done in one full cycle.
    /// </summary>
    public ComplexityExpression CombinedWork
    {
        get
        {
            ComplexityExpression total = ConstantComplexity.Zero;
            foreach (var component in Components)
            {
                total = ComplexityComposition.Sequential(total, component.NonRecursiveWork);
            }
            return ComplexitySimplifier.Instance.Simplify(total);
        }
    }

    /// <summary>
    /// Converts the mutual recursion system to an equivalent single recurrence.
    /// 
    /// For a cycle A → B → C → A where each reduces by 1:
    /// Combined: T(n) = T(n - cycleLength) + CombinedWork
    /// </summary>
    public RecurrenceRelation ToSingleRecurrence()
    {
        var combinedWork = CombinedWork;

        // For subtraction-based recursion (T(n-1) patterns)
        if (IsSubtractionPattern)
        {
            // T(n) = T(n - cycleLength) + CombinedWork
            // This is still a linear recurrence, just with larger step
            var scaleFactor = 1.0 - (CycleLength * 0.01); // Approximation for n - cycleLength
            
            return new RecurrenceRelation(
                new[] { new RecurrenceRelationTerm(1.0, scaleFactor) },
                Variable,
                combinedWork);
        }

        // For division-based recursion (T(n/2) patterns)
        var combinedScaleFactor = Components
            .Select(c => c.ScaleFactor)
            .Aggregate(1.0, (acc, sf) => acc * sf);

        return new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(1.0, combinedScaleFactor) },
            Variable,
            combinedWork);
    }

    /// <summary>
    /// Whether this is a subtraction-based mutual recursion (each step reduces by constant).
    /// </summary>
    public bool IsSubtractionPattern => Components.All(c => c.ScaleFactor > 0.95);

    /// <summary>
    /// Whether this is a division-based mutual recursion (each step divides by constant).
    /// </summary>
    public bool IsDivisionPattern => Components.All(c => c.ScaleFactor > 0 && c.ScaleFactor < 0.95);

    /// <summary>
    /// Gets a human-readable description of the mutual recursion.
    /// </summary>
    public string GetDescription()
    {
        var cycle = string.Join(" → ", Components.Select(c => c.MethodName));
        return $"{cycle} → {Components[0].MethodName}";
    }
}

/// <summary>
/// Represents one method in a mutual recursion cycle.
/// </summary>
public sealed record MutualRecurrenceComponent
{
    /// <summary>
    /// The method name (for diagnostics).
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// The non-recursive work done by this method.
    /// </summary>
    public required ComplexityExpression NonRecursiveWork { get; init; }

    /// <summary>
    /// How much the problem size is reduced when calling the next method.
    /// For subtraction: reduction amount (e.g., 1 for n-1).
    /// </summary>
    public double Reduction { get; init; } = 1.0;

    /// <summary>
    /// Scale factor for divide-style patterns (1/b in T(n/b)).
    /// For subtraction patterns, this is close to 1 (e.g., 0.99).
    /// </summary>
    public double ScaleFactor { get; init; } = 0.99;

    /// <summary>
    /// The methods this component calls (within the cycle).
    /// </summary>
    public ImmutableList<string> Callees { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Result of solving a mutual recursion system.
/// </summary>
public sealed record MutualRecurrenceSolution
{
    /// <summary>
    /// Whether the system was successfully solved.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The complexity solution for the first method in the cycle.
    /// Since all methods in the cycle have the same asymptotic complexity
    /// (differing only by constants), this applies to all.
    /// </summary>
    public ComplexityExpression? Solution { get; init; }

    /// <summary>
    /// Individual solutions for each method (may differ by constant factors).
    /// </summary>
    public ImmutableDictionary<string, ComplexityExpression>? MethodSolutions { get; init; }

    /// <summary>
    /// The approach used to solve the recurrence.
    /// </summary>
    public string? Method { get; init; }

    /// <summary>
    /// Diagnostic information if solving failed.
    /// </summary>
    public string? FailureReason { get; init; }

    /// <summary>
    /// The equivalent single recurrence that was solved.
    /// </summary>
    public RecurrenceRelation? EquivalentRecurrence { get; init; }

    /// <summary>
    /// Creates a successful solution.
    /// </summary>
    public static MutualRecurrenceSolution Solved(
        ComplexityExpression solution,
        string method,
        RecurrenceRelation? equivalentRecurrence = null)
    {
        return new MutualRecurrenceSolution
        {
            Success = true,
            Solution = solution,
            Method = method,
            EquivalentRecurrence = equivalentRecurrence
        };
    }

    /// <summary>
    /// Creates a failed solution.
    /// </summary>
    public static MutualRecurrenceSolution Failed(string reason)
    {
        return new MutualRecurrenceSolution
        {
            Success = false,
            FailureReason = reason
        };
    }
}
