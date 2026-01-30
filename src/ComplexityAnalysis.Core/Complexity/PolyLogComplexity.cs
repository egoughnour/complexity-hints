using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents polylogarithmic complexity: O(n^k · log^j n).
///
/// This unified type is essential for:
/// - Master Theorem Case 2 solutions: Θ(n^d · log^(k+1) n)
/// - Common algorithms like merge sort: O(n log n)
/// - Iterated logarithms: O(n log log n)
///
/// The general form is: coefficient · n^polyDegree · (log_base n)^logExponent
/// </summary>
public sealed record PolyLogComplexity(
    /// <summary>
    /// The polynomial degree k in n^k.
    /// k = 0 gives pure logarithmic: O(log^j n)
    /// k = 1, j = 1 gives: O(n log n)
    /// </summary>
    double PolyDegree,

    /// <summary>
    /// The logarithmic exponent j in log^j n.
    /// j = 0 gives pure polynomial: O(n^k)
    /// j = 1 gives single log: O(n^k log n)
    /// j = 2 gives: O(n^k log² n)
    /// </summary>
    double LogExponent,

    /// <summary>
    /// The variable (typically n).
    /// </summary>
    Variable Var,

    /// <summary>
    /// Leading coefficient (absorbed in Big-O but preserved for precision).
    /// </summary>
    double Coefficient = 1.0,

    /// <summary>
    /// Base of the logarithm (default 2, but bases differ only by constant factor).
    /// </summary>
    double LogBase = 2.0) : ComplexityExpression
{
    /// <summary>
    /// True if this is a pure polynomial (no log factor).
    /// </summary>
    public bool IsPurePolynomial => LogExponent == 0;

    /// <summary>
    /// True if this is a pure logarithmic (no polynomial factor).
    /// </summary>
    public bool IsPureLogarithmic => PolyDegree == 0 && LogExponent > 0;

    /// <summary>
    /// True if this is the common n log n form.
    /// </summary>
    public bool IsNLogN => PolyDegree == 1 && LogExponent == 1;

    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement)
    {
        if (!variable.Equals(Var)) return this;

        // Substitution: (replacement)^k · log^j(replacement)
        ComplexityExpression result = new ConstantComplexity(Coefficient);

        if (PolyDegree != 0)
        {
            result = new BinaryOperationComplexity(
                result,
                BinaryOp.Multiply,
                new PowerComplexity(replacement, PolyDegree));
        }

        if (LogExponent != 0)
        {
            var logTerm = LogExponent == 1
                ? (ComplexityExpression)new LogOfComplexity(replacement, LogBase)
                : new PowerComplexity(new LogOfComplexity(replacement, LogBase), LogExponent);

            result = new BinaryOperationComplexity(result, BinaryOp.Multiply, logTerm);
        }

        return result;
    }

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        if (!assignments.TryGetValue(Var, out var n) || n <= 0)
            return null;

        var polyPart = PolyDegree == 0 ? 1.0 : Math.Pow(n, PolyDegree);
        var logPart = LogExponent == 0 ? 1.0 : Math.Pow(Math.Log(n, LogBase), LogExponent);

        return Coefficient * polyPart * logPart;
    }

    public override string ToBigONotation()
    {
        // Handle special cases for cleaner output
        if (PolyDegree == 0 && LogExponent == 0)
            return "O(1)";

        var parts = new List<string>();

        // Polynomial part
        if (PolyDegree != 0)
        {
            parts.Add(PolyDegree switch
            {
                1 => Var.Name,
                2 => $"{Var.Name}²",
                3 => $"{Var.Name}³",
                _ => $"{Var.Name}^{PolyDegree}"
            });
        }

        // Logarithmic part
        if (LogExponent != 0)
        {
            var logStr = LogBase == 2 ? "log" : $"log_{LogBase}";
            parts.Add(LogExponent switch
            {
                1 => $"{logStr} {Var.Name}",
                2 => $"{logStr}² {Var.Name}",
                _ => $"{logStr}^{LogExponent} {Var.Name}"
            });
        }

        var inner = string.Join(" · ", parts);
        return Coefficient == 1 ? $"O({inner})" : $"O({Coefficient}·{inner})";
    }

    #region Factory Methods

    /// <summary>
    /// Creates O(n log n) - common for efficient sorting/divide-and-conquer.
    /// </summary>
    public static PolyLogComplexity NLogN(Variable? var = null) =>
        new(1, 1, var ?? Variable.N);

    /// <summary>
    /// Creates O(n^k log n) - Master Theorem Case 2 with k=1.
    /// </summary>
    public static PolyLogComplexity PolyTimesLog(double polyDegree, Variable? var = null) =>
        new(polyDegree, 1, var ?? Variable.N);

    /// <summary>
    /// Creates O(n^d · log^(k+1) n) - General Master Theorem Case 2 solution.
    /// </summary>
    public static PolyLogComplexity MasterCase2Solution(double d, double k, Variable? var = null) =>
        new(d, k + 1, var ?? Variable.N);

    /// <summary>
    /// Creates O(log^k n) - pure iterated logarithm.
    /// </summary>
    public static PolyLogComplexity LogPower(double logExponent, Variable? var = null) =>
        new(0, logExponent, var ?? Variable.N);

    /// <summary>
    /// Creates O(n^k) - pure polynomial (for consistency).
    /// </summary>
    public static PolyLogComplexity Polynomial(double degree, Variable? var = null) =>
        new(degree, 0, var ?? Variable.N);

    #endregion

    #region Arithmetic Operations

    /// <summary>
    /// Multiplies two PolyLog expressions: (n^a log^b n) × (n^c log^d n) = n^(a+c) log^(b+d) n
    /// </summary>
    public PolyLogComplexity Multiply(PolyLogComplexity other)
    {
        if (!Var.Equals(other.Var))
            throw new ArgumentException("Cannot multiply PolyLog expressions with different variables");

        return new PolyLogComplexity(
            PolyDegree + other.PolyDegree,
            LogExponent + other.LogExponent,
            Var,
            Coefficient * other.Coefficient,
            LogBase);
    }

    /// <summary>
    /// Raises to a power: (n^a log^b n)^k = n^(ak) log^(bk) n
    /// </summary>
    public PolyLogComplexity Power(double exponent) =>
        new(PolyDegree * exponent, LogExponent * exponent, Var,
            Math.Pow(Coefficient, exponent), LogBase);

    #endregion
}
