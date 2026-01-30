using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Result of evaluating the Akra-Bazzi integral.
/// </summary>
public sealed record IntegralEvaluationResult
{
    /// <summary>Whether the integral could be evaluated (closed-form or symbolic).</summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The asymptotic form of the integral term.
    /// For Akra-Bazzi: Θ(n^p · (1 + ∫₁ⁿ g(u)/u^(p+1) du))
    /// This captures the integral contribution.
    /// </summary>
    public ComplexityExpression? IntegralTerm { get; init; }

    /// <summary>
    /// The complete Akra-Bazzi solution combining n^p with the integral.
    /// </summary>
    public ComplexityExpression? FullSolution { get; init; }

    /// <summary>Human-readable explanation of the evaluation.</summary>
    public string? Explanation { get; init; }

    /// <summary>Confidence in the result (0.0 to 1.0).</summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>Whether the result is symbolic (requires further refinement).</summary>
    public bool IsSymbolic { get; init; } = false;

    /// <summary>Special function type if applicable.</summary>
    public SpecialFunctionType? SpecialFunction { get; init; }

    public static IntegralEvaluationResult Evaluated(
        ComplexityExpression integralTerm,
        ComplexityExpression fullSolution,
        string explanation,
        double confidence = 1.0) =>
        new()
        {
            Success = true,
            IntegralTerm = integralTerm,
            FullSolution = fullSolution,
            Explanation = explanation,
            Confidence = confidence
        };

    public static IntegralEvaluationResult Symbolic(
        ComplexityExpression integralTerm,
        ComplexityExpression fullSolution,
        string explanation,
        SpecialFunctionType specialFunction,
        double confidence = 0.8) =>
        new()
        {
            Success = true,
            IntegralTerm = integralTerm,
            FullSolution = fullSolution,
            Explanation = explanation,
            Confidence = confidence,
            IsSymbolic = true,
            SpecialFunction = specialFunction
        };

    public static IntegralEvaluationResult Failed(string explanation) =>
        new()
        {
            Success = false,
            Explanation = explanation,
            Confidence = 0.0
        };
}

/// <summary>
/// Types of special functions that may arise in integral evaluation.
/// </summary>
public enum SpecialFunctionType
{
    /// <summary>Polylogarithm Li_s(z)</summary>
    Polylogarithm,

    /// <summary>Incomplete gamma function γ(s, x)</summary>
    IncompleteGamma,

    /// <summary>Incomplete beta function B(x; a, b)</summary>
    IncompleteBeta,

    /// <summary>Gauss hypergeometric ₂F₁(a, b; c; z)</summary>
    Hypergeometric2F1,

    /// <summary>Deferred symbolic integral</summary>
    SymbolicIntegral
}

/// <summary>
/// Evaluates the integral term in the Akra-Bazzi theorem.
///
/// Akra-Bazzi solution: T(n) = Θ(n^p · (1 + ∫₁ⁿ g(u)/u^(p+1) du))
///
/// For common g(u) forms, this integral has closed-form solutions:
///
/// | g(n)           | k vs p    | Integral Result          | Full Solution        |
/// |----------------|-----------|--------------------------|----------------------|
/// | n^k            | k &lt; p     | O(1)                     | Θ(n^p)               |
/// | n^k            | k = p     | O(log n)                 | Θ(n^p · log n)       |
/// | n^k            | k > p     | O(n^(k-p))               | Θ(n^k)               |
/// | n^k · log^j n  | k &lt; p     | O(1)                     | Θ(n^p)               |
/// | n^k · log^j n  | k = p     | O(log^(j+1) n)           | Θ(n^p · log^(j+1) n) |
/// | n^k · log^j n  | k > p     | O(n^(k-p) · log^j n)     | Θ(n^k · log^j n)     |
/// </summary>
public interface IAkraBazziIntegralEvaluator
{
    /// <summary>
    /// Evaluates the Akra-Bazzi integral for the given g(n) and critical exponent p.
    /// </summary>
    /// <param name="g">The non-recursive work function g(n).</param>
    /// <param name="variable">The variable (typically n).</param>
    /// <param name="p">The critical exponent satisfying Σᵢ aᵢ · bᵢ^p = 1.</param>
    /// <returns>The evaluation result with the full solution.</returns>
    IntegralEvaluationResult Evaluate(
        ComplexityExpression g,
        Variable variable,
        double p);
}

/// <summary>
/// Table-driven implementation for common integral forms with special function fallback.
///
/// Handles standard cases with closed forms and falls back to special functions
/// (hypergeometric, polylogarithm, gamma, beta) or symbolic integrals for
/// complex cases that require later refinement.
/// </summary>
public sealed class TableDrivenIntegralEvaluator : IAkraBazziIntegralEvaluator
{
    private readonly IExpressionClassifier _classifier;

    /// <summary>Tolerance for comparing k to p.</summary>
    private const double Tolerance = 1e-9;

    public TableDrivenIntegralEvaluator(IExpressionClassifier? classifier = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
    }

    public static TableDrivenIntegralEvaluator Instance { get; } = new();

    public IntegralEvaluationResult Evaluate(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        var classification = _classifier.Classify(g, variable);

        return classification.Form switch
        {
            ExpressionForm.Constant => EvaluateConstant(variable, p),
            ExpressionForm.Polynomial => EvaluatePolynomial(classification, variable, p),
            ExpressionForm.Logarithmic => EvaluateLogarithmic(classification, variable, p),
            ExpressionForm.PolyLog => EvaluatePolyLog(classification, variable, p),
            ExpressionForm.Exponential => EvaluateExponential(classification, g, variable, p),
            ExpressionForm.Unknown => EvaluateGeneric(g, variable, p),
            _ => EvaluateGeneric(g, variable, p)
        };
    }

    /// <summary>
    /// g(n) = c · b^n (exponential)
    /// ∫₁ⁿ b^u / u^(p+1) du
    ///
    /// This integral relates to the incomplete gamma function when transformed.
    /// For b > 1 and large n, the exponential dominates.
    /// </summary>
    private IntegralEvaluationResult EvaluateExponential(
        ExpressionClassification classification,
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        var b = classification.PrimaryParameter ?? 2;

        if (b <= 1)
        {
            // Decaying exponential - integral converges
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                PolyLogComplexity.Polynomial(p, variable),
                $"g(n) = O({b}^n), b ≤ 1: integral converges, solution is Θ(n^{p:F2})",
                confidence: 0.9);
        }

        // Growing exponential - use incomplete gamma representation
        // ∫₁ⁿ e^(u ln b) / u^(p+1) du relates to Γ(−p, −n ln b)
        var lnb = Math.Log(b);

        // Asymptotically, for large n: ∫ b^u / u^(p+1) du ~ b^n / (n^(p+1) · ln b)
        // So the integral grows as b^n / n^(p+1)
        // Full solution: n^p · b^n / n^(p+1) = b^n / n
        var integralTerm = new IncompleteGammaComplexity(
            -p,
            new BinaryOperationComplexity(
                new ConstantComplexity(-lnb),
                BinaryOp.Multiply,
                new VariableComplexity(variable)),
            variable);

        // Asymptotic bound: exponential dominates
        var asymptoticBound = new ExponentialComplexity(b, variable);

        return IntegralEvaluationResult.Symbolic(
            integralTerm,
            asymptoticBound,
            $"g(n) = O({b}^n): integral involves incomplete gamma, asymptotically Θ({b}^n)",
            SpecialFunctionType.IncompleteGamma,
            confidence: 0.85);
    }

    /// <summary>
    /// Generic fallback for unrecognized g(n) forms.
    /// Creates a symbolic integral with asymptotic bounds estimated heuristically.
    /// </summary>
    private IntegralEvaluationResult EvaluateGeneric(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        // Try to determine if g dominates or is dominated by n^p

        // Create the integrand g(u) / u^(p+1)
        var u = new Variable("u", VariableType.InputSize);
        var integrand = new BinaryOperationComplexity(
            g.Substitute(variable, new VariableComplexity(u)),
            BinaryOp.Multiply,
            new PowerComplexity(new VariableComplexity(u), -(p + 1)));

        // Symbolic integral with n^p as conservative bound
        var symbolicIntegral = SymbolicIntegralComplexity.WithBound(
            integrand,
            u,
            new ConstantComplexity(1),
            new VariableComplexity(variable),
            PolyLogComplexity.Polynomial(Math.Max(p, 0), variable));

        // For the full solution, we conservatively use max(n^p, g(n))
        var fullSolution = new BinaryOperationComplexity(
            PolyLogComplexity.Polynomial(p, variable),
            BinaryOp.Max,
            g);

        return IntegralEvaluationResult.Symbolic(
            symbolicIntegral,
            fullSolution,
            $"g(n) has unknown form: symbolic integral created for potential refinement. " +
            $"Conservative bound: max(n^{p:F2}, g(n))",
            SpecialFunctionType.SymbolicIntegral,
            confidence: 0.5);
    }

    /// <summary>
    /// g(n) = c (constant)
    /// ∫₁ⁿ c/u^(p+1) du = c · [-1/(p·u^p)]₁ⁿ = c/p · (1 - 1/n^p)
    ///
    /// For p > 0: this is O(1), so solution is Θ(n^p)
    /// For p = 0: ∫₁ⁿ c/u du = c · log(n), so solution is Θ(log n)
    /// For p &lt; 0: this grows, dominated by n^(-p) term
    /// </summary>
    private IntegralEvaluationResult EvaluateConstant(Variable variable, double p)
    {
        if (Math.Abs(p) < Tolerance)
        {
            // p ≈ 0: ∫ c/u du = c·log(n)
            var integralTerm = new LogarithmicComplexity(1.0, variable);
            return IntegralEvaluationResult.Evaluated(
                integralTerm,
                integralTerm,
                "g(n) = O(1), p ≈ 0: integral gives O(log n)",
                confidence: 1.0);
        }

        if (p > 0)
        {
            // Integral converges to O(1), solution is Θ(n^p)
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                PolyLogComplexity.Polynomial(p, variable),
                $"g(n) = O(1), p = {p:F4}: integral converges, solution is Θ(n^{p:F2})",
                confidence: 1.0);
        }

        // p < 0 is unusual for standard recurrences
        return IntegralEvaluationResult.Failed(
            $"Unusual case: p = {p:F4} < 0 for constant g(n)");
    }

    /// <summary>
    /// g(n) = n^k (pure polynomial)
    /// ∫₁ⁿ u^k/u^(p+1) du = ∫₁ⁿ u^(k-p-1) du
    ///
    /// If k - p - 1 = -1 (i.e., k = p): ∫ du/u = log(n)
    /// If k - p - 1 ≠ -1: [u^(k-p)/(k-p)]₁ⁿ = (n^(k-p) - 1)/(k-p)
    ///   - If k &lt; p: this is O(1)
    ///   - If k > p: this is O(n^(k-p))
    /// </summary>
    private IntegralEvaluationResult EvaluatePolynomial(
        ExpressionClassification classification,
        Variable variable,
        double p)
    {
        var k = classification.PrimaryParameter ?? 0;
        var diff = k - p;

        if (Math.Abs(diff) < Tolerance)
        {
            // k ≈ p: integral gives log(n)
            // Full solution: n^p · log(n)
            var solution = PolyLogComplexity.PolyTimesLog(p, variable);
            return IntegralEvaluationResult.Evaluated(
                new LogarithmicComplexity(1.0, variable),
                solution,
                $"g(n) = Θ(n^{k:F2}), k = p: integral gives Θ(log n), solution is Θ(n^{p:F2} · log n)",
                confidence: 1.0);
        }

        if (diff < 0)
        {
            // k < p: integral converges to O(1)
            // Full solution: n^p
            var solution = PolyLogComplexity.Polynomial(p, variable);
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                solution,
                $"g(n) = Θ(n^{k:F2}), k < p: integral converges, solution is Θ(n^{p:F2})",
                confidence: 1.0);
        }

        // k > p: integral grows as n^(k-p)
        // n^p · n^(k-p) = n^k
        // Full solution: n^k (work at root dominates)
        var fullSolution = PolyLogComplexity.Polynomial(k, variable);
        return IntegralEvaluationResult.Evaluated(
            PolyLogComplexity.Polynomial(diff, variable),
            fullSolution,
            $"g(n) = Θ(n^{k:F2}), k > p: integral gives Θ(n^{diff:F2}), solution is Θ(n^{k:F2})",
            confidence: 1.0);
    }

    /// <summary>
    /// g(n) = log^j(n) (pure logarithmic, k = 0)
    /// This is a special case of polylog with k = 0.
    /// </summary>
    private IntegralEvaluationResult EvaluateLogarithmic(
        ExpressionClassification classification,
        Variable variable,
        double p)
    {
        var j = classification.LogExponent ?? 1;

        // g(n) = log^j(n) is like n^0 · log^j(n)
        // Since k = 0 < p (for typical recurrences), integral converges
        // But the log factor adds complexity

        if (p > Tolerance)
        {
            // p > 0: ∫₁ⁿ log^j(u)/u^(p+1) du converges
            // The solution is dominated by n^p
            var solution = PolyLogComplexity.Polynomial(p, variable);
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                solution,
                $"g(n) = Θ(log^{j} n), p = {p:F4} > 0: integral converges, solution is Θ(n^{p:F2})",
                confidence: 0.95);
        }

        if (Math.Abs(p) < Tolerance)
        {
            // p ≈ 0: ∫₁ⁿ log^j(u)/u du
            // Let w = log(u), dw = du/u
            // = ∫₀^log(n) w^j dw = [w^(j+1)/(j+1)]₀^log(n) = log^(j+1)(n)/(j+1)
            var solution = PolyLogComplexity.LogPower(j + 1, variable);
            return IntegralEvaluationResult.Evaluated(
                PolyLogComplexity.LogPower(j + 1, variable),
                solution,
                $"g(n) = Θ(log^{j} n), p ≈ 0: integral gives Θ(log^{j + 1} n)",
                confidence: 1.0);
        }

        return IntegralEvaluationResult.Failed(
            $"Unusual case: p = {p:F4} < 0 for logarithmic g(n)");
    }

    /// <summary>
    /// g(n) = n^k · log^j(n) (polylogarithmic)
    /// ∫₁ⁿ u^k · log^j(u) / u^(p+1) du = ∫₁ⁿ u^(k-p-1) · log^j(u) du
    ///
    /// Case k = p: ∫ log^j(u)/u du = log^(j+1)(n)/(j+1)
    /// Case k &lt; p: Integral converges to O(1)
    /// Case k > p: Integral ~ n^(k-p) · log^j(n)
    /// </summary>
    private IntegralEvaluationResult EvaluatePolyLog(
        ExpressionClassification classification,
        Variable variable,
        double p)
    {
        var k = classification.PrimaryParameter ?? 0;
        var j = classification.LogExponent ?? 0;
        var diff = k - p;

        if (Math.Abs(diff) < Tolerance)
        {
            // k ≈ p: ∫ log^j(u)/u du = log^(j+1)(n)/(j+1)
            // Full solution: n^p · log^(j+1)(n)
            var solution = new PolyLogComplexity(p, j + 1, variable);
            return IntegralEvaluationResult.Evaluated(
                PolyLogComplexity.LogPower(j + 1, variable),
                solution,
                $"g(n) = Θ(n^{k:F2} · log^{j} n), k = p: integral gives Θ(log^{j + 1} n), " +
                $"solution is Θ(n^{p:F2} · log^{j + 1} n)",
                confidence: 1.0);
        }

        if (diff < 0)
        {
            // k < p: integral converges
            // Full solution: n^p
            var solution = PolyLogComplexity.Polynomial(p, variable);
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                solution,
                $"g(n) = Θ(n^{k:F2} · log^{j} n), k < p: integral converges, solution is Θ(n^{p:F2})",
                confidence: 1.0);
        }

        // k > p: integral grows as n^(k-p) · log^j(n)
        // n^p · n^(k-p) · log^j(n) = n^k · log^j(n)
        var fullSolution = new PolyLogComplexity(k, j, variable);
        return IntegralEvaluationResult.Evaluated(
            new PolyLogComplexity(diff, j, variable),
            fullSolution,
            $"g(n) = Θ(n^{k:F2} · log^{j} n), k > p: integral gives Θ(n^{diff:F2} · log^{j} n), " +
            $"solution is Θ(n^{k:F2} · log^{j} n)",
            confidence: 1.0);
    }
}

/// <summary>
/// Extended integral evaluator with hypergeometric and special function support.
///
/// This evaluator handles more complex g(n) forms that require special functions:
/// - Fractional polynomial exponents → Hypergeometric ₂F₁
/// - Products/ratios of polynomials → Beta functions
/// - Exponential-polynomial products → Incomplete gamma
/// - Iterated logarithms → Polylogarithm
/// </summary>
public sealed class ExtendedIntegralEvaluator : IAkraBazziIntegralEvaluator
{
    private readonly TableDrivenIntegralEvaluator _basicEvaluator;
    private readonly IExpressionClassifier _classifier;

    private const double Tolerance = 1e-9;

    public ExtendedIntegralEvaluator(IExpressionClassifier? classifier = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
        _basicEvaluator = new TableDrivenIntegralEvaluator(_classifier);
    }

    public static ExtendedIntegralEvaluator Instance { get; } = new();

    public IntegralEvaluationResult Evaluate(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        // Try basic evaluation first
        var basicResult = _basicEvaluator.Evaluate(g, variable, p);
        if (basicResult.Success && !basicResult.IsSymbolic)
            return basicResult;

        // Try specialized handlers
        return TryFractionalPolynomial(g, variable, p)
            ?? TryPolynomialRatio(g, variable, p)
            ?? TryIteratedLogarithm(g, variable, p)
            ?? TryProductForm(g, variable, p)
            ?? CreateSymbolicWithHeuristic(g, variable, p);
    }

    /// <summary>
    /// g(n) = n^k where k is not an integer (fractional exponents).
    /// ∫₁ⁿ u^(k-p-1) du = [u^(k-p) / (k-p)]₁ⁿ when k ≠ p
    ///
    /// Still elementary, but we ensure numerical stability for non-integer exponents.
    /// </summary>
    private IntegralEvaluationResult? TryFractionalPolynomial(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        if (g is not PowerComplexity power) return null;
        if (power.Base is not VariableComplexity vc || !vc.Var.Equals(variable)) return null;

        var k = power.Exponent;
        var diff = k - p;

        // Standard polynomial handling applies even for fractional k
        if (Math.Abs(diff) < Tolerance)
        {
            var solution = PolyLogComplexity.PolyTimesLog(p, variable);
            return IntegralEvaluationResult.Evaluated(
                new LogarithmicComplexity(1.0, variable),
                solution,
                $"g(n) = n^{k:F4} (fractional), k ≈ p: integral gives Θ(log n)",
                confidence: 1.0);
        }

        if (diff < 0)
        {
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                PolyLogComplexity.Polynomial(p, variable),
                $"g(n) = n^{k:F4} (fractional), k < p: integral converges",
                confidence: 1.0);
        }

        return IntegralEvaluationResult.Evaluated(
            PolyLogComplexity.Polynomial(diff, variable),
            PolyLogComplexity.Polynomial(k, variable),
            $"g(n) = n^{k:F4} (fractional), k > p: solution is Θ(n^{k:F4})",
            confidence: 1.0);
    }

    /// <summary>
    /// g(n) = n^a / (1 + n^b)^c - polynomial ratio forms
    /// These lead to Beta/hypergeometric functions via substitution u = n^b / (1 + n^b)
    /// </summary>
    private IntegralEvaluationResult? TryPolynomialRatio(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        // Detect pattern: poly / poly^c
        if (g is not BinaryOperationComplexity binOp) return null;
        if (binOp.Operation != BinaryOp.Multiply) return null;

        // Check for a / b = a * b^(-1) pattern
        var leftClass = _classifier.Classify(binOp.Left, variable);
        var rightClass = _classifier.Classify(binOp.Right, variable);

        if (leftClass.Form == ExpressionForm.Polynomial && rightClass.Form == ExpressionForm.Polynomial)
        {
            var a = leftClass.PrimaryParameter ?? 0;
            var b = rightClass.PrimaryParameter ?? 0;

            // If right has negative exponent (division), this is a ratio
            if (b < 0)
            {
                // Effective exponent of ratio
                var effectiveK = a + b;
                var diff = effectiveK - p;

                if (Math.Abs(diff) < Tolerance)
                {
                    // The ratio evaluates similarly to polylog
                    return IntegralEvaluationResult.Evaluated(
                        new LogarithmicComplexity(1.0, variable),
                        PolyLogComplexity.PolyTimesLog(p, variable),
                        $"g(n) = n^{a:F2}/n^{-b:F2}, effective k ≈ p: Θ(n^p log n)",
                        confidence: 0.95);
                }

                // Standard polynomial result with effective exponent
                var solution = diff > 0
                    ? PolyLogComplexity.Polynomial(effectiveK, variable)
                    : PolyLogComplexity.Polynomial(p, variable);

                return IntegralEvaluationResult.Evaluated(
                    diff > 0 ? PolyLogComplexity.Polynomial(diff, variable) : new ConstantComplexity(1.0),
                    solution,
                    $"g(n) = n^{a:F2}/n^{-b:F2} = n^{effectiveK:F2}",
                    confidence: 0.9);
            }
        }

        return null;
    }

    /// <summary>
    /// g(n) = log(log(n))^j - iterated logarithms
    /// These arise in algorithms with deep recursive structures.
    /// Integral: ∫ log(log(u))^j / u^(p+1) du involves polylogarithms.
    /// </summary>
    private IntegralEvaluationResult? TryIteratedLogarithm(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        // Detect log(log(n)) pattern
        if (g is not LogOfComplexity outerLog) return null;
        if (outerLog.Argument is not LogOfComplexity innerLog) return null;
        if (innerLog.Argument is not VariableComplexity vc || !vc.Var.Equals(variable)) return null;

        // log(log(n)) grows extremely slowly
        // For p > 0, the integral converges
        if (p > Tolerance)
        {
            return IntegralEvaluationResult.Evaluated(
                new ConstantComplexity(1.0),
                PolyLogComplexity.Polynomial(p, variable),
                $"g(n) = log(log(n)): extremely slow growth, integral converges for p = {p:F4}",
                confidence: 0.95);
        }

        // p ≈ 0: integral ~ log(log(n)) · log(n) (iterated log integration)
        if (Math.Abs(p) < Tolerance)
        {
            // Asymptotic: ∫ log(log(u))/u du ≈ log(n) · log(log(n)) - li(log(n))
            // where li is the logarithmic integral
            var polylogTerm = new PolylogarithmComplexity(
                1.0,
                new LogarithmicComplexity(1.0, variable),
                variable);

            return IntegralEvaluationResult.Symbolic(
                polylogTerm,
                new BinaryOperationComplexity(
                    new LogarithmicComplexity(1.0, variable),
                    BinaryOp.Multiply,
                    outerLog),
                $"g(n) = log(log(n)), p ≈ 0: involves Li₁(log(n))",
                SpecialFunctionType.Polylogarithm,
                confidence: 0.7);
        }

        return null;
    }

    /// <summary>
    /// g(n) = f₁(n) · f₂(n) - product forms
    /// Try to decompose and evaluate based on dominant factor.
    /// </summary>
    private IntegralEvaluationResult? TryProductForm(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        if (g is not BinaryOperationComplexity product || product.Operation != BinaryOp.Multiply)
            return null;

        var leftClass = _classifier.Classify(product.Left, variable);
        var rightClass = _classifier.Classify(product.Right, variable);

        // If one factor is constant, reduce to simpler form
        if (leftClass.Form == ExpressionForm.Constant)
        {
            return Evaluate(product.Right, variable, p);
        }
        if (rightClass.Form == ExpressionForm.Constant)
        {
            return Evaluate(product.Left, variable, p);
        }

        // Polynomial × Polynomial = higher degree polynomial
        if (leftClass.Form == ExpressionForm.Polynomial && rightClass.Form == ExpressionForm.Polynomial)
        {
            var k1 = leftClass.PrimaryParameter ?? 0;
            var k2 = rightClass.PrimaryParameter ?? 0;
            var k = k1 + k2;
            var diff = k - p;

            if (Math.Abs(diff) < Tolerance)
            {
                return IntegralEvaluationResult.Evaluated(
                    new LogarithmicComplexity(1.0, variable),
                    PolyLogComplexity.PolyTimesLog(p, variable),
                    $"g(n) = n^{k1:F2} · n^{k2:F2} = n^{k:F2}, k = p",
                    confidence: 1.0);
            }

            return IntegralEvaluationResult.Evaluated(
                diff > 0 ? PolyLogComplexity.Polynomial(diff, variable) : new ConstantComplexity(1.0),
                diff > 0 ? PolyLogComplexity.Polynomial(k, variable) : PolyLogComplexity.Polynomial(p, variable),
                $"g(n) = n^{k:F2}, product of polynomials",
                confidence: 1.0);
        }

        // Polynomial × Log = PolyLog (already handled by basic evaluator)
        // Fall through to symbolic

        return null;
    }

    /// <summary>
    /// Creates a symbolic integral with heuristic asymptotic bounds.
    /// </summary>
    private IntegralEvaluationResult CreateSymbolicWithHeuristic(
        ComplexityExpression g,
        Variable variable,
        double p)
    {
        var classification = _classifier.Classify(g, variable);

        // Estimate asymptotic behavior based on classification
        ComplexityExpression asymptoticBound = classification.Form switch
        {
            ExpressionForm.Factorial => new FactorialComplexity(variable),
            ExpressionForm.Exponential => new ExponentialComplexity(
                classification.PrimaryParameter ?? 2, variable),
            _ => PolyLogComplexity.Polynomial(Math.Max(p, classification.PrimaryParameter ?? 0), variable)
        };

        // Create the integrand
        var u = new Variable("u", VariableType.InputSize);
        var integrand = new BinaryOperationComplexity(
            g.Substitute(variable, new VariableComplexity(u)),
            BinaryOp.Multiply,
            new PowerComplexity(new VariableComplexity(u), -(p + 1)));

        var symbolicIntegral = SymbolicIntegralComplexity.WithBound(
            integrand,
            u,
            new ConstantComplexity(1),
            new VariableComplexity(variable),
            asymptoticBound);

        // Full solution is max of n^p and asymptotic bound of g
        var fullSolution = new BinaryOperationComplexity(
            PolyLogComplexity.Polynomial(p, variable),
            BinaryOp.Max,
            asymptoticBound);

        return IntegralEvaluationResult.Symbolic(
            symbolicIntegral,
            fullSolution,
            $"g(n) of form {classification.Form}: symbolic integral with heuristic bound. " +
            $"Asymptotic estimate: {asymptoticBound.ToBigONotation()}",
            SpecialFunctionType.SymbolicIntegral,
            confidence: 0.4);
    }
}
