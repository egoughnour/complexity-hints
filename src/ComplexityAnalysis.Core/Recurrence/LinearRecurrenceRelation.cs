using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Recurrence;

/// <summary>
/// Represents a linear recurrence relation: T(n) = Σᵢ aᵢ·T(n-i) + f(n).
/// </summary>
/// <remarks>
/// <para>
/// <b>General Form:</b> T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k) + f(n)
/// </para>
/// <para>
/// where:
/// </para>
/// <list type="bullet">
///   <item><description>aᵢ = coefficient for the (n-i)th term</description></item>
///   <item><description>k = order of the recurrence (number of previous terms)</description></item>
///   <item><description>f(n) = non-homogeneous term (driving function)</description></item>
/// </list>
/// 
/// <para>
/// <b>Solution Method:</b> The characteristic polynomial method:
/// </para>
/// <list type="number">
///   <item><description>
///     Form characteristic polynomial: x^k - a₁x^(k-1) - a₂x^(k-2) - ... - aₖ = 0
///   </description></item>
///   <item><description>
///     Find roots (may be real, repeated, or complex)
///   </description></item>
///   <item><description>
///     Build general solution from roots
///   </description></item>
///   <item><description>
///     Handle non-homogeneous term if present
///   </description></item>
/// </list>
/// 
/// <para>
/// <b>Complexity Implications:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Root Type</term>
///     <description>Solution Form</description>
///   </listheader>
///   <item>
///     <term>Single root r > 1</term>
///     <description>O(rⁿ) - exponential growth</description>
///   </item>
///   <item>
///     <term>Single root r = 1 (with T(n-1))</term>
///     <description>Summation: Σf(i)</description>
///   </item>
///   <item>
///     <term>Repeated root r (multiplicity m)</term>
///     <description>O(n^(m-1) · rⁿ) - polynomial times exponential</description>
///   </item>
///   <item>
///     <term>Complex roots r·e^(iθ)</term>
///     <description>Oscillatory: O(rⁿ) with periodic factor</description>
///   </item>
/// </list>
/// 
/// <para>
/// <b>Common Patterns:</b>
/// </para>
/// <code>
/// // Fibonacci: T(n) = T(n-1) + T(n-2) → O(φⁿ) where φ ≈ 1.618
/// var fib = LinearRecurrenceRelation.Create(new[] { 1.0, 1.0 }, O_1, n);
/// 
/// // Linear summation: T(n) = T(n-1) + O(1) → O(n)
/// var linear = LinearRecurrenceRelation.Create(new[] { 1.0 }, O_1, n);
/// 
/// // Exponential doubling: T(n) = 2T(n-1) + O(1) → O(2ⁿ)
/// var exp2 = LinearRecurrenceRelation.Create(new[] { 2.0 }, O_1, n);
/// </code>
/// </remarks>
/// <seealso cref="RecurrenceRelation"/>
/// <seealso cref="RecurrenceComplexity"/>
public sealed record LinearRecurrenceRelation
{
    /// <summary>
    /// The coefficients [a₁, a₂, ..., aₖ] for T(n-1), T(n-2), ..., T(n-k).
    /// </summary>
    /// <remarks>
    /// Coefficients[0] is the coefficient of T(n-1),
    /// Coefficients[1] is the coefficient of T(n-2), etc.
    /// </remarks>
    public ImmutableArray<double> Coefficients { get; }

    /// <summary>
    /// The non-homogeneous (driving) function f(n) in T(n) = ... + f(n).
    /// </summary>
    /// <remarks>
    /// If the recurrence is homogeneous (no f(n) term), this should be
    /// <see cref="ConstantComplexity.Zero"/>.
    /// </remarks>
    public ComplexityExpression NonRecursiveWork { get; }

    /// <summary>
    /// The variable representing the input size (typically n).
    /// </summary>
    public Variable Variable { get; }

    /// <summary>
    /// The order of the recurrence (k in T(n-k)).
    /// </summary>
    public int Order => Coefficients.Length;

    /// <summary>
    /// Whether this is a homogeneous recurrence (no f(n) term).
    /// </summary>
    public bool IsHomogeneous =>
        NonRecursiveWork is ConstantComplexity c && c.Value == 0;

    /// <summary>
    /// Whether this is a first-order recurrence T(n) = a·T(n-1) + f(n).
    /// </summary>
    public bool IsFirstOrder => Order == 1;

    /// <summary>
    /// Whether this is a simple summation T(n) = T(n-1) + f(n).
    /// </summary>
    public bool IsSummation =>
        IsFirstOrder && Math.Abs(Coefficients[0] - 1.0) < 1e-9;

    private LinearRecurrenceRelation(
        ImmutableArray<double> coefficients,
        ComplexityExpression nonRecursiveWork,
        Variable variable)
    {
        Coefficients = coefficients;
        NonRecursiveWork = nonRecursiveWork;
        Variable = variable;
    }

    /// <summary>
    /// Creates a linear recurrence relation.
    /// </summary>
    /// <param name="coefficients">
    /// Coefficients [a₁, a₂, ..., aₖ] where T(n) = a₁T(n-1) + a₂T(n-2) + ... + f(n).
    /// </param>
    /// <param name="nonRecursiveWork">The non-homogeneous term f(n).</param>
    /// <param name="variable">The recurrence variable (typically n).</param>
    /// <returns>A new linear recurrence relation.</returns>
    /// <exception cref="ArgumentException">If coefficients is empty.</exception>
    public static LinearRecurrenceRelation Create(
        double[] coefficients,
        ComplexityExpression nonRecursiveWork,
        Variable variable)
    {
        if (coefficients.Length == 0)
            throw new ArgumentException("At least one coefficient required", nameof(coefficients));

        return new LinearRecurrenceRelation(
            coefficients.ToImmutableArray(),
            nonRecursiveWork,
            variable);
    }

    /// <summary>
    /// Creates a linear recurrence relation with immutable coefficients.
    /// </summary>
    public static LinearRecurrenceRelation Create(
        ImmutableArray<double> coefficients,
        ComplexityExpression nonRecursiveWork,
        Variable variable)
    {
        if (coefficients.IsDefaultOrEmpty)
            throw new ArgumentException("At least one coefficient required", nameof(coefficients));

        return new LinearRecurrenceRelation(coefficients, nonRecursiveWork, variable);
    }

    /// <summary>
    /// Creates the Fibonacci recurrence: T(n) = T(n-1) + T(n-2) + f(n).
    /// </summary>
    public static LinearRecurrenceRelation Fibonacci(
        ComplexityExpression? nonRecursiveWork = null,
        Variable? variable = null)
    {
        return Create(
            new[] { 1.0, 1.0 },
            nonRecursiveWork ?? ConstantComplexity.One,
            variable ?? Variable.N);
    }

    /// <summary>
    /// Creates a simple summation recurrence: T(n) = T(n-1) + f(n).
    /// </summary>
    public static LinearRecurrenceRelation Summation(
        ComplexityExpression workPerStep,
        Variable? variable = null)
    {
        return Create(
            new[] { 1.0 },
            workPerStep,
            variable ?? Variable.N);
    }

    /// <summary>
    /// Creates an exponential recurrence: T(n) = a·T(n-1) + f(n).
    /// </summary>
    public static LinearRecurrenceRelation Exponential(
        double baseValue,
        ComplexityExpression? nonRecursiveWork = null,
        Variable? variable = null)
    {
        return Create(
            new[] { baseValue },
            nonRecursiveWork ?? ConstantComplexity.One,
            variable ?? Variable.N);
    }

    /// <summary>
    /// Gets a human-readable representation of the recurrence.
    /// </summary>
    public override string ToString()
    {
        var terms = new List<string>();

        for (int i = 0; i < Coefficients.Length; i++)
        {
            var coeff = Coefficients[i];
            if (Math.Abs(coeff) < 1e-9) continue;

            var offset = i + 1;
            var termStr = coeff == 1.0
                ? $"T({Variable.Name}-{offset})"
                : $"{coeff}·T({Variable.Name}-{offset})";
            terms.Add(termStr);
        }

        var recursivePart = string.Join(" + ", terms);
        var nonRecursivePart = NonRecursiveWork.ToBigONotation()
            .Replace("O(", "").TrimEnd(')');

        if (IsHomogeneous)
            return $"T({Variable.Name}) = {recursivePart}";

        return $"T({Variable.Name}) = {recursivePart} + {nonRecursivePart}";
    }
}

/// <summary>
/// Result of solving a linear recurrence relation.
/// </summary>
/// <remarks>
/// Contains both the asymptotic solution and details about how it was derived.
/// </remarks>
public sealed record LinearRecurrenceSolution
{
    /// <summary>
    /// The closed-form asymptotic solution.
    /// </summary>
    public ComplexityExpression Solution { get; init; } = ConstantComplexity.One;

    /// <summary>
    /// Description of the solution method used.
    /// </summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// The roots of the characteristic polynomial.
    /// </summary>
    public ImmutableArray<CharacteristicRoot> Roots { get; init; } =
        ImmutableArray<CharacteristicRoot>.Empty;

    /// <summary>
    /// The dominant root (largest magnitude).
    /// </summary>
    public CharacteristicRoot? DominantRoot { get; init; }

    /// <summary>
    /// Whether the solution involves polynomial factors from repeated roots.
    /// </summary>
    public bool HasPolynomialFactor { get; init; }

    /// <summary>
    /// Detailed explanation of the solution derivation.
    /// </summary>
    public string Explanation { get; init; } = string.Empty;
}

/// <summary>
/// A root of the characteristic polynomial with its properties.
/// </summary>
/// <remarks>
/// Roots can be real or complex. Complex roots always come in conjugate pairs
/// for recurrences with real coefficients.
/// </remarks>
public sealed record CharacteristicRoot
{
    /// <summary>
    /// The real part of the root.
    /// </summary>
    public double RealPart { get; init; }

    /// <summary>
    /// The imaginary part of the root (0 for real roots).
    /// </summary>
    public double ImaginaryPart { get; init; }

    /// <summary>
    /// The magnitude |r| = √(a² + b²) for complex root a + bi.
    /// </summary>
    public double Magnitude { get; init; }

    /// <summary>
    /// The multiplicity (how many times this root appears).
    /// </summary>
    public int Multiplicity { get; init; } = 1;

    /// <summary>
    /// Whether this is a real root (imaginary part ≈ 0).
    /// </summary>
    public bool IsReal => Math.Abs(ImaginaryPart) < 1e-9;

    /// <summary>
    /// Whether this is a repeated root (multiplicity > 1).
    /// </summary>
    public bool IsRepeated => Multiplicity > 1;

    /// <summary>
    /// Creates a real root.
    /// </summary>
    public static CharacteristicRoot Real(double value, int multiplicity = 1) =>
        new()
        {
            RealPart = value,
            ImaginaryPart = 0,
            Magnitude = Math.Abs(value),
            Multiplicity = multiplicity
        };

    /// <summary>
    /// Creates a complex root.
    /// </summary>
    public static CharacteristicRoot Complex(double real, double imaginary, int multiplicity = 1) =>
        new()
        {
            RealPart = real,
            ImaginaryPart = imaginary,
            Magnitude = Math.Sqrt(real * real + imaginary * imaginary),
            Multiplicity = multiplicity
        };

    public override string ToString() =>
        IsReal
            ? (Multiplicity > 1 ? $"{RealPart:F4} (×{Multiplicity})" : $"{RealPart:F4}")
            : $"{RealPart:F4} ± {Math.Abs(ImaginaryPart):F4}i";
}
