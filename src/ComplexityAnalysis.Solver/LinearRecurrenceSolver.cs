using System.Collections.Immutable;
using System.Numerics;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.RootFinding;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Solves linear recurrence relations using the characteristic polynomial method.
/// </summary>
/// <remarks>
/// <para>
/// <b>Problem Form:</b> T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k) + f(n)
/// </para>
/// 
/// <para>
/// <b>Solution Algorithm:</b>
/// </para>
/// <list type="number">
///   <item><description>
///     <b>Characteristic Polynomial:</b> Form p(x) = x^k - a₁x^(k-1) - ... - aₖ
///   </description></item>
///   <item><description>
///     <b>Root Finding:</b> Find all roots using companion matrix eigendecomposition
///   </description></item>
///   <item><description>
///     <b>Homogeneous Solution:</b> Build from roots with multiplicities
///   </description></item>
///   <item><description>
///     <b>Particular Solution:</b> Handle non-homogeneous term f(n)
///   </description></item>
///   <item><description>
///     <b>Asymptotic Form:</b> Extract dominant term for Big-O notation
///   </description></item>
/// </list>
/// 
/// <para>
/// <b>Root Types and Their Contributions:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Root Type</term>
///     <description>Contribution to Solution</description>
///   </listheader>
///   <item>
///     <term>Real root r (simple)</term>
///     <description>c·rⁿ</description>
///   </item>
///   <item>
///     <term>Real root r (multiplicity m)</term>
///     <description>(c₀ + c₁n + ... + c_{m-1}n^{m-1})·rⁿ</description>
///   </item>
///   <item>
///     <term>Complex pair α±βi</term>
///     <description>ρⁿ(c₁cos(nθ) + c₂sin(nθ)) where ρ = √(α²+β²)</description>
///   </item>
/// </list>
/// 
/// <para>
/// <b>Common Solutions:</b>
/// </para>
/// <code>
/// // T(n) = T(n-1) + 1 → O(n)
/// // T(n) = T(n-1) + n → O(n²)
/// // T(n) = 2T(n-1) + 1 → O(2ⁿ)
/// // T(n) = T(n-1) + T(n-2) → O(φⁿ) ≈ O(1.618ⁿ)
/// // T(n) = 4T(n-1) - 4T(n-2) → O(n·2ⁿ) (repeated root)
/// </code>
/// </remarks>
public sealed class LinearRecurrenceSolver : ILinearRecurrenceSolver
{
    private readonly IExpressionClassifier _classifier;

    /// <summary>Tolerance for root comparison and numerical stability.</summary>
    private const double Epsilon = 1e-9;

    /// <summary>Tolerance for considering roots equal (for multiplicity detection).</summary>
    private const double RootEqualityTolerance = 1e-6;

    public LinearRecurrenceSolver(IExpressionClassifier? classifier = null)
    {
        _classifier = classifier ?? StandardExpressionClassifier.Instance;
    }

    /// <summary>
    /// Default singleton instance.
    /// </summary>
    public static LinearRecurrenceSolver Instance { get; } = new();

    /// <summary>
    /// Solves a linear recurrence relation and returns the asymptotic complexity.
    /// </summary>
    /// <param name="recurrence">The linear recurrence to solve.</param>
    /// <returns>The solution, or null if the recurrence cannot be solved.</returns>
    public LinearRecurrenceSolution? Solve(LinearRecurrenceRelation recurrence)
    {
        try
        {
            // Special case: simple summation T(n) = T(n-1) + f(n)
            if (recurrence.IsSummation)
            {
                return SolveSummation(recurrence);
            }

            // Find characteristic roots
            var roots = FindCharacteristicRoots(recurrence.Coefficients);
            if (roots.IsDefaultOrEmpty)
                return null;

            // Group roots by value and determine multiplicities
            var groupedRoots = GroupRootsByMultiplicity(roots);

            // Find dominant root (largest magnitude)
            var dominantRoot = groupedRoots.MaxBy(r => r.Magnitude);
            if (dominantRoot == null)
                return null;

            // Build homogeneous solution from dominant root
            var homogeneousSolution = BuildHomogeneousSolution(dominantRoot, recurrence.Variable);

            // Handle non-homogeneous term
            ComplexityExpression finalSolution;
            string method;

            if (recurrence.IsHomogeneous)
            {
                finalSolution = homogeneousSolution;
                method = BuildMethodDescription(dominantRoot, groupedRoots);
            }
            else
            {
                // Combine homogeneous and particular solutions
                var (combinedSolution, combinedMethod) = CombineWithParticular(
                    homogeneousSolution,
                    dominantRoot,
                    recurrence.NonRecursiveWork,
                    recurrence.Variable);
                finalSolution = combinedSolution;
                method = combinedMethod;
            }

            return new LinearRecurrenceSolution
            {
                Solution = finalSolution,
                Method = method,
                Roots = groupedRoots,
                DominantRoot = dominantRoot,
                HasPolynomialFactor = dominantRoot.IsRepeated,
                Explanation = BuildExplanation(recurrence, groupedRoots, dominantRoot, finalSolution)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Solves a simple summation recurrence T(n) = T(n-1) + f(n).
    /// </summary>
    private LinearRecurrenceSolution SolveSummation(LinearRecurrenceRelation recurrence)
    {
        var f = recurrence.NonRecursiveWork;
        var variable = recurrence.Variable;

        // Classify f(n)
        var classification = _classifier.Classify(f, variable);

        // The solution is Σᵢ₌₁ⁿ f(i), which increases the polynomial degree by 1
        ComplexityExpression solution;
        string method;

        if (f is ConstantComplexity)
        {
            // Σ c = c·n = O(n)
            solution = new LinearComplexity(1.0, variable);
            method = "summation of constant: Σ c = O(n)";
        }
        else if (_classifier.TryExtractPolynomialDegree(f, variable, out var degree))
        {
            // Σ n^k = O(n^(k+1))
            solution = PolyLogComplexity.Polynomial(degree + 1, variable);
            method = $"summation of polynomial: Σ n^{degree:F1} = O(n^{degree + 1:F1})";
        }
        else if (classification.Form == ExpressionForm.Logarithmic)
        {
            // Σ log(i) ≈ n log n - n = O(n log n)
            solution = PolyLogComplexity.NLogN(variable);
            method = "summation of logarithm: Σ log(i) = O(n log n)";
        }
        else if (_classifier.TryExtractPolyLogForm(f, variable, out var polyDeg, out var logExp))
        {
            // Σ n^k log^j(n) = O(n^(k+1) log^j(n))
            solution = new PolyLogComplexity(polyDeg + 1, logExp, variable);
            method = $"summation of polylog: Σ n^{polyDeg:F1}·log^{logExp} n = O(n^{polyDeg + 1:F1}·log^{logExp} n)";
        }
        else if (f is ExponentialComplexity expF)
        {
            // Σ a^i = (a^(n+1) - 1)/(a - 1) = O(a^n) for a > 1
            solution = new ExponentialComplexity(expF.Base, variable);
            method = $"summation of exponential: Σ {expF.Base}^i = O({expF.Base}^n)";
        }
        else
        {
            // Default: treat as O(n × f(n))
            solution = new BinaryOperationComplexity(
                new VariableComplexity(variable),
                BinaryOp.Multiply,
                f);
            method = "summation bound: Σ f(i) ≤ n × f(n)";
        }

        var unitRoot = CharacteristicRoot.Real(1.0);

        return new LinearRecurrenceSolution
        {
            Solution = solution,
            Method = method,
            Roots = ImmutableArray.Create(unitRoot),
            DominantRoot = unitRoot,
            HasPolynomialFactor = false,
            Explanation = $"Summation recurrence T(n) = T(n-1) + {f.ToBigONotation()} solved by {method}"
        };
    }

    /// <summary>
    /// Finds the roots of the characteristic polynomial using companion matrix eigendecomposition.
    /// </summary>
    /// <remarks>
    /// For a recurrence T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k),
    /// the characteristic polynomial is: x^k - a₁x^(k-1) - a₂x^(k-2) - ... - aₖ = 0
    /// 
    /// We find roots by computing eigenvalues of the companion matrix.
    /// </remarks>
    private ImmutableArray<CharacteristicRoot> FindCharacteristicRoots(ImmutableArray<double> coefficients)
    {
        var k = coefficients.Length;

        if (k == 1)
        {
            // First-order: x = a₁
            return ImmutableArray.Create(CharacteristicRoot.Real(coefficients[0]));
        }

        if (k == 2)
        {
            // Second-order: x² - a₁x - a₂ = 0
            // Quadratic formula: x = (a₁ ± √(a₁² + 4a₂)) / 2
            return SolveQuadratic(coefficients[0], coefficients[1]);
        }

        // General case: use companion matrix
        return SolveUsingCompanionMatrix(coefficients);
    }

    /// <summary>
    /// Solves a quadratic characteristic equation: x² - a₁x - a₂ = 0.
    /// </summary>
    private ImmutableArray<CharacteristicRoot> SolveQuadratic(double a1, double a2)
    {
        // x = (a₁ ± √(a₁² + 4a₂)) / 2
        var discriminant = a1 * a1 + 4 * a2;

        if (discriminant >= 0)
        {
            // Real roots
            var sqrtD = Math.Sqrt(discriminant);
            var r1 = (a1 + sqrtD) / 2;
            var r2 = (a1 - sqrtD) / 2;

            if (Math.Abs(r1 - r2) < RootEqualityTolerance)
            {
                // Repeated root
                return ImmutableArray.Create(CharacteristicRoot.Real(r1, multiplicity: 2));
            }

            return ImmutableArray.Create(
                CharacteristicRoot.Real(r1),
                CharacteristicRoot.Real(r2));
        }
        else
        {
            // Complex conjugate pair
            var realPart = a1 / 2;
            var imagPart = Math.Sqrt(-discriminant) / 2;

            return ImmutableArray.Create(
                CharacteristicRoot.Complex(realPart, imagPart),
                CharacteristicRoot.Complex(realPart, -imagPart));
        }
    }

    /// <summary>
    /// Solves a characteristic equation using companion matrix eigendecomposition.
    /// </summary>
    private ImmutableArray<CharacteristicRoot> SolveUsingCompanionMatrix(ImmutableArray<double> coefficients)
    {
        var k = coefficients.Length;

        // Build companion matrix for: x^k - a₁x^(k-1) - ... - aₖ = 0
        // Companion matrix form (Frobenius):
        // [a₁  a₂  a₃  ... aₖ]
        // [1   0   0   ... 0 ]
        // [0   1   0   ... 0 ]
        // [...              ]
        // [0   0   ... 1   0 ]

        var companion = Matrix<double>.Build.Dense(k, k);

        // First row: coefficients
        for (int j = 0; j < k; j++)
        {
            companion[0, j] = coefficients[j];
        }

        // Sub-diagonal: ones
        for (int i = 1; i < k; i++)
        {
            companion[i, i - 1] = 1.0;
        }

        // Compute eigenvalues
        var evd = companion.Evd();
        var eigenvalues = evd.EigenValues;

        var roots = new List<CharacteristicRoot>();
        foreach (var ev in eigenvalues)
        {
            if (Math.Abs(ev.Imaginary) < Epsilon)
            {
                roots.Add(CharacteristicRoot.Real(ev.Real));
            }
            else
            {
                roots.Add(CharacteristicRoot.Complex(ev.Real, ev.Imaginary));
            }
        }

        return roots.ToImmutableArray();
    }

    /// <summary>
    /// Groups roots by value and determines multiplicities.
    /// </summary>
    private ImmutableArray<CharacteristicRoot> GroupRootsByMultiplicity(ImmutableArray<CharacteristicRoot> roots)
    {
        var grouped = new List<CharacteristicRoot>();
        var used = new bool[roots.Length];

        for (int i = 0; i < roots.Length; i++)
        {
            if (used[i]) continue;

            var root = roots[i];
            // Start with the root's existing multiplicity (in case quadratic solver already computed it)
            int multiplicity = root.Multiplicity;

            // Count duplicates
            for (int j = i + 1; j < roots.Length; j++)
            {
                if (used[j]) continue;

                var other = roots[j];
                if (AreRootsEqual(root, other))
                {
                    multiplicity += other.Multiplicity;
                    used[j] = true;
                }
            }

            used[i] = true;

            grouped.Add(new CharacteristicRoot
            {
                RealPart = root.RealPart,
                ImaginaryPart = root.ImaginaryPart,
                Magnitude = root.Magnitude,
                Multiplicity = multiplicity
            });
        }

        return grouped.ToImmutableArray();
    }

    private bool AreRootsEqual(CharacteristicRoot a, CharacteristicRoot b)
    {
        return Math.Abs(a.RealPart - b.RealPart) < RootEqualityTolerance &&
               Math.Abs(a.ImaginaryPart - b.ImaginaryPart) < RootEqualityTolerance;
    }

    /// <summary>
    /// Builds the asymptotic solution from the dominant root.
    /// </summary>
    private ComplexityExpression BuildHomogeneousSolution(CharacteristicRoot dominant, Variable variable)
    {
        var magnitude = dominant.Magnitude;

        // Special case: |r| ≈ 1 means bounded/constant for homogeneous
        if (Math.Abs(magnitude - 1.0) < Epsilon && !dominant.IsRepeated)
        {
            return ConstantComplexity.One;
        }

        // Special case: |r| < 1 means decaying, so O(1)
        if (magnitude < 1.0 - Epsilon)
        {
            return ConstantComplexity.One;
        }

        // Build exponential part
        ComplexityExpression result;

        if (Math.Abs(magnitude - 1.0) < Epsilon)
        {
            // Root is 1 with multiplicity m → O(n^(m-1))
            if (dominant.Multiplicity > 1)
            {
                result = PolyLogComplexity.Polynomial(dominant.Multiplicity - 1, variable);
            }
            else
            {
                result = ConstantComplexity.One;
            }
        }
        else
        {
            // Root magnitude > 1 → O(r^n) or O(n^(m-1) · r^n)
            var exponentialPart = new ExponentialComplexity(magnitude, variable);

            if (dominant.IsRepeated)
            {
                // Repeated root: multiply by n^(m-1)
                var polyPart = PolyLogComplexity.Polynomial(dominant.Multiplicity - 1, variable);
                result = new BinaryOperationComplexity(polyPart, BinaryOp.Multiply, exponentialPart);
            }
            else
            {
                result = exponentialPart;
            }
        }

        return result;
    }

    /// <summary>
    /// Combines homogeneous solution with particular solution for non-homogeneous term.
    /// </summary>
    private (ComplexityExpression solution, string method) CombineWithParticular(
        ComplexityExpression homogeneous,
        CharacteristicRoot dominant,
        ComplexityExpression f,
        Variable variable)
    {
        // For T(n) = (homogeneous terms) + f(n), the solution is max(homogeneous, particular)
        // where particular comes from the summation/accumulation of f

        var classification = _classifier.Classify(f, variable);

        // Determine the particular solution contribution
        ComplexityExpression particular;

        if (Math.Abs(dominant.Magnitude - 1.0) < Epsilon && !dominant.IsRepeated)
        {
            // When dominant root is 1 (simple), this is effectively a summation
            if (f is ConstantComplexity)
            {
                particular = new LinearComplexity(1.0, variable);
            }
            else if (_classifier.TryExtractPolynomialDegree(f, variable, out var deg))
            {
                particular = PolyLogComplexity.Polynomial(deg + 1, variable);
            }
            else if (f is ExponentialComplexity expF)
            {
                particular = expF;
            }
            else
            {
                particular = new BinaryOperationComplexity(
                    new VariableComplexity(variable), BinaryOp.Multiply, f);
            }
        }
        else if (f is ExponentialComplexity expF && Math.Abs(expF.Base - dominant.Magnitude) < Epsilon)
        {
            // f(n) = a^n where a = dominant root → resonance case
            // Particular solution has extra polynomial factor
            particular = new BinaryOperationComplexity(
                new VariableComplexity(variable),
                BinaryOp.Multiply,
                f);
        }
        else if (f is ExponentialComplexity expF2)
        {
            particular = expF2;
        }
        else
        {
            // General case: particular is bounded by homogeneous × f
            particular = f;
        }

        // Take max of homogeneous and particular
        var combined = CompareAndTakeMax(homogeneous, particular, variable);
        var method = $"characteristic roots + particular solution";

        return (combined, method);
    }

    /// <summary>
    /// Compares two complexities and returns the asymptotically larger one.
    /// </summary>
    private ComplexityExpression CompareAndTakeMax(
        ComplexityExpression a,
        ComplexityExpression b,
        Variable variable)
    {
        // Simple heuristic comparison
        var aClass = _classifier.Classify(a, variable);
        var bClass = _classifier.Classify(b, variable);

        // Exponential > Polynomial > Polylog > Log > Constant
        if (aClass.Form == ExpressionForm.Exponential && bClass.Form != ExpressionForm.Exponential)
            return a;
        if (bClass.Form == ExpressionForm.Exponential && aClass.Form != ExpressionForm.Exponential)
            return b;

        if (aClass.Form == ExpressionForm.Exponential && bClass.Form == ExpressionForm.Exponential)
        {
            // Compare bases
            if (a is ExponentialComplexity expA && b is ExponentialComplexity expB)
            {
                return expA.Base >= expB.Base ? a : b;
            }
        }

        // Compare polynomial degrees
        if (aClass.PrimaryParameter.HasValue && bClass.PrimaryParameter.HasValue)
        {
            return aClass.PrimaryParameter.Value >= bClass.PrimaryParameter.Value ? a : b;
        }

        if (aClass.PrimaryParameter.HasValue)
            return a;
        if (bClass.PrimaryParameter.HasValue)
            return b;

        return a;
    }

    private string BuildMethodDescription(CharacteristicRoot dominant, ImmutableArray<CharacteristicRoot> allRoots)
    {
        if (allRoots.Length == 1)
        {
            if (dominant.IsRepeated)
                return $"characteristic root {dominant.RealPart:F3} (multiplicity {dominant.Multiplicity})";
            return $"characteristic root {dominant.RealPart:F3}";
        }

        var rootDesc = string.Join(", ", allRoots.Take(3).Select(r => r.ToString()));
        if (allRoots.Length > 3)
            rootDesc += $", ... ({allRoots.Length} total)";

        return $"characteristic roots [{rootDesc}], dominant: {dominant.Magnitude:F3}";
    }

    private string BuildExplanation(
        LinearRecurrenceRelation recurrence,
        ImmutableArray<CharacteristicRoot> roots,
        CharacteristicRoot dominant,
        ComplexityExpression solution)
    {
        var lines = new List<string>
        {
            $"Recurrence: {recurrence}",
            $"Order: {recurrence.Order}",
            $"Characteristic polynomial roots: {string.Join(", ", roots.Select(r => r.ToString()))}",
            $"Dominant root: {dominant} (magnitude {dominant.Magnitude:F4})"
        };

        if (dominant.IsRepeated)
        {
            lines.Add($"Repeated root contributes polynomial factor n^{dominant.Multiplicity - 1}");
        }

        lines.Add($"Asymptotic solution: {solution.ToBigONotation()}");

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>
/// Interface for linear recurrence solvers.
/// </summary>
public interface ILinearRecurrenceSolver
{
    /// <summary>
    /// Solves a linear recurrence relation.
    /// </summary>
    LinearRecurrenceSolution? Solve(LinearRecurrenceRelation recurrence);
}

/// <summary>
/// Theorem applicability result for linear recurrences solved by characteristic polynomial.
/// </summary>
public sealed record CharacteristicPolynomialSolved(
    /// <summary>The closed-form solution.</summary>
    ComplexityExpression SolutionExpr,

    /// <summary>The full solution details.</summary>
    LinearRecurrenceSolution Details) : TheoremApplicability
{
    public override bool IsApplicable => true;
    public override ComplexityExpression? Solution => SolutionExpr;

    public override string Explanation =>
        $"Linear recurrence solved by characteristic polynomial. " +
        $"Dominant root: {Details.DominantRoot}. " +
        $"Solution: {SolutionExpr.ToBigONotation()}";
}
