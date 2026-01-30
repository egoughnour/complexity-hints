using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents special mathematical functions that arise in complexity analysis,
/// particularly from Akra-Bazzi integral evaluation.
///
/// These provide symbolic representations when closed-form elementary solutions
/// don't exist, enabling later refinement via numerical methods or CAS integration.
/// </summary>
public abstract record SpecialFunctionComplexity : ComplexityExpression
{
    /// <summary>
    /// Whether this function has a known asymptotic expansion.
    /// </summary>
    public abstract bool HasAsymptoticExpansion { get; }

    /// <summary>
    /// Gets the dominant asymptotic term, if known.
    /// </summary>
    public abstract ComplexityExpression? DominantTerm { get; }
}

/// <summary>
/// Polylogarithm Li_s(z) = Σₖ₌₁^∞ z^k / k^s
///
/// Arises when integrating log terms. For |z| ≤ 1:
/// - Li_1(z) = -ln(1-z)
/// - Li_0(z) = z/(1-z)
/// - Li_{-1}(z) = z/(1-z)²
///
/// For complexity analysis, we often have Li_s(1) = ζ(s) (Riemann zeta).
/// </summary>
public sealed record PolylogarithmComplexity(
    double Order,           // s in Li_s
    ComplexityExpression Argument,
    Variable Var) : SpecialFunctionComplexity
{
    public override bool HasAsymptoticExpansion => true;

    public override ComplexityExpression? DominantTerm =>
        Order switch
        {
            1.0 => new LogarithmicComplexity(1.0, Var),  // Li_1 ~ log
            _ when Order > 1 => new ConstantComplexity(1.0),  // Li_s converges for s > 1
            _ => null
        };

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is ISpecialFunctionVisitor<T> sfv ? sfv.VisitPolylogarithm(this) : visitor.VisitUnknown(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? this with { Argument = Argument.Substitute(variable, replacement) } : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var).Union(Argument.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        // Numerical evaluation via series for |z| < 1
        if (!assignments.TryGetValue(Var, out var n)) return null;
        var z = Argument.Evaluate(assignments);
        if (!z.HasValue || Math.Abs(z.Value) >= 1) return null;

        double sum = 0;
        for (int k = 1; k <= 100; k++)
        {
            var term = Math.Pow(z.Value, k) / Math.Pow(k, Order);
            sum += term;
            if (Math.Abs(term) < 1e-15) break;
        }
        return sum;
    }

    public override string ToBigONotation() =>
        $"Li_{Order}({Argument.ToBigONotation().Replace("O(", "").TrimEnd(')')})";
}

/// <summary>
/// Incomplete Gamma function γ(s, x) = ∫₀ˣ t^(s-1) e^(-t) dt
///
/// Arises from exponential-polynomial integrals. Asymptotically:
/// - For large x: γ(s, x) → Γ(s) (complete gamma)
/// - For small x: γ(s, x) ≈ x^s / s
/// </summary>
public sealed record IncompleteGammaComplexity(
    double Shape,           // s parameter
    ComplexityExpression UpperLimit,
    Variable Var) : SpecialFunctionComplexity
{
    public override bool HasAsymptoticExpansion => true;

    public override ComplexityExpression? DominantTerm
    {
        get
        {
            // For large upper limit, approaches Γ(s) which is constant
            if (Shape > 0)
                return new ConstantComplexity(Gamma(Shape));
            return null;
        }
    }

    private static double Gamma(double s)
    {
        // Stirling approximation for positive s
        if (s > 0 && s < 172)
        {
            // Use reflection formula and recurrence
            if (s < 1) return Gamma(s + 1) / s;
            if (s < 2) return Math.Sqrt(2 * Math.PI / s) * Math.Pow(s / Math.E, s);
            return (s - 1) * Gamma(s - 1);
        }
        return double.PositiveInfinity;
    }

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is ISpecialFunctionVisitor<T> sfv ? sfv.VisitIncompleteGamma(this) : visitor.VisitUnknown(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? this with { UpperLimit = UpperLimit.Substitute(variable, replacement) } : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var).Union(UpperLimit.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var x = UpperLimit.Evaluate(assignments);
        if (!x.HasValue || x.Value < 0) return null;

        // Series expansion for small x
        if (x.Value < 10)
        {
            double sum = 0;
            double term = Math.Pow(x.Value, Shape) / Shape;
            sum = term;
            for (int k = 1; k <= 100; k++)
            {
                term *= -x.Value / (Shape + k);
                sum += term;
                if (Math.Abs(term) < 1e-15 * Math.Abs(sum)) break;
            }
            return sum;
        }

        // Asymptotic expansion for large x
        return Gamma(Shape); // Approximation
    }

    public override string ToBigONotation() =>
        $"γ({Shape}, {UpperLimit.ToBigONotation().Replace("O(", "").TrimEnd(')')})";
}

/// <summary>
/// Incomplete Beta function B(x; a, b) = ∫₀ˣ t^(a-1) (1-t)^(b-1) dt
///
/// Related to regularized incomplete beta I_x(a,b) = B(x;a,b) / B(a,b).
/// Arises in probability and from polynomial ratio integrals.
/// </summary>
public sealed record IncompleteBetaComplexity(
    double A,               // First shape parameter
    double B,               // Second shape parameter
    ComplexityExpression UpperLimit,
    Variable Var) : SpecialFunctionComplexity
{
    public override bool HasAsymptoticExpansion => true;

    public override ComplexityExpression? DominantTerm
    {
        get
        {
            // For x → 1, approaches complete Beta B(a,b)
            if (A > 0 && B > 0)
                return new ConstantComplexity(Beta(A, B));
            return null;
        }
    }

    private static double Beta(double a, double b)
    {
        // B(a,b) = Γ(a)Γ(b)/Γ(a+b)
        // For small values, use lgamma
        return Math.Exp(LogGamma(a) + LogGamma(b) - LogGamma(a + b));
    }

    private static double LogGamma(double x)
    {
        // Lanczos approximation
        if (x <= 0) return double.PositiveInfinity;
        double[] c = { 76.18009172947146, -86.50532032941677, 24.01409824083091,
                       -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5 };
        double sum = 1.000000000190015;
        for (int i = 0; i < 6; i++) sum += c[i] / (x + i + 1);
        return (x + 0.5) * Math.Log(x + 5.5) - (x + 5.5) + Math.Log(2.5066282746310005 * sum / x);
    }

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is ISpecialFunctionVisitor<T> sfv ? sfv.VisitIncompleteBeta(this) : visitor.VisitUnknown(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? this with { UpperLimit = UpperLimit.Substitute(variable, replacement) } : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var).Union(UpperLimit.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var x = UpperLimit.Evaluate(assignments);
        if (!x.HasValue || x.Value < 0 || x.Value > 1) return null;

        // Use regularized incomplete beta and multiply by B(a,b)
        // Continued fraction representation
        return RegularizedBeta(x.Value, A, B) * Beta(A, B);
    }

    private static double RegularizedBeta(double x, double a, double b)
    {
        // Lentz's continued fraction
        if (x == 0) return 0;
        if (x == 1) return 1;

        // Use symmetry: I_x(a,b) = 1 - I_{1-x}(b,a)
        if (x > (a + 1) / (a + b + 2))
            return 1 - RegularizedBeta(1 - x, b, a);

        double qab = a + b;
        double qap = a + 1;
        double qam = a - 1;
        double c = 1;
        double d = 1 - qab * x / qap;
        if (Math.Abs(d) < 1e-30) d = 1e-30;
        d = 1 / d;
        double h = d;

        for (int m = 1; m <= 100; m++)
        {
            int m2 = 2 * m;
            double aa = m * (b - m) * x / ((qam + m2) * (a + m2));
            d = 1 + aa * d;
            if (Math.Abs(d) < 1e-30) d = 1e-30;
            c = 1 + aa / c;
            if (Math.Abs(c) < 1e-30) c = 1e-30;
            d = 1 / d;
            h *= d * c;

            aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2));
            d = 1 + aa * d;
            if (Math.Abs(d) < 1e-30) d = 1e-30;
            c = 1 + aa / c;
            if (Math.Abs(c) < 1e-30) c = 1e-30;
            d = 1 / d;
            double del = d * c;
            h *= del;
            if (Math.Abs(del - 1) < 1e-10) break;
        }

        return Math.Exp(a * Math.Log(x) + b * Math.Log(1 - x) - LogGamma(a) - LogGamma(b) + LogGamma(a + b)) * h / a;
    }

    public override string ToBigONotation() =>
        $"B({UpperLimit.ToBigONotation().Replace("O(", "").TrimEnd(')')}; {A}, {B})";
}

/// <summary>
/// Gauss Hypergeometric function ₂F₁(a, b; c; z)
///
/// The most general special function needed for Akra-Bazzi integrals.
/// Many special functions are cases of ₂F₁:
/// - log(1+z) = z · ₂F₁(1, 1; 2; -z)
/// - arcsin(z) = z · ₂F₁(1/2, 1/2; 3/2; z²)
/// - (1-z)^(-a) = ₂F₁(a, b; b; z) for any b
/// </summary>
public sealed record HypergeometricComplexity(
    double A1,              // First numerator parameter
    double A2,              // Second numerator parameter
    double B1,              // Denominator parameter
    ComplexityExpression Argument,
    Variable Var) : SpecialFunctionComplexity
{
    public override bool HasAsymptoticExpansion => true;

    public override ComplexityExpression? DominantTerm
    {
        get
        {
            // For z → 0: ₂F₁ → 1
            // For z → 1: depends on c - a - b
            var cab = B1 - A1 - A2;
            if (cab > 0)
                return new ConstantComplexity(1.0); // Converges at z=1
            return null;
        }
    }

    /// <summary>
    /// Recognizes if this hypergeometric is actually a simpler function.
    /// </summary>
    public ComplexityExpression? SimplifiedForm
    {
        get
        {
            // ₂F₁(a, b; b; z) = (1-z)^(-a)
            if (Math.Abs(A2 - B1) < 1e-10)
                return new PowerComplexity(
                    new BinaryOperationComplexity(
                        new ConstantComplexity(1),
                        BinaryOp.Plus,
                        new BinaryOperationComplexity(
                            new ConstantComplexity(-1),
                            BinaryOp.Multiply,
                            Argument)),
                    -A1);

            // ₂F₁(1, 1; 2; -z) = log(1+z)/z
            if (Math.Abs(A1 - 1) < 1e-10 && Math.Abs(A2 - 1) < 1e-10 && Math.Abs(B1 - 2) < 1e-10)
                return new LogOfComplexity(
                    new BinaryOperationComplexity(
                        new ConstantComplexity(1),
                        BinaryOp.Plus,
                        Argument));

            return null;
        }
    }

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is ISpecialFunctionVisitor<T> sfv ? sfv.VisitHypergeometric(this) : visitor.VisitUnknown(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement) =>
        variable.Equals(Var) ? this with { Argument = Argument.Substitute(variable, replacement) } : this;

    public override ImmutableHashSet<Variable> FreeVariables =>
        ImmutableHashSet.Create(Var).Union(Argument.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        var z = Argument.Evaluate(assignments);
        if (!z.HasValue) return null;

        // Series evaluation for |z| < 1
        if (Math.Abs(z.Value) >= 1) return null;

        double sum = 1;
        double term = 1;
        for (int n = 1; n <= 500; n++)
        {
            term *= (A1 + n - 1) * (A2 + n - 1) / ((B1 + n - 1) * n) * z.Value;
            sum += term;
            if (Math.Abs(term) < 1e-15 * Math.Abs(sum)) break;
        }
        return sum;
    }

    public override string ToBigONotation() =>
        $"₂F₁({A1}, {A2}; {B1}; {Argument.ToBigONotation().Replace("O(", "").TrimEnd(')')})";
}

/// <summary>
/// Represents a symbolic integral that cannot be evaluated in closed form.
/// Preserves the integrand for potential later refinement.
/// </summary>
public sealed record SymbolicIntegralComplexity(
    ComplexityExpression Integrand,
    Variable IntegrationVariable,
    ComplexityExpression LowerBound,
    ComplexityExpression UpperBound,
    ComplexityExpression? AsymptoticBound = null) : SpecialFunctionComplexity
{
    public override bool HasAsymptoticExpansion => AsymptoticBound != null;
    public override ComplexityExpression? DominantTerm => AsymptoticBound;

    public override T Accept<T>(IComplexityVisitor<T> visitor) =>
        visitor is ISpecialFunctionVisitor<T> sfv ? sfv.VisitSymbolicIntegral(this) : visitor.VisitUnknown(this);

    public override ComplexityExpression Substitute(Variable variable, ComplexityExpression replacement)
    {
        if (variable.Equals(IntegrationVariable))
            return this; // Don't substitute bound variable

        return this with
        {
            Integrand = Integrand.Substitute(variable, replacement),
            LowerBound = LowerBound.Substitute(variable, replacement),
            UpperBound = UpperBound.Substitute(variable, replacement),
            AsymptoticBound = AsymptoticBound?.Substitute(variable, replacement)
        };
    }

    public override ImmutableHashSet<Variable> FreeVariables =>
        Integrand.FreeVariables
            .Remove(IntegrationVariable)
            .Union(LowerBound.FreeVariables)
            .Union(UpperBound.FreeVariables);

    public override double? Evaluate(IReadOnlyDictionary<Variable, double> assignments)
    {
        // Numerical integration via adaptive Simpson's rule
        var lower = LowerBound.Evaluate(assignments);
        var upper = UpperBound.Evaluate(assignments);
        if (!lower.HasValue || !upper.HasValue) return null;

        return AdaptiveSimpson(
            u =>
            {
                var augmented = new Dictionary<Variable, double>(assignments)
                {
                    [IntegrationVariable] = u
                };
                return Integrand.Evaluate(augmented) ?? 0;
            },
            lower.Value,
            upper.Value,
            1e-8,
            maxDepth: 20);
    }

    private static double AdaptiveSimpson(Func<double, double> f, double a, double b, double tol, int maxDepth)
    {
        double c = (a + b) / 2;
        double h = b - a;
        double fa = f(a), fb = f(b), fc = f(c);
        double s = h / 6 * (fa + 4 * fc + fb);
        return AdaptiveSimpsonAux(f, a, b, tol, s, fa, fb, fc, maxDepth);
    }

    private static double AdaptiveSimpsonAux(
        Func<double, double> f, double a, double b, double tol,
        double whole, double fa, double fb, double fc, int depth)
    {
        double c = (a + b) / 2;
        double h = b - a;
        double d = (a + c) / 2;
        double e = (c + b) / 2;
        double fd = f(d), fe = f(e);
        double left = h / 12 * (fa + 4 * fd + fc);
        double right = h / 12 * (fc + 4 * fe + fb);
        double delta = left + right - whole;

        if (depth <= 0 || Math.Abs(delta) <= 15 * tol)
            return left + right + delta / 15;

        return AdaptiveSimpsonAux(f, a, c, tol / 2, left, fa, fc, fd, depth - 1)
             + AdaptiveSimpsonAux(f, c, b, tol / 2, right, fc, fb, fe, depth - 1);
    }

    public override string ToBigONotation()
    {
        var integrandStr = Integrand.ToBigONotation().Replace("O(", "").TrimEnd(')');
        var lowerStr = LowerBound.ToBigONotation().Replace("O(", "").TrimEnd(')');
        var upperStr = UpperBound.ToBigONotation().Replace("O(", "").TrimEnd(')');
        return $"∫[{lowerStr}→{upperStr}] {integrandStr} d{IntegrationVariable.Name}";
    }

    /// <summary>
    /// Creates a symbolic integral with an asymptotic bound estimate.
    /// </summary>
    public static SymbolicIntegralComplexity WithBound(
        ComplexityExpression integrand,
        Variable integrationVar,
        ComplexityExpression lower,
        ComplexityExpression upper,
        ComplexityExpression bound) =>
        new(integrand, integrationVar, lower, upper, bound);
}

/// <summary>
/// Extended visitor interface for special functions.
/// </summary>
public interface ISpecialFunctionVisitor<T> : IComplexityVisitor<T>
{
    T VisitPolylogarithm(PolylogarithmComplexity expr);
    T VisitIncompleteGamma(IncompleteGammaComplexity expr);
    T VisitIncompleteBeta(IncompleteBetaComplexity expr);
    T VisitHypergeometric(HypergeometricComplexity expr);
    T VisitSymbolicIntegral(SymbolicIntegralComplexity expr);
}
