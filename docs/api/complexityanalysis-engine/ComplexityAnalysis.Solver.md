# ComplexityAnalysis.Solver #

## Type IntegralEvaluationResult

 Result of evaluating the Akra-Bazzi integral. 



---
#### Property IntegralEvaluationResult.Success

Whether the integral could be evaluated (closed-form or symbolic).



---
#### Property IntegralEvaluationResult.IntegralTerm

 The asymptotic form of the integral term. For Akra-Bazzi: Θ(n^p · (1 + ∫₁ⁿ g(u)/u^(p+1) du)) This captures the integral contribution. 



---
#### Property IntegralEvaluationResult.FullSolution

 The complete Akra-Bazzi solution combining n^p with the integral. 



---
#### Property IntegralEvaluationResult.Explanation

Human-readable explanation of the evaluation.



---
#### Property IntegralEvaluationResult.Confidence

Confidence in the result (0.0 to 1.0).



---
#### Property IntegralEvaluationResult.IsSymbolic

Whether the result is symbolic (requires further refinement).



---
#### Property IntegralEvaluationResult.SpecialFunction

Special function type if applicable.



---
## Type SpecialFunctionType

 Types of special functions that may arise in integral evaluation. 



---
#### Field SpecialFunctionType.Polylogarithm

Polylogarithm Li_s(z)



---
#### Field SpecialFunctionType.IncompleteGamma

Incomplete gamma function γ(s, x)



---
#### Field SpecialFunctionType.IncompleteBeta

Incomplete beta function B(x; a, b)



---
#### Field SpecialFunctionType.Hypergeometric2F1

Gauss hypergeometric ₂F₁(a, b; c; z)



---
#### Field SpecialFunctionType.SymbolicIntegral

Deferred symbolic integral



---
## Type IAkraBazziIntegralEvaluator

 Evaluates the integral term in the Akra-Bazzi theorem. Akra-Bazzi solution: T(n) = Θ(n^p · (1 + ∫₁ⁿ g(u)/u^(p+1) du)) For common g(u) forms, this integral has closed-form solutions: | g(n) | k vs p | Integral Result | Full Solution | |----------------|-----------|--------------------------|----------------------| | n^k | k < p | O(1) | Θ(n^p) | | n^k | k = p | O(log n) | Θ(n^p · log n) | | n^k | k > p | O(n^(k-p)) | Θ(n^k) | | n^k · log^j n | k < p | O(1) | Θ(n^p) | | n^k · log^j n | k = p | O(log^(j+1) n) | Θ(n^p · log^(j+1) n) | | n^k · log^j n | k > p | O(n^(k-p) · log^j n) | Θ(n^k · log^j n) | 



---
#### Method IAkraBazziIntegralEvaluator.Evaluate(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Evaluates the Akra-Bazzi integral for the given g(n) and critical exponent p. 

|Name | Description |
|-----|------|
|g: |The non-recursive work function g(n).|
|variable: |The variable (typically n).|
|p: |The critical exponent satisfying Σᵢ aᵢ · bᵢ^p = 1.|
**Returns**: The evaluation result with the full solution.



---
## Type TableDrivenIntegralEvaluator

 Table-driven implementation for common integral forms with special function fallback. Handles standard cases with closed forms and falls back to special functions (hypergeometric, polylogarithm, gamma, beta) or symbolic integrals for complex cases that require later refinement. 



---
#### Field TableDrivenIntegralEvaluator.Tolerance

Tolerance for comparing k to p.



---
#### Method TableDrivenIntegralEvaluator.EvaluateExponential(ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = c · b^n (exponential) ∫₁ⁿ b^u / u^(p+1) du This integral relates to the incomplete gamma function when transformed. For b > 1 and large n, the exponential dominates. 



---
#### Method TableDrivenIntegralEvaluator.EvaluateGeneric(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Generic fallback for unrecognized g(n) forms. Creates a symbolic integral with asymptotic bounds estimated heuristically. 



---
#### Method TableDrivenIntegralEvaluator.EvaluateConstant(ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = c (constant) ∫₁ⁿ c/u^(p+1) du = c · [-1/(p·u^p)]₁ⁿ = c/p · (1 - 1/n^p) For p > 0: this is O(1), so solution is Θ(n^p) For p = 0: ∫₁ⁿ c/u du = c · log(n), so solution is Θ(log n) For p < 0: this grows, dominated by n^(-p) term 



---
#### Method TableDrivenIntegralEvaluator.EvaluatePolynomial(ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = n^k (pure polynomial) ∫₁ⁿ u^k/u^(p+1) du = ∫₁ⁿ u^(k-p-1) du If k - p - 1 = -1 (i.e., k = p): ∫ du/u = log(n) If k - p - 1 ≠ -1: [u^(k-p)/(k-p)]₁ⁿ = (n^(k-p) - 1)/(k-p) - If k < p: this is O(1) - If k > p: this is O(n^(k-p)) 



---
#### Method TableDrivenIntegralEvaluator.EvaluateLogarithmic(ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = log^j(n) (pure logarithmic, k = 0) This is a special case of polylog with k = 0. 



---
#### Method TableDrivenIntegralEvaluator.EvaluatePolyLog(ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = n^k · log^j(n) (polylogarithmic) ∫₁ⁿ u^k · log^j(u) / u^(p+1) du = ∫₁ⁿ u^(k-p-1) · log^j(u) du Case k = p: ∫ log^j(u)/u du = log^(j+1)(n)/(j+1) Case k < p: Integral converges to O(1) Case k > p: Integral ~ n^(k-p) · log^j(n) 



---
## Type ExtendedIntegralEvaluator

 Extended integral evaluator with hypergeometric and special function support. This evaluator handles more complex g(n) forms that require special functions: - Fractional polynomial exponents → Hypergeometric ₂F₁ - Products/ratios of polynomials → Beta functions - Exponential-polynomial products → Incomplete gamma - Iterated logarithms → Polylogarithm 



---
#### Method ExtendedIntegralEvaluator.TryFractionalPolynomial(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = n^k where k is not an integer (fractional exponents). ∫₁ⁿ u^(k-p-1) du = [u^(k-p) / (k-p)]₁ⁿ when k ≠ p Still elementary, but we ensure numerical stability for non-integer exponents. 



---
#### Method ExtendedIntegralEvaluator.TryPolynomialRatio(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = n^a / (1 + n^b)^c - polynomial ratio forms These lead to Beta/hypergeometric functions via substitution u = n^b / (1 + n^b) 



---
#### Method ExtendedIntegralEvaluator.TryIteratedLogarithm(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = log(log(n))^j - iterated logarithms These arise in algorithms with deep recursive structures. Integral: ∫ log(log(u))^j / u^(p+1) du involves polylogarithms. 



---
#### Method ExtendedIntegralEvaluator.TryProductForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 g(n) = f₁(n) · f₂(n) - product forms Try to decompose and evaluate based on dominant factor. 



---
#### Method ExtendedIntegralEvaluator.CreateSymbolicWithHeuristic(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Creates a symbolic integral with heuristic asymptotic bounds. 



---
## Type SymPyIntegralEvaluator

 SymPy-based integral evaluator that calls Python subprocess for symbolic computation. Uses SymPy's powerful symbolic integration engine to evaluate arbitrary g(n): ∫₁ⁿ g(u)/u^(p+1) du Falls back to table-driven evaluation for common cases, using SymPy only when the expression form is complex or unknown. 



---
#### Method SymPyIntegralEvaluator.EvaluateWithSymPyAsync(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double,System.Threading.CancellationToken)

 Evaluates the integral using SymPy asynchronously. 



---
#### Method SymPyIntegralEvaluator.ToSymPyString(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Converts a ComplexityExpression to a SymPy-parseable string. 



---
#### Method SymPyIntegralEvaluator.ParseComplexityFromSymPy(System.String,ComplexityAnalysis.Core.Complexity.Variable)

 Parses a SymPy complexity string back into a ComplexityExpression. 



---
## Type ICriticalExponentSolver

 Solves for the critical exponent p in Akra-Bazzi theorem. Uses MathNet.Numerics for root finding. 



---
#### Method ICriticalExponentSolver.Solve(System.Collections.Generic.IReadOnlyList{System.ValueTuple{System.Double,System.Double}},System.Double,System.Int32)

 Solves Σᵢ aᵢ · bᵢ^p = 1 for p. 

|Name | Description |
|-----|------|
|terms: |The (aᵢ, bᵢ) pairs from the recurrence.|
|tolerance: |Convergence tolerance.|
|maxIterations: |Maximum iterations for root finding.|
**Returns**: The critical exponent p, or null if no solution found.



---
#### Method ICriticalExponentSolver.EvaluateSum(System.Collections.Generic.IReadOnlyList{System.ValueTuple{System.Double,System.Double}},System.Double)

 Evaluates Σᵢ aᵢ · bᵢ^p for a given p. 



---
#### Method ICriticalExponentSolver.EvaluateDerivative(System.Collections.Generic.IReadOnlyList{System.ValueTuple{System.Double,System.Double}},System.Double)

 Evaluates the derivative d/dp[Σᵢ aᵢ · bᵢ^p] = Σᵢ aᵢ · bᵢ^p · ln(bᵢ). 



---
## Type MathNetCriticalExponentSolver

 Standard implementation using MathNet.Numerics root finding. 



---
## Type KnownCriticalExponents

 Known solutions for common recurrence patterns. Used for verification and optimization. 



---
#### Method KnownCriticalExponents.MasterTheorem(System.Double,System.Double)

 For T(n) = aT(n/b) + f(n): p = log_b(a). 



---
#### Property KnownCriticalExponents.BinaryDivideAndConquer

 For T(n) = 2T(n/2) + f(n): p = 1. 



---
#### Property KnownCriticalExponents.BinarySearch

 For T(n) = T(n/2) + f(n): p = 0. 



---
#### Property KnownCriticalExponents.Karatsuba

 For T(n) = 3T(n/2) + f(n): p = log_2(3) ≈ 1.585. 



---
#### Property KnownCriticalExponents.Strassen

 For T(n) = 7T(n/2) + f(n): p = log_2(7) ≈ 2.807 (Strassen). 



---
## Type LinearRecurrenceSolver

 Solves linear recurrence relations using the characteristic polynomial method. 



>**Problem Form:** T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k) + f(n) 

**Solution Algorithm:**

- **Characteristic Polynomial:** Form p(x) = x^k - a₁x^(k-1) - ... - aₖ 
- **Root Finding:** Find all roots using companion matrix eigendecomposition 
- **Homogeneous Solution:** Build from roots with multiplicities 
- **Particular Solution:** Handle non-homogeneous term f(n) 
- **Asymptotic Form:** Extract dominant term for Big-O notation 


**Root Types and Their Contributions:**

**Root Type**: Contribution to Solution
- **Real root r (simple)**: c·rⁿ
- **Real root r (multiplicity m)**: (c₀ + c₁n + ... + c_{m-1}n^{m-1})·rⁿ
- **Complex pair α±βi**: ρⁿ(c₁cos(nθ) + c₂sin(nθ)) where ρ = √(α²+β²)


**Common Solutions:**



######  code

```
    // T(n) = T(n-1) + 1 → O(n)
    // T(n) = T(n-1) + n → O(n²)
    // T(n) = 2T(n-1) + 1 → O(2ⁿ)
    // T(n) = T(n-1) + T(n-2) → O(φⁿ) ≈ O(1.618ⁿ)
    // T(n) = 4T(n-1) - 4T(n-2) → O(n·2ⁿ) (repeated root)
```





---
#### Field LinearRecurrenceSolver.Epsilon

Tolerance for root comparison and numerical stability.



---
#### Field LinearRecurrenceSolver.RootEqualityTolerance

Tolerance for considering roots equal (for multiplicity detection).



---
#### Property LinearRecurrenceSolver.Instance

 Default singleton instance. 



---
#### Method LinearRecurrenceSolver.Solve(ComplexityAnalysis.Core.Recurrence.LinearRecurrenceRelation)

 Solves a linear recurrence relation and returns the asymptotic complexity. 

|Name | Description |
|-----|------|
|recurrence: |The linear recurrence to solve.|
**Returns**: The solution, or null if the recurrence cannot be solved.



---
#### Method LinearRecurrenceSolver.SolveSummation(ComplexityAnalysis.Core.Recurrence.LinearRecurrenceRelation)

 Solves a simple summation recurrence T(n) = T(n-1) + f(n). 



---
#### Method LinearRecurrenceSolver.FindCharacteristicRoots(System.Collections.Immutable.ImmutableArray{System.Double})

 Finds the roots of the characteristic polynomial using companion matrix eigendecomposition. 



> For a recurrence T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k), the characteristic polynomial is: x^k - a₁x^(k-1) - a₂x^(k-2) - ... - aₖ = 0 We find roots by computing eigenvalues of the companion matrix. 



---
#### Method LinearRecurrenceSolver.SolveQuadratic(System.Double,System.Double)

 Solves a quadratic characteristic equation: x² - a₁x - a₂ = 0. 



---
#### Method LinearRecurrenceSolver.SolveUsingCompanionMatrix(System.Collections.Immutable.ImmutableArray{System.Double})

 Solves a characteristic equation using companion matrix eigendecomposition. 



---
#### Method LinearRecurrenceSolver.GroupRootsByMultiplicity(System.Collections.Immutable.ImmutableArray{ComplexityAnalysis.Core.Recurrence.CharacteristicRoot})

 Groups roots by value and determines multiplicities. 



---
#### Method LinearRecurrenceSolver.BuildHomogeneousSolution(ComplexityAnalysis.Core.Recurrence.CharacteristicRoot,ComplexityAnalysis.Core.Complexity.Variable)

 Builds the asymptotic solution from the dominant root. 



---
#### Method LinearRecurrenceSolver.CombineWithParticular(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Recurrence.CharacteristicRoot,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Combines homogeneous solution with particular solution for non-homogeneous term. 



---
#### Method LinearRecurrenceSolver.CompareAndTakeMax(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Compares two complexities and returns the asymptotically larger one. 



---
## Type ILinearRecurrenceSolver

 Interface for linear recurrence solvers. 



---
#### Method ILinearRecurrenceSolver.Solve(ComplexityAnalysis.Core.Recurrence.LinearRecurrenceRelation)

 Solves a linear recurrence relation. 



---
## Type CharacteristicPolynomialSolved

 Theorem applicability result for linear recurrences solved by characteristic polynomial. 



---
#### Method CharacteristicPolynomialSolved.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Recurrence.LinearRecurrenceSolution)

 Theorem applicability result for linear recurrences solved by characteristic polynomial. 



---
## Type MutualRecurrenceSolver

 Solves mutual recursion systems by converting them to equivalent single recurrences. Key insight: A mutual recursion cycle A → B → C → A can be "unrolled" to a single recurrence by substitution. If each step reduces by 1: A(n) calls B(n-1), B(n) calls C(n-1), C(n) calls A(n-1) Combined: A(n) = f_A + f_B + f_C + A(n-3) This is T(n) = T(n-k) + g(n) where k = cycle length 



---
#### Method MutualRecurrenceSolver.Solve(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem)

 Solves a mutual recursion system. 



---
#### Method MutualRecurrenceSolver.SolveSubtractionPattern(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Solves subtraction-based mutual recursion: A(n) → B(n-1) → C(n-1) → A(n-1) For cycle of length k with each step reducing by 1: T(n) = T(n-k) + g(n) where g(n) is combined work This sums to: T(n) = Σᵢ g(n - i*k) + T(base) for i from 0 to n/k Approximately: T(n) = (n/k) * g(n) = Θ(n * g(n) / k) = Θ(n * g(n)) 



---
#### Method MutualRecurrenceSolver.SolveDivisionPattern(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Solves division-based mutual recursion: A(n) → B(n/2) → C(n/2) → A(n/2) For cycle of length k with each step dividing by b: Combined scale factor: b^k (e.g., if k=3 and b=2, scale = 1/8) Use standard theorem solving on the combined recurrence. 



---
#### Method MutualRecurrenceSolver.SolveMixedPattern(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Handles mixed patterns where methods use different reduction strategies. 



---
#### Method MutualRecurrenceSolver.SolveByHeuristic(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Heuristic solver when standard theorems don't apply. 



---
## Type MutualRecurrenceSolverExtensions

 Extension methods for mutual recursion solving. 



---
#### Method MutualRecurrenceSolverExtensions.Solve(ComplexityAnalysis.Core.Recurrence.MutualRecurrenceSystem)

 Solves a mutual recurrence system using the default solver. 



---
## Type Refinement.ConfidenceScorer

 Computes confidence scores for complexity analysis results. Takes into account multiple factors including: - Source of the analysis (theoretical vs numerical) - Verification results - Stability of numerical fits - Theorem applicability 



---
#### Field Refinement.ConfidenceScorer.SourceWeights

Base confidence weights for different analysis sources.



---
#### Method Refinement.ConfidenceScorer.ComputeConfidence(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Solver.Refinement.AnalysisContext)

 Computes an overall confidence score for a complexity result. 



---
#### Method Refinement.ConfidenceScorer.ComputeTheoremConfidence(ComplexityAnalysis.Core.Recurrence.TheoremApplicability)

 Computes confidence for a theorem applicability result. 



---
#### Method Refinement.ConfidenceScorer.ComputeRefinementConfidence(ComplexityAnalysis.Solver.Refinement.RefinementResult)

 Computes confidence for a refinement result. 



---
#### Method Refinement.ConfidenceScorer.ComputeConsensusConfidence(System.Collections.Generic.IReadOnlyList{System.Double})

 Computes combined confidence when multiple analyses agree. 



---
## Type Refinement.IConfidenceScorer

 Interface for confidence scoring. 



---
## Type Refinement.ConfidenceAssessment

 Complete confidence assessment for a complexity result. 



---
## Type Refinement.ConfidenceFactor

 A single factor contributing to confidence. 



---
#### Method Refinement.ConfidenceFactor.#ctor(System.String,System.Double,System.String)

 A single factor contributing to confidence. 



---
## Type Refinement.ConfidenceLevel

 Confidence level classification. 



---
## Type Refinement.AnalysisSource

 Source of complexity analysis. 



---
## Type Refinement.VerificationStatus

 Verification status of a result. 



---
## Type Refinement.AnalysisContext

 Context for confidence analysis. 



---
## Type Refinement.NumericalFitData

 Data from numerical fitting. 



---
#### Method Refinement.InductionVerifier.#ctor(ComplexityAnalysis.Solver.SymPyRecurrenceSolver)

 Creates an InductionVerifier. If sympySolver is provided, uses SymPy for exact verification. 



---
#### Field Refinement.InductionVerifier.Tolerance

Tolerance for numerical comparisons.



---
#### Field Refinement.InductionVerifier.SamplePoints

Sample points for numerical verification.



---
#### Field Refinement.InductionVerifier.LargeSamplePoints

Large sample points for asymptotic verification.



---
#### Property Refinement.InductionVerifier.Instance

Default instance without SymPy support.



---
#### Method Refinement.InductionVerifier.WithSymPy(ComplexityAnalysis.Solver.SymPyRecurrenceSolver)

Creates an instance with SymPy support for exact verification.



---
#### Method Refinement.InductionVerifier.VerifyRecurrenceSolution(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Solver.Refinement.BoundType)

 Verifies that a solution satisfies a recurrence relation. If SymPy solver is available, uses exact symbolic verification first. 



---
#### Method Refinement.InductionVerifier.VerifyUpperBound(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Verifies an upper bound: T(n) = O(f(n)). 



---
#### Method Refinement.InductionVerifier.VerifyLowerBound(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Verifies a lower bound: T(n) = Ω(f(n)). 



---
#### Method Refinement.InductionVerifier.VerifySymbolically(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Performs symbolic induction verification when possible. 



---
#### Method Refinement.InductionVerifier.TryVerifyWithSymPy(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Attempts verification using SymPy. Returns null if SymPy verification fails or is unavailable. 



---
#### Method Refinement.InductionVerifier.TryConvertToLinearRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,System.Double[]@,System.Collections.Generic.Dictionary{System.Int32,System.Double}@,System.String@)

 Converts a RecurrenceRelation to linear recurrence format for SymPy. 



---
## Type Refinement.IInductionVerifier

 Interface for induction-based verification. 



---
## Type Refinement.BoundType

 Type of asymptotic bound. 



---
## Type Refinement.InductionResult

 Result of induction verification. 



---
## Type Refinement.BaseCaseVerification

 Base case verification result. 



---
## Type Refinement.InductiveStepVerification

 Inductive step verification result. 



---
## Type Refinement.AsymptoticVerification

 Asymptotic behavior verification result. 



---
## Type Refinement.BoundVerificationResult

 Bound verification result. 



---
## Type Refinement.SymbolicInductionResult

 Result of symbolic induction verification. 



---
## Type Refinement.PerturbationExpansion

 Handles near-boundary cases where standard theorems have gaps. Uses perturbation analysis and Taylor expansion to derive tighter bounds. Key cases: 1. Master Theorem gap: f(n) = Θ(n^d) where d ≈ log_b(a) 2. Akra-Bazzi boundary: p ≈ integer values 3. Logarithmic factor boundaries: log^k(n) where k is non-integer 



---
#### Field Refinement.PerturbationExpansion.NearThreshold

Threshold for considering values "near" each other.



---
#### Field Refinement.PerturbationExpansion.MaxTaylorOrder

Maximum order of Taylor expansion.



---
#### Field Refinement.PerturbationExpansion.Tolerance

Tolerance for numerical comparisons.



---
#### Method Refinement.PerturbationExpansion.ExpandNearBoundary(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Solver.Refinement.BoundaryCase)

 Expands a recurrence solution near a boundary case. 



---
#### Method Refinement.PerturbationExpansion.DetectBoundary(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Recurrence.TheoremApplicability)

 Detects if a recurrence is near a boundary case. 



---
#### Method Refinement.PerturbationExpansion.TaylorExpandIntegral(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double,System.Double)

 Performs Taylor expansion of the Akra-Bazzi integral near a singular point. 



---
## Type Refinement.IPerturbationExpansion

 Interface for perturbation expansion. 



---
## Type Refinement.PerturbationResult

 Result of perturbation expansion. 



---
## Type Refinement.BoundaryCase

 Description of a boundary case. 



---
## Type Refinement.BoundaryCaseType

 Types of boundary cases. 



---
#### Field Refinement.BoundaryCaseType.MasterTheoremCase1To2

Near boundary between Master Theorem Case 1 and Case 2.



---
#### Field Refinement.BoundaryCaseType.MasterTheoremCase2To3

Near boundary between Master Theorem Case 2 and Case 3.



---
#### Field Refinement.BoundaryCaseType.AkraBazziIntegerExponent

Akra-Bazzi critical exponent near an integer.



---
#### Field Refinement.BoundaryCaseType.LogarithmicBoundary

Logarithmic exponent boundary.



---
## Type Refinement.TaylorExpansionResult

 Result of Taylor expansion. 



---
## Type Refinement.TaylorTerm

 A term in a Taylor expansion. 



---
#### Method Refinement.TaylorTerm.#ctor(System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 A term in a Taylor expansion. 



---
## Type Refinement.RefinementEngine

 Main refinement engine that coordinates all refinement components. Implements Phase C of the complexity analysis pipeline. Pipeline: 1. Receive initial solution from theorem solver (Phase B) 2. Detect boundary cases and apply perturbation expansion 3. Optimize slack variables for tighter bounds 4. Verify via induction 5. Compute confidence score 



---
#### Method Refinement.RefinementEngine.Refine(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Recurrence.TheoremApplicability,ComplexityAnalysis.Core.Progress.IAnalysisProgress)

 Refines a complexity solution through the full pipeline. 



---
#### Method Refinement.RefinementEngine.QuickRefine(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Performs quick refinement without full verification. 



---
#### Method Refinement.RefinementEngine.VerifyBound(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Solver.Refinement.BoundType)

 Verifies a proposed bound without refinement. 



---
## Type Refinement.IRefinementEngine

 Interface for the refinement engine. 



---
## Type Refinement.RefinementPipelineResult

 Complete result of the refinement pipeline. 



---
#### Property Refinement.RefinementPipelineResult.WasImproved

 Returns true if the solution was improved during refinement. 



---
## Type Refinement.RefinementStage

 A single stage in the refinement pipeline. 



---
## Type Refinement.QuickRefinementResult

 Result of quick refinement. 



---
## Type Refinement.SlackVariableOptimizer

 Optimizes complexity bounds by finding the tightest valid constants. Uses numerical verification to determine actual constant factors and asymptotic tightness. For example, if analysis yields O(n²), this optimizer can determine if the actual bound is Θ(n²) or if a tighter O(n log n) might apply. 



---
#### Field Refinement.SlackVariableOptimizer._samplePoints

Sample points for numerical verification.



---
#### Field Refinement.SlackVariableOptimizer.Tolerance

Tolerance for ratio comparisons.



---
#### Field Refinement.SlackVariableOptimizer.MaxIterations

Maximum iterations for optimization.



---
#### Method Refinement.SlackVariableOptimizer.Refine(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Refines a complexity bound by finding tighter constants. 



---
#### Method Refinement.SlackVariableOptimizer.RefineRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,ComplexityAnalysis.Core.Recurrence.TheoremApplicability)

 Refines a recurrence solution with verification. 



---
#### Method Refinement.SlackVariableOptimizer.RefineGap(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation,System.Double,System.Double)

 Finds tighter bounds for Master Theorem gap cases. 



---
## Type Refinement.ISlackVariableOptimizer

 Interface for slack variable optimization. 



---
## Type Refinement.RefinementResult

 Result of general refinement. 



---
## Type Refinement.RecurrenceRefinementResult

 Result of recurrence refinement. 



---
## Type Refinement.GapRefinementResult

 Result of gap refinement. 



---
## Type Refinement.VerificationResult

 Verification result for numerical checking. 



---
## Type Refinement.GrowthAnalysis

 Analysis of growth pattern. 



---
#### Method Refinement.GrowthAnalysis.#ctor(ComplexityAnalysis.Solver.Refinement.GrowthType,System.Double,System.Double)

 Analysis of growth pattern. 



---
## Type Refinement.GrowthType

 Types of growth patterns. 



---
## Type RegularityResult

 Result of checking the regularity condition for Master Theorem Case 3. The regularity condition requires: a·f(n/b) ≤ c·f(n) for some c < 1 and all sufficiently large n. 



---
#### Property RegularityResult.Holds

Whether the regularity condition holds.



---
#### Property RegularityResult.BestC

 The best (smallest) constant c found such that a·f(n/b) ≤ c·f(n). Null if regularity doesn't hold or couldn't be determined. 



---
#### Property RegularityResult.Reasoning

Human-readable explanation of the verification.



---
#### Property RegularityResult.Confidence

Confidence level (0.0 to 1.0) in the result.



---
#### Property RegularityResult.SamplePoints

The sample points used for numerical verification.



---
#### Method RegularityResult.Success(System.Double,System.String,System.Double)

 Creates a result indicating regularity holds. 



---
#### Method RegularityResult.Failure(System.String,System.Double)

 Creates a result indicating regularity does not hold. 



---
#### Method RegularityResult.Indeterminate(System.String)

 Creates a result indicating regularity could not be determined. 



---
## Type IRegularityChecker

 Verifies the regularity condition for Master Theorem Case 3. The regularity condition states: a·f(n/b) ≤ c·f(n) for some c < 1 and all sufficiently large n. This is equivalent to requiring that f(n) grows "regularly" without wild oscillations that could invalidate Case 3. 



---
#### Method IRegularityChecker.CheckRegularity(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Checks if the regularity condition holds for the given parameters. 

|Name | Description |
|-----|------|
|a: |Number of subproblems (a in T(n) = aT(n/b) + f(n)).|
|b: |Division factor (b in T(n) = aT(n/b) + f(n)).|
|f: |The non-recursive work function f(n).|
|variable: |The variable (typically n).|
**Returns**: Result indicating whether regularity holds.



---
## Type NumericalRegularityChecker

 Numerical implementation of regularity checking using sampling. For common polynomial forms, regularity can be verified analytically: - f(n) = n^k: a·(n/b)^k ≤ c·n^k → a/b^k ≤ c, so c = a/b^k For Case 3, k > log_b(a), so b^k > a, thus a/b^k < 1 ✓ For more complex forms, we use numerical sampling. 



---
#### Field NumericalRegularityChecker.DefaultSamplePoints

Default sample points for numerical verification.



---
#### Field NumericalRegularityChecker.Tolerance

Tolerance for numerical comparisons.



---
#### Field NumericalRegularityChecker.MaxC

Maximum acceptable c value (must be strictly less than 1).



---
#### Method NumericalRegularityChecker.TryAnalyticalVerification(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Attempts analytical verification for common f(n) forms. 



---
#### Method NumericalRegularityChecker.NumericalVerification(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double[])

 Numerical verification by sampling f(n) at multiple points. 



---
## Type SymPyRecurrenceSolver

 Solves recurrence relations using SymPy via a Python subprocess. Uses 'uv run' for zero-config isolated execution. 



---
#### Method SymPyRecurrenceSolver.SolveLinearAsync(System.Double[],System.Collections.Generic.Dictionary{System.Int32,System.Double},System.String,System.Threading.CancellationToken)

 Solves a linear recurrence: T(n) = sum(coeffs[i] * T(n-1-i)) + f(n) 



---
#### Method SymPyRecurrenceSolver.SolveDivideAndConquerAsync(System.Double,System.Double,System.String,System.Threading.CancellationToken)

 Solves a divide-and-conquer recurrence: T(n) = a*T(n/b) + f(n) 



---
#### Method SymPyRecurrenceSolver.VerifyAsync(System.String,System.String,System.Collections.Generic.Dictionary{System.Int32,System.Double},System.Threading.CancellationToken)

 Verifies that a proposed solution satisfies a recurrence. 



---
#### Method SymPyRecurrenceSolver.CompareAsync(System.String,System.String,System.String,System.Threading.CancellationToken)

 Compares asymptotic growth of two expressions using limits. Uses L'Hôpital's rule via SymPy for proper handling of indeterminate forms. 

|Name | Description |
|-----|------|
|f: |First expression (e.g., "n**2")|
|g: |Second expression (e.g., "n * log(n)")|
|boundType: |Type of bound to verify: "O", "Omega", or "Theta"|


---
## Type AsymptoticComparisonResult

 Result of asymptotic comparison between two expressions. 



---
#### Property AsymptoticComparisonResult.BoundType

Type of bound verified: "O", "Omega", or "Theta".



---
#### Property AsymptoticComparisonResult.Holds

Whether the bound holds.



---
#### Property AsymptoticComparisonResult.Constant

The constant c for O or Ω bounds.



---
#### Property AsymptoticComparisonResult.Constants

The constants (c1, c2) for Θ bounds.



---
#### Property AsymptoticComparisonResult.Comparison

 Comparison result: "f < g" (f = o(g)), "f ~ g" (f = Θ(g)), or "f > g" (f = ω(g)). 



---
#### Property AsymptoticComparisonResult.LimitRatio

The limit of f/g as n → ∞.



---
## Type RecurrenceSolution

 Result of solving a recurrence relation. 



---
## Type TheoremApplicabilityAnalyzer

 Main analyzer that determines which recurrence-solving theorem applies and computes the closed-form solution. 



>**Analysis Order:**

- **Master Theorem** - Tried first for single-term divide-and-conquer recurrences. Simpler conditions, more precise when applicable. 
- **Akra-Bazzi Theorem** - Falls back for multi-term recurrences or when Master Theorem has gaps. 
- **Linear Recurrence** - For T(n) = T(n-1) + f(n), solved by summation. 
- **Failure with Diagnostics** - Reports why analysis failed with suggestions. 


**Master Theorem:** For T(n) = a·T(n/b) + f(n) where a ≥ 1, b > 1: 

**Case**: Condition and Solution
- **Case 1**:  f(n) = O(n^(log_b(a) - ε)) for some ε > 0 ⟹ T(n) = Θ(n^log_b(a))   
Work dominated by leaves (recursion-heavy) 
- **Case 2**:  f(n) = Θ(n^log_b(a) · log^k n) for k ≥ 0 ⟹ T(n) = Θ(n^log_b(a) · log^(k+1) n)   
Work balanced across all levels 
- **Case 3**:  f(n) = Ω(n^(log_b(a) + ε)) for some ε > 0, and regularity holds ⟹ T(n) = Θ(f(n))   
Work dominated by root (merge-heavy) 


**Master Theorem Gaps:** The theorem has gaps when f(n) falls between cases without satisfying the polynomial separation requirement (ε > 0). For example, f(n) = n^log_b(a) / log(n) is asymptotically smaller than Θ(n^log_b(a)) but not polynomially smaller. 

**Akra-Bazzi Theorem:** For T(n) = Σᵢ aᵢ·T(bᵢn) + g(n) where aᵢ > 0 and 0 < bᵢ < 1: 



######  code

```
    T(n) = Θ(n^p · (1 + ∫₁ⁿ g(u)/u^(p+1) du))
```

 where p is the unique solution to Σᵢ aᵢ·bᵢ^p = 1. 

 Akra-Bazzi handles more cases than Master Theorem: 

- Multiple recursive terms (e.g., T(n) = T(n/3) + T(2n/3) + O(n))
- Non-equal subproblem sizes
- No polynomial gap requirement (covers Master Theorem gaps)




**See also**: [`MasterTheoremApplicable`](MasterTheoremApplicable)

**See also**: [`AkraBazziApplicable`](AkraBazziApplicable)

**See also**: [`TheoremApplicability`](TheoremApplicability)



---
#### Field TheoremApplicabilityAnalyzer.Epsilon

Tolerance for numerical comparisons.



---
#### Field TheoremApplicabilityAnalyzer.MinEpsilon

Minimum epsilon for Master Theorem cases 1 and 3.



---
#### Method TheoremApplicabilityAnalyzer.Analyze(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Analyzes a recurrence and determines which theorem applies. Tries Master Theorem first, then Akra-Bazzi, then linear recurrence. 



---
#### Method TheoremApplicabilityAnalyzer.AnalyzeWithAkraBazzi(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Forces Akra-Bazzi analysis even for single-term recurrences. Useful for cross-validation testing. 



---
#### Method TheoremApplicabilityAnalyzer.ValidateRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Validates that the recurrence is well-formed. 



---
#### Method TheoremApplicabilityAnalyzer.CheckMasterTheorem(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Checks Master Theorem applicability for T(n) = a·T(n/b) + f(n). 



> The Master Theorem requires: 

- Exactly one recursive term
- a ≥ 1 (at least one recursive call)
- b > 1 (subproblem must be smaller)


**Case Determination:** Computes log_b(a) and classifies f(n) to determine which case applies. The [[|F:ComplexityAnalysis.Solver.TheoremApplicabilityAnalyzer.MinEpsilon]] threshold (0.01) determines when f(n) is "polynomially" different from n^log_b(a). 

**Case 3 Regularity:** Requires that a·f(n/b) ≤ c·f(n) for some c < 1. This is verified by [[|T:ComplexityAnalysis.Solver.IRegularityChecker]]. 





---
#### Method TheoremApplicabilityAnalyzer.CheckAkraBazzi(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Checks Akra-Bazzi theorem applicability for multi-term recurrences. 



> The Akra-Bazzi theorem applies to recurrences of the form: T(n) = Σᵢ aᵢ·T(bᵢn + hᵢ(n)) + g(n) 

**Requirements:**

- All aᵢ > 0 (positive coefficients)
- All bᵢ ∈ (0, 1) (proper size reduction)
- g(n) satisfies polynomial growth condition


**Solution Process:**

-  Solve Σᵢ aᵢ·bᵢ^p = 1 for critical exponent p using Newton's method 
-  Evaluate ∫₁ⁿ g(u)/u^(p+1) du (the "driving function" integral) 
-  Combine: T(n) = Θ(n^p · (1 + integral result)) 


**Advantages over Master Theorem:**

- Handles multiple recursive terms
- No gaps between cases
- More general driving functions g(n)






---
#### Method TheoremApplicabilityAnalyzer.CheckLinearRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Checks for linear recurrence T(n) = T(n-1) + f(n). 



---
## Type RecurrenceAnalysisExtensions

 Extension methods for convenient analysis. 



---
#### Method RecurrenceAnalysisExtensions.Analyze(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Analyzes a recurrence relation using the default analyzer. 



---
#### Method RecurrenceAnalysisExtensions.Analyze(ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity)

 Analyzes a RecurrenceComplexity using the default analyzer. 



---
#### Method RecurrenceAnalysisExtensions.BinaryDivideAndConquer(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a binary divide-and-conquer recurrence T(n) = 2T(n/2) + f(n). 



---
#### Method RecurrenceAnalysisExtensions.KaratsubaStyle(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a Karatsuba-style recurrence T(n) = 3T(n/2) + f(n). 



---
#### Method RecurrenceAnalysisExtensions.StrassenStyle(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a Strassen-style recurrence T(n) = 7T(n/2) + f(n). 



---


