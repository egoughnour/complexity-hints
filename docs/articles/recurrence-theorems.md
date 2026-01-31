# Recurrence Theorem Applicability

The Complexity Analysis System implements two major recurrence-solving theorems for analyzing divide-and-conquer and recursive algorithms.

## Master Theorem

### Standard Form

The Master Theorem applies to recurrences of the form:

$$T(n) = a \cdot T\left(\frac{n}{b}\right) + f(n)$$

where:
- $a \geq 1$ (number of subproblems)
- $b > 1$ (factor by which input size is reduced)
- $f(n)$ is the non-recursive work (asymptotically positive)

### Applicability Conditions

1. **Single recursive term**: Exactly one $a \cdot T(n/b)$ term
2. **Constant subproblems**: $a$ must be a constant ≥ 1
3. **Constant division**: $b$ must be a constant > 1
4. **Regularity condition** (for Case 3): $a \cdot f(n/b) \leq c \cdot f(n)$ for some $c < 1$

### Three Cases

Let $d = \log_b(a)$ (the critical exponent):

| Case | Condition | Solution |
|------|-----------|----------|
| **Case 1** | $f(n) = O(n^{d-\varepsilon})$ for some $\varepsilon > 0$ | $\Theta(n^d)$ |
| **Case 2** | $f(n) = \Theta(n^d \cdot \log^k n)$ | $\Theta(n^d \cdot \log^{k+1} n)$ |
| **Case 3** | $f(n) = \Omega(n^{d+\varepsilon})$ with regularity | $\Theta(f(n))$ |

### Examples

| Recurrence | Case | Solution |
|------------|------|----------|
| $T(n) = 2T(n/2) + n$ | Case 2 (k=0) | $\Theta(n \log n)$ |
| $T(n) = 2T(n/2) + n^2$ | Case 3 | $\Theta(n^2)$ |
| $T(n) = 2T(n/2) + 1$ | Case 1 | $\Theta(n)$ |
| $T(n) = 4T(n/2) + n$ | Case 1 | $\Theta(n^2)$ |

### Master Theorem Gaps

The Master Theorem **cannot** handle:
- $f(n)$ between cases (e.g., $n^d / \log n$)
- Non-polynomial $f(n)$ like $n^d \cdot \log \log n$
- Variable $a$ or $b$
- Multiple recursive terms

---

## Akra-Bazzi Theorem

### Generalized Form

The Akra-Bazzi theorem handles the more general case:

$$T(n) = \sum_i a_i \cdot T(b_i \cdot n + h_i(n)) + g(n)$$

### Applicability Conditions

1. **Sufficient base cases**: $T(n)$ defined for $n \leq n_0$
2. **Positive coefficients**: $a_i > 0$ for all $i$
3. **Proper reduction**: $0 < b_i < 1$ for all $i$
4. **Bounded perturbation**: $|h_i(n)| = O(n / \log^2 n)$
5. **Regulated $g(n)$**: The non-recursive work must be a "regulated" function

### Solution Method

1. Find the unique **critical exponent** $p$ satisfying:
   $$\sum_i a_i \cdot b_i^p = 1$$

2. Then the solution is:
   $$T(n) = \Theta\left(n^p \cdot \left(1 + \int_1^n \frac{g(u)}{u^{p+1}} \, du\right)\right)$$

### What Akra-Bazzi Handles That Master Theorem Cannot

- **Multiple recursive terms**: $\sum_i a_i \cdot T(b_i \cdot n)$
- **Different subproblem sizes**: $b_1 \neq b_2$
- **Floor/ceiling effects** via $h_i(n)$ perturbation
- **More general $g(n)$** functions

### Examples

| Recurrence | Critical Exponent | Solution |
|------------|-------------------|----------|
| $T(n) = T(n/3) + T(2n/3) + n$ | $p = 1$ | $\Theta(n \log n)$ |
| $T(n) = T(n/4) + T(3n/4) + n$ | $p = 1$ | $\Theta(n \log n)$ |

---

## Implementation Details

### Critical Exponent Solver

The system uses Newton-Raphson root finding with Brent's method fallback:

```csharp
// For T(n) = 2T(n/4) + T(n/2) + n
var terms = new[] { (2.0, 0.25), (1.0, 0.5) };
double p = CriticalExponentSolver.Solve(terms);  // p ≈ 1.0
```

### Integral Evaluation

For common $g(n)$ forms, closed-form integrals are used:

| $g(n)$ | $\int_1^n \frac{g(u)}{u^{p+1}} du$ when... |
|--------|------------------------------------------|
| $n^k$ where $k < p$ | $O(1)$ |
| $n^k$ where $k = p$ | $O(\log n)$ |
| $n^k$ where $k > p$ | $O(n^{k-p})$ |

### Regularity Checking

For Master Theorem Case 3, the regularity condition is verified:

```csharp
var result = RegularityChecker.CheckRegularity(a: 2, b: 2, f, Variable.N);
// result.Holds, result.BestC, result.Reasoning
```

---

## Usage in Code Analysis

When the system detects a recursive method, it:

1. Extracts the recurrence structure from the call graph
2. Tries the Master Theorem first (simpler, more efficient)
3. Falls back to Akra-Bazzi for multi-term or gap cases
4. Uses **linear recurrence solving** for $T(n) = T(n-1) + f(n)$ and similar subtract-style recurrences

> For linear recurrences with subtract (not divide) subproblems, see [Linear Recurrence Relations](linear-recurrence.md).

```csharp
var analyzer = new TheoremApplicabilityAnalyzer();
var result = analyzer.Analyze(recurrence);

switch (result)
{
    case MasterTheoremApplicable mt:
        Console.WriteLine($"Master Theorem Case {mt.Case}: {mt.Solution}");
        break;
    case AkraBazziApplicable ab:
        Console.WriteLine($"Akra-Bazzi (p={ab.CriticalExponent}): {ab.Solution}");
        break;
    case TheoremNotApplicable na:
        Console.WriteLine($"Cannot solve: {na.Reason}");
        break;
}
```
