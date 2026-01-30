using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Interface for transforming and simplifying complexity expressions.
/// </summary>
public interface IComplexityTransformer
{
    /// <summary>
    /// Simplify an expression by applying algebraic rules.
    /// </summary>
    ComplexityExpression Simplify(ComplexityExpression expression);

    /// <summary>
    /// Normalize to a canonical form for comparison.
    /// </summary>
    ComplexityExpression NormalizeForm(ComplexityExpression expression);

    /// <summary>
    /// Drop constant factors for Big-O equivalence.
    /// O(3n²) → O(n²)
    /// </summary>
    ComplexityExpression DropConstantFactors(ComplexityExpression expression);

    /// <summary>
    /// Drop lower-order terms for asymptotic equivalence.
    /// O(n² + n + 1) → O(n²)
    /// </summary>
    ComplexityExpression DropLowerOrderTerms(ComplexityExpression expression);
}

/// <summary>
/// Compares complexity expressions for asymptotic ordering.
/// </summary>
public interface IComplexityComparator
{
    /// <summary>
    /// Compare two expressions asymptotically.
    /// Returns: -1 if left &lt; right, 0 if equal, 1 if left > right.
    /// </summary>
    int Compare(ComplexityExpression left, ComplexityExpression right);

    /// <summary>
    /// Determines if left is dominated by right (left ∈ O(right)).
    /// </summary>
    bool IsDominated(ComplexityExpression left, ComplexityExpression right);

    /// <summary>
    /// Determines if two expressions are asymptotically equivalent.
    /// </summary>
    bool AreEquivalent(ComplexityExpression left, ComplexityExpression right);
}

/// <summary>
/// Standard implementation of complexity simplification.
/// </summary>
public sealed class ComplexitySimplifier : IComplexityTransformer
{
    public static readonly ComplexitySimplifier Instance = new();

    /// <inheritdoc />
    public ComplexityExpression Simplify(ComplexityExpression expression)
    {
        // Apply simplification rules iteratively until fixed point
        var current = expression;
        ComplexityExpression previous;

        do
        {
            previous = current;
            current = SimplifyOnce(current);
        } while (!current.Equals(previous));

        return current;
    }

    private ComplexityExpression SimplifyOnce(ComplexityExpression expr)
    {
        return expr switch
        {
            BinaryOperationComplexity bin => SimplifyBinaryOp(bin),
            PowerComplexity pow => SimplifyPower(pow),
            _ => expr
        };
    }

    private ComplexityExpression SimplifyBinaryOp(BinaryOperationComplexity bin)
    {
        var left = SimplifyOnce(bin.Left);
        var right = SimplifyOnce(bin.Right);

        return bin.Operation switch
        {
            BinaryOp.Plus => SimplifyPlus(left, right),
            BinaryOp.Multiply => SimplifyMultiply(left, right),
            BinaryOp.Max => SimplifyMax(left, right),
            BinaryOp.Min => SimplifyMin(left, right),
            _ => new BinaryOperationComplexity(left, bin.Operation, right)
        };
    }

    private ComplexityExpression SimplifyPlus(ComplexityExpression left, ComplexityExpression right)
    {
        // 0 + x = x
        if (left is ConstantComplexity { Value: 0 }) return right;
        if (right is ConstantComplexity { Value: 0 }) return left;

        // c1 + c2 = c3
        if (left is ConstantComplexity c1 && right is ConstantComplexity c2)
            return new ConstantComplexity(c1.Value + c2.Value);

        // kn + mn = (k+m)n
        if (left is LinearComplexity lin1 && right is LinearComplexity lin2 && lin1.Var.Equals(lin2.Var))
            return new LinearComplexity(lin1.Coefficient + lin2.Coefficient, lin1.Var);

        return new BinaryOperationComplexity(left, BinaryOp.Plus, right);
    }

    private ComplexityExpression SimplifyMultiply(ComplexityExpression left, ComplexityExpression right)
    {
        // 1 × x = x
        if (left is ConstantComplexity { Value: 1 }) return right;
        if (right is ConstantComplexity { Value: 1 }) return left;

        // 0 × x = 0
        if (left is ConstantComplexity { Value: 0 } || right is ConstantComplexity { Value: 0 })
            return ConstantComplexity.Zero;

        // c1 × c2 = c3
        if (left is ConstantComplexity c1 && right is ConstantComplexity c2)
            return new ConstantComplexity(c1.Value * c2.Value);

        // c × kn = (c×k)n
        if (left is ConstantComplexity c && right is LinearComplexity lin)
            return new LinearComplexity(c.Value * lin.Coefficient, lin.Var);
        if (right is ConstantComplexity c3 && left is LinearComplexity lin2)
            return new LinearComplexity(c3.Value * lin2.Coefficient, lin2.Var);

        // n × n = n²
        if (left is VariableComplexity v1 && right is VariableComplexity v2 && v1.Var.Equals(v2.Var))
            return PolynomialComplexity.OfDegree(2, v1.Var);

        // n × n^k = n^(k+1)
        if (left is VariableComplexity v && right is PolynomialComplexity poly && v.Var.Equals(poly.Var))
            return PolynomialComplexity.OfDegree(poly.Degree + 1, v.Var);
        if (right is VariableComplexity v3 && left is PolynomialComplexity poly2 && v3.Var.Equals(poly2.Var))
            return PolynomialComplexity.OfDegree(poly2.Degree + 1, v3.Var);

        return new BinaryOperationComplexity(left, BinaryOp.Multiply, right);
    }

    private ComplexityExpression SimplifyMax(ComplexityExpression left, ComplexityExpression right)
    {
        // max(x, x) = x
        if (left.Equals(right)) return left;

        // max(c1, c2) = max value
        if (left is ConstantComplexity c1 && right is ConstantComplexity c2)
            return new ConstantComplexity(Math.Max(c1.Value, c2.Value));

        // max(0, x) = x for positive complexities
        if (left is ConstantComplexity { Value: 0 }) return right;
        if (right is ConstantComplexity { Value: 0 }) return left;

        return new BinaryOperationComplexity(left, BinaryOp.Max, right);
    }

    private ComplexityExpression SimplifyMin(ComplexityExpression left, ComplexityExpression right)
    {
        // min(x, x) = x
        if (left.Equals(right)) return left;

        // min(c1, c2) = min value
        if (left is ConstantComplexity c1 && right is ConstantComplexity c2)
            return new ConstantComplexity(Math.Min(c1.Value, c2.Value));

        return new BinaryOperationComplexity(left, BinaryOp.Min, right);
    }

    private ComplexityExpression SimplifyPower(PowerComplexity pow)
    {
        var baseExpr = SimplifyOnce(pow.Base);

        // x^0 = 1
        if (pow.Exponent == 0) return ConstantComplexity.One;

        // x^1 = x
        if (pow.Exponent == 1) return baseExpr;

        // c^k = c'
        if (baseExpr is ConstantComplexity c)
            return new ConstantComplexity(Math.Pow(c.Value, pow.Exponent));

        return new PowerComplexity(baseExpr, pow.Exponent);
    }

    /// <inheritdoc />
    public ComplexityExpression NormalizeForm(ComplexityExpression expression)
    {
        // First simplify
        var simplified = Simplify(expression);

        // Then drop constants and lower terms for canonical Big-O form
        return DropLowerOrderTerms(DropConstantFactors(simplified));
    }

    /// <inheritdoc />
    public ComplexityExpression DropConstantFactors(ComplexityExpression expression)
    {
        return expression switch
        {
            ConstantComplexity => ConstantComplexity.One,
            LinearComplexity lin => new LinearComplexity(1, lin.Var),
            PolynomialComplexity poly => PolynomialComplexity.OfDegree(poly.Degree, poly.Var),
            LogarithmicComplexity log => new LogarithmicComplexity(1, log.Var, log.Base),
            ExponentialComplexity exp => new ExponentialComplexity(exp.Base, exp.Var, 1),
            FactorialComplexity fac => new FactorialComplexity(fac.Var, 1),
            BinaryOperationComplexity bin => new BinaryOperationComplexity(
                DropConstantFactors(bin.Left),
                bin.Operation,
                DropConstantFactors(bin.Right)),
            _ => expression
        };
    }

    /// <inheritdoc />
    public ComplexityExpression DropLowerOrderTerms(ComplexityExpression expression)
    {
        // For sums, keep only the dominant term
        if (expression is BinaryOperationComplexity { Operation: BinaryOp.Plus } bin)
        {
            var left = DropLowerOrderTerms(bin.Left);
            var right = DropLowerOrderTerms(bin.Right);

            var comparison = AsymptoticComparator.Instance.Compare(left, right);
            return comparison switch
            {
                > 0 => left,   // left dominates
                < 0 => right,  // right dominates
                _ => left      // equal, keep either
            };
        }

        // For products, recursively process
        if (expression is BinaryOperationComplexity { Operation: BinaryOp.Multiply } mul)
        {
            return new BinaryOperationComplexity(
                DropLowerOrderTerms(mul.Left),
                BinaryOp.Multiply,
                DropLowerOrderTerms(mul.Right));
        }

        return expression;
    }
}

/// <summary>
/// Compares complexity expressions by asymptotic growth rate.
/// </summary>
public sealed class AsymptoticComparator : IComplexityComparator
{
    public static readonly AsymptoticComparator Instance = new();

    /// <summary>
    /// Asymptotic ordering (from slowest to fastest growth):
    /// O(1) &lt; O(log n) &lt; O(n) &lt; O(n log n) &lt; O(n²) &lt; O(n³) &lt; O(2ⁿ) &lt; O(n!)
    /// </summary>
    public int Compare(ComplexityExpression left, ComplexityExpression right)
    {
        var leftOrder = GetAsymptoticOrder(left);
        var rightOrder = GetAsymptoticOrder(right);

        return leftOrder.CompareTo(rightOrder);
    }

    /// <inheritdoc />
    public bool IsDominated(ComplexityExpression left, ComplexityExpression right) =>
        Compare(left, right) <= 0;

    /// <inheritdoc />
    public bool AreEquivalent(ComplexityExpression left, ComplexityExpression right) =>
        Compare(left, right) == 0;

    /// <summary>
    /// Gets a numeric order for asymptotic comparison.
    /// Higher values = faster growth.
    /// </summary>
    private double GetAsymptoticOrder(ComplexityExpression expr)
    {
        return expr switch
        {
            ConstantComplexity => 0,
            LogarithmicComplexity => 1,
            VariableComplexity => 2,
            LinearComplexity => 2,
            PolynomialComplexity poly => 2 + poly.Degree - 1, // n=2, n²=3, n³=4, etc.
            ExponentialComplexity => 100, // Much faster than polynomial
            FactorialComplexity => 200,   // Faster than exponential

            BinaryOperationComplexity { Operation: BinaryOp.Plus } bin =>
                Math.Max(GetAsymptoticOrder(bin.Left), GetAsymptoticOrder(bin.Right)),

            BinaryOperationComplexity { Operation: BinaryOp.Multiply } mul =>
                GetAsymptoticOrder(mul.Left) + GetAsymptoticOrder(mul.Right),

            BinaryOperationComplexity { Operation: BinaryOp.Max } max =>
                Math.Max(GetAsymptoticOrder(max.Left), GetAsymptoticOrder(max.Right)),

            // Special cases
            LogOfComplexity => 1,
            ExponentialOfComplexity => 100,
            FactorialOfComplexity => 200,
            PowerComplexity pow => GetAsymptoticOrder(pow.Base) * pow.Exponent,

            _ => 10 // Default for unknown types
        };
    }
}

/// <summary>
/// Extension methods for complexity expressions.
/// </summary>
public static class ComplexityExpressionExtensions
{
    /// <summary>
    /// Simplifies the expression using the default simplifier.
    /// </summary>
    public static ComplexityExpression Simplified(this ComplexityExpression expr) =>
        ComplexitySimplifier.Instance.Simplify(expr);

    /// <summary>
    /// Normalizes to canonical Big-O form.
    /// </summary>
    public static ComplexityExpression Normalized(this ComplexityExpression expr) =>
        ComplexitySimplifier.Instance.NormalizeForm(expr);

    /// <summary>
    /// Compares asymptotically to another expression.
    /// </summary>
    public static int CompareAsymptotically(this ComplexityExpression expr, ComplexityExpression other) =>
        AsymptoticComparator.Instance.Compare(expr, other);

    /// <summary>
    /// Checks if this expression is dominated by another.
    /// </summary>
    public static bool IsDominatedBy(this ComplexityExpression expr, ComplexityExpression other) =>
        AsymptoticComparator.Instance.IsDominated(expr, other);

    /// <summary>
    /// Checks if this expression dominates another.
    /// </summary>
    public static bool Dominates(this ComplexityExpression expr, ComplexityExpression other) =>
        AsymptoticComparator.Instance.Compare(expr, other) > 0;
}
