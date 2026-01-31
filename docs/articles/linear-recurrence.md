# Linear Recurrence Relations

The Complexity Analysis System solves linear recurrence relations using the **characteristic polynomial method**. This complements the Master Theorem and Akra-Bazzi theorem for divide-and-conquer recurrences.

## What Are Linear Recurrences?

A **linear recurrence relation** has the form:

$$T(n) = \sum_{i=1}^{k} a_i \cdot T(n-i) + f(n)$$

where:
- $a_i$ are constant coefficients
- $k$ is the **order** of the recurrence
- $f(n)$ is the **non-homogeneous term** (non-recursive work)

### Common Examples

| Recurrence | Name | Solution |
|------------|------|----------|
| $T(n) = T(n-1) + 1$ | Simple summation | $\Theta(n)$ |
| $T(n) = T(n-1) + n$ | Arithmetic series | $\Theta(n^2)$ |
| $T(n) = 2 \cdot T(n-1) + 1$ | Exponential | $\Theta(2^n)$ |
| $T(n) = T(n-1) + T(n-2)$ | Fibonacci | $\Theta(\varphi^n)$ |

---

## The Characteristic Polynomial Method

### Step 1: Form the Characteristic Equation

For a recurrence $T(n) = a_1 T(n-1) + a_2 T(n-2) + \ldots + a_k T(n-k)$, the **characteristic polynomial** is:

$$x^k - a_1 x^{k-1} - a_2 x^{k-2} - \ldots - a_k = 0$$

### Step 2: Find the Roots

The roots of the characteristic polynomial determine the solution form:

| Root Type | Contribution to Solution |
|-----------|--------------------------|
| Distinct real $r$ | $c \cdot r^n$ |
| Repeated root $r$ (multiplicity $m$) | $(c_0 + c_1 n + \ldots + c_{m-1} n^{m-1}) \cdot r^n$ |
| Complex conjugates $\alpha \pm \beta i$ | $r^n \cdot (c_1 \cos(n\theta) + c_2 \sin(n\theta))$ |

where $r = \sqrt{\alpha^2 + \beta^2}$ and $\theta = \arctan(\beta/\alpha)$.

### Step 3: Determine Asymptotic Complexity

The **dominant root** (largest magnitude) determines the asymptotic growth:

- For distinct dominant root $r$: $T(n) = \Theta(r^n)$
- For repeated dominant root $r$ (multiplicity $m$): $T(n) = \Theta(n^{m-1} \cdot r^n)$

---

## Solution Examples

### Example 1: Fibonacci Sequence

$$T(n) = T(n-1) + T(n-2)$$

**Characteristic equation**: $x^2 - x - 1 = 0$

**Roots**: 
$$r_1 = \frac{1 + \sqrt{5}}{2} = \varphi \approx 1.618 \quad (\text{golden ratio})$$
$$r_2 = \frac{1 - \sqrt{5}}{2} \approx -0.618$$

**Solution**: $T(n) = \Theta(\varphi^n)$

### Example 2: Repeated Root

$$T(n) = 2 \cdot T(n-1) - T(n-2)$$

**Characteristic equation**: $x^2 - 2x + 1 = 0$

**Roots**: $r = 1$ with multiplicity 2

**Solution**: $T(n) = \Theta(n \cdot 1^n) = \Theta(n)$ (linear, not exponential!)

### Example 3: Complex Roots

$$T(n) = T(n-2)$$

**Characteristic equation**: $x^2 - 1 = 0$

**Roots**: $r_1 = 1$, $r_2 = -1$ (can be viewed as complex conjugates on the unit circle)

**Solution**: $T(n) = \Theta(1)$ (constant)

---

## Summation Recurrences

A **summation recurrence** is a special case:

$$T(n) = T(n-1) + f(n)$$

These have an explicit closed form:

$$T(n) = T(0) + \sum_{i=1}^{n} f(i)$$

| Non-recursive Work $f(n)$ | Solution |
|---------------------------|----------|
| $O(1)$ | $O(n)$ |
| $O(n)$ | $O(n^2)$ |
| $O(n^k)$ | $O(n^{k+1})$ |
| $O(\log n)$ | $O(n \log n)$ |

The solver directly handles summation recurrences without computing characteristic roots.

---

## Non-Homogeneous Recurrences

When $f(n) \neq 0$, the recurrence is **non-homogeneous**:

$$T(n) = \sum_{i=1}^{k} a_i \cdot T(n-i) + f(n)$$

The solution is:

$$T(n) = T_h(n) + T_p(n)$$

where:
- $T_h(n)$ is the **homogeneous solution** (from characteristic roots)
- $T_p(n)$ is a **particular solution** (often the integral of $f$)

### Asymptotic Behavior

The dominant term is whichever grows faster:
- If $r_{\max}^n$ dominates: $T(n) = \Theta(r_{\max}^n)$
- If $f(n)$ dominates: $T(n) = \Theta(T_p(n))$

---

## Implementation

### LinearRecurrenceRelation

Represents a linear recurrence:

```csharp
// Create T(n) = T(n-1) + T(n-2) (Fibonacci)
var fibonacci = LinearRecurrenceRelation.Create(
    coefficients: new[] { 1.0, 1.0 },
    nonRecursiveWork: ConstantComplexity.O1,
    variable: Variable.N);

// Factory methods for common forms
var summation = LinearRecurrenceRelation.Summation(f_n, Variable.N);
var exponential = LinearRecurrenceRelation.Exponential(base: 2, Variable.N);
var fib = LinearRecurrenceRelation.Fibonacci(Variable.N);
```

### ILinearRecurrenceSolver

Solves recurrences:

```csharp
var solver = new CharacteristicEquationSolver(classifier);
LinearRecurrenceSolution result = solver.Solve(recurrence);

Console.WriteLine($"Solution: {result.Solution}");
Console.WriteLine($"Method: {result.Method}");
Console.WriteLine($"Dominant root: {result.DominantRoot}");

foreach (var root in result.Roots)
{
    Console.WriteLine($"  Root: {root.RealPart} + {root.ImaginaryPart}i " +
                      $"(multiplicity {root.Multiplicity})");
}
```

### Solution Methods

The solver automatically chooses the best approach:

1. **Summation** for $T(n) = T(n-1) + f(n)$
2. **Quadratic Formula** for order-2 recurrences (faster, numerically stable)
3. **Companion Matrix** for order ≥ 3 (uses eigenvalue decomposition)

---

## Comparison with Divide-and-Conquer Theorems

| Aspect | Linear Recurrence | Master/Akra-Bazzi |
|--------|-------------------|-------------------|
| **Subproblem structure** | $T(n-i)$ (subtract) | $T(n/b)$ (divide) |
| **Typical growth** | Exponential | Polynomial |
| **Example algorithms** | Dynamic programming, backtracking | Merge sort, divide-and-conquer |
| **Solution method** | Characteristic roots | Critical exponent |

### When Each Applies

- **Linear recurrence**: Sequential algorithms, DP, linear scans with memoization
- **Master Theorem**: Simple divide-and-conquer with single recursive call type
- **Akra-Bazzi**: Complex divide-and-conquer with multiple recursive call types

---

## Common Pitfalls

### 1. Confusing Subtract vs Divide

- $T(n) = 2T(n-1)$ → **Linear recurrence**, $\Theta(2^n)$ exponential
- $T(n) = 2T(n/2)$ → **Divide-and-conquer**, $\Theta(n)$ polynomial

### 2. Root Multiplicity Matters

- Single root $r$: $\Theta(r^n)$
- Double root $r$: $\Theta(n \cdot r^n)$

### 3. Complex Roots

Complex roots indicate **oscillating behavior** but magnitude still determines asymptotic growth. For the Complexity Analysis System, we report the magnitude as the growth rate.

---

## API Reference

See the API documentation for:
- [`LinearRecurrenceRelation`](../api/ComplexityAnalysis.Core.Recurrence.LinearRecurrenceRelation.html) - Recurrence representation
- [`ILinearRecurrenceSolver`](../api/ComplexityAnalysis.Solver.ILinearRecurrenceSolver.html) - Solver interface
- [`CharacteristicEquationSolver`](../api/ComplexityAnalysis.Solver.CharacteristicEquationSolver.html) - Default implementation
- [`LinearRecurrenceSolution`](../api/ComplexityAnalysis.Solver.LinearRecurrenceSolution.html) - Solution record
- [`CharacteristicRoot`](../api/ComplexityAnalysis.Solver.CharacteristicRoot.html) - Root representation
