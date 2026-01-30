using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Recurrence;

/// <summary>
/// Represents a recurrence relation for complexity analysis.
/// General form: T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n)
///
/// This is used for recursive algorithms where the complexity depends on
/// the complexity of subproblems.
/// </summary>
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
/// For T(n) = 2·T(n/3) + O(n), the term is (coefficient=2, argument=n/3).
/// </summary>
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
/// Represents a fully specified recurrence relation with explicit terms.
/// Used as input to recurrence solvers.
/// </summary>
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
