using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Base type for all complexity expressions. Represents an abstract syntax tree (AST)
/// for algorithmic complexity in Big-O notation and related forms.
///
/// Designed as immutable records for functional composition and transformation.
/// </summary>
public abstract record ComplexityExpression
{
    /// <summary>
    /// Accept a visitor for the expression tree.
    /// </summary>
    public abstract T Accept<T>(IComplexityVisitor<T> visitor);

    /// <summary>
    /// Substitute a variable with another expression.
    /// </summary>
    public abstract ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement);

    /// <summary>
    /// Get all free (unbound) variables in this expression.
    /// </summary>
    public abstract ImmutableHashSet<Variable> FreeVariables { get; }

    /// <summary>
    /// Evaluate the expression for a given variable assignment.
    /// Returns null if evaluation is not possible (e.g., missing variables).
    /// </summary>
    public abstract double? Evaluate(IReadOnlyDictionary<Variable, double> assignments);

    /// <summary>
    /// Get a human-readable string representation in Big-O notation.
    /// </summary>
    public abstract string ToBigONotation();
}

/// <summary>
/// Represents a constant complexity: O(1), O(k) for some constant k.
/// </summary>
public sealed record ConstantComplexity(double Value) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) => this;

    public override ImmutableHashSet<Variable> FreeVariables => ImmutableHashSet<Variable>.Empty;

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) => Value;

    public override string ToBigONotation() =>
        Value == 1 ? "O(1)" : $"O({Value})";

    /// <summary>
    /// The canonical O(1) constant complexity.
    /// </summary>
    public static ConstantComplexity One => new(1);

    /// <summary>
    /// Zero complexity (for base cases).
    /// </summary>
    public static ConstantComplexity Zero => new(0);
}

/// <summary>
/// Represents a single variable complexity: O(n), O(V), O(E), etc.
/// </summary>
public sealed record VariableComplexity(Variable Var) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? replacement : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        assignments.TryGetValue(Var, out var value) ? value : null;

    public override string ToBigONotation() => $"O({Var.Name})";
}

/// <summary>
/// Represents linear complexity with a coefficient: O(k·n).
/// </summary>
public sealed record LinearComplexity(double Coefficient, Variable Var) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var)
            ? new BinaryOperationComplexity(new ConstantComplexity(Coefficient), BinaryOp.Multiply, replacement)
            : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        assignments.TryGetValue(Var, out var value) ? Coefficient * value : null;

    public override string ToBigONotation() =>
        Coefficient == 1 ? $"O({Var.Name})" : $"O({Coefficient}·{Var.Name})";
}

/// <summary>
/// Represents polynomial complexity: O(n²), O(n³), or general polynomial.
/// Coefficients maps degree → coefficient (e.g., {2: 1} for n², {2: 3, 1: 2} for 3n² + 2n).
/// </summary>
public sealed record PolynomialComplexity(
    ImmutableDictionary<int, double> Coefficients,
    Variable Var) : ComplexityExpression
{
    /// <summary>
    /// The highest degree in the polynomial (dominant term).
    /// </summary>
    public int Degree => Coefficients.Keys.DefaultIfEmpty(0).Max();

    /// <summary>
    /// The coefficient of the highest degree term.
    /// </summary>
    public double LeadingCoefficient =>
        Coefficients.TryGetValue(Degree, out var coeff) ? coeff : 0;

    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement)
    {
        if (!variable.Equals(Var)) return this;

        // Substitution into polynomial requires expansion
        // For now, return a composed expression
        return Coefficients.Aggregate(
            (ComplexityExpression)ConstantComplexity.Zero,
            (acc, kvp) =>
            {
                var term = new BinaryOperationComplexity(
                    new ConstantComplexity(kvp.Value),
                    BinaryOp.Multiply,
                    new PowerComplexity(replacement, kvp.Key));
                return new BinaryOperationComplexity(acc, BinaryOp.Plus, term);
            });
    }

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        if (!assignments.TryGetValue(Var, out var n)) return null;
        return Coefficients.Sum(kvp => kvp.Value * Math.Pow(n, kvp.Key));
    }

    public override string ToBigONotation()
    {
        var degree = Degree;
        return degree switch
        {
            0 => "O(1)",
            1 => $"O({Var.Name})",
            2 => $"O({Var.Name}²)",
            3 => $"O({Var.Name}³)",
            _ => $"O({Var.Name}^{degree})"
        };
    }

    /// <summary>
    /// Creates a simple polynomial of the form O(n^k).
    /// </summary>
    public static PolynomialComplexity OfDegree(int degree, Variable var) =>
        new(ImmutableDictionary<int, double>.Empty.Add(degree, 1.0), var);

    /// <summary>
    /// Creates a polynomial approximation for non-integer degrees.
    /// Note: This rounds to the nearest integer since PolynomialComplexity
    /// only supports integer exponents. For exact non-integer exponents,
    /// use PowerComplexity instead.
    /// </summary>
    public static PolynomialComplexity OfDegree(double degree, Variable var) =>
        new(ImmutableDictionary<int, double>.Empty.Add((int)Math.Round(degree), 1.0), var);
}

/// <summary>
/// Represents logarithmic complexity: O(log n), O(k·log n), with configurable base.
/// </summary>
public sealed record LogarithmicComplexity(
    double Coefficient,
    Variable Var,
    double Base = 2.0) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var)
            ? new BinaryOperationComplexity(
                new ConstantComplexity(Coefficient),
                BinaryOp.Multiply,
                new LogOfComplexity(replacement, Base))
            : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        assignments.TryGetValue(Var, out var n) && n > 0
            ? Coefficient * Math.Log(n, Base)
            : null;

    public override string ToBigONotation()
    {
        var logBase = Base == 2 ? "log" : $"log_{Base}";
        return Coefficient == 1
            ? $"O({logBase} {Var.Name})"
            : $"O({Coefficient}·{logBase} {Var.Name})";
    }
}

/// <summary>
/// Represents exponential complexity: O(k^n), O(2^n), etc.
/// </summary>
public sealed record ExponentialComplexity(
    double Base,
    Variable Var,
    double Coefficient = 1.0) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var)
            ? new BinaryOperationComplexity(
                new ConstantComplexity(Coefficient),
                BinaryOp.Multiply,
                new ExponentialOfComplexity(Base, replacement))
            : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments) =>
        assignments.TryGetValue(Var, out var n)
            ? Coefficient * Math.Pow(Base, n)
            : null;

    public override string ToBigONotation() =>
        Coefficient == 1
            ? $"O({Base}^{Var.Name})"
            : $"O({Coefficient}·{Base}^{Var.Name})";
}

/// <summary>
/// Represents factorial complexity: O(n!).
/// </summary>
public sealed record FactorialComplexity(Variable Var, double Coefficient = 1.0) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var)
            ? new BinaryOperationComplexity(
                new ConstantComplexity(Coefficient),
                BinaryOp.Multiply,
                new FactorialOfComplexity(replacement))
            : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        if (!assignments.TryGetValue(Var, out var n) || n < 0 || n != Math.Floor(n))
            return null;
        return Coefficient * Factorial((int)n);
    }

    private static double Factorial(int n)
    {
        if (n <= 1) return 1;
        double result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    public override string ToBigONotation() =>
        Coefficient == 1 ? $"O({Var.Name}!)" : $"O({Coefficient}·{Var.Name}!)";
}

/// <summary>
/// Binary operation on complexity expressions (addition, multiplication, max, min).
/// </summary>
public sealed record BinaryOperationComplexity(
    ComplexityExpression Left,
    BinaryOp Operation,
    ComplexityExpression Right) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new BinaryOperationComplexity(
            Left.Substitute(variable, replacement),
            Operation,
            Right.Substitute(variable, replacement));

    public override ImmutableHashSet<Variable> FreeVariables =>
        Left.FreeVariables.Union(Right.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var leftVal = Left.Evaluate(assignments);
        var rightVal = Right.Evaluate(assignments);
        if (leftVal is null || rightVal is null) return null;

        return Operation switch
        {
            BinaryOp.Plus => leftVal + rightVal,
            BinaryOp.Multiply => leftVal * rightVal,
            BinaryOp.Max => Math.Max(leftVal.Value, rightVal.Value),
            BinaryOp.Min => Math.Min(leftVal.Value, rightVal.Value),
            _ => null
        };
    }

    public override string ToBigONotation()
    {
        var leftStr = Left.ToBigONotation().Replace("O(", "").TrimEnd(')');
        var rightStr = Right.ToBigONotation().Replace("O(", "").TrimEnd(')');

        return Operation switch
        {
            BinaryOp.Plus => $"O({leftStr} + {rightStr})",
            BinaryOp.Multiply => $"O({leftStr} · {rightStr})",
            BinaryOp.Max => $"O(max({leftStr}, {rightStr}))",
            BinaryOp.Min => $"O(min({leftStr}, {rightStr}))",
            _ => $"O({leftStr} ? {rightStr})"
        };
    }
}

/// <summary>
/// Binary operations for composing complexity expressions.
/// </summary>
public enum BinaryOp
{
    /// <summary>
    /// Addition: T₁ + T₂ (sequential composition).
    /// </summary>
    Plus,

    /// <summary>
    /// Multiplication: T₁ × T₂ (nested composition).
    /// </summary>
    Multiply,

    /// <summary>
    /// Maximum: max(T₁, T₂) (branching/worst case).
    /// </summary>
    Max,

    /// <summary>
    /// Minimum: min(T₁, T₂) (best case/early exit).
    /// </summary>
    Min
}

/// <summary>
/// Conditional complexity: different complexities based on runtime conditions.
/// </summary>
public sealed record ConditionalComplexity(
    string ConditionDescription,
    ComplexityExpression TrueBranch,
    ComplexityExpression FalseBranch) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new ConditionalComplexity(
            ConditionDescription,
            TrueBranch.Substitute(variable, replacement),
            FalseBranch.Substitute(variable, replacement));

    public override ImmutableHashSet<Variable> FreeVariables =>
        TrueBranch.FreeVariables.Union(FalseBranch.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        // Conservative: take the max of both branches
        var trueVal = TrueBranch.Evaluate(assignments);
        var falseVal = FalseBranch.Evaluate(assignments);
        if (trueVal is null || falseVal is null) return null;
        return Math.Max(trueVal.Value, falseVal.Value);
    }

    public override string ToBigONotation() =>
        $"O(max({TrueBranch.ToBigONotation().Replace("O(", "").TrimEnd(')')}, " +
        $"{FalseBranch.ToBigONotation().Replace("O(", "").TrimEnd(')')}))";
}

/// <summary>
/// Power of a complexity expression: expr^k.
/// </summary>
public sealed record PowerComplexity(ComplexityExpression Base, double Exponent) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new PowerComplexity(Base.Substitute(variable, replacement), Exponent);

    public override ImmutableHashSet<Variable> FreeVariables => Base.FreeVariables;

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var baseVal = Base.Evaluate(assignments);
        return baseVal.HasValue ? Math.Pow(baseVal.Value, Exponent) : null;
    }

    public override string ToBigONotation()
    {
        var baseStr = Base.ToBigONotation().Replace("O(", "").TrimEnd(')');
        return $"O(({baseStr})^{Exponent})";
    }
}

/// <summary>
/// Logarithm of a complexity expression: log(expr).
/// </summary>
public sealed record LogOfComplexity(ComplexityExpression Argument, double Base = 2.0) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new LogOfComplexity(Argument.Substitute(variable, replacement), Base);

    public override ImmutableHashSet<Variable> FreeVariables => Argument.FreeVariables;

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var argVal = Argument.Evaluate(assignments);
        return argVal.HasValue && argVal.Value > 0 ? Math.Log(argVal.Value, Base) : null;
    }

    public override string ToBigONotation()
    {
        var argStr = Argument.ToBigONotation().Replace("O(", "").TrimEnd(')');
        var logBase = Base == 2 ? "log" : $"log_{Base}";
        return $"O({logBase}({argStr}))";
    }
}

/// <summary>
/// Exponential of a complexity expression: base^expr.
/// </summary>
public sealed record ExponentialOfComplexity(double Base, ComplexityExpression Exponent) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new ExponentialOfComplexity(Base, Exponent.Substitute(variable, replacement));

    public override ImmutableHashSet<Variable> FreeVariables => Exponent.FreeVariables;

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var expVal = Exponent.Evaluate(assignments);
        return expVal.HasValue ? Math.Pow(Base, expVal.Value) : null;
    }

    public override string ToBigONotation()
    {
        var expStr = Exponent.ToBigONotation().Replace("O(", "").TrimEnd(')');
        return $"O({Base}^({expStr}))";
    }
}

/// <summary>
/// Factorial of a complexity expression: expr!.
/// </summary>
public sealed record FactorialOfComplexity(ComplexityExpression Argument) : ComplexityExpression
{
    public override T Accept<T>(IComplexityVisitor<T> visitor) => visitor.Visit(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        new FactorialOfComplexity(Argument.Substitute(variable, replacement));

    public override ImmutableHashSet<Variable> FreeVariables => Argument.FreeVariables;

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var argVal = Argument.Evaluate(assignments);
        if (!argVal.HasValue || argVal.Value < 0 || argVal.Value != Math.Floor(argVal.Value))
            return null;

        var n = (int)argVal.Value;
        double result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;
        return result;
    }

    public override string ToBigONotation()
    {
        var argStr = Argument.ToBigONotation().Replace("O(", "").TrimEnd(')');
        return $"O(({argStr})!)";
    }
}
