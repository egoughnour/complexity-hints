using ComplexityAnalysis.Core.Complexity;
using Xunit;

namespace ComplexityAnalysis.Tests.Core;

/// <summary>
/// Tests for expression classification.
/// </summary>
public class ExpressionClassifierTests
{
    private readonly IExpressionClassifier _classifier = StandardExpressionClassifier.Instance;

    #region Basic Form Classification

    [Fact]
    public void Classify_Constant_ReturnsConstantForm()
    {
        var expr = new ConstantComplexity(42);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Constant, result.Form);
        Assert.Equal(0, result.PrimaryParameter);
        Assert.Equal(42, result.Coefficient);
    }

    [Fact]
    public void Classify_Variable_ReturnsPolynomialDegree1()
    {
        var expr = new VariableComplexity(Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(1.0, result.PrimaryParameter);
    }

    [Fact]
    public void Classify_Linear_ReturnsPolynomialDegree1()
    {
        var expr = new LinearComplexity(5.0, Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(1.0, result.PrimaryParameter);
        Assert.Equal(5.0, result.Coefficient);
    }

    [Fact]
    public void Classify_Polynomial_ReturnsCorrectDegree()
    {
        var expr = PolyLogComplexity.Polynomial(3.5, Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(3.5, result.PrimaryParameter);
    }

    [Fact]
    public void Classify_Logarithmic_ReturnsLogarithmicForm()
    {
        var expr = new LogarithmicComplexity(2.0, Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Logarithmic, result.Form);
        Assert.Equal(0, result.PrimaryParameter);
        Assert.Equal(1, result.LogExponent);
    }

    [Fact]
    public void Classify_PolyLog_ReturnsPolyLogForm()
    {
        var expr = PolyLogComplexity.NLogN(Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.PolyLog, result.Form);
        Assert.Equal(1.0, result.PrimaryParameter);
        Assert.Equal(1.0, result.LogExponent);
    }

    [Fact]
    public void Classify_Exponential_ReturnsExponentialForm()
    {
        var expr = new ExponentialComplexity(2.0, Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Exponential, result.Form);
        Assert.Equal(2.0, result.PrimaryParameter);
    }

    [Fact]
    public void Classify_Factorial_ReturnsFactorialForm()
    {
        var expr = new FactorialComplexity(Variable.N);

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Factorial, result.Form);
    }

    #endregion

    #region Composite Expression Classification

    [Fact]
    public void Classify_Sum_ReturnsDominantForm()
    {
        // n² + n → n² dominates
        var expr = new BinaryOperationComplexity(
            PolyLogComplexity.Polynomial(2, Variable.N),
            BinaryOp.Plus,
            new LinearComplexity(1.0, Variable.N));

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(2.0, result.PrimaryParameter);
    }

    [Fact]
    public void Classify_Product_AddsExponents()
    {
        // n × n = n²
        var expr = new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Multiply,
            new LinearComplexity(1.0, Variable.N));

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(2.0, result.PrimaryParameter);
    }

    [Fact]
    public void Classify_PolynomialTimesLog_ReturnsPolyLog()
    {
        // n × log(n)
        var expr = new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Multiply,
            new LogarithmicComplexity(1.0, Variable.N));

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.PolyLog, result.Form);
        Assert.Equal(1.0, result.PrimaryParameter);
        Assert.Equal(1.0, result.LogExponent);
    }

    [Fact]
    public void Classify_Max_ReturnsDominant()
    {
        // max(n, n²) → n²
        var expr = new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Max,
            PolyLogComplexity.Polynomial(2, Variable.N));

        var result = _classifier.Classify(expr, Variable.N);

        Assert.Equal(ExpressionForm.Polynomial, result.Form);
        Assert.Equal(2.0, result.PrimaryParameter);
    }

    #endregion

    #region Extraction Methods

    [Fact]
    public void TryExtractPolynomialDegree_FromPolynomial_Succeeds()
    {
        var expr = PolyLogComplexity.Polynomial(2.5, Variable.N);

        var success = _classifier.TryExtractPolynomialDegree(expr, Variable.N, out var degree);

        Assert.True(success);
        Assert.Equal(2.5, degree);
    }

    [Fact]
    public void TryExtractPolynomialDegree_FromNonPolynomial_Fails()
    {
        var expr = new ExponentialComplexity(2.0, Variable.N);

        var success = _classifier.TryExtractPolynomialDegree(expr, Variable.N, out _);

        Assert.False(success);
    }

    [Fact]
    public void TryExtractPolyLogForm_FromPolyLog_Succeeds()
    {
        var expr = new PolyLogComplexity(2, 3, Variable.N);

        var success = _classifier.TryExtractPolyLogForm(expr, Variable.N, out var polyDegree, out var logExponent);

        Assert.True(success);
        Assert.Equal(2.0, polyDegree);
        Assert.Equal(3.0, logExponent);
    }

    [Fact]
    public void TryExtractPolyLogForm_FromPolynomial_ReturnsDegreeWithZeroLog()
    {
        var expr = PolyLogComplexity.Polynomial(2.5, Variable.N);

        var success = _classifier.TryExtractPolyLogForm(expr, Variable.N, out var polyDegree, out var logExponent);

        Assert.True(success);
        Assert.Equal(2.5, polyDegree);
        Assert.Equal(0.0, logExponent);
    }

    #endregion

    #region Comparison Methods

    [Fact]
    public void IsBoundedByPolynomial_Linear_BoundedByQuadratic()
    {
        var expr = new LinearComplexity(1.0, Variable.N);

        Assert.True(_classifier.IsBoundedByPolynomial(expr, Variable.N, 2.0));
        Assert.True(_classifier.IsBoundedByPolynomial(expr, Variable.N, 1.0));
        Assert.False(_classifier.IsBoundedByPolynomial(expr, Variable.N, 0.5));
    }

    [Fact]
    public void IsBoundedByPolynomial_Logarithmic_BoundedByAnyPolynomial()
    {
        var expr = new LogarithmicComplexity(1.0, Variable.N);

        // log(n) ∈ O(n^ε) for any ε > 0
        Assert.True(_classifier.IsBoundedByPolynomial(expr, Variable.N, 0.001));
        Assert.True(_classifier.IsBoundedByPolynomial(expr, Variable.N, 1.0));
    }

    [Fact]
    public void IsBoundedByPolynomial_Exponential_NeverBounded()
    {
        var expr = new ExponentialComplexity(2.0, Variable.N);

        Assert.False(_classifier.IsBoundedByPolynomial(expr, Variable.N, 100));
    }

    [Fact]
    public void DominatesPolynomial_Quadratic_DominatesLinear()
    {
        var expr = PolyLogComplexity.Polynomial(2, Variable.N);

        Assert.True(_classifier.DominatesPolynomial(expr, Variable.N, 1.0));
        Assert.True(_classifier.DominatesPolynomial(expr, Variable.N, 1.5));
        Assert.False(_classifier.DominatesPolynomial(expr, Variable.N, 2.0));
        Assert.False(_classifier.DominatesPolynomial(expr, Variable.N, 3.0));
    }

    [Fact]
    public void DominatesPolynomial_Exponential_DominatesAllPolynomials()
    {
        var expr = new ExponentialComplexity(2.0, Variable.N);

        Assert.True(_classifier.DominatesPolynomial(expr, Variable.N, 0));
        Assert.True(_classifier.DominatesPolynomial(expr, Variable.N, 100));
        Assert.True(_classifier.DominatesPolynomial(expr, Variable.N, 1000));
    }

    #endregion

    #region PolyLogComplexity Helper Classification

    [Fact]
    public void PolyLogComplexity_IsPurePolynomial_WhenLogExponentIsZero()
    {
        var poly = PolyLogComplexity.Polynomial(2, Variable.N);

        Assert.True(poly.IsPurePolynomial);
        Assert.False(poly.IsPureLogarithmic);
        Assert.False(poly.IsNLogN);
    }

    [Fact]
    public void PolyLogComplexity_IsPureLogarithmic_WhenPolyDegreeIsZero()
    {
        var log = PolyLogComplexity.LogPower(2, Variable.N);

        Assert.True(log.IsPureLogarithmic);
        Assert.False(log.IsPurePolynomial);
        Assert.False(log.IsNLogN);
    }

    [Fact]
    public void PolyLogComplexity_IsNLogN_ForNLogN()
    {
        var nlogn = PolyLogComplexity.NLogN(Variable.N);

        Assert.True(nlogn.IsNLogN);
        Assert.False(nlogn.IsPurePolynomial);
        Assert.False(nlogn.IsPureLogarithmic);
    }

    #endregion
}
