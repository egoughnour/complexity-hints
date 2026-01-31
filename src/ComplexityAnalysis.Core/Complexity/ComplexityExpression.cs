using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Base type for all complexity expressions representing algorithmic time or space complexity.
/// </summary>
/// <remarks>
/// <para>
/// <b>Design Philosophy:</b> This forms the core of an expression-based complexity algebra
/// that represents Big-O expressions as composable Abstract Syntax Trees (AST). This design
/// enables:
/// </para>
/// <list type="bullet">
///   <item><description>Type-safe composition of complexity expressions</description></item>
///   <item><description>Algebraic simplification (e.g., O(n) + O(n²) → O(n²))</description></item>
///   <item><description>Variable substitution for parametric complexity</description></item>
///   <item><description>Evaluation for specific input sizes</description></item>
///   <item><description>Visitor pattern for transformation and analysis</description></item>
/// </list>
/// 
/// <para>
/// <b>Type Hierarchy:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Category</term>
///     <description>Types</description>
///   </listheader>
///   <item>
///     <term>Primitive</term>
///     <description>
///       <see cref="ConstantComplexity"/> (O(1)),
///       <see cref="VariableComplexity"/> (O(n)),
///       <see cref="LinearComplexity"/> (O(k·n))
///     </description>
///   </item>
///   <item>
///     <term>Polynomial</term>
///     <description>
///       <see cref="PolynomialComplexity"/> (O(n²), O(n³), etc.),
///       <see cref="PolyLogComplexity"/> (O(n log n))
///     </description>
///   </item>
///   <item>
///     <term>Transcendental</term>
///     <description>
///       <see cref="LogarithmicComplexity"/> (O(log n)),
///       <see cref="ExponentialComplexity"/> (O(2ⁿ)),
///       <see cref="FactorialComplexity"/> (O(n!))
///     </description>
///   </item>
///   <item>
///     <term>Compositional</term>
///     <description>
///       <see cref="BinaryOperationComplexity"/> (+, ×, max, min),
///       <see cref="ConditionalComplexity"/> (branching)
///     </description>
///   </item>
/// </list>
/// 
/// <para>
/// <b>Composition Rules:</b>
/// </para>
/// <code>
/// // Sequential (addition): loops following loops
/// var seq = new BinaryOperationComplexity(O_n, BinaryOp.Plus, O_logN);
/// // → O(n + log n) → O(n) after simplification
/// 
/// // Nested (multiplication): loops inside loops
/// var nested = new BinaryOperationComplexity(O_n, BinaryOp.Multiply, O_m);
/// // → O(n × m)
/// 
/// // Branching (max): if-else with different complexities
/// var branch = new BinaryOperationComplexity(O_n, BinaryOp.Max, O_nSquared);
/// // → O(max(n, n²)) → O(n²)
/// </code>
/// 
/// <para>
/// All expressions are implemented as immutable records for thread-safety and
/// functional composition patterns.
/// </para>
/// </remarks>
/// <seealso cref="IComplexityVisitor{T}"/>
/// <seealso cref="ComplexityComposition"/>
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
/// Represents a constant complexity: O(1) or O(k) for some constant k.
/// </summary>
/// <remarks>
/// <para>
/// Constant complexity represents operations whose execution time does not depend
/// on input size. Common sources include:
/// </para>
/// <list type="bullet">
///   <item><description>Array indexing: <c>arr[i]</c></description></item>
///   <item><description>Hash table lookup (amortized): <c>dict[key]</c></description></item>
///   <item><description>Arithmetic operations: <c>a + b * c</c></description></item>
///   <item><description>Base cases of recursive algorithms</description></item>
/// </list>
/// <para>
/// The <see cref="Value"/> property captures any constant factor, though in
/// asymptotic analysis O(1) = O(k) for any constant k.
/// </para>
/// </remarks>
/// <param name="Value">The constant value (typically 1).</param>
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
/// <remarks>
/// <para>
/// This is the simplest form of linear complexity—a single variable without
/// a coefficient. For complexity with coefficients, see <see cref="LinearComplexity"/>.
/// </para>
/// <para>
/// Common variable types defined in <see cref="Variable"/>:
/// </para>
/// <list type="bullet">
///   <item><description><c>n</c> - General input size</description></item>
///   <item><description><c>V</c> - Vertex count in graphs</description></item>
///   <item><description><c>E</c> - Edge count in graphs</description></item>
///   <item><description><c>m</c> - Secondary size parameter (e.g., pattern length)</description></item>
/// </list>
/// </remarks>
/// <param name="Var">The variable representing the input size.</param>
/// <seealso cref="Variable"/>
/// <seealso cref="VariableType"/>
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
/// Represents polynomial complexity: O(n²), O(n³), or general polynomial forms.
/// </summary>
/// <remarks>
/// <para>
/// Polynomials represent algorithms with nested loops or recursive patterns that
/// process proportional fractions of input at each level.
/// </para>
/// <para>
/// <b>Structure:</b> The <see cref="Coefficients"/> dictionary maps degree → coefficient.
/// For example:
/// </para>
/// <list type="bullet">
///   <item><description><c>{2: 1}</c> represents n²</description></item>
///   <item><description><c>{2: 3, 1: 2}</c> represents 3n² + 2n</description></item>
///   <item><description><c>{3: 1, 2: 1, 1: 1}</c> represents n³ + n² + n</description></item>
/// </list>
/// <para>
/// <b>Common algorithmic sources:</b>
/// </para>
/// <list type="bullet">
///   <item><description>O(n²): Bubble sort, insertion sort, naive matrix operations</description></item>
///   <item><description>O(n³): Standard matrix multiplication, Floyd-Warshall</description></item>
///   <item><description>O(n⁴): Naive bipartite matching</description></item>
/// </list>
/// <para>
/// <b>Note:</b> For non-integer exponents (e.g., O(n^2.807) for Strassen), use
/// <see cref="PowerComplexity"/> or <see cref="PolyLogComplexity"/> instead.
/// </para>
/// </remarks>
/// <param name="Coefficients">Dictionary mapping degree → coefficient.</param>
/// <param name="Var">The variable over which the polynomial is defined.</param>
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
/// <remarks>
/// <para>
/// Logarithmic complexity typically arises from algorithms that halve (or divide by
/// a constant) the problem size at each step.
/// </para>
/// <para>
/// <b>Common algorithmic sources:</b>
/// </para>
/// <list type="bullet">
///   <item><description>Binary search: O(log n)</description></item>
///   <item><description>Balanced BST operations: O(log n)</description></item>
///   <item><description>Exponentiation by squaring: O(log n)</description></item>
/// </list>
/// <para>
/// <b>Base equivalence:</b> In asymptotic analysis, log₂(n) = Θ(logₖ(n)) for any
/// constant k > 1, since logₖ(n) = log₂(n) / log₂(k). The base is preserved
/// for precision in constant factor analysis.
/// </para>
/// </remarks>
/// <param name="Coefficient">Multiplicative coefficient (default 1).</param>
/// <param name="Var">The variable inside the logarithm.</param>
/// <param name="Base">Logarithm base (default 2 for binary algorithms).</param>
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
/// <remarks>
/// <para>
/// Exponential complexity indicates algorithms with explosive growth, typically
/// arising from exhaustive enumeration or branching recursive patterns without
/// memoization.
/// </para>
/// <para>
/// <b>Common algorithmic sources:</b>
/// </para>
/// <list type="bullet">
///   <item><description>Brute-force subset enumeration: O(2ⁿ)</description></item>
///   <item><description>Naive recursive Fibonacci: O(φⁿ) ≈ O(1.618ⁿ)</description></item>
///   <item><description>Traveling salesman (brute force): O(n! × n) ≈ O(nⁿ)</description></item>
///   <item><description>3-SAT exhaustive search: O(3ⁿ)</description></item>
/// </list>
/// <para>
/// <b>Growth comparison:</b> 2¹⁰ = 1,024 but 2²⁰ ≈ 1 million and 2³⁰ ≈ 1 billion.
/// Exponential algorithms become infeasible very quickly.
/// </para>
/// </remarks>
/// <param name="Base">The exponential base (e.g., 2 for O(2ⁿ)).</param>
/// <param name="Var">The variable in the exponent.</param>
/// <param name="Coefficient">Optional multiplicative coefficient.</param>
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
/// <remarks>
/// <para>
/// Factorial complexity represents the most extreme form of combinatorial explosion,
/// growing faster than exponential. By Stirling's approximation:
/// n! ≈ √(2πn) × (n/e)ⁿ
/// </para>
/// <para>
/// <b>Common algorithmic sources:</b>
/// </para>
/// <list type="bullet">
///   <item><description>Generating all permutations: O(n!)</description></item>
///   <item><description>Traveling salesman brute force: O(n!)</description></item>
///   <item><description>Determinant by definition: O(n!)</description></item>
/// </list>
/// <para>
/// <b>Growth illustration:</b> 10! = 3,628,800 while 20! ≈ 2.4 × 10¹⁸.
/// Factorial algorithms are typically only feasible for n ≤ 12.
/// </para>
/// </remarks>
/// <param name="Var">The variable in the factorial.</param>
/// <param name="Coefficient">Optional multiplicative coefficient.</param>
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
/// Binary operation on complexity expressions for compositional analysis.
/// </summary>
/// <remarks>
/// <para>
/// Binary operations form the backbone of complexity composition, mapping code
/// structure to complexity algebra:
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Operation</term>
///     <description>Code Pattern</description>
///   </listheader>
///   <item>
///     <term><see cref="BinaryOp.Plus"/> (T₁ + T₂)</term>
///     <description>Sequential code blocks: <c>loop1(); loop2();</c></description>
///   </item>
///   <item>
///     <term><see cref="BinaryOp.Multiply"/> (T₁ × T₂)</term>
///     <description>Nested loops: <c>for(...) { for(...) { } }</c></description>
///   </item>
///   <item>
///     <term><see cref="BinaryOp.Max"/> (max(T₁, T₂))</term>
///     <description>Branching: <c>if(cond) { slow } else { fast }</c></description>
///   </item>
///   <item>
///     <term><see cref="BinaryOp.Min"/> (min(T₁, T₂))</term>
///     <description>Best-case/early exit analysis</description>
///   </item>
/// </list>
/// <para>
/// <b>Simplification Rules:</b>
/// </para>
/// <code>
/// O(n) + O(n²) = O(n²)           // Max dominates in addition
/// O(n) × O(m) = O(n·m)           // Multiplication combines
/// max(O(n), O(n²)) = O(n²)       // Max selects dominant
/// O(1) × O(f(n)) = O(f(n))       // Identity for multiplication
/// </code>
/// </remarks>
/// <param name="Left">Left operand expression.</param>
/// <param name="Operation">The binary operation to perform.</param>
/// <param name="Right">Right operand expression.</param>
/// <seealso cref="BinaryOp"/>
/// <seealso cref="ComplexityComposition"/>
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
/// <remarks>
/// These operations model how code structure translates to complexity composition:
/// <list type="bullet">
///   <item><description><c>Plus</c>: Sequential execution (loop₁; loop₂)</description></item>
///   <item><description><c>Multiply</c>: Nested execution (for { for { } })</description></item>
///   <item><description><c>Max</c>: Worst-case branching (if-else)</description></item>
///   <item><description><c>Min</c>: Best-case / early exit</description></item>
/// </list>
/// </remarks>
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
/// Conditional complexity: represents different complexities based on runtime conditions.
/// </summary>
/// <remarks>
/// <para>
/// Models code branches where different paths have different complexities:
/// </para>
/// <code>
/// if (isSorted) {
///     BinarySearch();     // O(log n)
/// } else {
///     LinearSearch();     // O(n)
/// }
/// // → ConditionalComplexity("isSorted", O(log n), O(n))
/// </code>
/// <para>
/// <b>Evaluation Strategy:</b> For worst-case analysis, <see cref="Evaluate"/>
/// conservatively returns max(TrueBranch, FalseBranch). For best-case or
/// average-case analysis, see the speculative analysis infrastructure.
/// </para>
/// </remarks>
/// <param name="ConditionDescription">Human-readable description of the condition.</param>
/// <param name="TrueBranch">Complexity when condition is true.</param>
/// <param name="FalseBranch">Complexity when condition is false.</param>
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
