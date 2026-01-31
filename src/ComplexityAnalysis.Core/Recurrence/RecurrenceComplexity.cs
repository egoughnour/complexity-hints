using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Recurrence;

/// <summary>
/// Represents a recurrence relation for complexity analysis of recursive algorithms.
/// </summary>
/// <remarks>
/// <para>
/// <b>General Form:</b> T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n)
/// </para>
/// <para>
/// where:
/// </para>
/// <list type="bullet">
///   <item><description>aᵢ = number of recursive calls of type i</description></item>
///   <item><description>bᵢ = scale factor for subproblem size (0 &lt; bᵢ &lt; 1)</description></item>
///   <item><description>hᵢ(n) = perturbation function (often 0)</description></item>
///   <item><description>g(n) = non-recursive work at each level</description></item>
/// </list>
/// 
/// <para>
/// <b>Analysis Theorems:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Theorem</term>
///     <description>Applicability</description>
///   </listheader>
///   <item>
///     <term>Master Theorem</term>
///     <description>Single-term: T(n) = a·T(n/b) + f(n), where a ≥ 1, b > 1</description>
///   </item>
///   <item>
///     <term>Akra-Bazzi</term>
///     <description>Multi-term: T(n) = Σᵢ aᵢ·T(bᵢn) + g(n), where aᵢ > 0, 0 &lt; bᵢ &lt; 1</description>
///   </item>
///   <item>
///     <term>Linear Recurrence</term>
///     <description>T(n) = T(n-1) + f(n), solved by summation</description>
///   </item>
/// </list>
/// 
/// <para>
/// <b>Common Patterns:</b>
/// </para>
/// <code>
/// // Merge Sort: T(n) = 2T(n/2) + O(n) → O(n log n)
/// var mergeSort = RecurrenceComplexity.DivideAndConquer(2, 2, O_n, n);
/// 
/// // Binary Search: T(n) = T(n/2) + O(1) → O(log n)
/// var binarySearch = RecurrenceComplexity.DivideAndConquer(1, 2, O_1, n);
/// 
/// // Strassen: T(n) = 7T(n/2) + O(n²) → O(n^2.807)
/// var strassen = RecurrenceComplexity.DivideAndConquer(7, 2, O_n2, n);
/// </code>
/// 
/// <para>
/// See the TheoremApplicabilityAnalyzer in ComplexityAnalysis.Solver for the analysis engine
/// that solves these recurrences.
/// </para>
/// </remarks>
/// <seealso cref="RecurrenceRelation"/>
/// <seealso cref="RecurrenceTerm"/>
public sealed record RecurrenceComplexity(
    /// <summary>
    /// The recursive terms: each term is aᵢ·T(subproblem_size).
    /// </summary>
    ImmutableList<RecurrenceTerm> Terms,

    /// <summary>
    /// The variable representing the input size in the recurrence.
    /// </summary>
    Variable RecurrenceVariable,

    /// <summary>
    /// The non-recursive work done at each level: g(n) or f(n).
    /// </summary>
    ComplexityExpression NonRecursiveWork,

    /// <summary>
    /// The complexity at the base case (when recursion terminates).
    /// </summary>
    ComplexityExpression BaseCaseComplexity) : ComplexityExpression
{
    /// <summary>
    /// Gets the total number of recursive calls (sum of coefficients).
    /// For T(n) = 2T(n/2) + O(n), this returns 2.
    /// </summary>
    public double TotalRecursiveCalls =>
        Terms.Sum(t => t.Coefficient);

    /// <summary>
    /// Determines if this recurrence fits the Master Theorem pattern:
    /// T(n) = a·T(n/b) + f(n) where a ≥ 1, b > 1.
    /// </summary>
    public bool FitsMasterTheorem =>
        Terms.Count == 1 &&
        Terms[0].Coefficient >= 1 &&
        Terms[0].ScaleFactor > 0 &&
        Terms[0].ScaleFactor < 1;

    /// <summary>
    /// Determines if this recurrence fits the Akra-Bazzi pattern
    /// (more general than Master Theorem).
    /// </summary>
    public bool FitsAkraBazzi =>
        Terms.All(t => t.Coefficient > 0 && t.ScaleFactor > 0 && t.ScaleFactor < 1);

    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement)
    {
        if (!variable.Equals(RecurrenceVariable))
        {
            // Substitute in the non-recursive work
            return this with
            {
                NonRecursiveWork = NonRecursiveWork.Substitute(variable, replacement),
                BaseCaseComplexity = BaseCaseComplexity.Substitute(variable, replacement)
            };
        }

        // Substituting the recurrence variable itself would change the recurrence structure
        // This is a more complex transformation
        return this;
    }

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(RecurrenceVariable)
            .Union(NonRecursiveWork.FreeVariables)
            .Union(BaseCaseComplexity.FreeVariables)
            .Union(Terms.SelectMany(t => t.Argument.FreeVariables));

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        // Recurrence evaluation requires solving the recurrence
        // This is a placeholder - actual evaluation happens in the solver
        if (!assignments.TryGetValue(RecurrenceVariable, out var n))
            return null;

        if (n <= 1)
            return BaseCaseComplexity.Evaluate(assignments);

        // Simple numerical unrolling for small n
        if (n <= 100)
        {
            return EvaluateByUnrolling(n, assignments);
        }

        return null; // Requires analytical solver for large n
    }

    private double? EvaluateByUnrolling(double n, IReadOnlyDictionary<Variable, double> assignments)
    {
        if (n <= 1)
            return BaseCaseComplexity.Evaluate(assignments);

        var modifiedAssignments = new Dictionary<Variable, double>(assignments)
        {
            [RecurrenceVariable] = n
        };

        var nonRecursiveVal = NonRecursiveWork.Evaluate(modifiedAssignments);
        if (!nonRecursiveVal.HasValue) return null;

        double recursiveSum = 0;
        foreach (var term in Terms)
        {
            var subproblemSize = term.ScaleFactor * n;
            if (subproblemSize >= n) return null; // Would cause infinite recursion

            var subResult = EvaluateByUnrolling(subproblemSize, assignments);
            if (!subResult.HasValue) return null;

            recursiveSum += term.Coefficient * subResult.Value;
        }

        return recursiveSum + nonRecursiveVal.Value;
    }

    public override string ToBigONotation()
    {
        var termsStr = string.Join(" + ", Terms.Select(t =>
        {
            var argStr = t.Argument.ToBigONotation().Replace("O(", "").TrimEnd(')');
            return t.Coefficient == 1 ? $"T({argStr})" : $"{t.Coefficient}·T({argStr})";
        }));

        var workStr = NonRecursiveWork.ToBigONotation().Replace("O(", "").TrimEnd(')');
        return $"T({RecurrenceVariable.Name}) = {termsStr} + {workStr}";
    }

    /// <summary>
    /// Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + O(n^d).
    /// </summary>
    public static RecurrenceComplexity DivideAndConquer(
        double subproblems,
        double divisionFactor,
        ComplexityExpression mergeWork,
        Variable variable)
    {
        return new RecurrenceComplexity(
            ImmutableList.Create(
                new RecurrenceTerm(
                    subproblems,
                    new BinaryOperationComplexity(
                        new VariableComplexity(variable),
                        BinaryOp.Multiply,
                        new ConstantComplexity(1.0 / divisionFactor)),
                    1.0 / divisionFactor)),
            variable,
            mergeWork,
            ConstantComplexity.One);
    }

    /// <summary>
    /// Creates a linear recursion: T(n) = T(n-1) + O(f(n)).
    /// </summary>
    public static RecurrenceComplexity Linear(
        ComplexityExpression workPerStep,
        Variable variable)
    {
        return new RecurrenceComplexity(
            ImmutableList.Create(
                new RecurrenceTerm(
                    1,
                    new BinaryOperationComplexity(
                        new VariableComplexity(variable),
                        BinaryOp.Plus,
                        new ConstantComplexity(-1)),
                    1.0)), // Scale factor of 1 indicates linear reduction
            variable,
            workPerStep,
            ConstantComplexity.One);
    }
}

/// <summary>
/// Represents a single term in a recurrence relation.
/// </summary>
/// <remarks>
/// <para>
/// For a recurrence like T(n) = 2·T(n/3) + O(n), the term is:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Coefficient"/> = 2 (number of recursive calls)</description></item>
///   <item><description><see cref="Argument"/> = n/3 (subproblem size expression)</description></item>
///   <item><description><see cref="ScaleFactor"/> = 1/3 (reduction ratio)</description></item>
/// </list>
/// <para>
/// <b>Well-formedness:</b> For theorem applicability, terms must satisfy:
/// </para>
/// <list type="bullet">
///   <item><description>Coefficient > 0 (at least one recursive call)</description></item>
///   <item><description>0 &lt; ScaleFactor &lt; 1 (subproblem is smaller)</description></item>
/// </list>
/// </remarks>
/// <param name="Coefficient">The multiplier for this recursive call (a in a·T(f(n))).</param>
/// <param name="Argument">The argument to the recursive call (f(n) in T(f(n))).</param>
/// <param name="ScaleFactor">The scale factor for the subproblem size (1/b in T(n/b)).</param>
public sealed record RecurrenceTerm(
    /// <summary>
    /// The multiplier for this recursive call (a in a·T(f(n))).
    /// </summary>
    double Coefficient,

    /// <summary>
    /// The argument to the recursive call (f(n) in T(f(n))).
    /// </summary>
    ComplexityExpression Argument,

    /// <summary>
    /// The scale factor for the subproblem size (b in T(n/b)).
    /// Used for Master/Akra-Bazzi theorem applicability.
    /// </summary>
    double ScaleFactor = 0.5)
{
    /// <summary>
    /// Determines if this term represents a proper reduction (subproblem smaller than original).
    /// </summary>
    public bool IsReducing => ScaleFactor > 0 && ScaleFactor < 1;

    public override string ToString() =>
        Coefficient == 1
            ? $"T({Argument.ToBigONotation().Replace("O(", "").TrimEnd(')')})"
            : $"{Coefficient}·T({Argument.ToBigONotation().Replace("O(", "").TrimEnd(')')})";
}

/// <summary>
/// A term in a recurrence relation with coefficient and scale factor.
/// </summary>
public sealed record RecurrenceRelationTerm(double Coefficient, double ScaleFactor);

/// <summary>
/// Represents a fully specified recurrence relation with explicit terms for analysis.
/// </summary>
/// <remarks>
/// <para>
/// This is the normalized form used as input to recurrence solvers. It extracts the
/// essential mathematical components from <see cref="RecurrenceComplexity"/>:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Terms"/>: The recursive structure [(aᵢ, bᵢ)]</description></item>
///   <item><description><see cref="NonRecursiveWork"/>: The g(n) function</description></item>
///   <item><description><see cref="BaseCase"/>: The T(1) boundary condition</description></item>
/// </list>
/// 
/// <para>
/// <b>Theorem Selection:</b>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <see cref="FitsMasterTheorem"/>: Single term with a ≥ 1, b > 1
///   </description></item>
///   <item><description>
///     <see cref="FitsAkraBazzi"/>: All terms have aᵢ > 0 and 0 &lt; bᵢ &lt; 1
///   </description></item>
/// </list>
/// 
/// <para>
/// <b>Convenience Factories:</b>
/// </para>
/// <code>
/// // Standard divide-and-conquer
/// var rec = RecurrenceRelation.DivideAndConquer(2, 2, O_n, Variable.N);
/// 
/// // From existing RecurrenceComplexity
/// var rel = RecurrenceRelation.FromComplexity(recurrence);
/// </code>
/// </remarks>
/// <seealso cref="RecurrenceComplexity"/>
public sealed record RecurrenceRelation
{
    private readonly ImmutableList<RecurrenceRelationTerm> _terms;

    /// <summary>
    /// The recursive terms: [(aᵢ, bᵢ)] where T(n) contains aᵢ·T(bᵢ·n).
    /// </summary>
    public ImmutableList<RecurrenceRelationTerm> Terms => _terms;

    /// <summary>
    /// The non-recursive work function g(n).
    /// </summary>
    public ComplexityExpression NonRecursiveWork { get; }

    /// <summary>
    /// The base case complexity T(1).
    /// </summary>
    public ComplexityExpression BaseCase { get; }

    /// <summary>
    /// The recurrence variable (typically n).
    /// </summary>
    public Variable Variable { get; }

    /// <summary>
    /// Creates a recurrence relation from explicit terms.
    /// </summary>
    public RecurrenceRelation(
        IEnumerable<RecurrenceRelationTerm> terms,
        Variable variable,
        ComplexityExpression nonRecursiveWork,
        ComplexityExpression? baseCase = null)
    {
        _terms = terms.ToImmutableList();
        Variable = variable;
        NonRecursiveWork = nonRecursiveWork;
        BaseCase = baseCase ?? ConstantComplexity.One;
    }

    /// <summary>
    /// Checks if this recurrence fits the Master Theorem form.
    /// </summary>
    public bool FitsMasterTheorem =>
        Terms.Count == 1 &&
        Terms[0].Coefficient >= 1 &&
        Terms[0].ScaleFactor > 0 &&
        Terms[0].ScaleFactor < 1;

    /// <summary>
    /// Checks if this recurrence fits the Akra-Bazzi pattern.
    /// </summary>
    public bool FitsAkraBazzi =>
        Terms.All(t => t.Coefficient > 0 && t.ScaleFactor > 0 && t.ScaleFactor < 1);

    /// <summary>
    /// For Master Theorem: a in T(n) = a·T(n/b) + f(n).
    /// </summary>
    public double A => Terms.FirstOrDefault()?.Coefficient ?? 0;

    /// <summary>
    /// For Master Theorem: b in T(n) = a·T(n/b) + f(n).
    /// </summary>
    public double B => Terms.Count > 0 ? 1.0 / Terms[0].ScaleFactor : 1;

    /// <summary>
    /// Creates a RecurrenceRelation from a RecurrenceComplexity.
    /// </summary>
    public static RecurrenceRelation FromComplexity(RecurrenceComplexity complexity) =>
        new(
            complexity.Terms.Select(t => new RecurrenceRelationTerm(t.Coefficient, t.ScaleFactor)),
            complexity.RecurrenceVariable,
            complexity.NonRecursiveWork,
            complexity.BaseCaseComplexity);

    /// <summary>
    /// Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + f(n).
    /// </summary>
    public static RecurrenceRelation DivideAndConquer(
        double subproblems,
        double divisionFactor,
        ComplexityExpression mergeWork,
        Variable variable)
    {
        return new RecurrenceRelation(
            new[] { new RecurrenceRelationTerm(subproblems, 1.0 / divisionFactor) },
            variable,
            mergeWork);
    }
}
