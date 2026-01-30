namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Classifies the dominant asymptotic form of complexity expressions.
/// Essential for determining theorem applicability.
/// </summary>
public enum ExpressionForm
{
    /// <summary>O(1) - constant complexity.</summary>
    Constant,

    /// <summary>O(log^k n) - pure logarithmic (no polynomial factor).</summary>
    Logarithmic,

    /// <summary>O(n^k) - pure polynomial.</summary>
    Polynomial,

    /// <summary>O(n^k · log^j n) - polylogarithmic.</summary>
    PolyLog,

    /// <summary>O(k^n) - exponential.</summary>
    Exponential,

    /// <summary>O(n!) - factorial.</summary>
    Factorial,

    /// <summary>Cannot be classified into standard forms.</summary>
    Unknown
}

/// <summary>
/// Result of classifying an expression's asymptotic form.
/// </summary>
public sealed record ExpressionClassification
{
    /// <summary>The dominant asymptotic form.</summary>
    public required ExpressionForm Form { get; init; }

    /// <summary>The variable the classification is with respect to.</summary>
    public required Variable Variable { get; init; }

    /// <summary>
    /// For Polynomial/PolyLog: the polynomial degree k in n^k.
    /// For Logarithmic: 0.
    /// For Exponential: the base.
    /// </summary>
    public double? PrimaryParameter { get; init; }

    /// <summary>
    /// For PolyLog/Logarithmic: the log exponent j in log^j n.
    /// </summary>
    public double? LogExponent { get; init; }

    /// <summary>
    /// Leading coefficient (preserved for non-asymptotic analysis).
    /// </summary>
    public double Coefficient { get; init; } = 1.0;

    /// <summary>
    /// Confidence level in the classification (0.0 to 1.0).
    /// Lower for complex composed expressions.
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Converts to a normalized PolyLogComplexity if applicable.
    /// </summary>
    public PolyLogComplexity? ToPolyLog() => Form switch
    {
        ExpressionForm.Constant => new PolyLogComplexity(0, 0, Variable, Coefficient),
        ExpressionForm.Logarithmic => new PolyLogComplexity(0, LogExponent ?? 1, Variable, Coefficient),
        ExpressionForm.Polynomial => new PolyLogComplexity(PrimaryParameter ?? 1, 0, Variable, Coefficient),
        ExpressionForm.PolyLog => new PolyLogComplexity(PrimaryParameter ?? 1, LogExponent ?? 1, Variable, Coefficient),
        _ => null
    };

    /// <summary>
    /// Compares the polynomial degree to a target value.
    /// Returns: &lt;0 if degree &lt; target, 0 if equal (within epsilon), &gt;0 if degree &gt; target.
    /// </summary>
    public int CompareDegreeTo(double target, double epsilon = 1e-9)
    {
        var degree = PrimaryParameter ?? 0;
        if (Math.Abs(degree - target) < epsilon) return 0;
        return degree < target ? -1 : 1;
    }
}

/// <summary>
/// Interface for classifying complexity expressions into standard forms.
/// Used to determine theorem applicability.
/// </summary>
public interface IExpressionClassifier
{
    /// <summary>
    /// Classifies the dominant asymptotic form of an expression.
    /// </summary>
    ExpressionClassification Classify(ComplexityExpression expr, Variable variable);

    /// <summary>
    /// Attempts to extract polynomial degree if expression is O(n^k).
    /// </summary>
    bool TryExtractPolynomialDegree(ComplexityExpression expr, Variable variable, out double degree);

    /// <summary>
    /// Attempts to extract polylog form parameters if expression is O(n^k · log^j n).
    /// </summary>
    bool TryExtractPolyLogForm(
        ComplexityExpression expr,
        Variable variable,
        out double polyDegree,
        out double logExponent);

    /// <summary>
    /// Determines if expression is bounded by O(n^d) for given d.
    /// </summary>
    bool IsBoundedByPolynomial(ComplexityExpression expr, Variable variable, double degree);

    /// <summary>
    /// Determines if expression dominates Ω(n^d) for given d.
    /// </summary>
    bool DominatesPolynomial(ComplexityExpression expr, Variable variable, double degree);
}

/// <summary>
/// Standard implementation of expression classification.
/// Uses pattern matching and visitor traversal.
/// </summary>
public sealed class StandardExpressionClassifier : IExpressionClassifier
{
    public static readonly StandardExpressionClassifier Instance = new();

    public ExpressionClassification Classify(ComplexityExpression expr, Variable variable)
    {
        return expr switch
        {
            ConstantComplexity c => new ExpressionClassification
            {
                Form = ExpressionForm.Constant,
                Variable = variable,
                PrimaryParameter = 0,
                Coefficient = c.Value
            },

            VariableComplexity v when v.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Polynomial,
                Variable = variable,
                PrimaryParameter = 1,
                Coefficient = 1
            },

            LinearComplexity lin when lin.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Polynomial,
                Variable = variable,
                PrimaryParameter = 1,
                Coefficient = lin.Coefficient
            },

            PolynomialComplexity poly when poly.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Polynomial,
                Variable = variable,
                PrimaryParameter = poly.Degree,
                Coefficient = poly.LeadingCoefficient
            },

            LogarithmicComplexity log when log.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Logarithmic,
                Variable = variable,
                PrimaryParameter = 0,
                LogExponent = 1,
                Coefficient = log.Coefficient
            },

            PolyLogComplexity pl when pl.Var.Equals(variable) => new ExpressionClassification
            {
                Form = pl.IsPurePolynomial ? ExpressionForm.Polynomial
                    : pl.IsPureLogarithmic ? ExpressionForm.Logarithmic
                    : ExpressionForm.PolyLog,
                Variable = variable,
                PrimaryParameter = pl.PolyDegree,
                LogExponent = pl.LogExponent,
                Coefficient = pl.Coefficient
            },

            ExponentialComplexity exp when exp.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Exponential,
                Variable = variable,
                PrimaryParameter = exp.Base,
                Coefficient = exp.Coefficient
            },

            FactorialComplexity fac when fac.Var.Equals(variable) => new ExpressionClassification
            {
                Form = ExpressionForm.Factorial,
                Variable = variable,
                Coefficient = fac.Coefficient
            },

            BinaryOperationComplexity bin => ClassifyBinaryOp(bin, variable),

            PowerComplexity pow => ClassifyPower(pow, variable),

            _ => new ExpressionClassification
            {
                Form = ExpressionForm.Unknown,
                Variable = variable,
                Confidence = 0.5
            }
        };
    }

    private ExpressionClassification ClassifyBinaryOp(BinaryOperationComplexity bin, Variable variable)
    {
        var leftClass = Classify(bin.Left, variable);
        var rightClass = Classify(bin.Right, variable);

        return bin.Operation switch
        {
            BinaryOp.Plus => ClassifySum(leftClass, rightClass, variable),
            BinaryOp.Multiply => ClassifyProduct(leftClass, rightClass, variable),
            BinaryOp.Max => ClassifyMax(leftClass, rightClass, variable),
            _ => new ExpressionClassification { Form = ExpressionForm.Unknown, Variable = variable }
        };
    }

    private ExpressionClassification ClassifySum(
        ExpressionClassification left,
        ExpressionClassification right,
        Variable variable)
    {
        // Sum is dominated by the higher-order term
        var leftOrder = GetAsymptoticOrder(left);
        var rightOrder = GetAsymptoticOrder(right);

        var dominant = leftOrder >= rightOrder ? left : right;
        return dominant with { Confidence = Math.Min(left.Confidence, right.Confidence) * 0.95 };
    }

    private ExpressionClassification ClassifyProduct(
        ExpressionClassification left,
        ExpressionClassification right,
        Variable variable)
    {
        // Product: n^a × n^b = n^(a+b), log^a × log^b = log^(a+b)
        // n^a × log^b = polylog

        // Both constants
        if (left.Form == ExpressionForm.Constant && right.Form == ExpressionForm.Constant)
        {
            return new ExpressionClassification
            {
                Form = ExpressionForm.Constant,
                Variable = variable,
                Coefficient = (left.Coefficient) * (right.Coefficient)
            };
        }

        // Constant × anything = scale
        if (left.Form == ExpressionForm.Constant)
            return right with { Coefficient = (left.Coefficient) * right.Coefficient };
        if (right.Form == ExpressionForm.Constant)
            return left with { Coefficient = left.Coefficient * (right.Coefficient) };

        // Polynomial × Polynomial
        if (left.Form == ExpressionForm.Polynomial && right.Form == ExpressionForm.Polynomial)
        {
            return new ExpressionClassification
            {
                Form = ExpressionForm.Polynomial,
                Variable = variable,
                PrimaryParameter = (left.PrimaryParameter ?? 0) + (right.PrimaryParameter ?? 0),
                Coefficient = left.Coefficient * right.Coefficient,
                Confidence = Math.Min(left.Confidence, right.Confidence)
            };
        }

        // Polynomial × Logarithmic = PolyLog
        if ((left.Form == ExpressionForm.Polynomial && right.Form == ExpressionForm.Logarithmic) ||
            (left.Form == ExpressionForm.Logarithmic && right.Form == ExpressionForm.Polynomial))
        {
            var poly = left.Form == ExpressionForm.Polynomial ? left : right;
            var log = left.Form == ExpressionForm.Logarithmic ? left : right;

            return new ExpressionClassification
            {
                Form = ExpressionForm.PolyLog,
                Variable = variable,
                PrimaryParameter = poly.PrimaryParameter,
                LogExponent = log.LogExponent ?? 1,
                Coefficient = left.Coefficient * right.Coefficient,
                Confidence = Math.Min(left.Confidence, right.Confidence)
            };
        }

        // PolyLog × PolyLog
        if (left.Form == ExpressionForm.PolyLog && right.Form == ExpressionForm.PolyLog)
        {
            return new ExpressionClassification
            {
                Form = ExpressionForm.PolyLog,
                Variable = variable,
                PrimaryParameter = (left.PrimaryParameter ?? 0) + (right.PrimaryParameter ?? 0),
                LogExponent = (left.LogExponent ?? 0) + (right.LogExponent ?? 0),
                Coefficient = left.Coefficient * right.Coefficient,
                Confidence = Math.Min(left.Confidence, right.Confidence) * 0.9
            };
        }

        // Default: unknown product
        return new ExpressionClassification
        {
            Form = ExpressionForm.Unknown,
            Variable = variable,
            Confidence = 0.5
        };
    }

    private ExpressionClassification ClassifyMax(
        ExpressionClassification left,
        ExpressionClassification right,
        Variable variable)
    {
        // Max is the higher-order term
        var leftOrder = GetAsymptoticOrder(left);
        var rightOrder = GetAsymptoticOrder(right);

        var dominant = leftOrder >= rightOrder ? left : right;
        return dominant with { Confidence = Math.Min(left.Confidence, right.Confidence) };
    }

    private ExpressionClassification ClassifyPower(PowerComplexity pow, Variable variable)
    {
        var baseClass = Classify(pow.Base, variable);

        if (baseClass.Form == ExpressionForm.Polynomial)
        {
            return new ExpressionClassification
            {
                Form = ExpressionForm.Polynomial,
                Variable = variable,
                PrimaryParameter = (baseClass.PrimaryParameter ?? 1) * pow.Exponent,
                Coefficient = Math.Pow(baseClass.Coefficient, pow.Exponent),
                Confidence = baseClass.Confidence * 0.95
            };
        }

        return new ExpressionClassification
        {
            Form = ExpressionForm.Unknown,
            Variable = variable,
            Confidence = 0.5
        };
    }

    private double GetAsymptoticOrder(ExpressionClassification c) => c.Form switch
    {
        ExpressionForm.Constant => 0,
        ExpressionForm.Logarithmic => 0.5 + (c.LogExponent ?? 1) * 0.01, // log < any polynomial
        ExpressionForm.Polynomial => c.PrimaryParameter ?? 1,
        ExpressionForm.PolyLog => (c.PrimaryParameter ?? 1) + (c.LogExponent ?? 0) * 0.01,
        ExpressionForm.Exponential => 1000 + (c.PrimaryParameter ?? 2),
        ExpressionForm.Factorial => 10000,
        _ => 100 // Unknown - assume fairly high
    };

    public bool TryExtractPolynomialDegree(ComplexityExpression expr, Variable variable, out double degree)
    {
        var classification = Classify(expr, variable);
        if (classification.Form == ExpressionForm.Polynomial && classification.PrimaryParameter.HasValue)
        {
            degree = classification.PrimaryParameter.Value;
            return true;
        }
        degree = 0;
        return false;
    }

    public bool TryExtractPolyLogForm(
        ComplexityExpression expr,
        Variable variable,
        out double polyDegree,
        out double logExponent)
    {
        var classification = Classify(expr, variable);

        if (classification.Form is ExpressionForm.Polynomial or ExpressionForm.PolyLog or ExpressionForm.Logarithmic)
        {
            polyDegree = classification.PrimaryParameter ?? 0;
            logExponent = classification.LogExponent ?? 0;
            return true;
        }

        polyDegree = 0;
        logExponent = 0;
        return false;
    }

    public bool IsBoundedByPolynomial(ComplexityExpression expr, Variable variable, double degree)
    {
        var classification = Classify(expr, variable);

        return classification.Form switch
        {
            ExpressionForm.Constant => true,
            ExpressionForm.Logarithmic => true, // log n ∈ O(n^ε) for any ε > 0
            ExpressionForm.Polynomial => (classification.PrimaryParameter ?? 0) <= degree,
            ExpressionForm.PolyLog => (classification.PrimaryParameter ?? 0) < degree, // n^k log n ∈ O(n^(k+ε))
            ExpressionForm.Exponential => false,
            ExpressionForm.Factorial => false,
            _ => false // Conservative
        };
    }

    public bool DominatesPolynomial(ComplexityExpression expr, Variable variable, double degree)
    {
        var classification = Classify(expr, variable);

        return classification.Form switch
        {
            ExpressionForm.Constant => degree < 0, // Only if looking for o(1)
            ExpressionForm.Logarithmic => false, // log n ∈ o(n^ε)
            ExpressionForm.Polynomial => (classification.PrimaryParameter ?? 0) > degree,
            ExpressionForm.PolyLog => (classification.PrimaryParameter ?? 0) > degree, // Dominates by poly part
            ExpressionForm.Exponential => true,
            ExpressionForm.Factorial => true,
            _ => false // Conservative
        };
    }
}
