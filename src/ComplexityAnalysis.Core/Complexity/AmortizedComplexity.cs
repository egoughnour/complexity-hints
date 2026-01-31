using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents amortized complexity - the average cost per operation over a sequence.
/// 
/// Amortized analysis accounts for expensive operations that happen infrequently,
/// giving a more accurate picture of average-case performance.
/// 
/// Examples:
/// - Dynamic array Add: O(n) worst case, O(1) amortized
/// - Hash table insert: O(n) worst case, O(1) amortized
/// - Splay tree operations: O(n) worst case, O(log n) amortized
/// </summary>
public sealed record AmortizedComplexity : ComplexityExpression
{
    /// <summary>
    /// The amortized (average) complexity per operation.
    /// </summary>
    public required ComplexityExpression AmortizedCost { get; init; }

    /// <summary>
    /// The worst-case complexity for a single operation.
    /// </summary>
    public required ComplexityExpression WorstCaseCost { get; init; }

    /// <summary>
    /// The method used to derive the amortized bound.
    /// </summary>
    public AmortizationMethod Method { get; init; } = AmortizationMethod.Aggregate;

    /// <summary>
    /// Optional potential function used for analysis.
    /// </summary>
    public PotentialFunction? Potential { get; init; }

    /// <summary>
    /// Description of the amortization scenario.
    /// </summary>
    public string? Description { get; init; }

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is IAmortizedComplexityVisitor<T> amortizedVisitor
            ? amortizedVisitor.VisitAmortized(this)
            : AmortizedCost.Accept(visitor); // Fall back to amortized cost

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        this with
        {
            AmortizedCost = AmortizedCost.Substitute(variable, replacement),
            WorstCaseCost = WorstCaseCost.Substitute(variable, replacement)
        };

    public override ImmutableHashSet<Variable> FreeVariables =>
        AmortizedCost.FreeVariables.Union(WorstCaseCost.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        AmortizedCost.Evaluate(assignments);

    public override string ToBigONotation() =>
        $"{AmortizedCost.ToBigONotation()} amortized (worst: {WorstCaseCost.ToBigONotation()})";

    /// <summary>
    /// Creates an amortized constant complexity (like List.Add).
    /// </summary>
    public static AmortizedComplexity ConstantAmortized(Variable var) =>
        new()
        {
            AmortizedCost = ConstantComplexity.One,
            WorstCaseCost = new LinearComplexity(1.0, var),
            Method = AmortizationMethod.Aggregate,
            Description = "Doubling strategy: occasional O(n) resize, O(1) amortized"
        };

    /// <summary>
    /// Creates an amortized logarithmic complexity (like splay tree operations).
    /// </summary>
    public static AmortizedComplexity LogarithmicAmortized(Variable var) =>
        new()
        {
            AmortizedCost = new LogarithmicComplexity(1.0, var),
            WorstCaseCost = new LinearComplexity(1.0, var),
            Method = AmortizationMethod.Potential,
            Description = "Self-adjusting structure: O(n) worst case, O(log n) amortized"
        };

    /// <summary>
    /// Creates an inverse Ackermann amortized complexity (like Union-Find).
    /// </summary>
    public static AmortizedComplexity InverseAckermannAmortized(Variable var) =>
        new()
        {
            AmortizedCost = new InverseAckermannComplexity(var),
            WorstCaseCost = new LogarithmicComplexity(1.0, var),
            Method = AmortizationMethod.Potential,
            Description = "Union-Find with path compression: O(α(n)) amortized"
        };
}

/// <summary>
/// Methods for deriving amortized bounds.
/// </summary>
public enum AmortizationMethod
{
    /// <summary>
    /// Aggregate method: Total cost / number of operations.
    /// Simple but doesn't give per-operation insight.
    /// </summary>
    Aggregate,

    /// <summary>
    /// Accounting method: Assign credits to operations.
    /// Cheap operations pay for expensive ones.
    /// </summary>
    Accounting,

    /// <summary>
    /// Potential method: Define potential function Φ(state).
    /// Amortized cost = actual cost + ΔΦ.
    /// Most powerful, gives tight bounds.
    /// </summary>
    Potential
}

/// <summary>
/// Represents a potential function for amortized analysis.
/// Φ: DataStructureState → ℝ≥0
/// </summary>
public sealed record PotentialFunction
{
    /// <summary>
    /// Name/description of the potential function.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Mathematical description of the potential function.
    /// </summary>
    public required string Formula { get; init; }

    /// <summary>
    /// The variable representing the data structure size.
    /// </summary>
    public Variable SizeVariable { get; init; } = Variable.N;

    /// <summary>
    /// Common potential functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Dynamic array: Φ = 2n - capacity
        /// </summary>
        public static PotentialFunction DynamicArray => new()
        {
            Name = "Dynamic Array",
            Formula = "Φ = 2·count - capacity"
        };

        /// <summary>
        /// Hash table: Φ = 2n - buckets
        /// </summary>
        public static PotentialFunction HashTable => new()
        {
            Name = "Hash Table",
            Formula = "Φ = 2·count - buckets"
        };

        /// <summary>
        /// Binary counter: Φ = number of 1-bits
        /// </summary>
        public static PotentialFunction BinaryCounter => new()
        {
            Name = "Binary Counter",
            Formula = "Φ = |{i : bit[i] = 1}|"
        };

        /// <summary>
        /// Stack with multipop: Φ = stack size
        /// </summary>
        public static PotentialFunction MultipopStack => new()
        {
            Name = "Multipop Stack",
            Formula = "Φ = |stack|"
        };

        /// <summary>
        /// Splay tree: Φ = Σ log(size of subtree)
        /// </summary>
        public static PotentialFunction SplayTree => new()
        {
            Name = "Splay Tree",
            Formula = "Φ = Σᵢ log(sᵢ) where sᵢ = size of subtree at node i"
        };

        /// <summary>
        /// Union-Find: Φ based on ranks
        /// </summary>
        public static PotentialFunction UnionFind => new()
        {
            Name = "Union-Find",
            Formula = "Φ = Σᵢ (α(n) - rank[i]) · size[i]"
        };
    }
}

/// <summary>
/// Inverse Ackermann complexity: O(α(n)) - effectively constant for practical inputs.
/// Used in Union-Find with path compression and union by rank.
/// </summary>
public sealed record InverseAckermannComplexity(Variable Var) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is IAmortizedComplexityVisitor<T> amortizedVisitor
            ? amortizedVisitor.VisitInverseAckermann(this)
            : visitor.Visit(ConstantComplexity.One); // Treat as constant for most purposes

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? this : this; // α(n) structure doesn't change with substitution

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        if (!assignments.TryGetValue(Var, out var n) || n < 1)
            return null;

        // α(n) ≤ 4 for n < 10^80 (more atoms than in universe)
        // For practical purposes, return a small constant
        return InverseAckermann((long)n);
    }

    public override string ToBigONotation() => $"O(α({Var.Name}))";

    /// <summary>
    /// Computes inverse Ackermann function α(n).
    /// α(n) = min { k : A(k, k) ≥ n } where A is Ackermann function.
    /// For all practical n, α(n) ≤ 4.
    /// </summary>
    private static double InverseAckermann(long n)
    {
        if (n <= 0) return 0;
        if (n <= 1) return 0;
        if (n <= 3) return 1;
        if (n <= 7) return 2;
        if (n <= 61) return 3;
        if (n <= 2_147_483_647) return 4; // 2^31 - 1 (A(4,4) ≈ 2^65536)
        return 5; // For any practically representable number
    }
}

/// <summary>
/// Extended visitor interface for amortized complexity types.
/// </summary>
public interface IAmortizedComplexityVisitor<T> : IComplexityVisitor<T>
{
    T VisitAmortized(AmortizedComplexity amortized);
    T VisitInverseAckermann(InverseAckermannComplexity inverseAckermann);
}
