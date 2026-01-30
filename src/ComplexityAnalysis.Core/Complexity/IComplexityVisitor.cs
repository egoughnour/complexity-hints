using System.Collections.Immutable;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Visitor pattern interface for traversing complexity expression trees.
/// Enables operations like simplification, evaluation, and transformation.
/// </summary>
public interface IComplexityVisitor<out T>
{
    T Visit(ConstantComplexity expr);
    T Visit(VariableComplexity expr);
    T Visit(LinearComplexity expr);
    T Visit(PolynomialComplexity expr);
    T Visit(LogarithmicComplexity expr);
    T Visit(ExponentialComplexity expr);
    T Visit(FactorialComplexity expr);
    T Visit(BinaryOperationComplexity expr);
    T Visit(ConditionalComplexity expr);
    T Visit(PowerComplexity expr);
    T Visit(LogOfComplexity expr);
    T Visit(ExponentialOfComplexity expr);
    T Visit(FactorialOfComplexity expr);
    T Visit(RecurrenceComplexity expr);
    T Visit(PolyLogComplexity expr);

    /// <summary>
    /// Fallback for unknown/unrecognized expression types (e.g., special functions).
    /// </summary>
    T VisitUnknown(ComplexityExpression expr);
}

/// <summary>
/// Base implementation of IComplexityVisitor that returns default values.
/// Override specific methods to handle particular expression types.
/// </summary>
public abstract class ComplexityVisitorBase<T> : IComplexityVisitor<T>
{
    protected abstract T Default { get; }

    public virtual T Visit(ConstantComplexity expr) => Default;
    public virtual T Visit(VariableComplexity expr) => Default;
    public virtual T Visit(LinearComplexity expr) => Default;
    public virtual T Visit(PolynomialComplexity expr) => Default;
    public virtual T Visit(LogarithmicComplexity expr) => Default;
    public virtual T Visit(ExponentialComplexity expr) => Default;
    public virtual T Visit(FactorialComplexity expr) => Default;
    public virtual T Visit(BinaryOperationComplexity expr) => Default;
    public virtual T Visit(ConditionalComplexity expr) => Default;
    public virtual T Visit(PowerComplexity expr) => Default;
    public virtual T Visit(LogOfComplexity expr) => Default;
    public virtual T Visit(ExponentialOfComplexity expr) => Default;
    public virtual T Visit(FactorialOfComplexity expr) => Default;
    public virtual T Visit(RecurrenceComplexity expr) => Default;
    public virtual T Visit(PolyLogComplexity expr) => Default;
    public virtual T VisitUnknown(ComplexityExpression expr) => Default;
}

/// <summary>
/// Visitor that recursively transforms complexity expressions.
/// Override methods to modify specific node types during traversal.
/// </summary>
public abstract class ComplexityTransformVisitor : IComplexityVisitor<ComplexityExpression>
{
    public virtual ComplexityExpression Visit(ConstantComplexity expr) => expr;
    public virtual ComplexityExpression Visit(VariableComplexity expr) => expr;
    public virtual ComplexityExpression Visit(LinearComplexity expr) => expr;
    public virtual ComplexityExpression Visit(PolynomialComplexity expr) => expr;
    public virtual ComplexityExpression Visit(LogarithmicComplexity expr) => expr;
    public virtual ComplexityExpression Visit(ExponentialComplexity expr) => expr;
    public virtual ComplexityExpression Visit(FactorialComplexity expr) => expr;

    public virtual ComplexityExpression Visit(BinaryOperationComplexity expr) =>
        new BinaryOperationComplexity(
            expr.Left.Accept(this),
            expr.Operation,
            expr.Right.Accept(this));

    public virtual ComplexityExpression Visit(ConditionalComplexity expr) =>
        new ConditionalComplexity(
            expr.ConditionDescription,
            expr.TrueBranch.Accept(this),
            expr.FalseBranch.Accept(this));

    public virtual ComplexityExpression Visit(PowerComplexity expr) =>
        new PowerComplexity(expr.Base.Accept(this), expr.Exponent);

    public virtual ComplexityExpression Visit(LogOfComplexity expr) =>
        new LogOfComplexity(expr.Argument.Accept(this), expr.Base);

    public virtual ComplexityExpression Visit(ExponentialOfComplexity expr) =>
        new ExponentialOfComplexity(expr.Base, expr.Exponent.Accept(this));

    public virtual ComplexityExpression Visit(FactorialOfComplexity expr) =>
        new FactorialOfComplexity(expr.Argument.Accept(this));

    public virtual ComplexityExpression Visit(RecurrenceComplexity expr) =>
        new RecurrenceComplexity(
            expr.Terms.Select(t => t with { Argument = t.Argument.Accept(this) }).ToImmutableList(),
            expr.RecurrenceVariable,
            expr.NonRecursiveWork.Accept(this),
            expr.BaseCaseComplexity.Accept(this));

    public virtual ComplexityExpression Visit(PolyLogComplexity expr) => expr;

    public virtual ComplexityExpression VisitUnknown(ComplexityExpression expr) => expr;
}
