using System.Collections.Immutable;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Provides methods for composing complexity expressions based on control flow patterns.
/// These rules form the foundation of static complexity analysis.
/// </summary>
public static class ComplexityComposition
{
    /// <summary>
    /// Sequential composition: T₁ followed by T₂.
    /// Total complexity: O(T₁ + T₂)
    ///
    /// In Big-O terms, the dominant term will dominate:
    /// O(n) + O(n²) = O(n²)
    /// </summary>
    public static ComplexityExpression Sequential(
        ComplexityExpression first,
        ComplexityExpression second)
    {
        // Optimize common cases
        if (first is ConstantComplexity { Value: 0 })
            return second;
        if (second is ConstantComplexity { Value: 0 })
            return first;

        return new BinaryOperationComplexity(first, BinaryOp.Plus, second);
    }

    /// <summary>
    /// Sequential composition of multiple expressions.
    /// </summary>
    public static ComplexityExpression Sequential(params ComplexityExpression[] expressions) =>
        Sequential(expressions.AsEnumerable());

    /// <summary>
    /// Sequential composition of multiple expressions.
    /// </summary>
    public static ComplexityExpression Sequential(IEnumerable<ComplexityExpression> expressions)
    {
        var list = expressions.Where(e => e is not ConstantComplexity { Value: 0 }).ToList();

        return list.Count switch
        {
            0 => ConstantComplexity.Zero,
            1 => list[0],
            _ => list.Aggregate(Sequential)
        };
    }

    /// <summary>
    /// Nested composition: T₁ inside T₂ (e.g., nested loops).
    /// Total complexity: O(T₁ × T₂)
    ///
    /// Example: for i in 0..n: for j in 0..n: O(1)
    /// Result: O(n) × O(n) × O(1) = O(n²)
    /// </summary>
    public static ComplexityExpression Nested(
        ComplexityExpression outer,
        ComplexityExpression inner)
    {
        // Optimize common cases
        if (outer is ConstantComplexity { Value: 1 })
            return inner;
        if (inner is ConstantComplexity { Value: 1 })
            return outer;
        if (outer is ConstantComplexity { Value: 0 } || inner is ConstantComplexity { Value: 0 })
            return ConstantComplexity.Zero;

        return new BinaryOperationComplexity(outer, BinaryOp.Multiply, inner);
    }

    /// <summary>
    /// Nested composition of multiple expressions (deeply nested loops).
    /// </summary>
    public static ComplexityExpression Nested(params ComplexityExpression[] expressions) =>
        Nested(expressions.AsEnumerable());

    /// <summary>
    /// Nested composition of multiple expressions.
    /// </summary>
    public static ComplexityExpression Nested(IEnumerable<ComplexityExpression> expressions)
    {
        var list = expressions.Where(e => e is not ConstantComplexity { Value: 1 }).ToList();

        // Check for zero - any zero makes the whole thing zero
        if (list.Any(e => e is ConstantComplexity { Value: 0 }))
            return ConstantComplexity.Zero;

        return list.Count switch
        {
            0 => ConstantComplexity.One,
            1 => list[0],
            _ => list.Aggregate(Nested)
        };
    }

    /// <summary>
    /// Branching composition: if-else statement.
    /// Total complexity: O(max(T_true, T_false))
    ///
    /// We take the worst case because either branch might execute.
    /// </summary>
    public static ComplexityExpression Branching(
        ComplexityExpression trueBranch,
        ComplexityExpression falseBranch)
    {
        // Optimize common cases
        if (trueBranch.Equals(falseBranch))
            return trueBranch;

        return new BinaryOperationComplexity(trueBranch, BinaryOp.Max, falseBranch);
    }

    /// <summary>
    /// Branching composition with condition overhead.
    /// Total complexity: O(T_condition + max(T_true, T_false))
    /// </summary>
    public static ComplexityExpression BranchingWithCondition(
        ComplexityExpression condition,
        ComplexityExpression trueBranch,
        ComplexityExpression falseBranch)
    {
        return Sequential(condition, Branching(trueBranch, falseBranch));
    }

    /// <summary>
    /// Multi-way branching (switch/match).
    /// Total complexity: O(max(T₁, T₂, ..., Tₙ))
    /// </summary>
    public static ComplexityExpression Switch(IEnumerable<ComplexityExpression> cases)
    {
        var list = cases.ToList();
        return list.Count switch
        {
            0 => ConstantComplexity.Zero,
            1 => list[0],
            _ => list.Aggregate(Branching)
        };
    }

    /// <summary>
    /// Loop composition with known iteration count.
    /// Total complexity: O(iterations × body)
    /// </summary>
    public static ComplexityExpression Loop(
        ComplexityExpression iterations,
        ComplexityExpression body) =>
        Nested(iterations, body);

    /// <summary>
    /// For loop with linear iterations: for i = 0 to n.
    /// Total complexity: O(n × body)
    /// </summary>
    public static ComplexityExpression ForLoop(
        Variable iterationVariable,
        ComplexityExpression body) =>
        Loop(new VariableComplexity(iterationVariable), body);

    /// <summary>
    /// For loop with bounded iterations: for i = 0 to constant.
    /// Total complexity: O(body) (the constant factor is absorbed)
    /// </summary>
    public static ComplexityExpression BoundedForLoop(
        int iterations,
        ComplexityExpression body)
    {
        // In Big-O, a constant number of iterations just multiplies by a constant
        // O(k × body) = O(body) for constant k
        // But we preserve it for precision in non-asymptotic analysis
        return Loop(new ConstantComplexity(iterations), body);
    }

    /// <summary>
    /// Logarithmic loop: for i = 1; i &lt; n; i *= 2.
    /// Total complexity: O(log n × body)
    /// </summary>
    public static ComplexityExpression LogarithmicLoop(
        Variable sizeVariable,
        ComplexityExpression body,
        double logBase = 2.0) =>
        Loop(new LogarithmicComplexity(1, sizeVariable, logBase), body);

    /// <summary>
    /// Early exit pattern: loop that may terminate early.
    /// Total complexity: O(min(early_exit, full_iterations) × body)
    ///
    /// For worst-case analysis, we typically use the full iterations.
    /// For average-case, the expected early exit point matters.
    /// </summary>
    public static ComplexityExpression LoopWithEarlyExit(
        ComplexityExpression maxIterations,
        ComplexityExpression earlyExitIterations,
        ComplexityExpression body,
        bool useWorstCase = true)
    {
        var iterations = useWorstCase
            ? maxIterations
            : new BinaryOperationComplexity(earlyExitIterations, BinaryOp.Min, maxIterations);

        return Loop(iterations, body);
    }

    /// <summary>
    /// Recursive composition: function calls itself.
    /// Returns a RecurrenceComplexity that needs to be solved.
    ///
    /// For T(n) = T(n-1) + work, this creates:
    /// RecurrenceComplexity with linear reduction.
    /// </summary>
    public static ComplexityExpression LinearRecursion(
        Variable sizeVariable,
        ComplexityExpression workPerCall)
    {
        return RecurrenceComplexity.Linear(workPerCall, sizeVariable);
    }

    /// <summary>
    /// Divide and conquer recursion: T(n) = a × T(n/b) + work.
    /// Returns a RecurrenceComplexity that can be solved via Master Theorem.
    /// </summary>
    public static ComplexityExpression DivideAndConquer(
        Variable sizeVariable,
        int subproblems,
        int divisionFactor,
        ComplexityExpression mergeWork)
    {
        return RecurrenceComplexity.DivideAndConquer(
            subproblems,
            divisionFactor,
            mergeWork,
            sizeVariable);
    }

    /// <summary>
    /// Binary recursion: T(n) = 2T(n/2) + work.
    /// Common pattern for divide and conquer algorithms.
    /// </summary>
    public static ComplexityExpression BinaryRecursion(
        Variable sizeVariable,
        ComplexityExpression mergeWork) =>
        DivideAndConquer(sizeVariable, 2, 2, mergeWork);

    /// <summary>
    /// Function call composition: calling a function with known complexity.
    /// Total complexity: O(argument_setup + function_complexity)
    /// </summary>
    public static ComplexityExpression FunctionCall(
        ComplexityExpression functionComplexity,
        ComplexityExpression argumentSetup = null!)
    {
        argumentSetup ??= ConstantComplexity.One;
        return Sequential(argumentSetup, functionComplexity);
    }

    /// <summary>
    /// Amortized operation: multiple operations with varying individual costs
    /// but known total cost over n operations.
    ///
    /// Example: n insertions into a dynamic array = O(n) total, O(1) amortized per op.
    /// </summary>
    public static ComplexityExpression Amortized(
        ComplexityExpression totalCostForNOperations,
        Variable operationCount)
    {
        // Per-operation amortized cost
        return new BinaryOperationComplexity(
            totalCostForNOperations,
            BinaryOp.Multiply,
            new BinaryOperationComplexity(
                ConstantComplexity.One,
                BinaryOp.Multiply,
                new PowerComplexity(new VariableComplexity(operationCount), -1)));
    }

    /// <summary>
    /// Conditional complexity: different complexity based on runtime condition.
    /// </summary>
    public static ComplexityExpression Conditional(
        string conditionDescription,
        ComplexityExpression trueBranch,
        ComplexityExpression falseBranch) =>
        new ConditionalComplexity(conditionDescription, trueBranch, falseBranch);
}

/// <summary>
/// Fluent builder for constructing complex complexity expressions.
/// </summary>
public sealed class ComplexityBuilder
{
    private ComplexityExpression _current;

    public ComplexityBuilder(ComplexityExpression initial)
    {
        _current = initial;
    }

    /// <summary>
    /// Start building with O(1).
    /// </summary>
    public static ComplexityBuilder Constant() =>
        new(ConstantComplexity.One);

    /// <summary>
    /// Start building with O(n).
    /// </summary>
    public static ComplexityBuilder Linear(Variable? variable = null) =>
        new(new VariableComplexity(variable ?? Variable.N));

    /// <summary>
    /// Add sequential operation: current + next.
    /// </summary>
    public ComplexityBuilder Then(ComplexityExpression next)
    {
        _current = ComplexityComposition.Sequential(_current, next);
        return this;
    }

    /// <summary>
    /// Nest inside a loop: iterations × current.
    /// </summary>
    public ComplexityBuilder InsideLoop(ComplexityExpression iterations)
    {
        _current = ComplexityComposition.Loop(iterations, _current);
        return this;
    }

    /// <summary>
    /// Nest inside a loop over n.
    /// </summary>
    public ComplexityBuilder InsideLinearLoop(Variable? variable = null)
    {
        _current = ComplexityComposition.ForLoop(variable ?? Variable.N, _current);
        return this;
    }

    /// <summary>
    /// Add branch: max(current, alternative).
    /// </summary>
    public ComplexityBuilder OrBranch(ComplexityExpression alternative)
    {
        _current = ComplexityComposition.Branching(_current, alternative);
        return this;
    }

    /// <summary>
    /// Multiply by a factor.
    /// </summary>
    public ComplexityBuilder Times(ComplexityExpression factor)
    {
        _current = ComplexityComposition.Nested(_current, factor);
        return this;
    }

    /// <summary>
    /// Build the final expression.
    /// </summary>
    public ComplexityExpression Build() => _current;

    /// <summary>
    /// Implicit conversion to ComplexityExpression.
    /// </summary>
    public static implicit operator ComplexityExpression(ComplexityBuilder builder) =>
        builder.Build();
}
