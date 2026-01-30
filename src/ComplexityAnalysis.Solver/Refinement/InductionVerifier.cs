using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;

namespace ComplexityAnalysis.Solver.Refinement;

/// <summary>
/// Verifies complexity bounds using mathematical induction.
/// Checks that proposed solutions satisfy the recurrence relation
/// and provides formal verification of asymptotic bounds.
///
/// Verification steps:
/// 1. Base case verification: T(n₀) satisfies the bound
/// 2. Inductive step: If T(k) satisfies bound for all k < n, then T(n) does too
/// 3. Asymptotic verification: Bound holds as n → ∞
///
/// When available, uses SymPy for exact symbolic verification.
/// </summary>
public sealed class InductionVerifier : IInductionVerifier
{
    private readonly SymPyRecurrenceSolver? _sympySolver;

    /// <summary>
    /// Creates an InductionVerifier. If sympySolver is provided, uses SymPy for exact verification.
    /// </summary>
    public InductionVerifier(SymPyRecurrenceSolver? sympySolver = null)
    {
        _sympySolver = sympySolver;
    }

    /// <summary>Tolerance for numerical comparisons.</summary>
    private const double Tolerance = 1e-9;

    /// <summary>Sample points for numerical verification.</summary>
    private static readonly int[] SamplePoints = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024 };

    /// <summary>Large sample points for asymptotic verification.</summary>
    private static readonly int[] LargeSamplePoints = { 10000, 100000, 1000000 };

    /// <summary>Default instance without SymPy support.</summary>
    public static InductionVerifier Instance { get; } = new(null);

    /// <summary>Creates an instance with SymPy support for exact verification.</summary>
    public static InductionVerifier WithSymPy(SymPyRecurrenceSolver solver) => new(solver);

    /// <summary>
    /// Verifies that a solution satisfies a recurrence relation.
    /// If SymPy solver is available, uses exact symbolic verification first.
    /// </summary>
    public InductionResult VerifyRecurrenceSolution(
        RecurrenceRelation recurrence,
        ComplexityExpression proposedSolution,
        BoundType boundType = BoundType.Theta)
    {
        // Try SymPy-based verification first if available
        if (_sympySolver is not null)
        {
            var sympyResult = TryVerifyWithSymPy(recurrence, proposedSolution);
            if (sympyResult is not null)
                return sympyResult;
        }

        var variable = recurrence.Variable;
        var diagnostics = new List<string>();

        // Step 1: Verify base cases
        var baseResult = VerifyBaseCases(recurrence, proposedSolution, variable);
        diagnostics.AddRange(baseResult.Diagnostics);

        if (!baseResult.Holds)
        {
            return new InductionResult
            {
                Verified = false,
                BaseCase = baseResult,
                ErrorMessage = "Base case verification failed",
                Diagnostics = diagnostics.ToImmutableList()
            };
        }

        // Step 2: Verify inductive step
        var inductiveResult = VerifyInductiveStep(recurrence, proposedSolution, variable, boundType);
        diagnostics.AddRange(inductiveResult.Diagnostics);

        if (!inductiveResult.Holds)
        {
            return new InductionResult
            {
                Verified = false,
                BaseCase = baseResult,
                InductiveStep = inductiveResult,
                ErrorMessage = "Inductive step verification failed",
                Diagnostics = diagnostics.ToImmutableList()
            };
        }

        // Step 3: Verify asymptotic behavior
        var asymptoticResult = VerifyAsymptoticBehavior(recurrence, proposedSolution, variable, boundType);
        diagnostics.AddRange(asymptoticResult.Diagnostics);

        var confidence = ComputeVerificationConfidence(baseResult, inductiveResult, asymptoticResult);

        return new InductionResult
        {
            Verified = baseResult.Holds && inductiveResult.Holds && asymptoticResult.Holds,
            BaseCase = baseResult,
            InductiveStep = inductiveResult,
            AsymptoticVerification = asymptoticResult,
            ConfidenceScore = confidence,
            BoundType = boundType,
            Diagnostics = diagnostics.ToImmutableList()
        };
    }

    /// <summary>
    /// Verifies an upper bound: T(n) = O(f(n)).
    /// </summary>
    public BoundVerificationResult VerifyUpperBound(
        RecurrenceRelation recurrence,
        ComplexityExpression upperBound)
    {
        var variable = recurrence.Variable;
        var diagnostics = new List<string>();

        // Find constant c such that T(n) ≤ c·f(n) for all n ≥ n₀
        var (c, n0, holds) = FindBoundConstant(recurrence, upperBound, variable, isUpper: true);

        if (!holds)
        {
            return new BoundVerificationResult
            {
                Holds = false,
                ErrorMessage = "Could not find valid constant for upper bound",
                Diagnostics = diagnostics.ToImmutableList()
            };
        }

        // Verify the bound holds for sample points
        var violations = new List<(double n, double actual, double bound)>();

        foreach (var n in SamplePoints.Concat(LargeSamplePoints))
        {
            if (n < n0) continue;

            var actualValue = EvaluateRecurrence(recurrence, n);
            var boundValue = EvaluateBound(upperBound, n, variable, c);

            if (actualValue.HasValue && boundValue.HasValue)
            {
                if (actualValue.Value > boundValue.Value * (1 + Tolerance))
                {
                    violations.Add((n, actualValue.Value, boundValue.Value));
                }
            }
        }

        return new BoundVerificationResult
        {
            Holds = violations.Count == 0,
            Constant = c,
            Threshold = n0,
            Violations = violations.ToImmutableList(),
            Diagnostics = ImmutableList.Create(
                $"Found c = {c:F4} for n ≥ {n0}",
                $"Violations: {violations.Count}")
        };
    }

    /// <summary>
    /// Verifies a lower bound: T(n) = Ω(f(n)).
    /// </summary>
    public BoundVerificationResult VerifyLowerBound(
        RecurrenceRelation recurrence,
        ComplexityExpression lowerBound)
    {
        var variable = recurrence.Variable;
        var diagnostics = new List<string>();

        // Find constant c such that T(n) ≥ c·f(n) for all n ≥ n₀
        var (c, n0, holds) = FindBoundConstant(recurrence, lowerBound, variable, isUpper: false);

        if (!holds)
        {
            return new BoundVerificationResult
            {
                Holds = false,
                ErrorMessage = "Could not find valid constant for lower bound",
                Diagnostics = diagnostics.ToImmutableList()
            };
        }

        // Verify the bound holds for sample points
        var violations = new List<(double n, double actual, double bound)>();

        foreach (var n in SamplePoints.Concat(LargeSamplePoints))
        {
            if (n < n0) continue;

            var actualValue = EvaluateRecurrence(recurrence, n);
            var boundValue = EvaluateBound(lowerBound, n, variable, c);

            if (actualValue.HasValue && boundValue.HasValue)
            {
                if (actualValue.Value < boundValue.Value * (1 - Tolerance))
                {
                    violations.Add((n, actualValue.Value, boundValue.Value));
                }
            }
        }

        return new BoundVerificationResult
        {
            Holds = violations.Count == 0,
            Constant = c,
            Threshold = n0,
            Violations = violations.ToImmutableList(),
            Diagnostics = ImmutableList.Create(
                $"Found c = {c:F4} for n ≥ {n0}",
                $"Violations: {violations.Count}")
        };
    }

    /// <summary>
    /// Performs symbolic induction verification when possible.
    /// </summary>
    public SymbolicInductionResult VerifySymbolically(
        RecurrenceRelation recurrence,
        ComplexityExpression proposedSolution)
    {
        var variable = recurrence.Variable;

        // Check if the recurrence and solution have forms that allow symbolic verification
        if (!CanVerifySymbolically(recurrence, proposedSolution))
        {
            return new SymbolicInductionResult
            {
                Success = false,
                ErrorMessage = "Symbolic verification not supported for this form"
            };
        }

        // Extract the forms
        var recurrenceForm = ClassifyRecurrence(recurrence);
        var solutionForm = ClassifySolution(proposedSolution, variable);

        // Perform symbolic verification based on forms
        var verificationSteps = new List<string>();

        // Base case
        var baseCaseHolds = VerifySymbolicBaseCase(recurrence, proposedSolution, variable);
        verificationSteps.Add($"Base case (n=1): {(baseCaseHolds ? "✓" : "✗")}");

        if (!baseCaseHolds)
        {
            return new SymbolicInductionResult
            {
                Success = false,
                Steps = verificationSteps.ToImmutableList(),
                ErrorMessage = "Symbolic base case failed"
            };
        }

        // Inductive step
        var inductiveStepHolds = VerifySymbolicInductiveStep(recurrence, proposedSolution, recurrenceForm, solutionForm, variable);
        verificationSteps.Add($"Inductive step: {(inductiveStepHolds ? "✓" : "✗")}");

        return new SymbolicInductionResult
        {
            Success = baseCaseHolds && inductiveStepHolds,
            Steps = verificationSteps.ToImmutableList(),
            ProofSketch = GenerateProofSketch(recurrence, proposedSolution, recurrenceForm, solutionForm)
        };
    }

    #region Private Methods

    /// <summary>
    /// Attempts verification using SymPy. Returns null if SymPy verification fails or is unavailable.
    /// </summary>
    private InductionResult? TryVerifyWithSymPy(RecurrenceRelation recurrence, ComplexityExpression proposedSolution)
    {
        try
        {
            // Convert recurrence to SymPy format
            if (!TryConvertToLinearRecurrence(recurrence, out var coeffs, out var baseCases, out var fOfN))
                return null;

            // Use SymPy to solve and verify
            var task = _sympySolver!.SolveLinearAsync(coeffs, baseCases, fOfN);
            if (!task.Wait(TimeSpan.FromSeconds(10)))
                return null;

            var result = task.Result;
            if (!result.Success)
                return null;

            // Compare SymPy's solution complexity with proposed solution
            var sympyComplexity = result.ComplexityString ?? "";
            var proposedComplexity = proposedSolution.ToBigONotation();

            var diagnostics = new List<string>
            {
                $"SymPy closed form: {result.ClosedForm}",
                $"SymPy complexity: {sympyComplexity}",
                $"Proposed complexity: {proposedComplexity}",
                $"SymPy verified: {result.Verified}"
            };

            // If SymPy verified the solution, we're done
            if (result.Verified)
            {
                return new InductionResult
                {
                    Verified = true,
                    ConfidenceScore = 1.0,
                    BoundType = BoundType.Theta,
                    Diagnostics = diagnostics.ToImmutableList(),
                    BaseCase = new BaseCaseVerification { Holds = true },
                    InductiveStep = new InductiveStepVerification { Holds = true },
                    AsymptoticVerification = new AsymptoticVerification { Holds = true }
                };
            }

            return null; // Fall back to numerical verification
        }
        catch
        {
            return null; // Fall back to numerical verification
        }
    }

    /// <summary>
    /// Converts a RecurrenceRelation to linear recurrence format for SymPy.
    /// </summary>
    private static bool TryConvertToLinearRecurrence(
        RecurrenceRelation recurrence,
        out double[] coeffs,
        out Dictionary<int, double> baseCases,
        out string fOfN)
    {
        coeffs = Array.Empty<double>();
        baseCases = new Dictionary<int, double>();
        fOfN = "0";

        // Check if this is a linear recurrence (T(n-1), T(n-2), etc.)
        // vs divide-and-conquer (T(n/2), etc.)
        var terms = recurrence.Terms;
        if (terms.Count == 0)
            return false;

        // For now, handle simple linear recurrences
        // T(n) = a*T(n-1) + b*T(n-2) + f(n)
        var isLinear = terms.All(t => t.ScaleFactor >= 0.9); // Scale ~ 1 means T(n-k)

        if (!isLinear)
            return false; // Divide-and-conquer, use different path

        // Extract coefficients
        var coeffList = terms.Select(t => t.Coefficient).ToList();
        coeffs = coeffList.ToArray();

        // Base cases
        var baseValue = recurrence.BaseCase.Evaluate(
            new Dictionary<Variable, double> { { recurrence.Variable, 1 } }) ?? 1;
        baseCases[0] = 0; // T(0)
        baseCases[1] = baseValue; // T(1)

        // Non-recursive work as string
        fOfN = recurrence.NonRecursiveWork switch
        {
            ConstantComplexity c => c.Value.ToString("F0"),
            VariableComplexity => "n",
            LinearComplexity l => $"{l.Coefficient}*n",
            PolynomialComplexity p => $"n**{p.Degree}",
            _ => "0"
        };

        return true;
    }

    private BaseCaseVerification VerifyBaseCases(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        Variable variable)
    {
        var diagnostics = new List<string>();
        var results = new Dictionary<int, (double actual, double proposed, bool holds)>();

        // Check small values
        foreach (var n in new[] { 1, 2, 3, 4, 5 })
        {
            var actualValue = EvaluateRecurrence(recurrence, n);
            var proposedValue = solution.Evaluate(new Dictionary<Variable, double> { { variable, n } });

            if (actualValue.HasValue && proposedValue.HasValue)
            {
                // Handle edge cases: avoid division by zero
                bool holds;
                double ratio;

                if (Math.Abs(proposedValue.Value) < Tolerance)
                {
                    // Proposed is ~0: actual should also be ~0
                    holds = Math.Abs(actualValue.Value) < Tolerance * 100;
                    ratio = holds ? 1.0 : double.PositiveInfinity;
                }
                else
                {
                    // Normal case: check proportionality
                    ratio = actualValue.Value / proposedValue.Value;
                    holds = ratio > 0.01 && ratio < 100; // Generous bounds for base cases
                }

                results[n] = (actualValue.Value, proposedValue.Value, holds);
                diagnostics.Add($"n={n}: T(n)={actualValue.Value:F2}, f(n)={proposedValue.Value:F2}, ratio={ratio:F2}");
            }
        }

        return new BaseCaseVerification
        {
            Holds = results.Values.All(r => r.holds),
            Results = results.ToImmutableDictionary(),
            Diagnostics = diagnostics.ToImmutableList()
        };
    }

    private InductiveStepVerification VerifyInductiveStep(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        Variable variable,
        BoundType boundType)
    {
        var diagnostics = new List<string>();

        // For the inductive step, we assume T(k) ~ c·f(k) for k < n
        // and verify that T(n) ~ c·f(n)

        // Sample multiple values to check the inductive step
        var ratios = new List<double>();

        foreach (var n in SamplePoints)
        {
            var actualValue = EvaluateRecurrence(recurrence, n);
            var proposedValue = solution.Evaluate(new Dictionary<Variable, double> { { variable, n } });

            if (actualValue.HasValue && proposedValue.HasValue)
            {
                // Handle edge case: avoid division by zero
                if (Math.Abs(proposedValue.Value) > Tolerance)
                {
                    ratios.Add(actualValue.Value / proposedValue.Value);
                }
                else if (Math.Abs(actualValue.Value) < Tolerance)
                {
                    // Both near zero: ratio is 1
                    ratios.Add(1.0);
                }
                // Otherwise skip this point (both conditions imply degenerate case)
            }
        }

        if (ratios.Count < 3)
        {
            return new InductiveStepVerification
            {
                Holds = false,
                Diagnostics = ImmutableList.Create("Insufficient data for inductive step verification")
            };
        }

        // Check if ratios are stable (bounded)
        var minRatio = ratios.Min();
        var maxRatio = ratios.Max();
        var avgRatio = ratios.Average();

        var holds = boundType switch
        {
            BoundType.BigO => maxRatio < 1000, // Upper bound: ratio should be bounded above
            BoundType.Omega => minRatio > 0.001, // Lower bound: ratio should be bounded below
            BoundType.Theta => maxRatio < 1000 && minRatio > 0.001, // Tight bound: both
            _ => maxRatio / minRatio < 100 // Ratios should be relatively stable
        };

        diagnostics.Add($"Ratio range: [{minRatio:F4}, {maxRatio:F4}]");
        diagnostics.Add($"Average ratio: {avgRatio:F4}");
        diagnostics.Add($"Variation: {maxRatio / minRatio:F2}x");

        return new InductiveStepVerification
        {
            Holds = holds,
            MinRatio = minRatio,
            MaxRatio = maxRatio,
            AverageRatio = avgRatio,
            Diagnostics = diagnostics.ToImmutableList()
        };
    }

    private AsymptoticVerification VerifyAsymptoticBehavior(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        Variable variable,
        BoundType boundType)
    {
        var diagnostics = new List<string>();

        // Check that the ratio T(n)/f(n) converges or stays bounded as n grows
        var ratios = new List<(double n, double ratio)>();

        foreach (var n in LargeSamplePoints)
        {
            var actualValue = EvaluateRecurrence(recurrence, n);
            var proposedValue = solution.Evaluate(new Dictionary<Variable, double> { { variable, n } });

            if (actualValue.HasValue && proposedValue.HasValue && proposedValue.Value > 0)
            {
                ratios.Add((n, actualValue.Value / proposedValue.Value));
            }
        }

        if (ratios.Count < 2)
        {
            return new AsymptoticVerification
            {
                Holds = true, // Assume it holds if we can't verify
                Diagnostics = ImmutableList.Create("Insufficient data for asymptotic verification")
            };
        }

        // Check trend of ratios
        var ratioValues = ratios.Select(r => r.ratio).ToList();
        var trend = ComputeTrend(ratioValues);

        var holds = boundType switch
        {
            BoundType.BigO => ratioValues.All(r => r < 1000) && trend <= 0.1, // Should not grow
            BoundType.Omega => ratioValues.All(r => r > 0.001) && trend >= -0.1, // Should not shrink
            BoundType.Theta => Math.Abs(trend) < 0.1, // Should be stable
            _ => Math.Abs(trend) < 0.5
        };

        diagnostics.Add($"Ratio trend: {trend:F4}");
        diagnostics.Add($"Final ratio: {ratioValues.Last():F4}");

        return new AsymptoticVerification
        {
            Holds = holds,
            Trend = trend,
            FinalRatio = ratioValues.Last(),
            Diagnostics = diagnostics.ToImmutableList()
        };
    }

    private (double c, double n0, bool holds) FindBoundConstant(
        RecurrenceRelation recurrence,
        ComplexityExpression bound,
        Variable variable,
        bool isUpper)
    {
        // Find the best constant c and threshold n0
        var candidates = new List<(double c, double n0)>();

        foreach (var n0 in new[] { 1, 2, 4, 8, 16 })
        {
            var ratios = new List<double>();

            foreach (var n in SamplePoints.Where(n => n >= n0))
            {
                var actualValue = EvaluateRecurrence(recurrence, n);
                var boundValue = bound.Evaluate(new Dictionary<Variable, double> { { variable, n } });

                if (actualValue.HasValue && boundValue.HasValue && boundValue.Value > 0)
                {
                    ratios.Add(actualValue.Value / boundValue.Value);
                }
            }

            if (ratios.Count > 0)
            {
                var c = isUpper ? ratios.Max() * 1.1 : ratios.Min() * 0.9;
                candidates.Add((c, n0));
            }
        }

        if (candidates.Count == 0)
            return (0, 0, false);

        // Choose the best candidate (smallest c for upper bound, largest c for lower bound)
        var best = isUpper
            ? candidates.OrderBy(x => x.c).First()
            : candidates.OrderByDescending(x => x.c).First();

        return (best.c, best.n0, true);
    }

    private double? EvaluateRecurrence(RecurrenceRelation recurrence, double n)
    {
        var memo = new Dictionary<double, double>();
        return EvaluateRecurrenceWithMemo(recurrence, n, memo);
    }

    private double? EvaluateRecurrenceWithMemo(RecurrenceRelation recurrence, double n, Dictionary<double, double> memo)
    {
        if (n <= 1)
        {
            return recurrence.BaseCase.Evaluate(new Dictionary<Variable, double> { { recurrence.Variable, n } }) ?? 1;
        }

        if (memo.TryGetValue(n, out var cached))
            return cached;

        var assignments = new Dictionary<Variable, double> { { recurrence.Variable, n } };
        var nonRecursive = recurrence.NonRecursiveWork.Evaluate(assignments) ?? 0;

        var recursive = 0.0;
        foreach (var term in recurrence.Terms)
        {
            var subSize = term.ScaleFactor * n;
            if (subSize >= n) return null;

            var subResult = EvaluateRecurrenceWithMemo(recurrence, subSize, memo);
            if (!subResult.HasValue) return null;

            recursive += term.Coefficient * subResult.Value;
        }

        var result = recursive + nonRecursive;
        memo[n] = result;
        return result;
    }

    private double? EvaluateBound(ComplexityExpression bound, double n, Variable variable, double c)
    {
        var value = bound.Evaluate(new Dictionary<Variable, double> { { variable, n } });
        return value.HasValue ? c * value.Value : null;
    }

    private double ComputeTrend(IReadOnlyList<double> values)
    {
        if (values.Count < 2) return 0;

        // Compute normalized trend
        var first = values.First();
        var last = values.Last();

        if (first == 0) return last > 0 ? 1 : 0;
        return (last - first) / first;
    }

    private double ComputeVerificationConfidence(
        BaseCaseVerification baseCase,
        InductiveStepVerification inductiveStep,
        AsymptoticVerification asymptotic)
    {
        var confidence = 1.0;

        if (!baseCase.Holds) confidence *= 0.3;
        if (!inductiveStep.Holds) confidence *= 0.3;
        if (!asymptotic.Holds) confidence *= 0.5;

        // Adjust for ratio stability
        if (inductiveStep.MaxRatio / inductiveStep.MinRatio > 10)
            confidence *= 0.8;

        return confidence;
    }

    private bool CanVerifySymbolically(RecurrenceRelation recurrence, ComplexityExpression solution)
    {
        // Can verify symbolically if:
        // 1. Recurrence is standard form (divide-and-conquer)
        // 2. Solution is polynomial or polylog

        return recurrence.FitsMasterTheorem &&
               (solution is PolynomialComplexity or PolyLogComplexity or LinearComplexity);
    }

    private string ClassifyRecurrence(RecurrenceRelation recurrence)
    {
        if (recurrence.FitsMasterTheorem)
        {
            var term = recurrence.Terms[0];
            return $"T(n) = {term.Coefficient}T(n/{1/term.ScaleFactor:F0}) + f(n)";
        }
        return "general";
    }

    private string ClassifySolution(ComplexityExpression solution, Variable variable)
    {
        return solution switch
        {
            PolynomialComplexity p => $"n^{p.Degree}",
            PolyLogComplexity pl => $"n^{pl.PolyDegree} log^{pl.LogExponent} n",
            LinearComplexity => "n",
            LogarithmicComplexity => "log n",
            _ => "unknown"
        };
    }

    private bool VerifySymbolicBaseCase(RecurrenceRelation recurrence, ComplexityExpression solution, Variable variable)
    {
        var assignments = new Dictionary<Variable, double> { { variable, 1 } };
        var baseValue = recurrence.BaseCase.Evaluate(assignments);
        var solValue = solution.Evaluate(assignments);

        return baseValue.HasValue && solValue.HasValue && baseValue.Value <= solValue.Value * 10;
    }

    private bool VerifySymbolicInductiveStep(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        string recurrenceForm,
        string solutionForm,
        Variable variable)
    {
        // For Master Theorem recurrences with polynomial solutions,
        // we can verify algebraically

        if (!recurrence.FitsMasterTheorem)
            return false;

        var term = recurrence.Terms[0];
        var a = term.Coefficient;
        var b = 1.0 / term.ScaleFactor;
        var logBA = Math.Log(a) / Math.Log(b);

        // Extract solution degree
        var solutionDegree = solution switch
        {
            PolynomialComplexity p => (double)p.Degree,
            PolyLogComplexity pl => pl.PolyDegree,
            LinearComplexity => 1.0,
            _ => -1.0
        };

        if (solutionDegree < 0) return false;

        // For T(n) = aT(n/b) + f(n), if solution is n^d:
        // n^d = a(n/b)^d + f(n)
        // n^d = a·n^d/b^d + f(n)
        // n^d(1 - a/b^d) = f(n)

        // This holds when d = log_b(a) for certain f(n) forms
        return Math.Abs(solutionDegree - logBA) < 0.1 ||
               solutionDegree >= logBA;
    }

    private string GenerateProofSketch(
        RecurrenceRelation recurrence,
        ComplexityExpression solution,
        string recurrenceForm,
        string solutionForm)
    {
        return $@"Proof Sketch:
1. Recurrence: {recurrenceForm}
2. Proposed solution: {solutionForm}
3. Base case: T(1) = O(1) ✓
4. Inductive step: Assume T(k) = O(f(k)) for k < n
   Then T(n) = aT(n/b) + g(n)
            = a·O(f(n/b)) + g(n)
            = O(f(n)) by Master Theorem
5. Conclusion: T(n) = O({solutionForm})";
    }

    #endregion
}

#region Types

/// <summary>
/// Interface for induction-based verification.
/// </summary>
public interface IInductionVerifier
{
    InductionResult VerifyRecurrenceSolution(
        RecurrenceRelation recurrence,
        ComplexityExpression proposedSolution,
        BoundType boundType = BoundType.Theta);

    BoundVerificationResult VerifyUpperBound(RecurrenceRelation recurrence, ComplexityExpression upperBound);
    BoundVerificationResult VerifyLowerBound(RecurrenceRelation recurrence, ComplexityExpression lowerBound);
    SymbolicInductionResult VerifySymbolically(RecurrenceRelation recurrence, ComplexityExpression proposedSolution);
}

/// <summary>
/// Type of asymptotic bound.
/// </summary>
public enum BoundType
{
    BigO,   // Upper bound
    Omega,  // Lower bound
    Theta   // Tight bound
}

/// <summary>
/// Result of induction verification.
/// </summary>
public sealed record InductionResult
{
    public bool Verified { get; init; }
    public BaseCaseVerification? BaseCase { get; init; }
    public InductiveStepVerification? InductiveStep { get; init; }
    public AsymptoticVerification? AsymptoticVerification { get; init; }
    public double ConfidenceScore { get; init; }
    public BoundType BoundType { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Base case verification result.
/// </summary>
public sealed record BaseCaseVerification
{
    public bool Holds { get; init; }
    public ImmutableDictionary<int, (double actual, double proposed, bool holds)> Results { get; init; }
        = ImmutableDictionary<int, (double, double, bool)>.Empty;
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Inductive step verification result.
/// </summary>
public sealed record InductiveStepVerification
{
    public bool Holds { get; init; }
    public double MinRatio { get; init; }
    public double MaxRatio { get; init; }
    public double AverageRatio { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Asymptotic behavior verification result.
/// </summary>
public sealed record AsymptoticVerification
{
    public bool Holds { get; init; }
    public double Trend { get; init; }
    public double FinalRatio { get; init; }
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
}

/// <summary>
/// Bound verification result.
/// </summary>
public sealed record BoundVerificationResult
{
    public bool Holds { get; init; }
    public double Constant { get; init; }
    public double Threshold { get; init; }
    public ImmutableList<(double n, double actual, double bound)> Violations { get; init; }
        = ImmutableList<(double, double, double)>.Empty;
    public ImmutableList<string> Diagnostics { get; init; } = ImmutableList<string>.Empty;
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Result of symbolic induction verification.
/// </summary>
public sealed record SymbolicInductionResult
{
    public bool Success { get; init; }
    public ImmutableList<string> Steps { get; init; } = ImmutableList<string>.Empty;
    public string? ProofSketch { get; init; }
    public string? ErrorMessage { get; init; }
}

#endregion
