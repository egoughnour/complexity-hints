# Complexity Expression Types

The Complexity Analysis System uses a rich type hierarchy to represent algorithmic complexity. All types inherit from the abstract `ComplexityExpression` base class.

## Type Hierarchy

```
ComplexityExpression (abstract base)
├── ConstantComplexity(double Value)           // O(1), O(k)
├── VariableComplexity(Variable Var)           // O(n), O(V), O(E)
├── LinearComplexity(double Coef, Variable)    // O(kn)
├── PolynomialComplexity(Dict<int,double>, Var)// O(n²), O(n³), general polynomial
├── LogarithmicComplexity(double, Var, Base)   // O(log n), O(k log n)
├── PolyLogComplexity(k, j, Var)               // O(n^k · log^j n)
├── ExponentialComplexity(Base, Var, Coef)     // O(2ⁿ), O(k·bⁿ)
├── FactorialComplexity(Variable, Coef)        // O(n!)
├── BinaryOperationComplexity(L, Op, R)        // Composition: +, ×, max, min
├── ConditionalComplexity(Desc, True, False)   // Branch-dependent
├── PowerComplexity(Base, Exponent)            // expr^k
├── LogOfComplexity(Argument, Base)            // log(expr)
├── ExponentialOfComplexity(Base, Exponent)    // base^expr
├── FactorialOfComplexity(Argument)            // (expr)!
├── RecurrenceComplexity(Terms, Var, Work, Base) // T(n) = Σ aᵢT(bᵢn) + g(n)
├── AmortizedComplexity(Amortized, WorstCase)  // Amortized analysis
├── ParallelComplexity(Work, Span)             // Parallel algorithms
├── MemoryComplexity(Stack, Heap, Auxiliary)   // Space complexity
└── ProbabilisticComplexity(Expected, Worst)   // Randomized algorithms
```

## Variable Types

Variables represent different kinds of input sizes:

| Variable Type | Description | Example |
|--------------|-------------|---------|
| `InputSize` | General input n | Array length |
| `DataCount` | Collection size | List.Count |
| `VertexCount` | Graph vertices V | Graph.Vertices |
| `EdgeCount` | Graph edges E | Graph.Edges |
| `TreeHeight` | Tree height h | BST depth |
| `StringLength` | String length | String.Length |
| `SecondarySize` | Secondary dimension m | Matrix width |

## Binary Operations

Complexity expressions can be composed using binary operations:

| Operation | Semantics | Use Case |
|-----------|-----------|----------|
| `Plus` | T₁ + T₂ | Sequential composition |
| `Multiply` | T₁ × T₂ | Nested loops, product |
| `Max` | max(T₁, T₂) | Branching (worst case) |
| `Min` | min(T₁, T₂) | Best case, early exit |

## Composition Rules

The `ComplexityComposition` class provides factory methods:

```csharp
// Sequential: A; B → O(A + B)
var sequential = ComplexityComposition.Sequential(a, b);

// Nested: for i in n: A → O(n × A)
var nested = ComplexityComposition.Loop(n, body);

// Branching: if c: A else B → O(max(A, B))
var branch = ComplexityComposition.Branching(thenBranch, elseBranch);

// Divide and conquer recurrence
var recurrence = ComplexityComposition.DivideAndConquer(Variable.N, a: 2, b: 2, work);
```

## Common Patterns

### O(n log n) - PolyLogComplexity

```csharp
// Merge sort, heap sort, etc.
var nLogN = PolyLogComplexity.NLogN(Variable.N);

// General form: O(n^k · log^j n)
var custom = new PolyLogComplexity(
    polyDegree: 2,    // n²
    logExponent: 1,   // log n
    variable: Variable.N
);  // O(n² log n)
```

### Recurrence Relations

```csharp
// T(n) = 2T(n/2) + O(n) → O(n log n)
var mergeSort = RecurrenceComplexity.DivideAndConquer(
    Variable.N,
    subproblems: 2,
    divisionFactor: 2,
    work: new LinearComplexity(1, Variable.N)
);
```

## Key Methods

Every `ComplexityExpression` provides:

- `Accept<T>(IComplexityVisitor<T>)` - Visitor pattern traversal
- `Substitute(Variable, ComplexityExpression)` - Variable substitution
- `FreeVariables` - All unbound variables
- `Evaluate(assignments)` - Numerical evaluation
- `ToBigONotation()` - Human-readable Big-O string
