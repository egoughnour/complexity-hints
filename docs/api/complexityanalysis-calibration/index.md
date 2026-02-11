# ComplexityAnalysis.Core #

## Type Complexity.AmortizedComplexity

 Represents amortized complexity - the average cost per operation over a sequence. Amortized analysis accounts for expensive operations that happen infrequently, giving a more accurate picture of average-case performance. Examples: - Dynamic array Add: O(n) worst case, O(1) amortized - Hash table insert: O(n) worst case, O(1) amortized - Splay tree operations: O(n) worst case, O(log n) amortized 



---
#### Property Complexity.AmortizedComplexity.AmortizedCost

 The amortized (average) complexity per operation. 



---
#### Property Complexity.AmortizedComplexity.WorstCaseCost

 The worst-case complexity for a single operation. 



---
#### Property Complexity.AmortizedComplexity.Method

 The method used to derive the amortized bound. 



---
#### Property Complexity.AmortizedComplexity.Potential

 Optional potential function used for analysis. 



---
#### Property Complexity.AmortizedComplexity.Description

 Description of the amortization scenario. 



---
#### Method Complexity.AmortizedComplexity.ConstantAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an amortized constant complexity (like List.Add). 



---
#### Method Complexity.AmortizedComplexity.LogarithmicAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an amortized logarithmic complexity (like splay tree operations). 



---
#### Method Complexity.AmortizedComplexity.InverseAckermannAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an inverse Ackermann amortized complexity (like Union-Find). 



---
## Type Complexity.AmortizationMethod

 Methods for deriving amortized bounds. 



---
#### Field Complexity.AmortizationMethod.Aggregate

 Aggregate method: Total cost / number of operations. Simple but doesn't give per-operation insight. 



---
#### Field Complexity.AmortizationMethod.Accounting

 Accounting method: Assign credits to operations. Cheap operations pay for expensive ones. 



---
#### Field Complexity.AmortizationMethod.Potential

 Potential method: Define potential function Φ(state). Amortized cost = actual cost + ΔΦ. Most powerful, gives tight bounds. 



---
## Type Complexity.PotentialFunction

 Represents a potential function for amortized analysis. Φ: DataStructureState → ℝ≥0 



---
#### Property Complexity.PotentialFunction.Name

 Name/description of the potential function. 



---
#### Property Complexity.PotentialFunction.Formula

 Mathematical description of the potential function. 



---
#### Property Complexity.PotentialFunction.SizeVariable

 The variable representing the data structure size. 



---
## Type Complexity.PotentialFunction.Common

 Common potential functions. 



---
#### Property Complexity.PotentialFunction.Common.DynamicArray

 Dynamic array: Φ = 2n - capacity 



---
#### Property Complexity.PotentialFunction.Common.HashTable

 Hash table: Φ = 2n - buckets 



---
#### Property Complexity.PotentialFunction.Common.BinaryCounter

 Binary counter: Φ = number of 1-bits 



---
#### Property Complexity.PotentialFunction.Common.MultipopStack

 Stack with multipop: Φ = stack size 



---
#### Property Complexity.PotentialFunction.Common.SplayTree

 Splay tree: Φ = Σ log(size of subtree) 



---
#### Property Complexity.PotentialFunction.Common.UnionFind

 Union-Find: Φ based on ranks 



---
## Type Complexity.InverseAckermannComplexity

 Inverse Ackermann complexity: O(α(n)) - effectively constant for practical inputs. Used in Union-Find with path compression and union by rank. 



---
#### Method Complexity.InverseAckermannComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable)

 Inverse Ackermann complexity: O(α(n)) - effectively constant for practical inputs. Used in Union-Find with path compression and union by rank. 



---
#### Method Complexity.InverseAckermannComplexity.InverseAckermann(System.Int64)

 Computes inverse Ackermann function α(n). α(n) = min { k : A(k, k) ≥ n } where A is Ackermann function. For all practical n, α(n) ≤ 4. 



---
## Type Complexity.IAmortizedComplexityVisitor`1

 Extended visitor interface for amortized complexity types. 



---
## Type Complexity.ComplexityComposition

 Provides methods for composing complexity expressions based on control flow patterns. These rules form the foundation of static complexity analysis. 



---
#### Method Complexity.ComplexityComposition.Sequential(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Sequential composition: T₁ followed by T₂. Total complexity: O(T₁ + T₂) In Big-O terms, the dominant term will dominate: O(n) + O(n²) = O(n²) 



---
#### Method Complexity.ComplexityComposition.Sequential(ComplexityAnalysis.Core.Complexity.ComplexityExpression[])

 Sequential composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Sequential(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Sequential composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Nested(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Nested composition: T₁ inside T₂ (e.g., nested loops). Total complexity: O(T₁ × T₂) Example: for i in 0..n: for j in 0..n: O(1) Result: O(n) × O(n) × O(1) = O(n²) 



---
#### Method Complexity.ComplexityComposition.Nested(ComplexityAnalysis.Core.Complexity.ComplexityExpression[])

 Nested composition of multiple expressions (deeply nested loops). 



---
#### Method Complexity.ComplexityComposition.Nested(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Nested composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Branching(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Branching composition: if-else statement. Total complexity: O(max(T_true, T_false)) We take the worst case because either branch might execute. 



---
#### Method Complexity.ComplexityComposition.BranchingWithCondition(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Branching composition with condition overhead. Total complexity: O(T_condition + max(T_true, T_false)) 



---
#### Method Complexity.ComplexityComposition.Switch(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Multi-way branching (switch/match). Total complexity: O(max(T₁, T₂, ..., Tₙ)) 



---
#### Method Complexity.ComplexityComposition.Loop(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Loop composition with known iteration count. Total complexity: O(iterations × body) 



---
#### Method Complexity.ComplexityComposition.ForLoop(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 For loop with linear iterations: for i = 0 to n. Total complexity: O(n × body) 



---
#### Method Complexity.ComplexityComposition.BoundedForLoop(System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 For loop with bounded iterations: for i = 0 to constant. Total complexity: O(body) (the constant factor is absorbed) 



---
#### Method Complexity.ComplexityComposition.LogarithmicLoop(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Logarithmic loop: for i = 1; i < n; i *= 2. Total complexity: O(log n × body) 



---
#### Method Complexity.ComplexityComposition.LoopWithEarlyExit(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Early exit pattern: loop that may terminate early. Total complexity: O(min(early_exit, full_iterations) × body) For worst-case analysis, we typically use the full iterations. For average-case, the expected early exit point matters. 



---
#### Method Complexity.ComplexityComposition.LinearRecursion(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Recursive composition: function calls itself. Returns a RecurrenceComplexity that needs to be solved. For T(n) = T(n-1) + work, this creates: RecurrenceComplexity with linear reduction. 



---
#### Method Complexity.ComplexityComposition.DivideAndConquer(ComplexityAnalysis.Core.Complexity.Variable,System.Int32,System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Divide and conquer recursion: T(n) = a × T(n/b) + work. Returns a RecurrenceComplexity that can be solved via Master Theorem. 



---
#### Method Complexity.ComplexityComposition.BinaryRecursion(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Binary recursion: T(n) = 2T(n/2) + work. Common pattern for divide and conquer algorithms. 



---
#### Method Complexity.ComplexityComposition.FunctionCall(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Function call composition: calling a function with known complexity. Total complexity: O(argument_setup + function_complexity) 



---
#### Method Complexity.ComplexityComposition.Amortized(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Amortized operation: multiple operations with varying individual costs but known total cost over n operations. Example: n insertions into a dynamic array = O(n) total, O(1) amortized per op. 



---
#### Method Complexity.ComplexityComposition.Conditional(System.String,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Conditional complexity: different complexity based on runtime condition. 



---
## Type Complexity.ComplexityBuilder

 Fluent builder for constructing complex complexity expressions. 



---
#### Method Complexity.ComplexityBuilder.Constant

 Start building with O(1). 



---
#### Method Complexity.ComplexityBuilder.Linear(ComplexityAnalysis.Core.Complexity.Variable)

 Start building with O(n). 



---
#### Method Complexity.ComplexityBuilder.Then(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Add sequential operation: current + next. 



---
#### Method Complexity.ComplexityBuilder.InsideLoop(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Nest inside a loop: iterations × current. 



---
#### Method Complexity.ComplexityBuilder.InsideLinearLoop(ComplexityAnalysis.Core.Complexity.Variable)

 Nest inside a loop over n. 



---
#### Method Complexity.ComplexityBuilder.OrBranch(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Add branch: max(current, alternative). 



---
#### Method Complexity.ComplexityBuilder.Times(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Multiply by a factor. 



---
#### Method Complexity.ComplexityBuilder.Build

 Build the final expression. 



---
#### Method Complexity.ComplexityBuilder.op_Implicit(ComplexityAnalysis.Core.Complexity.ComplexityBuilder)~ComplexityAnalysis.Core.Complexity.ComplexityExpression

 Implicit conversion to ComplexityExpression. 



---
## Type Complexity.ComplexityExpression

 Base type for all complexity expressions representing algorithmic time or space complexity. 



>**Design Philosophy:** This forms the core of an expression-based complexity algebra that represents Big-O expressions as composable Abstract Syntax Trees (AST). This design enables: 

- Type-safe composition of complexity expressions
- Algebraic simplification (e.g., O(n) + O(n²) → O(n²))
- Variable substitution for parametric complexity
- Evaluation for specific input sizes
- Visitor pattern for transformation and analysis


**Type Hierarchy:**

**Category**: Types
- **Primitive**: [[|T:ComplexityAnalysis.Core.Complexity.ConstantComplexity]] (O(1)), [[|T:ComplexityAnalysis.Core.Complexity.VariableComplexity]] (O(n)), [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]] (O(k·n)) 
- **Polynomial**: [[|T:ComplexityAnalysis.Core.Complexity.PolynomialComplexity]] (O(n²), O(n³), etc.), [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] (O(n log n)) 
- **Transcendental**: [[|T:ComplexityAnalysis.Core.Complexity.LogarithmicComplexity]] (O(log n)), [[|T:ComplexityAnalysis.Core.Complexity.ExponentialComplexity]] (O(2ⁿ)), [[|T:ComplexityAnalysis.Core.Complexity.FactorialComplexity]] (O(n!)) 
- **Compositional**: [[|T:ComplexityAnalysis.Core.Complexity.BinaryOperationComplexity]] (+, ×, max, min), [[|T:ComplexityAnalysis.Core.Complexity.ConditionalComplexity]] (branching) 


**Composition Rules:**



######  code

```
    // Sequential (addition): loops following loops
    var seq = new BinaryOperationComplexity(O_n, BinaryOp.Plus, O_logN);
    // → O(n + log n) → O(n) after simplification
    
    // Nested (multiplication): loops inside loops
    var nested = new BinaryOperationComplexity(O_n, BinaryOp.Multiply, O_m);
    // → O(n × m)
    
    // Branching (max): if-else with different complexities
    var branch = new BinaryOperationComplexity(O_n, BinaryOp.Max, O_nSquared);
    // → O(max(n, n²)) → O(n²)
```

 All expressions are implemented as immutable records for thread-safety and functional composition patterns. 



**See also**: [`IComplexityVisitor`1`](IComplexityVisitor`1)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Method Complexity.ComplexityExpression.Accept``1(ComplexityAnalysis.Core.Complexity.IComplexityVisitor{``0})

 Accept a visitor for the expression tree. 



---
#### Method Complexity.ComplexityExpression.Substitute(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Substitute a variable with another expression. 



---
#### Property Complexity.ComplexityExpression.FreeVariables

 Get all free (unbound) variables in this expression. 



---
#### Method Complexity.ComplexityExpression.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})

 Evaluate the expression for a given variable assignment. Returns null if evaluation is not possible (e.g., missing variables). 



---
#### Method Complexity.ComplexityExpression.ToBigONotation

 Get a human-readable string representation in Big-O notation. 



---
## Type Complexity.ConstantComplexity

 Represents a constant complexity: O(1) or O(k) for some constant k. 



> Constant complexity represents operations whose execution time does not depend on input size. Common sources include: 

- Array indexing:  `arr[i]` 
- Hash table lookup (amortized):  `dict[key]` 
- Arithmetic operations:  `a + b * c` 
- Base cases of recursive algorithms


 The [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Value]] property captures any constant factor, though in asymptotic analysis O(1) = O(k) for any constant k. 



|Name | Description |
|-----|------|
|Value: |The constant value (typically 1).|


---
#### Method Complexity.ConstantComplexity.#ctor(System.Double)

 Represents a constant complexity: O(1) or O(k) for some constant k. 



> Constant complexity represents operations whose execution time does not depend on input size. Common sources include: 

- Array indexing:  `arr[i]` 
- Hash table lookup (amortized):  `dict[key]` 
- Arithmetic operations:  `a + b * c` 
- Base cases of recursive algorithms


 The [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Value]] property captures any constant factor, though in asymptotic analysis O(1) = O(k) for any constant k. 



|Name | Description |
|-----|------|
|Value: |The constant value (typically 1).|


---
#### Property Complexity.ConstantComplexity.Value

The constant value (typically 1).



---
#### Property Complexity.ConstantComplexity.One

 The canonical O(1) constant complexity. 



---
#### Property Complexity.ConstantComplexity.Zero

 Zero complexity (for base cases). 



---
## Type Complexity.VariableComplexity

 Represents a single variable complexity: O(n), O(V), O(E), etc. 



> This is the simplest form of linear complexity—a single variable without a coefficient. For complexity with coefficients, see [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]]. 

 Common variable types defined in [[|T:ComplexityAnalysis.Core.Complexity.Variable]]: 

-  `n`  - General input size
-  `V`  - Vertex count in graphs
-  `E`  - Edge count in graphs
-  `m`  - Secondary size parameter (e.g., pattern length)




|Name | Description |
|-----|------|
|Var: |The variable representing the input size.|
**See also**: [`Variable`](Variable)

**See also**: [`VariableType`](VariableType)



---
#### Method Complexity.VariableComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable)

 Represents a single variable complexity: O(n), O(V), O(E), etc. 



> This is the simplest form of linear complexity—a single variable without a coefficient. For complexity with coefficients, see [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]]. 

 Common variable types defined in [[|T:ComplexityAnalysis.Core.Complexity.Variable]]: 

-  `n`  - General input size
-  `V`  - Vertex count in graphs
-  `E`  - Edge count in graphs
-  `m`  - Secondary size parameter (e.g., pattern length)




|Name | Description |
|-----|------|
|Var: |The variable representing the input size.|
**See also**: [`Variable`](Variable)

**See also**: [`VariableType`](VariableType)



---
#### Property Complexity.VariableComplexity.Var

The variable representing the input size.



---
## Type Complexity.LinearComplexity

 Represents linear complexity with a coefficient: O(k·n). 



---
#### Method Complexity.LinearComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Represents linear complexity with a coefficient: O(k·n). 



---
## Type Complexity.PolynomialComplexity

 Represents polynomial complexity: O(n²), O(n³), or general polynomial forms. 



> Polynomials represent algorithms with nested loops or recursive patterns that process proportional fractions of input at each level. 

**Structure:** The [[|P:ComplexityAnalysis.Core.Complexity.PolynomialComplexity.Coefficients]] dictionary maps degree → coefficient. For example: 

-  `{2: 1}`  represents n²
-  `{2: 3, 1: 2}`  represents 3n² + 2n
-  `{3: 1, 2: 1, 1: 1}`  represents n³ + n² + n


**Common algorithmic sources:**

- O(n²): Bubble sort, insertion sort, naive matrix operations
- O(n³): Standard matrix multiplication, Floyd-Warshall
- O(n⁴): Naive bipartite matching


**Note:** For non-integer exponents (e.g., O(n^2.807) for Strassen), use [[|T:ComplexityAnalysis.Core.Complexity.PowerComplexity]] or [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] instead. 



|Name | Description |
|-----|------|
|Coefficients: |Dictionary mapping degree → coefficient.|
|Var: |The variable over which the polynomial is defined.|


---
#### Method Complexity.PolynomialComplexity.#ctor(System.Collections.Immutable.ImmutableDictionary{System.Int32,System.Double},ComplexityAnalysis.Core.Complexity.Variable)

 Represents polynomial complexity: O(n²), O(n³), or general polynomial forms. 



> Polynomials represent algorithms with nested loops or recursive patterns that process proportional fractions of input at each level. 

**Structure:** The [[|P:ComplexityAnalysis.Core.Complexity.PolynomialComplexity.Coefficients]] dictionary maps degree → coefficient. For example: 

-  `{2: 1}`  represents n²
-  `{2: 3, 1: 2}`  represents 3n² + 2n
-  `{3: 1, 2: 1, 1: 1}`  represents n³ + n² + n


**Common algorithmic sources:**

- O(n²): Bubble sort, insertion sort, naive matrix operations
- O(n³): Standard matrix multiplication, Floyd-Warshall
- O(n⁴): Naive bipartite matching


**Note:** For non-integer exponents (e.g., O(n^2.807) for Strassen), use [[|T:ComplexityAnalysis.Core.Complexity.PowerComplexity]] or [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] instead. 



|Name | Description |
|-----|------|
|Coefficients: |Dictionary mapping degree → coefficient.|
|Var: |The variable over which the polynomial is defined.|


---
#### Property Complexity.PolynomialComplexity.Coefficients

Dictionary mapping degree → coefficient.



---
#### Property Complexity.PolynomialComplexity.Var

The variable over which the polynomial is defined.



---
#### Property Complexity.PolynomialComplexity.Degree

 The highest degree in the polynomial (dominant term). 



---
#### Property Complexity.PolynomialComplexity.LeadingCoefficient

 The coefficient of the highest degree term. 



---
#### Method Complexity.PolynomialComplexity.OfDegree(System.Int32,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple polynomial of the form O(n^k). 



---
#### Method Complexity.PolynomialComplexity.OfDegree(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a polynomial approximation for non-integer degrees. Note: This rounds to the nearest integer since PolynomialComplexity only supports integer exponents. For exact non-integer exponents, use PowerComplexity instead. 



---
## Type Complexity.LogarithmicComplexity

 Represents logarithmic complexity: O(log n), O(k·log n), with configurable base. 



> Logarithmic complexity typically arises from algorithms that halve (or divide by a constant) the problem size at each step. 

**Common algorithmic sources:**

- Binary search: O(log n)
- Balanced BST operations: O(log n)
- Exponentiation by squaring: O(log n)


**Base equivalence:** In asymptotic analysis, log₂(n) = Θ(logₖ(n)) for any constant k > 1, since logₖ(n) = log₂(n) / log₂(k). The base is preserved for precision in constant factor analysis. 



|Name | Description |
|-----|------|
|Coefficient: |Multiplicative coefficient (default 1).|
|Var: |The variable inside the logarithm.|
|Base: |Logarithm base (default 2 for binary algorithms).|


---
#### Method Complexity.LogarithmicComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents logarithmic complexity: O(log n), O(k·log n), with configurable base. 



> Logarithmic complexity typically arises from algorithms that halve (or divide by a constant) the problem size at each step. 

**Common algorithmic sources:**

- Binary search: O(log n)
- Balanced BST operations: O(log n)
- Exponentiation by squaring: O(log n)


**Base equivalence:** In asymptotic analysis, log₂(n) = Θ(logₖ(n)) for any constant k > 1, since logₖ(n) = log₂(n) / log₂(k). The base is preserved for precision in constant factor analysis. 



|Name | Description |
|-----|------|
|Coefficient: |Multiplicative coefficient (default 1).|
|Var: |The variable inside the logarithm.|
|Base: |Logarithm base (default 2 for binary algorithms).|


---
#### Property Complexity.LogarithmicComplexity.Coefficient

Multiplicative coefficient (default 1).



---
#### Property Complexity.LogarithmicComplexity.Var

The variable inside the logarithm.



---
#### Property Complexity.LogarithmicComplexity.Base

Logarithm base (default 2 for binary algorithms).



---
## Type Complexity.ExponentialComplexity

 Represents exponential complexity: O(k^n), O(2^n), etc. 



> Exponential complexity indicates algorithms with explosive growth, typically arising from exhaustive enumeration or branching recursive patterns without memoization. 

**Common algorithmic sources:**

- Brute-force subset enumeration: O(2ⁿ)
- Naive recursive Fibonacci: O(φⁿ) ≈ O(1.618ⁿ)
- Traveling salesman (brute force): O(n! × n) ≈ O(nⁿ)
- 3-SAT exhaustive search: O(3ⁿ)


**Growth comparison:** 2¹⁰ = 1,024 but 2²⁰ ≈ 1 million and 2³⁰ ≈ 1 billion. Exponential algorithms become infeasible very quickly. 



|Name | Description |
|-----|------|
|Base: |The exponential base (e.g., 2 for O(2ⁿ)).|
|Var: |The variable in the exponent.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Method Complexity.ExponentialComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents exponential complexity: O(k^n), O(2^n), etc. 



> Exponential complexity indicates algorithms with explosive growth, typically arising from exhaustive enumeration or branching recursive patterns without memoization. 

**Common algorithmic sources:**

- Brute-force subset enumeration: O(2ⁿ)
- Naive recursive Fibonacci: O(φⁿ) ≈ O(1.618ⁿ)
- Traveling salesman (brute force): O(n! × n) ≈ O(nⁿ)
- 3-SAT exhaustive search: O(3ⁿ)


**Growth comparison:** 2¹⁰ = 1,024 but 2²⁰ ≈ 1 million and 2³⁰ ≈ 1 billion. Exponential algorithms become infeasible very quickly. 



|Name | Description |
|-----|------|
|Base: |The exponential base (e.g., 2 for O(2ⁿ)).|
|Var: |The variable in the exponent.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Property Complexity.ExponentialComplexity.Base

The exponential base (e.g., 2 for O(2ⁿ)).



---
#### Property Complexity.ExponentialComplexity.Var

The variable in the exponent.



---
#### Property Complexity.ExponentialComplexity.Coefficient

Optional multiplicative coefficient.



---
## Type Complexity.FactorialComplexity

 Represents factorial complexity: O(n!). 



> Factorial complexity represents the most extreme form of combinatorial explosion, growing faster than exponential. By Stirling's approximation: n! ≈ √(2πn) × (n/e)ⁿ 

**Common algorithmic sources:**

- Generating all permutations: O(n!)
- Traveling salesman brute force: O(n!)
- Determinant by definition: O(n!)


**Growth illustration:** 10! = 3,628,800 while 20! ≈ 2.4 × 10¹⁸. Factorial algorithms are typically only feasible for n ≤ 12. 



|Name | Description |
|-----|------|
|Var: |The variable in the factorial.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Method Complexity.FactorialComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents factorial complexity: O(n!). 



> Factorial complexity represents the most extreme form of combinatorial explosion, growing faster than exponential. By Stirling's approximation: n! ≈ √(2πn) × (n/e)ⁿ 

**Common algorithmic sources:**

- Generating all permutations: O(n!)
- Traveling salesman brute force: O(n!)
- Determinant by definition: O(n!)


**Growth illustration:** 10! = 3,628,800 while 20! ≈ 2.4 × 10¹⁸. Factorial algorithms are typically only feasible for n ≤ 12. 



|Name | Description |
|-----|------|
|Var: |The variable in the factorial.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Property Complexity.FactorialComplexity.Var

The variable in the factorial.



---
#### Property Complexity.FactorialComplexity.Coefficient

Optional multiplicative coefficient.



---
## Type Complexity.BinaryOperationComplexity

 Binary operation on complexity expressions for compositional analysis. 



> Binary operations form the backbone of complexity composition, mapping code structure to complexity algebra: 

**Operation**: Code Pattern
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Plus]] (T₁ + T₂)**: Sequential code blocks:  `loop1(); loop2();` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Multiply]] (T₁ × T₂)**: Nested loops:  `for(...) { for(...) { } }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Max]] (max(T₁, T₂))**: Branching:  `if(cond) { slow } else { fast }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Min]] (min(T₁, T₂))**: Best-case/early exit analysis


**Simplification Rules:**



######  code

```
    O(n) + O(n²) = O(n²)           // Max dominates in addition
    O(n) × O(m) = O(n·m)           // Multiplication combines
    max(O(n), O(n²)) = O(n²)       // Max selects dominant
    O(1) × O(f(n)) = O(f(n))       // Identity for multiplication
```



|Name | Description |
|-----|------|
|Left: |Left operand expression.|
|Operation: |The binary operation to perform.|
|Right: |Right operand expression.|
**See also**: [`BinaryOp`](BinaryOp)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Method Complexity.BinaryOperationComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.BinaryOp,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Binary operation on complexity expressions for compositional analysis. 



> Binary operations form the backbone of complexity composition, mapping code structure to complexity algebra: 

**Operation**: Code Pattern
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Plus]] (T₁ + T₂)**: Sequential code blocks:  `loop1(); loop2();` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Multiply]] (T₁ × T₂)**: Nested loops:  `for(...) { for(...) { } }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Max]] (max(T₁, T₂))**: Branching:  `if(cond) { slow } else { fast }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Min]] (min(T₁, T₂))**: Best-case/early exit analysis


**Simplification Rules:**



######  code

```
    O(n) + O(n²) = O(n²)           // Max dominates in addition
    O(n) × O(m) = O(n·m)           // Multiplication combines
    max(O(n), O(n²)) = O(n²)       // Max selects dominant
    O(1) × O(f(n)) = O(f(n))       // Identity for multiplication
```



|Name | Description |
|-----|------|
|Left: |Left operand expression.|
|Operation: |The binary operation to perform.|
|Right: |Right operand expression.|
**See also**: [`BinaryOp`](BinaryOp)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Property Complexity.BinaryOperationComplexity.Left

Left operand expression.



---
#### Property Complexity.BinaryOperationComplexity.Operation

The binary operation to perform.



---
#### Property Complexity.BinaryOperationComplexity.Right

Right operand expression.



---
## Type Complexity.BinaryOp

 Binary operations for composing complexity expressions. 



> These operations model how code structure translates to complexity composition: -  `Plus` : Sequential execution (loop₁; loop₂)
-  `Multiply` : Nested execution (for { for { } })
-  `Max` : Worst-case branching (if-else)
-  `Min` : Best-case / early exit






---
#### Field Complexity.BinaryOp.Plus

 Addition: T₁ + T₂ (sequential composition). 



---
#### Field Complexity.BinaryOp.Multiply

 Multiplication: T₁ × T₂ (nested composition). 



---
#### Field Complexity.BinaryOp.Max

 Maximum: max(T₁, T₂) (branching/worst case). 



---
#### Field Complexity.BinaryOp.Min

 Minimum: min(T₁, T₂) (best case/early exit). 



---
## Type Complexity.ConditionalComplexity

 Conditional complexity: represents different complexities based on runtime conditions. 



> Models code branches where different paths have different complexities: 



######  code

```
    if (isSorted) {
        BinarySearch();     // O(log n)
    } else {
        LinearSearch();     // O(n)
    }
    // → ConditionalComplexity("isSorted", O(log n), O(n))
```

**Evaluation Strategy:** For worst-case analysis, [[|M:ComplexityAnalysis.Core.Complexity.ConditionalComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})]] conservatively returns max(TrueBranch, FalseBranch). For best-case or average-case analysis, see the speculative analysis infrastructure. 



|Name | Description |
|-----|------|
|ConditionDescription: |Human-readable description of the condition.|
|TrueBranch: |Complexity when condition is true.|
|FalseBranch: |Complexity when condition is false.|


---
#### Method Complexity.ConditionalComplexity.#ctor(System.String,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Conditional complexity: represents different complexities based on runtime conditions. 



> Models code branches where different paths have different complexities: 



######  code

```
    if (isSorted) {
        BinarySearch();     // O(log n)
    } else {
        LinearSearch();     // O(n)
    }
    // → ConditionalComplexity("isSorted", O(log n), O(n))
```

**Evaluation Strategy:** For worst-case analysis, [[|M:ComplexityAnalysis.Core.Complexity.ConditionalComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})]] conservatively returns max(TrueBranch, FalseBranch). For best-case or average-case analysis, see the speculative analysis infrastructure. 



|Name | Description |
|-----|------|
|ConditionDescription: |Human-readable description of the condition.|
|TrueBranch: |Complexity when condition is true.|
|FalseBranch: |Complexity when condition is false.|


---
#### Property Complexity.ConditionalComplexity.ConditionDescription

Human-readable description of the condition.



---
#### Property Complexity.ConditionalComplexity.TrueBranch

Complexity when condition is true.



---
#### Property Complexity.ConditionalComplexity.FalseBranch

Complexity when condition is false.



---
## Type Complexity.PowerComplexity

 Power of a complexity expression: expr^k. 



---
#### Method Complexity.PowerComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Power of a complexity expression: expr^k. 



---
## Type Complexity.LogOfComplexity

 Logarithm of a complexity expression: log(expr). 



---
#### Method Complexity.LogOfComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Logarithm of a complexity expression: log(expr). 



---
## Type Complexity.ExponentialOfComplexity

 Exponential of a complexity expression: base^expr. 



---
#### Method Complexity.ExponentialOfComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Exponential of a complexity expression: base^expr. 



---
## Type Complexity.FactorialOfComplexity

 Factorial of a complexity expression: expr!. 



---
#### Method Complexity.FactorialOfComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Factorial of a complexity expression: expr!. 



---
## Type Complexity.SourceType

 The type of source for a complexity claim. Ordered from most to least authoritative. 



---
#### Field Complexity.SourceType.Documented

 Documented in official Microsoft docs with explicit complexity. Highest confidence. 



---
#### Field Complexity.SourceType.Attested

 Attested in academic papers, CLRS, or other authoritative sources. High confidence. 



---
#### Field Complexity.SourceType.Empirical

 Measured via benchmarking with verification. Good confidence, but environment-dependent. 



---
#### Field Complexity.SourceType.Inferred

 Inferred from source code analysis. Medium confidence. 



---
#### Field Complexity.SourceType.Heuristic

 Conservative estimate when exact complexity is unknown. Prefer overestimate to underestimate. 



---
#### Field Complexity.SourceType.Unknown

 Unknown source or unverified claim. Lowest confidence. 



---
## Type Complexity.ComplexitySource

 Records the source and confidence level for a complexity claim. Essential for audit trails and conservative estimation. 



---
#### Property Complexity.ComplexitySource.Type

 The type of source for this complexity claim. 



---
#### Property Complexity.ComplexitySource.Citation

 Citation or reference for the source. Examples: - URL to Microsoft docs - "CLRS 4th ed., Chapter 7" - "Measured via BenchmarkDotNet" - "Conservative estimate: worst-case resize" 



---
#### Property Complexity.ComplexitySource.Confidence

 Confidence level in the claim (0.0 to 1.0). - 1.0: Certain (documented, verified) - 0.8-0.9: High confidence (attested, empirical) - 0.5-0.7: Medium confidence (inferred, heuristic) - <0.5: Low confidence (uncertain) 



---
#### Property Complexity.ComplexitySource.IsUpperBound

 Whether this is an upper bound (conservative overestimate). When true, actual complexity may be lower. 



---
#### Property Complexity.ComplexitySource.IsAmortized

 Whether this is an amortized complexity. Individual operations may exceed this bound. 



---
#### Property Complexity.ComplexitySource.IsWorstCase

 Whether this complexity is for the worst case. 



---
#### Property Complexity.ComplexitySource.Notes

 Optional notes about edge cases or assumptions. 



---
#### Property Complexity.ComplexitySource.LastVerified

 Date the source was last verified (if applicable). 



---
#### Method Complexity.ComplexitySource.FromMicrosoftDocs(System.String,System.String)

 Creates a documented source from Microsoft docs. 



---
#### Method Complexity.ComplexitySource.FromAcademic(System.String,System.Double)

 Creates an attested source from academic literature. 



---
#### Method Complexity.ComplexitySource.FromBenchmark(System.String,System.Double)

 Creates an empirical source from benchmarking. 



---
#### Method Complexity.ComplexitySource.Inferred(System.String,System.Double)

 Creates an inferred source from code analysis. 



---
#### Method Complexity.ComplexitySource.ConservativeHeuristic(System.String,System.Double)

 Creates a conservative heuristic estimate. Always marks as upper bound. 



---
#### Method Complexity.ComplexitySource.Unknown

 Creates an unknown source (used when no information is available). 



---
#### Method Complexity.ComplexitySource.Documented(System.String)

 Creates a documented source with citation. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Attested(System.String)

 Creates an attested source with citation. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Empirical(System.String)

 Creates an empirical source with description. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Heuristic(System.String)

 Creates a heuristic source with reasoning. Shorthand for BCL mapping declarations. 



---
## Type Complexity.AttributedComplexity

 A complexity expression paired with its source attribution. 



---
#### Property Complexity.AttributedComplexity.Expression

 The complexity expression. 



---
#### Property Complexity.AttributedComplexity.Source

 The source of the complexity claim. 



---
#### Property Complexity.AttributedComplexity.RequiresReview

 Whether this result requires human review. 



---
#### Property Complexity.AttributedComplexity.ReviewReason

 Reason for requiring review (if applicable). 



---
#### Method Complexity.AttributedComplexity.Documented(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String,System.String)

 Creates an attributed complexity from documented source. 



---
#### Method Complexity.AttributedComplexity.Attested(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates an attributed complexity from academic source. 



---
#### Method Complexity.AttributedComplexity.Heuristic(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates a conservative heuristic complexity. 



---
## Type Complexity.ComplexityResult

 Complete result of complexity analysis for a method or code block. 



---
#### Property Complexity.ComplexityResult.Expression

 The computed complexity expression. 



---
#### Property Complexity.ComplexityResult.Source

 Source attribution for the complexity claim. 



---
#### Property Complexity.ComplexityResult.RequiresReview

 Whether this result requires human review. 



---
#### Property Complexity.ComplexityResult.ReviewReason

 Reason for requiring review (if applicable). 



---
#### Property Complexity.ComplexityResult.Location

 Location in source code where this complexity was computed. 



---
#### Property Complexity.ComplexityResult.SubResults

 Sub-results that contributed to this complexity. Useful for explaining how the total was derived. 



---
#### Method Complexity.ComplexityResult.Create(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexitySource,ComplexityAnalysis.Core.Complexity.SourceLocation)

 Creates a result with automatic review flagging based on source. 



---
## Type Complexity.SourceLocation

 Location in source code. 



---
#### Property Complexity.SourceLocation.FilePath

 File path. 



---
#### Property Complexity.SourceLocation.StartLine

 Starting line number (1-based). 



---
#### Property Complexity.SourceLocation.StartColumn

 Starting column (0-based). 



---
#### Property Complexity.SourceLocation.EndLine

 Ending line number (1-based). 



---
#### Property Complexity.SourceLocation.EndColumn

 Ending column (0-based). 



---
## Type Complexity.ExpressionForm

 Classifies the dominant asymptotic form of complexity expressions. Essential for determining theorem applicability. 



---
#### Field Complexity.ExpressionForm.Constant

O(1) - constant complexity.



---
#### Field Complexity.ExpressionForm.Logarithmic

O(log^k n) - pure logarithmic (no polynomial factor).



---
#### Field Complexity.ExpressionForm.Polynomial

O(n^k) - pure polynomial.



---
#### Field Complexity.ExpressionForm.PolyLog

O(n^k · log^j n) - polylogarithmic.



---
#### Field Complexity.ExpressionForm.Exponential

O(k^n) - exponential.



---
#### Field Complexity.ExpressionForm.Factorial

O(n!) - factorial.



---
#### Field Complexity.ExpressionForm.Unknown

Cannot be classified into standard forms.



---
## Type Complexity.ExpressionClassification

 Result of classifying an expression's asymptotic form. 



---
#### Property Complexity.ExpressionClassification.Form

The dominant asymptotic form.



---
#### Property Complexity.ExpressionClassification.Variable

The variable the classification is with respect to.



---
#### Property Complexity.ExpressionClassification.PrimaryParameter

 For Polynomial/PolyLog: the polynomial degree k in n^k. For Logarithmic: 0. For Exponential: the base. 



---
#### Property Complexity.ExpressionClassification.LogExponent

 For PolyLog/Logarithmic: the log exponent j in log^j n. 



---
#### Property Complexity.ExpressionClassification.Coefficient

 Leading coefficient (preserved for non-asymptotic analysis). 



---
#### Property Complexity.ExpressionClassification.Confidence

 Confidence level in the classification (0.0 to 1.0). Lower for complex composed expressions. 



---
#### Method Complexity.ExpressionClassification.ToPolyLog

 Converts to a normalized PolyLogComplexity if applicable. 



---
#### Method Complexity.ExpressionClassification.CompareDegreeTo(System.Double,System.Double)

 Compares the polynomial degree to a target value. Returns: <0 if degree < target, 0 if equal (within epsilon), >0 if degree > target. 



---
## Type Complexity.IExpressionClassifier

 Interface for classifying complexity expressions into standard forms. Used to determine theorem applicability. 



---
#### Method Complexity.IExpressionClassifier.Classify(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Classifies the dominant asymptotic form of an expression. 



---
#### Method Complexity.IExpressionClassifier.TryExtractPolynomialDegree(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double@)

 Attempts to extract polynomial degree if expression is O(n^k). 



---
#### Method Complexity.IExpressionClassifier.TryExtractPolyLogForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double@,System.Double@)

 Attempts to extract polylog form parameters if expression is O(n^k · log^j n). 



---
#### Method Complexity.IExpressionClassifier.IsBoundedByPolynomial(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Determines if expression is bounded by O(n^d) for given d. 



---
#### Method Complexity.IExpressionClassifier.DominatesPolynomial(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Determines if expression dominates Ω(n^d) for given d. 



---
## Type Complexity.StandardExpressionClassifier

 Standard implementation of expression classification. Uses pattern matching and visitor traversal. 



---
## Type Complexity.IComplexityTransformer

 Interface for transforming and simplifying complexity expressions. 



---
#### Method Complexity.IComplexityTransformer.Simplify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Simplify an expression by applying algebraic rules. 



---
#### Method Complexity.IComplexityTransformer.NormalizeForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Normalize to a canonical form for comparison. 



---
#### Method Complexity.IComplexityTransformer.DropConstantFactors(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Drop constant factors for Big-O equivalence. O(3n²) → O(n²) 



---
#### Method Complexity.IComplexityTransformer.DropLowerOrderTerms(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Drop lower-order terms for asymptotic equivalence. O(n² + n + 1) → O(n²) 



---
## Type Complexity.IComplexityComparator

 Compares complexity expressions for asymptotic ordering. 



---
#### Method Complexity.IComplexityComparator.Compare(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Compare two expressions asymptotically. Returns: -1 if left < right, 0 if equal, 1 if left > right. 



---
#### Method Complexity.IComplexityComparator.IsDominated(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Determines if left is dominated by right (left ∈ O(right)). 



---
#### Method Complexity.IComplexityComparator.AreEquivalent(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Determines if two expressions are asymptotically equivalent. 



---
## Type Complexity.ComplexitySimplifier

 Standard implementation of complexity simplification. 



---
#### Method Complexity.ComplexitySimplifier.Simplify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.NormalizeForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.DropConstantFactors(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.DropLowerOrderTerms(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
## Type Complexity.AsymptoticComparator

 Compares complexity expressions by asymptotic growth rate. 



---
#### Method Complexity.AsymptoticComparator.Compare(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Asymptotic ordering (from slowest to fastest growth): O(1) < O(log n) < O(n) < O(n log n) < O(n²) < O(n³) < O(2ⁿ) < O(n!) 



---
#### Method Complexity.AsymptoticComparator.IsDominated(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.AsymptoticComparator.AreEquivalent(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.AsymptoticComparator.GetAsymptoticOrder(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Gets a numeric order for asymptotic comparison. Higher values = faster growth. 



---
## Type Complexity.ComplexityExpressionExtensions

 Extension methods for complexity expressions. 



---
#### Method Complexity.ComplexityExpressionExtensions.Simplified(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Simplifies the expression using the default simplifier. 



---
#### Method Complexity.ComplexityExpressionExtensions.Normalized(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Normalizes to canonical Big-O form. 



---
#### Method Complexity.ComplexityExpressionExtensions.CompareAsymptotically(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Compares asymptotically to another expression. 



---
#### Method Complexity.ComplexityExpressionExtensions.IsDominatedBy(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Checks if this expression is dominated by another. 



---
#### Method Complexity.ComplexityExpressionExtensions.Dominates(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Checks if this expression dominates another. 



---
## Type Complexity.IComplexityVisitor`1

 Visitor pattern interface for traversing complexity expression trees. Enables operations like simplification, evaluation, and transformation. 



---
#### Method Complexity.IComplexityVisitor`1.VisitUnknown(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Fallback for unknown/unrecognized expression types (e.g., special functions). 



---
## Type Complexity.ComplexityVisitorBase`1

 Base implementation of IComplexityVisitor that returns default values. Override specific methods to handle particular expression types. 



---
## Type Complexity.ComplexityTransformVisitor

 Visitor that recursively transforms complexity expressions. Override methods to modify specific node types during traversal. 



---
## Type Complexity.ParallelComplexity

 Represents complexity of parallel/concurrent algorithms. Parallel complexity considers: - Work: Total operations across all processors (sequential equivalent) - Span/Depth: Longest chain of dependent operations (critical path) - Parallelism: Work / Span ratio (how parallelizable the algorithm is) Examples: - Parallel.For over n items: Work O(n), Span O(1) if independent - Parallel merge sort: Work O(n log n), Span O(log² n) - Parallel prefix sum: Work O(n), Span O(log n) 



---
#### Property Complexity.ParallelComplexity.Work

 Total work across all processors (sequential time complexity). 



---
#### Property Complexity.ParallelComplexity.Span

 Span/depth - the longest chain of dependent operations. Also known as critical path length. 



---
#### Property Complexity.ParallelComplexity.ProcessorCount

 Number of processors/cores assumed. Use Variable.P for parameterized, or a constant for fixed. 



---
#### Property Complexity.ParallelComplexity.PatternType

 The type of parallel pattern detected. 



---
#### Property Complexity.ParallelComplexity.IsTaskBased

 Whether the parallelism is task-based (async/await, Task.Run). 



---
#### Property Complexity.ParallelComplexity.HasSynchronizationOverhead

 Whether the parallel operations have synchronization overhead. 



---
#### Property Complexity.ParallelComplexity.Description

 Description of the parallel pattern. 



---
#### Property Complexity.ParallelComplexity.Parallelism

 Gets the parallelism (Work / Span ratio). Higher values indicate better parallelizability. 



---
#### Property Complexity.ParallelComplexity.ParallelTime

 Gets the parallel time (with p processors): max(Work/p, Span). By Brent's theorem: T_p ≤ (Work - Span)/p + Span 



---
#### Method Complexity.ParallelComplexity.EmbarrassinglyParallel(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a parallel complexity for embarrassingly parallel work. Work = O(n), Span = O(1). 



---
#### Method Complexity.ParallelComplexity.Reduction(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for reduction/aggregation patterns. Work = O(n), Span = O(log n). 



---
#### Method Complexity.ParallelComplexity.DivideAndConquer(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for divide-and-conquer patterns. Work = O(n log n), Span = O(log² n). 



---
#### Method Complexity.ParallelComplexity.PrefixScan(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for prefix/scan operations. Work = O(n), Span = O(log n). 



---
#### Method Complexity.ParallelComplexity.Pipeline(ComplexityAnalysis.Core.Complexity.Variable,System.Int32)

 Creates parallel complexity for pipeline patterns. Work = O(n × stages), Span = O(n + stages). 



---
#### Method Complexity.ParallelComplexity.TaskBased(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates complexity for async/await task-based concurrency. 



---
## Type Complexity.ParallelPatternType

 Types of parallel patterns. 



---
#### Field Complexity.ParallelPatternType.Generic

 Generic parallel pattern. 



---
#### Field Complexity.ParallelPatternType.ParallelFor

 Parallel.For / Parallel.ForEach - data parallelism. 



---
#### Field Complexity.ParallelPatternType.PLINQ

 PLINQ - parallel LINQ. 



---
#### Field Complexity.ParallelPatternType.TaskBased

 Task.Run / Task.WhenAll - task parallelism. 



---
#### Field Complexity.ParallelPatternType.AsyncAwait

 async/await patterns. 



---
#### Field Complexity.ParallelPatternType.Reduction

 Parallel reduction/aggregation. 



---
#### Field Complexity.ParallelPatternType.Scan

 Parallel prefix scan. 



---
#### Field Complexity.ParallelPatternType.DivideAndConquer

 Divide-and-conquer parallelism. 



---
#### Field Complexity.ParallelPatternType.Pipeline

 Pipeline parallelism. 



---
#### Field Complexity.ParallelPatternType.ForkJoin

 Fork-join pattern. 



---
#### Field Complexity.ParallelPatternType.ProducerConsumer

 Producer-consumer pattern. 



---
## Type Complexity.ParallelVariables

 Variable for processor count. 



---
#### Property Complexity.ParallelVariables.P

 Number of processors (p). 



---
#### Method Complexity.ParallelVariables.Processors(System.Int32)

 Creates a processor count variable with a specific value. 



---
#### Property Complexity.ParallelVariables.InfiniteProcessors

 Infinite processors (theoretical analysis). 



---
## Type Complexity.IParallelComplexityVisitor`1

 Extended visitor interface for parallel complexity. 



---
## Type Complexity.ParallelAnalysisResult

 Analysis result for parallel patterns. 



---
#### Property Complexity.ParallelAnalysisResult.Complexity

 The detected parallel complexity. 



---
#### Property Complexity.ParallelAnalysisResult.Speedup

 Speedup factor: T_1 / T_p (sequential time / parallel time). 



---
#### Property Complexity.ParallelAnalysisResult.Efficiency

 Efficiency: Speedup / p (how well processors are utilized). 



---
#### Property Complexity.ParallelAnalysisResult.IsScalable

 Whether the pattern has good scalability. 



---
#### Property Complexity.ParallelAnalysisResult.Warnings

 Potential issues or warnings. 



---
#### Property Complexity.ParallelAnalysisResult.Recommendations

 Recommendations for improving parallelism. 



---
## Type Complexity.ParallelAlgorithms

 Common parallel algorithm complexities. 



---
#### Method Complexity.ParallelAlgorithms.ParallelSum

 Parallel sum/reduction: Work O(n), Span O(log n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelMergeSort

 Parallel merge sort: Work O(n log n), Span O(log² n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelMatrixMultiply

 Parallel matrix multiply (naive): Work O(n³), Span O(log n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelQuickSort

 Parallel quick sort: Work O(n log n), Span O(log² n) expected. 



---
#### Method Complexity.ParallelAlgorithms.ParallelBFS

 Parallel BFS: Work O(V + E), Span O(diameter × log V). 



---
#### Method Complexity.ParallelAlgorithms.PLINQFilter

 PLINQ Where/Select: Work O(n), Span O(n/p + log p). 



---
## Type Complexity.PolyLogComplexity

 Represents polylogarithmic complexity: O(n^k · log^j n). 



>**General Form:** coefficient · n^polyDegree · (log_base n)^logExponent 

 This unified type is essential for representing many common complexity classes: 

**Parameters**: Result
- **k=1, j=1**: O(n log n) - Merge sort, heap sort, optimal comparison sorts
- **k=2, j=0**: O(n²) - Pure polynomial (quadratic)
- **k=0, j=1**: O(log n) - Pure logarithmic (binary search)
- **k=1, j=2**: O(n log² n) - Some advanced algorithms
- **k=0, j=2**: O(log² n) - Iterated binary search


**Master Theorem Connection:**

 Case 2 of the Master Theorem produces polylog solutions. For T(n) = a·T(n/b) + Θ(n^d · log^k n) where d = log_b(a): 



######  code

```
    T(n) = Θ(n^d · log^(k+1) n)
```

 The factory method [[|M:ComplexityAnalysis.Core.Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)]] creates these solutions directly. 

**Algebraic Properties:**



######  code

```
    // Multiplication combines exponents:
    (n^a log^b n) × (n^c log^d n) = n^(a+c) · log^(b+d) n
    
    // Power distributes:
    (n^a log^b n)^k = n^(ak) · log^(bk) n
```



**See also**: [`PolynomialComplexity`](PolynomialComplexity)

**See also**: [`LogarithmicComplexity`](LogarithmicComplexity)



---
#### Method Complexity.PolyLogComplexity.#ctor(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double,System.Double)

 Represents polylogarithmic complexity: O(n^k · log^j n). 



>**General Form:** coefficient · n^polyDegree · (log_base n)^logExponent 

 This unified type is essential for representing many common complexity classes: 

**Parameters**: Result
- **k=1, j=1**: O(n log n) - Merge sort, heap sort, optimal comparison sorts
- **k=2, j=0**: O(n²) - Pure polynomial (quadratic)
- **k=0, j=1**: O(log n) - Pure logarithmic (binary search)
- **k=1, j=2**: O(n log² n) - Some advanced algorithms
- **k=0, j=2**: O(log² n) - Iterated binary search


**Master Theorem Connection:**

 Case 2 of the Master Theorem produces polylog solutions. For T(n) = a·T(n/b) + Θ(n^d · log^k n) where d = log_b(a): 



######  code

```
    T(n) = Θ(n^d · log^(k+1) n)
```

 The factory method [[|M:ComplexityAnalysis.Core.Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)]] creates these solutions directly. 

**Algebraic Properties:**



######  code

```
    // Multiplication combines exponents:
    (n^a log^b n) × (n^c log^d n) = n^(a+c) · log^(b+d) n
    
    // Power distributes:
    (n^a log^b n)^k = n^(ak) · log^(bk) n
```



**See also**: [`PolynomialComplexity`](PolynomialComplexity)

**See also**: [`LogarithmicComplexity`](LogarithmicComplexity)



---
#### Property Complexity.PolyLogComplexity.IsPurePolynomial

 True if this is a pure polynomial (no log factor). 



---
#### Property Complexity.PolyLogComplexity.IsPureLogarithmic

 True if this is a pure logarithmic (no polynomial factor). 



---
#### Property Complexity.PolyLogComplexity.IsNLogN

 True if this is the common n log n form. 



---
#### Method Complexity.PolyLogComplexity.NLogN(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n log n) - common for efficient sorting/divide-and-conquer. 



---
#### Method Complexity.PolyLogComplexity.PolyTimesLog(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^k log n) - Master Theorem Case 2 with k=1. 



---
#### Method Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^d · log^(k+1) n) - General Master Theorem Case 2 solution. 



---
#### Method Complexity.PolyLogComplexity.LogPower(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(log^k n) - pure iterated logarithm. 



---
#### Method Complexity.PolyLogComplexity.Polynomial(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^k) - pure polynomial (for consistency). 



---
#### Method Complexity.PolyLogComplexity.Multiply(ComplexityAnalysis.Core.Complexity.PolyLogComplexity)

 Multiplies two PolyLog expressions: (n^a log^b n) × (n^c log^d n) = n^(a+c) log^(b+d) n 



---
#### Method Complexity.PolyLogComplexity.Power(System.Double)

 Raises to a power: (n^a log^b n)^k = n^(ak) log^(bk) n 



---
## Type Complexity.RandomnessSource

 Specifies the source of randomness in a probabilistic algorithm. 



---
#### Field Complexity.RandomnessSource.InputDistribution

 Randomness comes from the input distribution (average-case analysis). Example: QuickSort with random input permutation. 



---
#### Field Complexity.RandomnessSource.AlgorithmRandomness

 Randomness comes from the algorithm itself (Las Vegas algorithms). Example: Randomized QuickSort with random pivot selection. 



---
#### Field Complexity.RandomnessSource.MonteCarlo

 Monte Carlo algorithms that may produce incorrect results with small probability. Example: Miller-Rabin primality test. 



---
#### Field Complexity.RandomnessSource.HashFunction

 Hash function randomness (universal hashing, expected behavior). Example: Hash table operations assuming uniform hashing. 



---
#### Field Complexity.RandomnessSource.Mixed

 Multiple sources of randomness combined. 



---
## Type Complexity.ProbabilityDistribution

 Specifies the probability distribution of the complexity. 



---
#### Field Complexity.ProbabilityDistribution.Uniform

 Uniform distribution over all inputs. 



---
#### Field Complexity.ProbabilityDistribution.Exponential

 Exponential distribution (common in queueing theory). 



---
#### Field Complexity.ProbabilityDistribution.Geometric

 Geometric distribution (common in randomized algorithms). 



---
#### Field Complexity.ProbabilityDistribution.HighProbabilityBound

 Bounded/concentrated distribution with high probability guarantees. 



---
#### Field Complexity.ProbabilityDistribution.InputDependent

 Distribution determined by specific input characteristics. 



---
#### Field Complexity.ProbabilityDistribution.Unknown

 Unknown or unspecified distribution. 



---
## Type Complexity.ProbabilisticComplexity

 Represents probabilistic complexity analysis for randomized algorithms. Captures expected (average), best-case, and worst-case complexities along with probability distribution information. 



> This is used for analyzing: - Average-case complexity (QuickSort, hash tables) - Randomized algorithms (randomized QuickSort, randomized selection) - Monte Carlo algorithms (primality testing) - Las Vegas algorithms (randomized algorithms that always produce correct results) 



---
#### Property Complexity.ProbabilisticComplexity.ExpectedComplexity

 Gets the expected (average-case) complexity. This represents E[T(n)] - the expected running time. 



---
#### Property Complexity.ProbabilisticComplexity.WorstCaseComplexity

 Gets the worst-case complexity. This is the upper bound that holds for all inputs/random choices. 



---
#### Property Complexity.ProbabilisticComplexity.BestCaseComplexity

 Gets the best-case complexity. Optional - when not specified, defaults to constant. 



---
#### Property Complexity.ProbabilisticComplexity.Source

 Gets the source of randomness in the algorithm. 



---
#### Property Complexity.ProbabilisticComplexity.Distribution

 Gets the probability distribution of the complexity. 



---
#### Property Complexity.ProbabilisticComplexity.Variance

 Gets the variance of the complexity if known. Null indicates unknown variance. 



---
#### Property Complexity.ProbabilisticComplexity.HighProbability

 Gets the high-probability bound if applicable. For algorithms with concentration bounds: Pr[T(n) > bound] ≤ probability. 



---
#### Property Complexity.ProbabilisticComplexity.Assumptions

 Gets any assumptions required for the expected complexity to hold. Example: "uniform random input permutation", "independent hash function" 



---
#### Property Complexity.ProbabilisticComplexity.Description

 Gets an optional description of the probabilistic analysis. 



---
#### Method Complexity.ProbabilisticComplexity.Accept``1(ComplexityAnalysis.Core.Complexity.IComplexityVisitor{``0})

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.Substitute(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Property Complexity.ProbabilisticComplexity.FreeVariables

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.ToBigONotation

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.QuickSortLike(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.RandomnessSource)

 Creates a probabilistic complexity with expected O(n log n) and worst O(n²). Common for randomized sorting algorithms like QuickSort. 



---
#### Method Complexity.ProbabilisticComplexity.HashTableLookup(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for hash table operations. Expected O(1), worst O(n). 



---
#### Method Complexity.ProbabilisticComplexity.RandomizedSelection(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for randomized selection (Quickselect). Expected O(n), worst O(n²). 



---
#### Method Complexity.ProbabilisticComplexity.SkipListOperation(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for skip list operations. Expected O(log n), worst O(n). 



---
#### Method Complexity.ProbabilisticComplexity.BloomFilterLookup(System.Int32)

 Creates a probabilistic complexity for Bloom filter operations. O(k) where k is the number of hash functions, with false positive probability. 



---
#### Method Complexity.ProbabilisticComplexity.MonteCarlo(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double,System.String)

 Creates a Monte Carlo complexity where the result may be incorrect with some probability. 



---
## Type Complexity.HighProbabilityBound

 Represents a high-probability bound: Pr[T(n) ≤ bound] ≥ probability. 



---
#### Property Complexity.HighProbabilityBound.Bound

 Gets the complexity bound that holds with high probability. 



---
#### Property Complexity.HighProbabilityBound.Probability

 Gets the probability that the bound holds. For "with high probability" bounds, this is typically 1 - 1/n^c for some constant c. 



---
#### Property Complexity.HighProbabilityBound.ProbabilityExpression

 Gets an optional expression for the probability as a function of n. Example: 1 - 1/n for bounds that hold "with high probability". 



---
## Type Complexity.IProbabilisticComplexityVisitor`1

 Extension of IComplexityVisitor for probabilistic complexity. 



---
#### Method Complexity.IProbabilisticComplexityVisitor`1.VisitProbabilistic(ComplexityAnalysis.Core.Complexity.ProbabilisticComplexity)

 Visits a probabilistic complexity expression. 



---
## Type Complexity.SpecialFunctionComplexity

 Represents special mathematical functions that arise in complexity analysis, particularly from Akra-Bazzi integral evaluation. These provide symbolic representations when closed-form elementary solutions don't exist, enabling later refinement via numerical methods or CAS integration. 



---
#### Property Complexity.SpecialFunctionComplexity.HasAsymptoticExpansion

 Whether this function has a known asymptotic expansion. 



---
#### Property Complexity.SpecialFunctionComplexity.DominantTerm

 Gets the dominant asymptotic term, if known. 



---
## Type Complexity.PolylogarithmComplexity

 Polylogarithm Li_s(z) = Σₖ₌₁^∞ z^k / k^s Arises when integrating log terms. For |z| ≤ 1: - Li_1(z) = -ln(1-z) - Li_0(z) = z/(1-z) - Li_{-1}(z) = z/(1-z)² For complexity analysis, we often have Li_s(1) = ζ(s) (Riemann zeta). 



---
#### Method Complexity.PolylogarithmComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Polylogarithm Li_s(z) = Σₖ₌₁^∞ z^k / k^s Arises when integrating log terms. For |z| ≤ 1: - Li_1(z) = -ln(1-z) - Li_0(z) = z/(1-z) - Li_{-1}(z) = z/(1-z)² For complexity analysis, we often have Li_s(1) = ζ(s) (Riemann zeta). 



---
## Type Complexity.IncompleteGammaComplexity

 Incomplete Gamma function γ(s, x) = ∫₀ˣ t^(s-1) e^(-t) dt Arises from exponential-polynomial integrals. Asymptotically: - For large x: γ(s, x) → Γ(s) (complete gamma) - For small x: γ(s, x) ≈ x^s / s 



---
#### Method Complexity.IncompleteGammaComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Incomplete Gamma function γ(s, x) = ∫₀ˣ t^(s-1) e^(-t) dt Arises from exponential-polynomial integrals. Asymptotically: - For large x: γ(s, x) → Γ(s) (complete gamma) - For small x: γ(s, x) ≈ x^s / s 



---
## Type Complexity.IncompleteBetaComplexity

 Incomplete Beta function B(x; a, b) = ∫₀ˣ t^(a-1) (1-t)^(b-1) dt Related to regularized incomplete beta I_x(a,b) = B(x;a,b) / B(a,b). Arises in probability and from polynomial ratio integrals. 



---
#### Method Complexity.IncompleteBetaComplexity.#ctor(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Incomplete Beta function B(x; a, b) = ∫₀ˣ t^(a-1) (1-t)^(b-1) dt Related to regularized incomplete beta I_x(a,b) = B(x;a,b) / B(a,b). Arises in probability and from polynomial ratio integrals. 



---
## Type Complexity.HypergeometricComplexity

 Gauss Hypergeometric function ₂F₁(a, b; c; z) The most general special function needed for Akra-Bazzi integrals. Many special functions are cases of ₂F₁: - log(1+z) = z · ₂F₁(1, 1; 2; -z) - arcsin(z) = z · ₂F₁(1/2, 1/2; 3/2; z²) - (1-z)^(-a) = ₂F₁(a, b; b; z) for any b 



---
#### Method Complexity.HypergeometricComplexity.#ctor(System.Double,System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Gauss Hypergeometric function ₂F₁(a, b; c; z) The most general special function needed for Akra-Bazzi integrals. Many special functions are cases of ₂F₁: - log(1+z) = z · ₂F₁(1, 1; 2; -z) - arcsin(z) = z · ₂F₁(1/2, 1/2; 3/2; z²) - (1-z)^(-a) = ₂F₁(a, b; b; z) for any b 



---
#### Property Complexity.HypergeometricComplexity.SimplifiedForm

 Recognizes if this hypergeometric is actually a simpler function. 



---
## Type Complexity.SymbolicIntegralComplexity

 Represents a symbolic integral that cannot be evaluated in closed form. Preserves the integrand for potential later refinement. 



---
#### Method Complexity.SymbolicIntegralComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Represents a symbolic integral that cannot be evaluated in closed form. Preserves the integrand for potential later refinement. 



---
#### Method Complexity.SymbolicIntegralComplexity.WithBound(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Creates a symbolic integral with an asymptotic bound estimate. 



---
## Type Complexity.ISpecialFunctionVisitor`1

 Extended visitor interface for special functions. 



---
## Type Complexity.Variable

 Represents a variable in complexity expressions (e.g., n, V, E, degree). 



> Variables are symbolic placeholders for input sizes and algorithm parameters. Unlike mathematical variables, complexity variables carry semantic meaning through their [[|T:ComplexityAnalysis.Core.Complexity.VariableType]] to enable domain-specific analysis. 

**Variable Semantics by Domain:**

**Domain**: Common Variables
- **General**:  `n`  (input size),  `k`  (parameter count)
- **Graphs**:  `V`  (vertices),  `E`  (edges), with relationship E ≤ V²
- **Trees**:  `n`  (nodes),  `h`  (height), with h ∈ [log n, n]
- **Strings**:  `n`  (text length),  `m`  (pattern length)
- **Parallel**:  `n`  (work),  `p`  (processors)


**Multi-Variable Complexity:** Many algorithms have complexity dependent on multiple variables. The system supports this through expression composition: 



######  code

```
    // Graph algorithm: O(V + E)
    var graphComplexity = new BinaryOperationComplexity(
        new VariableComplexity(Variable.V),
        BinaryOp.Plus,
        new VariableComplexity(Variable.E));
        
    // String matching: O(n × m)
    var stringComplexity = new BinaryOperationComplexity(
        new VariableComplexity(Variable.N),
        BinaryOp.Multiply,
        new VariableComplexity(Variable.M));
```

**Implicit Relationships:** Some variables have implicit constraints: 

- In connected graphs: E ≥ V - 1
- In simple graphs: E ≤ V(V-1)/2
- In balanced trees: h = Θ(log n)
- In linked structures: h ≤ n




**See also**: [`VariableType`](VariableType)

**See also**: [`VariableComplexity`](VariableComplexity)



---
#### Property Complexity.Variable.Name

 The symbolic name of the variable (e.g., "n", "V", "E"). 



---
#### Property Complexity.Variable.Type

 The semantic type of the variable, indicating what it represents. 



---
#### Property Complexity.Variable.Description

 Optional description for documentation purposes. 



---
#### Property Complexity.Variable.N

 Creates a standard input size variable named "n". 



---
#### Property Complexity.Variable.V

 Creates a vertex count variable named "V". 



---
#### Property Complexity.Variable.E

 Creates an edge count variable named "E". 



---
#### Property Complexity.Variable.M

 Creates a secondary size variable named "m" (e.g., for pattern length in string search). 



---
#### Property Complexity.Variable.K

 Creates a count parameter variable named "k" (e.g., for Take(k), top-k queries). 



---
#### Property Complexity.Variable.H

 Creates a height/depth variable named "h" (e.g., for tree height). 



---
#### Property Complexity.Variable.P

 Creates a processor count variable named "p" (for parallel complexity). 



---
## Type Complexity.VariableType

 Semantic types for complexity variables, indicating what the variable represents. 



> Variable types enable semantic analysis and validation. For example, the analyzer can verify that graph algorithms use [[|F:ComplexityAnalysis.Core.Complexity.VariableType.VertexCount]] and [[|F:ComplexityAnalysis.Core.Complexity.VariableType.EdgeCount]] appropriately, or flag potential issues when tree algorithms don't account for [[|F:ComplexityAnalysis.Core.Complexity.VariableType.TreeHeight]]. 

**Type Relationships:**

- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.VertexCount]] and [[|F:ComplexityAnalysis.Core.Complexity.VariableType.EdgeCount]] often appear together: O(V + E)
- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.InputSize]] is the default for general algorithms
- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.SecondarySize]] is used when two independent sizes matter (O(n × m))






---
#### Field Complexity.VariableType.InputSize

 General input size (n) - default for most algorithms. 



---
#### Field Complexity.VariableType.DataCount

 Count of data elements in a collection. 



---
#### Field Complexity.VariableType.VertexCount

 Number of vertices in a graph (V). 



---
#### Field Complexity.VariableType.EdgeCount

 Number of edges in a graph (E). 



---
#### Field Complexity.VariableType.DegreeSum

 Sum of vertex degrees in a graph. 



---
#### Field Complexity.VariableType.TreeHeight

 Height or depth of a tree structure. 



---
#### Field Complexity.VariableType.ProcessorCount

 Number of processors/cores (for parallel complexity). 



---
#### Field Complexity.VariableType.Dimensions

 Number of dimensions (for multi-dimensional algorithms). 



---
#### Field Complexity.VariableType.StringLength

 Length of a string or character sequence. 



---
#### Field Complexity.VariableType.SecondarySize

 A secondary size parameter (e.g., m in O(n × m)). 



---
#### Field Complexity.VariableType.Custom

 Custom/user-defined variable type. 



---
## Type Complexity.VariableExtensions

 Extension methods for Variable. 



---
#### Method Complexity.VariableExtensions.ToVariableSet(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.Variable})

 Creates a variable set from multiple variables. 



---
#### Method Complexity.VariableExtensions.IsGraphVariable(ComplexityAnalysis.Core.Complexity.Variable)

 Determines if a variable represents a graph-related quantity. 



---
## Type Memory.MemoryComplexity

 Represents space/memory complexity analysis result. Space complexity measures memory usage as a function of input size. Components: - Stack space: Recursion depth, local variables - Heap space: Allocated objects, collections - Auxiliary space: Extra space beyond input 



---
#### Property Memory.MemoryComplexity.TotalSpace

 Total space complexity (dominant term). 



---
#### Property Memory.MemoryComplexity.StackSpace

 Stack space complexity (recursion depth). 



---
#### Property Memory.MemoryComplexity.HeapSpace

 Heap space complexity (allocated objects). 



---
#### Property Memory.MemoryComplexity.AuxiliarySpace

 Auxiliary space (extra space beyond input). 



---
#### Property Memory.MemoryComplexity.IsInPlace

 Whether the algorithm is in-place (O(1) auxiliary space). 



---
#### Property Memory.MemoryComplexity.IsTailRecursive

 Whether tail-call optimization can reduce stack space. 



---
#### Property Memory.MemoryComplexity.Description

 Description of memory usage pattern. 



---
#### Property Memory.MemoryComplexity.Allocations

 Breakdown of memory allocations by source. 



---
#### Method Memory.MemoryComplexity.Constant

 Creates O(1) constant space complexity (in-place). 



---
#### Method Memory.MemoryComplexity.Linear(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Memory.MemorySource)

 Creates O(n) linear space complexity. 



---
#### Method Memory.MemoryComplexity.Logarithmic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(log n) logarithmic space complexity (typical for recursion). 



---
#### Method Memory.MemoryComplexity.Quadratic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n²) quadratic space complexity. 



---
#### Method Memory.MemoryComplexity.FromRecursion(System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Creates memory complexity from recursion pattern. 



---
## Type Memory.MemorySource

 Where memory is allocated. 



---
#### Field Memory.MemorySource.Stack

 Stack allocation (local variables, recursion frames). 



---
#### Field Memory.MemorySource.Heap

 Heap allocation (new objects, collections). 



---
#### Field Memory.MemorySource.Both

 Both stack and heap. 



---
## Type Memory.AllocationInfo

 Information about a specific memory allocation. 



---
#### Property Memory.AllocationInfo.Description

 Description of what is being allocated. 



---
#### Property Memory.AllocationInfo.Size

 The size complexity of this allocation. 



---
#### Property Memory.AllocationInfo.Source

 Where the memory is allocated. 



---
#### Property Memory.AllocationInfo.TypeName

 The type being allocated (if known). 



---
#### Property Memory.AllocationInfo.Count

 How many times this allocation occurs. 



---
#### Property Memory.AllocationInfo.TotalSize

 Total memory from this allocation. 



---
## Type Memory.ComplexityAnalysisResult

 Combined time and space complexity result. 



---
#### Property Memory.ComplexityAnalysisResult.TimeComplexity

 Time complexity of the algorithm. 



---
#### Property Memory.ComplexityAnalysisResult.SpaceComplexity

 Space/memory complexity of the algorithm. 



---
#### Property Memory.ComplexityAnalysisResult.Name

 The method or algorithm name. 



---
#### Property Memory.ComplexityAnalysisResult.HasTimeSpaceTradeoff

 Whether time-space tradeoff is possible. 



---
#### Property Memory.ComplexityAnalysisResult.Notes

 Notes about the analysis. 



---
#### Property Memory.ComplexityAnalysisResult.Confidence

 Confidence in the analysis (0-1). 



---
## Type Memory.ComplexityAnalysisResult.CommonAlgorithms

 Common algorithms with their time/space complexities. 



---
## Type Memory.IMemoryComplexityVisitor`1

 Extended visitor interface for memory complexity types. 



---
## Type Memory.SpaceComplexityClass

 Categories of space complexity. 



---
#### Field Memory.SpaceComplexityClass.Constant

 O(1) - Constant space. 



---
#### Field Memory.SpaceComplexityClass.Logarithmic

 O(log n) - Logarithmic space. 



---
#### Field Memory.SpaceComplexityClass.Linear

 O(n) - Linear space. 



---
#### Field Memory.SpaceComplexityClass.Linearithmic

 O(n log n) - Linearithmic space. 



---
#### Field Memory.SpaceComplexityClass.Quadratic

 O(n²) - Quadratic space. 



---
#### Field Memory.SpaceComplexityClass.Cubic

 O(n³) - Cubic space. 



---
#### Field Memory.SpaceComplexityClass.Exponential

 O(2^n) - Exponential space. 



---
#### Field Memory.SpaceComplexityClass.Unknown

 Unknown space complexity. 



---
## Type Memory.SpaceComplexityClassifier

 Utility methods for space complexity classification. 



---
#### Method Memory.SpaceComplexityClassifier.Classify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Classifies a complexity expression into a space complexity class. 



---
#### Method Memory.SpaceComplexityClassifier.IsBetterThan(ComplexityAnalysis.Core.Memory.SpaceComplexityClass,ComplexityAnalysis.Core.Memory.SpaceComplexityClass)

 Determines if one space complexity class is better (lower) than another. 



---
#### Method Memory.SpaceComplexityClassifier.GetDescription(ComplexityAnalysis.Core.Memory.SpaceComplexityClass)

 Gets a human-readable description of the space complexity class. 



---
## Type Memory.MemoryTier

 Represents the memory access tier hierarchy with associated performance weights. Each successive tier is approximately 1000x slower than the previous. 



---
#### Field Memory.MemoryTier.CpuCache

 L1/L2 CPU cache - fastest access (~1-10 ns). Typical sizes: L1 64KB, L2 256KB-512KB. 



---
#### Field Memory.MemoryTier.MainMemory

 Main memory (RAM) - fast but slower than cache (~100 ns). Typical sizes: 8GB-128GB. 



---
#### Field Memory.MemoryTier.LocalDisk

 Local disk storage (SSD/HDD) - much slower (~100 µs for SSD). 



---
#### Field Memory.MemoryTier.LocalNetwork

 Local network (LAN, same datacenter) - network latency (~1-10 ms). 



---
#### Field Memory.MemoryTier.FarNetwork

 Far network (WAN, internet, cross-region) - high latency (~100+ ms). 



---
## Type Memory.MemoryTierWeights

 Provides weight values for memory tier access costs. Uses a ~1000x compounding factor between tiers. 



---
#### Field Memory.MemoryTierWeights.CpuCache

 Base weight for CPU cache access (normalized to 1). 



---
#### Field Memory.MemoryTierWeights.MainMemory

 Weight for main memory access (~1000x cache). 



---
#### Field Memory.MemoryTierWeights.LocalDisk

 Weight for local disk access (~1000x memory). 



---
#### Field Memory.MemoryTierWeights.LocalNetwork

 Weight for local network access (~1000x disk). 



---
#### Field Memory.MemoryTierWeights.FarNetwork

 Weight for far network access (~1000x local network). 



---
#### Field Memory.MemoryTierWeights.CompoundingFactor

 The compounding factor between adjacent tiers. 



---
#### Method Memory.MemoryTierWeights.GetWeight(ComplexityAnalysis.Core.Memory.MemoryTier)

 Gets the weight for a given memory tier. 



---
#### Method Memory.MemoryTierWeights.GetWeightByLevel(System.Int32)

 Gets the weight for a tier by its ordinal level. Level 0 = Cache, Level 1 = Memory, etc. 



---
#### Property Memory.MemoryTierWeights.AllTiers

 Gets all tiers and their weights. 



---
## Type Memory.MemoryAccess

 Represents a single memory access with its tier and access count. 



---
#### Property Memory.MemoryAccess.Tier

 The memory tier being accessed. 



---
#### Property Memory.MemoryAccess.AccessCount

 The number of accesses (as a complexity expression). 



---
#### Property Memory.MemoryAccess.Description

 Optional description of what this access represents. 



---
#### Property Memory.MemoryAccess.WeightPerAccess

 Gets the weight per access for this tier. 



---
#### Property Memory.MemoryAccess.TotalCost

 Gets the total weighted cost as a complexity expression. 



---
#### Method Memory.MemoryAccess.Constant(ComplexityAnalysis.Core.Memory.MemoryTier,System.Double,System.String)

 Creates a constant number of accesses to a tier. 



---
#### Method Memory.MemoryAccess.Linear(ComplexityAnalysis.Core.Memory.MemoryTier,ComplexityAnalysis.Core.Complexity.Variable,System.String)

 Creates linear accesses to a tier: O(n) accesses. 



---
#### Method Memory.MemoryAccess.WithComplexity(ComplexityAnalysis.Core.Memory.MemoryTier,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates accesses with a given complexity expression. 



---
## Type Memory.AccessPattern

 Represents a pattern of memory access behavior. Used to infer likely memory tier placement. 



---
#### Field Memory.AccessPattern.Sequential

 Sequential access (e.g., array iteration) - cache-friendly. 



---
#### Field Memory.AccessPattern.Random

 Random access (e.g., hash table lookup) - likely main memory. 



---
#### Field Memory.AccessPattern.TemporalLocality

 Temporal locality - same data accessed multiple times. 



---
#### Field Memory.AccessPattern.SpatialLocality

 Spatial locality - nearby data accessed together. 



---
#### Field Memory.AccessPattern.Strided

 Strided access (e.g., matrix column traversal). 



---
#### Field Memory.AccessPattern.FileIO

 File I/O access. 



---
#### Field Memory.AccessPattern.Network

 Network access. 



---
## Type Memory.MemoryHierarchyCost

 Aggregates multiple memory accesses into a hierarchical cost model. 



---
#### Property Memory.MemoryHierarchyCost.Accesses

 All memory accesses in this cost model. 



---
#### Property Memory.MemoryHierarchyCost.TotalCost

 Gets the total weighted cost as a complexity expression. 



---
#### Property Memory.MemoryHierarchyCost.DominantTier

 Gets the dominant tier (the one contributing most to total cost). 



---
#### Property Memory.MemoryHierarchyCost.ByTier

 Groups accesses by tier. 



---
#### Method Memory.MemoryHierarchyCost.Add(ComplexityAnalysis.Core.Memory.MemoryAccess)

 Adds a memory access to this cost model. 



---
#### Method Memory.MemoryHierarchyCost.Combine(ComplexityAnalysis.Core.Memory.MemoryHierarchyCost)

 Combines two memory hierarchy costs. 



---
#### Property Memory.MemoryHierarchyCost.Empty

 Creates an empty cost model. 



---
#### Method Memory.MemoryHierarchyCost.Single(ComplexityAnalysis.Core.Memory.MemoryAccess)

 Creates a cost model with a single access. 



---
## Type Memory.MemoryTierEstimator

 Heuristics for estimating memory tier from access patterns and data sizes. 



---
#### Field Memory.MemoryTierEstimator.L1CacheSize

 Typical L1 cache size in bytes. 



---
#### Field Memory.MemoryTierEstimator.L2CacheSize

 Typical L2 cache size in bytes. 



---
#### Field Memory.MemoryTierEstimator.L3CacheSize

 Typical L3 cache size in bytes. 



---
#### Method Memory.MemoryTierEstimator.EstimateTier(ComplexityAnalysis.Core.Memory.AccessPattern,System.Int64)

 Estimates the memory tier based on access pattern and working set size. 



---
#### Method Memory.MemoryTierEstimator.ConservativeEstimate(ComplexityAnalysis.Core.Memory.AccessPattern)

 Conservative estimate: assumes main memory unless evidence suggests otherwise. 



---
## Type Progress.AnalysisPhase

 The phases of complexity analysis. 



---
#### Field Progress.AnalysisPhase.StaticExtraction

 Phase A: Static complexity extraction from AST/CFG. 



---
#### Field Progress.AnalysisPhase.RecurrenceSolving

 Phase B: Solving recurrence relations. 



---
#### Field Progress.AnalysisPhase.Refinement

 Phase C: Refinement via slack variables and perturbation. 



---
#### Field Progress.AnalysisPhase.SpeculativeAnalysis

 Phase D: Speculative analysis for partial code. 



---
#### Field Progress.AnalysisPhase.Calibration

 Phase E: Hardware calibration and weight adjustment. 



---
## Type Progress.IAnalysisProgress

 Callback interface for receiving progress updates during complexity analysis. Enables real-time feedback, logging, and early termination detection. 



---
#### Method Progress.IAnalysisProgress.OnPhaseStarted(ComplexityAnalysis.Core.Progress.AnalysisPhase)

 Called when an analysis phase begins. 



---
#### Method Progress.IAnalysisProgress.OnPhaseCompleted(ComplexityAnalysis.Core.Progress.AnalysisPhase,ComplexityAnalysis.Core.Progress.PhaseResult)

 Called when an analysis phase completes. 



---
#### Method Progress.IAnalysisProgress.OnMethodAnalyzed(ComplexityAnalysis.Core.Progress.MethodComplexityResult)

 Called when a method's complexity has been analyzed. 



---
#### Method Progress.IAnalysisProgress.OnIntermediateResult(ComplexityAnalysis.Core.Progress.PartialComplexityResult)

 Called with intermediate results during analysis. 



---
#### Method Progress.IAnalysisProgress.OnRecurrenceDetected(ComplexityAnalysis.Core.Progress.RecurrenceDetectionResult)

 Called when a recurrence relation is detected. 



---
#### Method Progress.IAnalysisProgress.OnRecurrenceSolved(ComplexityAnalysis.Core.Progress.RecurrenceSolutionResult)

 Called when a recurrence relation has been solved. 



---
#### Method Progress.IAnalysisProgress.OnWarning(ComplexityAnalysis.Core.Progress.AnalysisWarning)

 Called when a warning or issue is encountered. 



---
#### Method Progress.IAnalysisProgress.OnProgressUpdated(System.Double,System.String)

 Called periodically with overall progress percentage. 



---
## Type Progress.PhaseResult

 Result of a completed analysis phase. 



---
#### Property Progress.PhaseResult.Phase

 The phase that completed. 



---
#### Property Progress.PhaseResult.Success

 Whether the phase completed successfully. 



---
#### Property Progress.PhaseResult.Duration

 Duration of the phase. 



---
#### Property Progress.PhaseResult.ItemsProcessed

 Number of items processed in this phase. 



---
#### Property Progress.PhaseResult.ErrorMessage

 Optional error message if the phase failed. 



---
#### Property Progress.PhaseResult.Metadata

 Additional metadata about the phase result. 



---
## Type Progress.MethodComplexityResult

 Result of analyzing a single method's complexity. 



---
#### Property Progress.MethodComplexityResult.MethodName

 The fully qualified name of the method. 



---
#### Property Progress.MethodComplexityResult.FilePath

 The file path containing the method. 



---
#### Property Progress.MethodComplexityResult.LineNumber

 Line number where the method is defined. 



---
#### Property Progress.MethodComplexityResult.TimeComplexity

 The computed time complexity. 



---
#### Property Progress.MethodComplexityResult.SpaceComplexity

 The computed space complexity (if available). 



---
#### Property Progress.MethodComplexityResult.Confidence

 Confidence level in the result (0.0 to 1.0). 



---
#### Property Progress.MethodComplexityResult.RequiresReview

 Whether this result requires human review. 



---
#### Property Progress.MethodComplexityResult.ReviewReason

 Reason for requiring review (if applicable). 



---
## Type Progress.PartialComplexityResult

 Intermediate complexity result during analysis. 



---
#### Property Progress.PartialComplexityResult.Description

 Description of what was analyzed. 



---
#### Property Progress.PartialComplexityResult.Complexity

 The partial complexity expression. 



---
#### Property Progress.PartialComplexityResult.IsComplete

 Whether this is a complete or partial result. 



---
#### Property Progress.PartialComplexityResult.Context

 Context about where this result comes from. 



---
## Type Progress.RecurrenceDetectionResult

 Result when a recurrence relation is detected. 



---
#### Property Progress.RecurrenceDetectionResult.MethodName

 The method containing the recurrence. 



---
#### Property Progress.RecurrenceDetectionResult.Recurrence

 The detected recurrence pattern. 



---
#### Property Progress.RecurrenceDetectionResult.Type

 Type of recurrence detected. 



---
#### Property Progress.RecurrenceDetectionResult.IsSolvable

 Whether this recurrence can be solved analytically. 



---
#### Property Progress.RecurrenceDetectionResult.RecommendedApproach

 Recommended solving approach. 



---
## Type Progress.RecurrenceType

 Types of recurrence relations. 



---
#### Field Progress.RecurrenceType.Linear

 Linear recursion: T(n) = T(n-1) + f(n). 



---
#### Field Progress.RecurrenceType.DivideAndConquer

 Divide and conquer: T(n) = a·T(n/b) + f(n). 



---
#### Field Progress.RecurrenceType.MultiTerm

 Multi-term: T(n) = Σᵢ aᵢ·T(bᵢ·n) + f(n). 



---
#### Field Progress.RecurrenceType.Mutual

 Mutual recursion between multiple functions. 



---
#### Field Progress.RecurrenceType.NonStandard

 Non-standard recurrence requiring special handling. 



---
## Type Progress.SolvingApproach

 Approaches for solving recurrence relations. 



---
#### Field Progress.SolvingApproach.MasterTheorem

 Master Theorem for standard divide-and-conquer. 



---
#### Field Progress.SolvingApproach.AkraBazzi

 Akra-Bazzi theorem for general multi-term recurrences. 



---
#### Field Progress.SolvingApproach.Expansion

 Direct expansion/substitution. 



---
#### Field Progress.SolvingApproach.Numerical

 Numerical approximation. 



---
#### Field Progress.SolvingApproach.Unsolvable

 Cannot be solved analytically. 



---
## Type Progress.RecurrenceSolutionResult

 Result of solving a recurrence relation. 



---
#### Property Progress.RecurrenceSolutionResult.Input

 The input recurrence that was solved. 



---
#### Property Progress.RecurrenceSolutionResult.Solution

 The closed-form solution. 



---
#### Property Progress.RecurrenceSolutionResult.ApproachUsed

 The approach used to solve it. 



---
#### Property Progress.RecurrenceSolutionResult.Confidence

 Confidence in the solution. 



---
#### Property Progress.RecurrenceSolutionResult.IsExact

 Whether the solution is exact or an approximation. 



---
#### Property Progress.RecurrenceSolutionResult.Notes

 Additional notes about the solution. 



---
## Type Progress.AnalysisWarning

 Warning encountered during analysis. 



---
#### Property Progress.AnalysisWarning.Code

 Unique warning code. 



---
#### Property Progress.AnalysisWarning.Message

 Human-readable warning message. 



---
#### Property Progress.AnalysisWarning.Severity

 Severity of the warning. 



---
#### Property Progress.AnalysisWarning.Location

 Location in source code (if applicable). 



---
#### Property Progress.AnalysisWarning.SuggestedAction

 Suggested action to resolve the warning. 



---
## Type Progress.WarningSeverity

 Severity levels for analysis warnings. 



---
#### Field Progress.WarningSeverity.Info

 Informational message. 



---
#### Field Progress.WarningSeverity.Warning

 Warning that may affect accuracy. 



---
#### Field Progress.WarningSeverity.Error

 Error that prevents accurate analysis. 



---
## Type Progress.NullAnalysisProgress

 Null implementation of IAnalysisProgress that ignores all callbacks. 



---
## Type Progress.CompositeAnalysisProgress

 Aggregates multiple progress handlers. 



---
## Type Progress.ConsoleAnalysisProgress

 Logs progress to console output. 



---
## Type Recurrence.LinearRecurrenceRelation

 Represents a linear recurrence relation: T(n) = Σᵢ aᵢ·T(n-i) + f(n). 



>**General Form:** T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k) + f(n) 

 where: 

- aᵢ = coefficient for the (n-i)th term
- k = order of the recurrence (number of previous terms)
- f(n) = non-homogeneous term (driving function)


**Solution Method:** The characteristic polynomial method: 

-  Form characteristic polynomial: x^k - a₁x^(k-1) - a₂x^(k-2) - ... - aₖ = 0 
-  Find roots (may be real, repeated, or complex) 
-  Build general solution from roots 
-  Handle non-homogeneous term if present 


**Complexity Implications:**

**Root Type**: Solution Form
- **Single root r > 1**: O(rⁿ) - exponential growth
- **Single root r = 1 (with T(n-1))**: Summation: Σf(i)
- **Repeated root r (multiplicity m)**: O(n^(m-1) · rⁿ) - polynomial times exponential
- **Complex roots r·e^(iθ)**: Oscillatory: O(rⁿ) with periodic factor


**Common Patterns:**



######  code

```
    // Fibonacci: T(n) = T(n-1) + T(n-2) → O(φⁿ) where φ ≈ 1.618
    var fib = LinearRecurrenceRelation.Create(new[] { 1.0, 1.0 }, O_1, n);
    
    // Linear summation: T(n) = T(n-1) + O(1) → O(n)
    var linear = LinearRecurrenceRelation.Create(new[] { 1.0 }, O_1, n);
    
    // Exponential doubling: T(n) = 2T(n-1) + O(1) → O(2ⁿ)
    var exp2 = LinearRecurrenceRelation.Create(new[] { 2.0 }, O_1, n);
```



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceComplexity`](RecurrenceComplexity)



---
#### Property Recurrence.LinearRecurrenceRelation.Coefficients

 The coefficients [a₁, a₂, ..., aₖ] for T(n-1), T(n-2), ..., T(n-k). 



> Coefficients[0] is the coefficient of T(n-1), Coefficients[1] is the coefficient of T(n-2), etc. 



---
#### Property Recurrence.LinearRecurrenceRelation.NonRecursiveWork

 The non-homogeneous (driving) function f(n) in T(n) = ... + f(n). 



> If the recurrence is homogeneous (no f(n) term), this should be [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Zero]]. 



---
#### Property Recurrence.LinearRecurrenceRelation.Variable

 The variable representing the input size (typically n). 



---
#### Property Recurrence.LinearRecurrenceRelation.Order

 The order of the recurrence (k in T(n-k)). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsHomogeneous

 Whether this is a homogeneous recurrence (no f(n) term). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsFirstOrder

 Whether this is a first-order recurrence T(n) = a·T(n-1) + f(n). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsSummation

 Whether this is a simple summation T(n) = T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Create(System.Double[],ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recurrence relation. 

|Name | Description |
|-----|------|
|coefficients: | Coefficients [a₁, a₂, ..., aₖ] where T(n) = a₁T(n-1) + a₂T(n-2) + ... + f(n). |
|nonRecursiveWork: |The non-homogeneous term f(n).|
|variable: |The recurrence variable (typically n).|
**Returns**: A new linear recurrence relation.

[[T:System.ArgumentException|T:System.ArgumentException]]: If coefficients is empty.



---
#### Method Recurrence.LinearRecurrenceRelation.Create(System.Collections.Immutable.ImmutableArray{System.Double},ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recurrence relation with immutable coefficients. 



---
#### Method Recurrence.LinearRecurrenceRelation.Fibonacci(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates the Fibonacci recurrence: T(n) = T(n-1) + T(n-2) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Summation(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple summation recurrence: T(n) = T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Exponential(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates an exponential recurrence: T(n) = a·T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.ToString

 Gets a human-readable representation of the recurrence. 



---
## Type Recurrence.LinearRecurrenceSolution

 Result of solving a linear recurrence relation. 



> Contains both the asymptotic solution and details about how it was derived. 



---
#### Property Recurrence.LinearRecurrenceSolution.Solution

 The closed-form asymptotic solution. 



---
#### Property Recurrence.LinearRecurrenceSolution.Method

 Description of the solution method used. 



---
#### Property Recurrence.LinearRecurrenceSolution.Roots

 The roots of the characteristic polynomial. 



---
#### Property Recurrence.LinearRecurrenceSolution.DominantRoot

 The dominant root (largest magnitude). 



---
#### Property Recurrence.LinearRecurrenceSolution.HasPolynomialFactor

 Whether the solution involves polynomial factors from repeated roots. 



---
#### Property Recurrence.LinearRecurrenceSolution.Explanation

 Detailed explanation of the solution derivation. 



---
## Type Recurrence.CharacteristicRoot

 A root of the characteristic polynomial with its properties. 



> Roots can be real or complex. Complex roots always come in conjugate pairs for recurrences with real coefficients. 



---
#### Property Recurrence.CharacteristicRoot.RealPart

 The real part of the root. 



---
#### Property Recurrence.CharacteristicRoot.ImaginaryPart

 The imaginary part of the root (0 for real roots). 



---
#### Property Recurrence.CharacteristicRoot.Magnitude

 The magnitude |r| = √(a² + b²) for complex root a + bi. 



---
#### Property Recurrence.CharacteristicRoot.Multiplicity

 The multiplicity (how many times this root appears). 



---
#### Property Recurrence.CharacteristicRoot.IsReal

 Whether this is a real root (imaginary part ≈ 0). 



---
#### Property Recurrence.CharacteristicRoot.IsRepeated

 Whether this is a repeated root (multiplicity > 1). 



---
#### Method Recurrence.CharacteristicRoot.Real(System.Double,System.Int32)

 Creates a real root. 



---
#### Method Recurrence.CharacteristicRoot.Complex(System.Double,System.Double,System.Int32)

 Creates a complex root. 



---
## Type Recurrence.MutualRecurrenceSystem

 Represents a system of mutually recursive recurrence relations. For mutually recursive functions A(n) and B(n): - A(n) = T_A(n-1) + f_A(n) where A calls B - B(n) = T_B(n-1) + f_B(n) where B calls A This can be combined into a single recurrence by substitution. 



---
#### Property Recurrence.MutualRecurrenceSystem.Components

 The methods involved in the mutual recursion cycle. The order represents the cycle: methods[0] → methods[1] → ... → methods[0] 



---
#### Property Recurrence.MutualRecurrenceSystem.Variable

 The recurrence variable (typically n). 



---
#### Property Recurrence.MutualRecurrenceSystem.CycleLength

 Number of methods in the cycle. 



---
#### Property Recurrence.MutualRecurrenceSystem.CombinedReduction

 The combined reduction per full cycle through all methods. For A → B → A with each doing -1, this is -2 (or scale 0.99^2 for divide pattern). 



---
#### Property Recurrence.MutualRecurrenceSystem.CombinedWork

 The combined non-recursive work done in one full cycle. 



---
#### Method Recurrence.MutualRecurrenceSystem.ToSingleRecurrence

 Converts the mutual recursion system to an equivalent single recurrence. For a cycle A → B → C → A where each reduces by 1: Combined: T(n) = T(n - cycleLength) + CombinedWork 



---
#### Property Recurrence.MutualRecurrenceSystem.IsSubtractionPattern

 Whether this is a subtraction-based mutual recursion (each step reduces by constant). 



---
#### Property Recurrence.MutualRecurrenceSystem.IsDivisionPattern

 Whether this is a division-based mutual recursion (each step divides by constant). 



---
#### Method Recurrence.MutualRecurrenceSystem.GetDescription

 Gets a human-readable description of the mutual recursion. 



---
## Type Recurrence.MutualRecurrenceComponent

 Represents one method in a mutual recursion cycle. 



---
#### Property Recurrence.MutualRecurrenceComponent.MethodName

 The method name (for diagnostics). 



---
#### Property Recurrence.MutualRecurrenceComponent.NonRecursiveWork

 The non-recursive work done by this method. 



---
#### Property Recurrence.MutualRecurrenceComponent.Reduction

 How much the problem size is reduced when calling the next method. For subtraction: reduction amount (e.g., 1 for n-1). 



---
#### Property Recurrence.MutualRecurrenceComponent.ScaleFactor

 Scale factor for divide-style patterns (1/b in T(n/b)). For subtraction patterns, this is close to 1 (e.g., 0.99). 



---
#### Property Recurrence.MutualRecurrenceComponent.Callees

 The methods this component calls (within the cycle). 



---
## Type Recurrence.MutualRecurrenceSolution

 Result of solving a mutual recursion system. 



---
#### Property Recurrence.MutualRecurrenceSolution.Success

 Whether the system was successfully solved. 



---
#### Property Recurrence.MutualRecurrenceSolution.Solution

 The complexity solution for the first method in the cycle. Since all methods in the cycle have the same asymptotic complexity (differing only by constants), this applies to all. 



---
#### Property Recurrence.MutualRecurrenceSolution.MethodSolutions

 Individual solutions for each method (may differ by constant factors). 



---
#### Property Recurrence.MutualRecurrenceSolution.Method

 The approach used to solve the recurrence. 



---
#### Property Recurrence.MutualRecurrenceSolution.FailureReason

 Diagnostic information if solving failed. 



---
#### Property Recurrence.MutualRecurrenceSolution.EquivalentRecurrence

 The equivalent single recurrence that was solved. 



---
#### Method Recurrence.MutualRecurrenceSolution.Solved(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Creates a successful solution. 



---
#### Method Recurrence.MutualRecurrenceSolution.Failed(System.String)

 Creates a failed solution. 



---
## Type Recurrence.RecurrenceComplexity

 Represents a recurrence relation for complexity analysis of recursive algorithms. 



>**General Form:** T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n) 

 where: 

- aᵢ = number of recursive calls of type i
- bᵢ = scale factor for subproblem size (0 < bᵢ < 1)
- hᵢ(n) = perturbation function (often 0)
- g(n) = non-recursive work at each level


**Analysis Theorems:**

**Theorem**: Applicability
- **Master Theorem**: Single-term: T(n) = a·T(n/b) + f(n), where a ≥ 1, b > 1
- **Akra-Bazzi**: Multi-term: T(n) = Σᵢ aᵢ·T(bᵢn) + g(n), where aᵢ > 0, 0 < bᵢ < 1
- **Linear Recurrence**: T(n) = T(n-1) + f(n), solved by summation


**Common Patterns:**



######  code

```
    // Merge Sort: T(n) = 2T(n/2) + O(n) → O(n log n)
    var mergeSort = RecurrenceComplexity.DivideAndConquer(2, 2, O_n, n);
    
    // Binary Search: T(n) = T(n/2) + O(1) → O(log n)
    var binarySearch = RecurrenceComplexity.DivideAndConquer(1, 2, O_1, n);
    
    // Strassen: T(n) = 7T(n/2) + O(n²) → O(n^2.807)
    var strassen = RecurrenceComplexity.DivideAndConquer(7, 2, O_n2, n);
```

 See the TheoremApplicabilityAnalyzer in ComplexityAnalysis.Solver for the analysis engine that solves these recurrences. 



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceTerm`](RecurrenceTerm)



---
#### Method Recurrence.RecurrenceComplexity.#ctor(System.Collections.Immutable.ImmutableList{ComplexityAnalysis.Core.Recurrence.RecurrenceTerm},ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Represents a recurrence relation for complexity analysis of recursive algorithms. 



>**General Form:** T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n) 

 where: 

- aᵢ = number of recursive calls of type i
- bᵢ = scale factor for subproblem size (0 < bᵢ < 1)
- hᵢ(n) = perturbation function (often 0)
- g(n) = non-recursive work at each level


**Analysis Theorems:**

**Theorem**: Applicability
- **Master Theorem**: Single-term: T(n) = a·T(n/b) + f(n), where a ≥ 1, b > 1
- **Akra-Bazzi**: Multi-term: T(n) = Σᵢ aᵢ·T(bᵢn) + g(n), where aᵢ > 0, 0 < bᵢ < 1
- **Linear Recurrence**: T(n) = T(n-1) + f(n), solved by summation


**Common Patterns:**



######  code

```
    // Merge Sort: T(n) = 2T(n/2) + O(n) → O(n log n)
    var mergeSort = RecurrenceComplexity.DivideAndConquer(2, 2, O_n, n);
    
    // Binary Search: T(n) = T(n/2) + O(1) → O(log n)
    var binarySearch = RecurrenceComplexity.DivideAndConquer(1, 2, O_1, n);
    
    // Strassen: T(n) = 7T(n/2) + O(n²) → O(n^2.807)
    var strassen = RecurrenceComplexity.DivideAndConquer(7, 2, O_n2, n);
```

 See the TheoremApplicabilityAnalyzer in ComplexityAnalysis.Solver for the analysis engine that solves these recurrences. 



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceTerm`](RecurrenceTerm)



---
#### Property Recurrence.RecurrenceComplexity.TotalRecursiveCalls

 Gets the total number of recursive calls (sum of coefficients). For T(n) = 2T(n/2) + O(n), this returns 2. 



---
#### Property Recurrence.RecurrenceComplexity.FitsMasterTheorem

 Determines if this recurrence fits the Master Theorem pattern: T(n) = a·T(n/b) + f(n) where a ≥ 1, b > 1. 



---
#### Property Recurrence.RecurrenceComplexity.FitsAkraBazzi

 Determines if this recurrence fits the Akra-Bazzi pattern (more general than Master Theorem). 



---
#### Method Recurrence.RecurrenceComplexity.DivideAndConquer(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + O(n^d). 



---
#### Method Recurrence.RecurrenceComplexity.Linear(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recursion: T(n) = T(n-1) + O(f(n)). 



---
## Type Recurrence.RecurrenceTerm

 Represents a single term in a recurrence relation. 



> For a recurrence like T(n) = 2·T(n/3) + O(n), the term is: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Coefficient]] = 2 (number of recursive calls)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Argument]] = n/3 (subproblem size expression)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.ScaleFactor]] = 1/3 (reduction ratio)


**Well-formedness:** For theorem applicability, terms must satisfy: 

- Coefficient > 0 (at least one recursive call)
- 0 < ScaleFactor < 1 (subproblem is smaller)




|Name | Description |
|-----|------|
|Coefficient: |The multiplier for this recursive call (a in a·T(f(n))).|
|Argument: |The argument to the recursive call (f(n) in T(f(n))).|
|ScaleFactor: |The scale factor for the subproblem size (1/b in T(n/b)).|


---
#### Method Recurrence.RecurrenceTerm.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Represents a single term in a recurrence relation. 



> For a recurrence like T(n) = 2·T(n/3) + O(n), the term is: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Coefficient]] = 2 (number of recursive calls)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Argument]] = n/3 (subproblem size expression)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.ScaleFactor]] = 1/3 (reduction ratio)


**Well-formedness:** For theorem applicability, terms must satisfy: 

- Coefficient > 0 (at least one recursive call)
- 0 < ScaleFactor < 1 (subproblem is smaller)




|Name | Description |
|-----|------|
|Coefficient: |The multiplier for this recursive call (a in a·T(f(n))).|
|Argument: |The argument to the recursive call (f(n) in T(f(n))).|
|ScaleFactor: |The scale factor for the subproblem size (1/b in T(n/b)).|


---
#### Property Recurrence.RecurrenceTerm.Coefficient

The multiplier for this recursive call (a in a·T(f(n))).



---
#### Property Recurrence.RecurrenceTerm.Argument

The argument to the recursive call (f(n) in T(f(n))).



---
#### Property Recurrence.RecurrenceTerm.ScaleFactor

The scale factor for the subproblem size (1/b in T(n/b)).



---
#### Property Recurrence.RecurrenceTerm.IsReducing

 Determines if this term represents a proper reduction (subproblem smaller than original). 



---
## Type Recurrence.RecurrenceRelationTerm

 A term in a recurrence relation with coefficient and scale factor. 



---
#### Method Recurrence.RecurrenceRelationTerm.#ctor(System.Double,System.Double)

 A term in a recurrence relation with coefficient and scale factor. 



---
## Type Recurrence.RecurrenceRelation

 Represents a fully specified recurrence relation with explicit terms for analysis. 



> This is the normalized form used as input to recurrence solvers. It extracts the essential mathematical components from [[|T:ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity]]: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.Terms]]: The recursive structure [(aᵢ, bᵢ)]
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.NonRecursiveWork]]: The g(n) function
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.BaseCase]]: The T(1) boundary condition


**Theorem Selection:**

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.FitsMasterTheorem]]: Single term with a ≥ 1, b > 1 
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.FitsAkraBazzi]]: All terms have aᵢ > 0 and 0 < bᵢ < 1 


**Convenience Factories:**



######  code

```
    // Standard divide-and-conquer
    var rec = RecurrenceRelation.DivideAndConquer(2, 2, O_n, Variable.N);
    
    // From existing RecurrenceComplexity
    var rel = RecurrenceRelation.FromComplexity(recurrence);
```



**See also**: [`RecurrenceComplexity`](RecurrenceComplexity)



---
#### Property Recurrence.RecurrenceRelation.Terms

 The recursive terms: [(aᵢ, bᵢ)] where T(n) contains aᵢ·T(bᵢ·n). 



---
#### Property Recurrence.RecurrenceRelation.NonRecursiveWork

 The non-recursive work function g(n). 



---
#### Property Recurrence.RecurrenceRelation.BaseCase

 The base case complexity T(1). 



---
#### Property Recurrence.RecurrenceRelation.Variable

 The recurrence variable (typically n). 



---
#### Method Recurrence.RecurrenceRelation.#ctor(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Recurrence.RecurrenceRelationTerm},ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Creates a recurrence relation from explicit terms. 



---
#### Property Recurrence.RecurrenceRelation.FitsMasterTheorem

 Checks if this recurrence fits the Master Theorem form. 



---
#### Property Recurrence.RecurrenceRelation.FitsAkraBazzi

 Checks if this recurrence fits the Akra-Bazzi pattern. 



---
#### Property Recurrence.RecurrenceRelation.A

 For Master Theorem: a in T(n) = a·T(n/b) + f(n). 



---
#### Property Recurrence.RecurrenceRelation.B

 For Master Theorem: b in T(n) = a·T(n/b) + f(n). 



---
#### Method Recurrence.RecurrenceRelation.FromComplexity(ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity)

 Creates a RecurrenceRelation from a RecurrenceComplexity. 



---
#### Method Recurrence.RecurrenceRelation.DivideAndConquer(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + f(n). 



---
## Type Recurrence.TheoremApplicability

 Base type for theorem applicability results. Captures which theorem applies (or not) and all relevant parameters. 



---
#### Property Recurrence.TheoremApplicability.IsApplicable

Whether any theorem successfully applies.



---
#### Property Recurrence.TheoremApplicability.Solution

The recommended solution if applicable.



---
#### Property Recurrence.TheoremApplicability.Explanation

Human-readable explanation of the result.



---
## Type Recurrence.MasterTheoremApplicable

 Master Theorem applies successfully. 



---
#### Method Recurrence.MasterTheoremApplicable.#ctor(ComplexityAnalysis.Core.Recurrence.MasterTheoremCase,System.Double,System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Master Theorem applies successfully. 



---
#### Property Recurrence.MasterTheoremApplicable.Epsilon

 For Case 1: the ε such that f(n) = O(n^(log_b(a) - ε)). For Case 3: the ε such that f(n) = Ω(n^(log_b(a) + ε)). For Case 2: 0 (exact match). 



---
#### Property Recurrence.MasterTheoremApplicable.LogExponentK

For Case 2: the k in f(n) = Θ(n^d · log^k n).



---
#### Property Recurrence.MasterTheoremApplicable.RegularityVerified

For Case 3: whether the regularity condition was verified.



---
## Type Recurrence.MasterTheoremCase

 The three cases of the Master Theorem. 



---
#### Field Recurrence.MasterTheoremCase.Case1

 f(n) = O(n^(log_b(a) - ε)) for some ε > 0. Work at leaves dominates. Solution: Θ(n^(log_b a)). 



---
#### Field Recurrence.MasterTheoremCase.Case2

 f(n) = Θ(n^(log_b a) · log^k n) for some k ≥ 0. Work balanced across levels. Solution: Θ(n^(log_b a) · log^(k+1) n). 



---
#### Field Recurrence.MasterTheoremCase.Case3

 f(n) = Ω(n^(log_b(a) + ε)) for some ε > 0, AND regularity holds. Work at root dominates. Solution: Θ(f(n)). 



---
#### Field Recurrence.MasterTheoremCase.Gap

 Falls between cases (Master Theorem gap). Use Akra-Bazzi or other methods. 



---
## Type Recurrence.AkraBazziApplicable

 Akra-Bazzi Theorem applies successfully. 



---
#### Method Recurrence.AkraBazziApplicable.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Akra-Bazzi Theorem applies successfully. 



---
#### Property Recurrence.AkraBazziApplicable.Terms

The recurrence terms used.



---
#### Property Recurrence.AkraBazziApplicable.GClassification

Classification of g(n).



---
## Type Recurrence.LinearRecurrenceSolved

 Linear recurrence T(n) = T(n-1) + f(n) solved directly. 



---
#### Method Recurrence.LinearRecurrenceSolved.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Linear recurrence T(n) = T(n-1) + f(n) solved directly. 



---
## Type Recurrence.TheoremNotApplicable

 No standard theorem applies. 



---
#### Method Recurrence.TheoremNotApplicable.#ctor(System.String,System.Collections.Immutable.ImmutableList{System.String})

 No standard theorem applies. 



---
#### Property Recurrence.TheoremNotApplicable.Suggestions

Suggested alternative approaches.



---
## Type Recurrence.ITheoremApplicabilityAnalyzer

 Analyzer that determines which theorem applies to a recurrence. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.Analyze(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Analyzes a recurrence and determines which theorem applies. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.CheckMasterTheorem(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Specifically checks Master Theorem applicability. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.CheckAkraBazzi(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Specifically checks Akra-Bazzi applicability. 



---
## Type Recurrence.TheoremApplicabilityExtensions

 Extension methods for working with theorem applicability. 



---
#### Method Recurrence.TheoremApplicabilityExtensions.AnalyzeRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity,ComplexityAnalysis.Core.Recurrence.ITheoremApplicabilityAnalyzer)

 Tries Master Theorem first, then Akra-Bazzi, then reports failure. 



---



# ComplexityAnalysis.Core #

## Type Complexity.AmortizedComplexity

 Represents amortized complexity - the average cost per operation over a sequence. Amortized analysis accounts for expensive operations that happen infrequently, giving a more accurate picture of average-case performance. Examples: - Dynamic array Add: O(n) worst case, O(1) amortized - Hash table insert: O(n) worst case, O(1) amortized - Splay tree operations: O(n) worst case, O(log n) amortized 



---
#### Property Complexity.AmortizedComplexity.AmortizedCost

 The amortized (average) complexity per operation. 



---
#### Property Complexity.AmortizedComplexity.WorstCaseCost

 The worst-case complexity for a single operation. 



---
#### Property Complexity.AmortizedComplexity.Method

 The method used to derive the amortized bound. 



---
#### Property Complexity.AmortizedComplexity.Potential

 Optional potential function used for analysis. 



---
#### Property Complexity.AmortizedComplexity.Description

 Description of the amortization scenario. 



---
#### Method Complexity.AmortizedComplexity.ConstantAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an amortized constant complexity (like List.Add). 



---
#### Method Complexity.AmortizedComplexity.LogarithmicAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an amortized logarithmic complexity (like splay tree operations). 



---
#### Method Complexity.AmortizedComplexity.InverseAckermannAmortized(ComplexityAnalysis.Core.Complexity.Variable)

 Creates an inverse Ackermann amortized complexity (like Union-Find). 



---
## Type Complexity.AmortizationMethod

 Methods for deriving amortized bounds. 



---
#### Field Complexity.AmortizationMethod.Aggregate

 Aggregate method: Total cost / number of operations. Simple but doesn't give per-operation insight. 



---
#### Field Complexity.AmortizationMethod.Accounting

 Accounting method: Assign credits to operations. Cheap operations pay for expensive ones. 



---
#### Field Complexity.AmortizationMethod.Potential

 Potential method: Define potential function Φ(state). Amortized cost = actual cost + ΔΦ. Most powerful, gives tight bounds. 



---
## Type Complexity.PotentialFunction

 Represents a potential function for amortized analysis. Φ: DataStructureState → ℝ≥0 



---
#### Property Complexity.PotentialFunction.Name

 Name/description of the potential function. 



---
#### Property Complexity.PotentialFunction.Formula

 Mathematical description of the potential function. 



---
#### Property Complexity.PotentialFunction.SizeVariable

 The variable representing the data structure size. 



---
## Type Complexity.PotentialFunction.Common

 Common potential functions. 



---
#### Property Complexity.PotentialFunction.Common.DynamicArray

 Dynamic array: Φ = 2n - capacity 



---
#### Property Complexity.PotentialFunction.Common.HashTable

 Hash table: Φ = 2n - buckets 



---
#### Property Complexity.PotentialFunction.Common.BinaryCounter

 Binary counter: Φ = number of 1-bits 



---
#### Property Complexity.PotentialFunction.Common.MultipopStack

 Stack with multipop: Φ = stack size 



---
#### Property Complexity.PotentialFunction.Common.SplayTree

 Splay tree: Φ = Σ log(size of subtree) 



---
#### Property Complexity.PotentialFunction.Common.UnionFind

 Union-Find: Φ based on ranks 



---
## Type Complexity.InverseAckermannComplexity

 Inverse Ackermann complexity: O(α(n)) - effectively constant for practical inputs. Used in Union-Find with path compression and union by rank. 



---
#### Method Complexity.InverseAckermannComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable)

 Inverse Ackermann complexity: O(α(n)) - effectively constant for practical inputs. Used in Union-Find with path compression and union by rank. 



---
#### Method Complexity.InverseAckermannComplexity.InverseAckermann(System.Int64)

 Computes inverse Ackermann function α(n). α(n) = min { k : A(k, k) ≥ n } where A is Ackermann function. For all practical n, α(n) ≤ 4. 



---
## Type Complexity.IAmortizedComplexityVisitor`1

 Extended visitor interface for amortized complexity types. 



---
## Type Complexity.ComplexityComposition

 Provides methods for composing complexity expressions based on control flow patterns. These rules form the foundation of static complexity analysis. 



---
#### Method Complexity.ComplexityComposition.Sequential(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Sequential composition: T₁ followed by T₂. Total complexity: O(T₁ + T₂) In Big-O terms, the dominant term will dominate: O(n) + O(n²) = O(n²) 



---
#### Method Complexity.ComplexityComposition.Sequential(ComplexityAnalysis.Core.Complexity.ComplexityExpression[])

 Sequential composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Sequential(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Sequential composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Nested(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Nested composition: T₁ inside T₂ (e.g., nested loops). Total complexity: O(T₁ × T₂) Example: for i in 0..n: for j in 0..n: O(1) Result: O(n) × O(n) × O(1) = O(n²) 



---
#### Method Complexity.ComplexityComposition.Nested(ComplexityAnalysis.Core.Complexity.ComplexityExpression[])

 Nested composition of multiple expressions (deeply nested loops). 



---
#### Method Complexity.ComplexityComposition.Nested(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Nested composition of multiple expressions. 



---
#### Method Complexity.ComplexityComposition.Branching(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Branching composition: if-else statement. Total complexity: O(max(T_true, T_false)) We take the worst case because either branch might execute. 



---
#### Method Complexity.ComplexityComposition.BranchingWithCondition(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Branching composition with condition overhead. Total complexity: O(T_condition + max(T_true, T_false)) 



---
#### Method Complexity.ComplexityComposition.Switch(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.ComplexityExpression})

 Multi-way branching (switch/match). Total complexity: O(max(T₁, T₂, ..., Tₙ)) 



---
#### Method Complexity.ComplexityComposition.Loop(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Loop composition with known iteration count. Total complexity: O(iterations × body) 



---
#### Method Complexity.ComplexityComposition.ForLoop(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 For loop with linear iterations: for i = 0 to n. Total complexity: O(n × body) 



---
#### Method Complexity.ComplexityComposition.BoundedForLoop(System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 For loop with bounded iterations: for i = 0 to constant. Total complexity: O(body) (the constant factor is absorbed) 



---
#### Method Complexity.ComplexityComposition.LogarithmicLoop(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Logarithmic loop: for i = 1; i < n; i *= 2. Total complexity: O(log n × body) 



---
#### Method Complexity.ComplexityComposition.LoopWithEarlyExit(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Early exit pattern: loop that may terminate early. Total complexity: O(min(early_exit, full_iterations) × body) For worst-case analysis, we typically use the full iterations. For average-case, the expected early exit point matters. 



---
#### Method Complexity.ComplexityComposition.LinearRecursion(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Recursive composition: function calls itself. Returns a RecurrenceComplexity that needs to be solved. For T(n) = T(n-1) + work, this creates: RecurrenceComplexity with linear reduction. 



---
#### Method Complexity.ComplexityComposition.DivideAndConquer(ComplexityAnalysis.Core.Complexity.Variable,System.Int32,System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Divide and conquer recursion: T(n) = a × T(n/b) + work. Returns a RecurrenceComplexity that can be solved via Master Theorem. 



---
#### Method Complexity.ComplexityComposition.BinaryRecursion(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Binary recursion: T(n) = 2T(n/2) + work. Common pattern for divide and conquer algorithms. 



---
#### Method Complexity.ComplexityComposition.FunctionCall(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Function call composition: calling a function with known complexity. Total complexity: O(argument_setup + function_complexity) 



---
#### Method Complexity.ComplexityComposition.Amortized(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Amortized operation: multiple operations with varying individual costs but known total cost over n operations. Example: n insertions into a dynamic array = O(n) total, O(1) amortized per op. 



---
#### Method Complexity.ComplexityComposition.Conditional(System.String,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Conditional complexity: different complexity based on runtime condition. 



---
## Type Complexity.ComplexityBuilder

 Fluent builder for constructing complex complexity expressions. 



---
#### Method Complexity.ComplexityBuilder.Constant

 Start building with O(1). 



---
#### Method Complexity.ComplexityBuilder.Linear(ComplexityAnalysis.Core.Complexity.Variable)

 Start building with O(n). 



---
#### Method Complexity.ComplexityBuilder.Then(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Add sequential operation: current + next. 



---
#### Method Complexity.ComplexityBuilder.InsideLoop(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Nest inside a loop: iterations × current. 



---
#### Method Complexity.ComplexityBuilder.InsideLinearLoop(ComplexityAnalysis.Core.Complexity.Variable)

 Nest inside a loop over n. 



---
#### Method Complexity.ComplexityBuilder.OrBranch(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Add branch: max(current, alternative). 



---
#### Method Complexity.ComplexityBuilder.Times(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Multiply by a factor. 



---
#### Method Complexity.ComplexityBuilder.Build

 Build the final expression. 



---
#### Method Complexity.ComplexityBuilder.op_Implicit(ComplexityAnalysis.Core.Complexity.ComplexityBuilder)~ComplexityAnalysis.Core.Complexity.ComplexityExpression

 Implicit conversion to ComplexityExpression. 



---
## Type Complexity.ComplexityExpression

 Base type for all complexity expressions representing algorithmic time or space complexity. 



>**Design Philosophy:** This forms the core of an expression-based complexity algebra that represents Big-O expressions as composable Abstract Syntax Trees (AST). This design enables: 

- Type-safe composition of complexity expressions
- Algebraic simplification (e.g., O(n) + O(n²) → O(n²))
- Variable substitution for parametric complexity
- Evaluation for specific input sizes
- Visitor pattern for transformation and analysis


**Type Hierarchy:**

**Category**: Types
- **Primitive**: [[|T:ComplexityAnalysis.Core.Complexity.ConstantComplexity]] (O(1)), [[|T:ComplexityAnalysis.Core.Complexity.VariableComplexity]] (O(n)), [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]] (O(k·n)) 
- **Polynomial**: [[|T:ComplexityAnalysis.Core.Complexity.PolynomialComplexity]] (O(n²), O(n³), etc.), [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] (O(n log n)) 
- **Transcendental**: [[|T:ComplexityAnalysis.Core.Complexity.LogarithmicComplexity]] (O(log n)), [[|T:ComplexityAnalysis.Core.Complexity.ExponentialComplexity]] (O(2ⁿ)), [[|T:ComplexityAnalysis.Core.Complexity.FactorialComplexity]] (O(n!)) 
- **Compositional**: [[|T:ComplexityAnalysis.Core.Complexity.BinaryOperationComplexity]] (+, ×, max, min), [[|T:ComplexityAnalysis.Core.Complexity.ConditionalComplexity]] (branching) 


**Composition Rules:**



######  code

```
    // Sequential (addition): loops following loops
    var seq = new BinaryOperationComplexity(O_n, BinaryOp.Plus, O_logN);
    // → O(n + log n) → O(n) after simplification
    
    // Nested (multiplication): loops inside loops
    var nested = new BinaryOperationComplexity(O_n, BinaryOp.Multiply, O_m);
    // → O(n × m)
    
    // Branching (max): if-else with different complexities
    var branch = new BinaryOperationComplexity(O_n, BinaryOp.Max, O_nSquared);
    // → O(max(n, n²)) → O(n²)
```

 All expressions are implemented as immutable records for thread-safety and functional composition patterns. 



**See also**: [`IComplexityVisitor`1`](IComplexityVisitor`1)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Method Complexity.ComplexityExpression.Accept``1(ComplexityAnalysis.Core.Complexity.IComplexityVisitor{``0})

 Accept a visitor for the expression tree. 



---
#### Method Complexity.ComplexityExpression.Substitute(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Substitute a variable with another expression. 



---
#### Property Complexity.ComplexityExpression.FreeVariables

 Get all free (unbound) variables in this expression. 



---
#### Method Complexity.ComplexityExpression.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})

 Evaluate the expression for a given variable assignment. Returns null if evaluation is not possible (e.g., missing variables). 



---
#### Method Complexity.ComplexityExpression.ToBigONotation

 Get a human-readable string representation in Big-O notation. 



---
## Type Complexity.ConstantComplexity

 Represents a constant complexity: O(1) or O(k) for some constant k. 



> Constant complexity represents operations whose execution time does not depend on input size. Common sources include: 

- Array indexing:  `arr[i]` 
- Hash table lookup (amortized):  `dict[key]` 
- Arithmetic operations:  `a + b * c` 
- Base cases of recursive algorithms


 The [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Value]] property captures any constant factor, though in asymptotic analysis O(1) = O(k) for any constant k. 



|Name | Description |
|-----|------|
|Value: |The constant value (typically 1).|


---
#### Method Complexity.ConstantComplexity.#ctor(System.Double)

 Represents a constant complexity: O(1) or O(k) for some constant k. 



> Constant complexity represents operations whose execution time does not depend on input size. Common sources include: 

- Array indexing:  `arr[i]` 
- Hash table lookup (amortized):  `dict[key]` 
- Arithmetic operations:  `a + b * c` 
- Base cases of recursive algorithms


 The [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Value]] property captures any constant factor, though in asymptotic analysis O(1) = O(k) for any constant k. 



|Name | Description |
|-----|------|
|Value: |The constant value (typically 1).|


---
#### Property Complexity.ConstantComplexity.Value

The constant value (typically 1).



---
#### Property Complexity.ConstantComplexity.One

 The canonical O(1) constant complexity. 



---
#### Property Complexity.ConstantComplexity.Zero

 Zero complexity (for base cases). 



---
## Type Complexity.VariableComplexity

 Represents a single variable complexity: O(n), O(V), O(E), etc. 



> This is the simplest form of linear complexity—a single variable without a coefficient. For complexity with coefficients, see [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]]. 

 Common variable types defined in [[|T:ComplexityAnalysis.Core.Complexity.Variable]]: 

-  `n`  - General input size
-  `V`  - Vertex count in graphs
-  `E`  - Edge count in graphs
-  `m`  - Secondary size parameter (e.g., pattern length)




|Name | Description |
|-----|------|
|Var: |The variable representing the input size.|
**See also**: [`Variable`](Variable)

**See also**: [`VariableType`](VariableType)



---
#### Method Complexity.VariableComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable)

 Represents a single variable complexity: O(n), O(V), O(E), etc. 



> This is the simplest form of linear complexity—a single variable without a coefficient. For complexity with coefficients, see [[|T:ComplexityAnalysis.Core.Complexity.LinearComplexity]]. 

 Common variable types defined in [[|T:ComplexityAnalysis.Core.Complexity.Variable]]: 

-  `n`  - General input size
-  `V`  - Vertex count in graphs
-  `E`  - Edge count in graphs
-  `m`  - Secondary size parameter (e.g., pattern length)




|Name | Description |
|-----|------|
|Var: |The variable representing the input size.|
**See also**: [`Variable`](Variable)

**See also**: [`VariableType`](VariableType)



---
#### Property Complexity.VariableComplexity.Var

The variable representing the input size.



---
## Type Complexity.LinearComplexity

 Represents linear complexity with a coefficient: O(k·n). 



---
#### Method Complexity.LinearComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Represents linear complexity with a coefficient: O(k·n). 



---
## Type Complexity.PolynomialComplexity

 Represents polynomial complexity: O(n²), O(n³), or general polynomial forms. 



> Polynomials represent algorithms with nested loops or recursive patterns that process proportional fractions of input at each level. 

**Structure:** The [[|P:ComplexityAnalysis.Core.Complexity.PolynomialComplexity.Coefficients]] dictionary maps degree → coefficient. For example: 

-  `{2: 1}`  represents n²
-  `{2: 3, 1: 2}`  represents 3n² + 2n
-  `{3: 1, 2: 1, 1: 1}`  represents n³ + n² + n


**Common algorithmic sources:**

- O(n²): Bubble sort, insertion sort, naive matrix operations
- O(n³): Standard matrix multiplication, Floyd-Warshall
- O(n⁴): Naive bipartite matching


**Note:** For non-integer exponents (e.g., O(n^2.807) for Strassen), use [[|T:ComplexityAnalysis.Core.Complexity.PowerComplexity]] or [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] instead. 



|Name | Description |
|-----|------|
|Coefficients: |Dictionary mapping degree → coefficient.|
|Var: |The variable over which the polynomial is defined.|


---
#### Method Complexity.PolynomialComplexity.#ctor(System.Collections.Immutable.ImmutableDictionary{System.Int32,System.Double},ComplexityAnalysis.Core.Complexity.Variable)

 Represents polynomial complexity: O(n²), O(n³), or general polynomial forms. 



> Polynomials represent algorithms with nested loops or recursive patterns that process proportional fractions of input at each level. 

**Structure:** The [[|P:ComplexityAnalysis.Core.Complexity.PolynomialComplexity.Coefficients]] dictionary maps degree → coefficient. For example: 

-  `{2: 1}`  represents n²
-  `{2: 3, 1: 2}`  represents 3n² + 2n
-  `{3: 1, 2: 1, 1: 1}`  represents n³ + n² + n


**Common algorithmic sources:**

- O(n²): Bubble sort, insertion sort, naive matrix operations
- O(n³): Standard matrix multiplication, Floyd-Warshall
- O(n⁴): Naive bipartite matching


**Note:** For non-integer exponents (e.g., O(n^2.807) for Strassen), use [[|T:ComplexityAnalysis.Core.Complexity.PowerComplexity]] or [[|T:ComplexityAnalysis.Core.Complexity.PolyLogComplexity]] instead. 



|Name | Description |
|-----|------|
|Coefficients: |Dictionary mapping degree → coefficient.|
|Var: |The variable over which the polynomial is defined.|


---
#### Property Complexity.PolynomialComplexity.Coefficients

Dictionary mapping degree → coefficient.



---
#### Property Complexity.PolynomialComplexity.Var

The variable over which the polynomial is defined.



---
#### Property Complexity.PolynomialComplexity.Degree

 The highest degree in the polynomial (dominant term). 



---
#### Property Complexity.PolynomialComplexity.LeadingCoefficient

 The coefficient of the highest degree term. 



---
#### Method Complexity.PolynomialComplexity.OfDegree(System.Int32,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple polynomial of the form O(n^k). 



---
#### Method Complexity.PolynomialComplexity.OfDegree(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a polynomial approximation for non-integer degrees. Note: This rounds to the nearest integer since PolynomialComplexity only supports integer exponents. For exact non-integer exponents, use PowerComplexity instead. 



---
## Type Complexity.LogarithmicComplexity

 Represents logarithmic complexity: O(log n), O(k·log n), with configurable base. 



> Logarithmic complexity typically arises from algorithms that halve (or divide by a constant) the problem size at each step. 

**Common algorithmic sources:**

- Binary search: O(log n)
- Balanced BST operations: O(log n)
- Exponentiation by squaring: O(log n)


**Base equivalence:** In asymptotic analysis, log₂(n) = Θ(logₖ(n)) for any constant k > 1, since logₖ(n) = log₂(n) / log₂(k). The base is preserved for precision in constant factor analysis. 



|Name | Description |
|-----|------|
|Coefficient: |Multiplicative coefficient (default 1).|
|Var: |The variable inside the logarithm.|
|Base: |Logarithm base (default 2 for binary algorithms).|


---
#### Method Complexity.LogarithmicComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents logarithmic complexity: O(log n), O(k·log n), with configurable base. 



> Logarithmic complexity typically arises from algorithms that halve (or divide by a constant) the problem size at each step. 

**Common algorithmic sources:**

- Binary search: O(log n)
- Balanced BST operations: O(log n)
- Exponentiation by squaring: O(log n)


**Base equivalence:** In asymptotic analysis, log₂(n) = Θ(logₖ(n)) for any constant k > 1, since logₖ(n) = log₂(n) / log₂(k). The base is preserved for precision in constant factor analysis. 



|Name | Description |
|-----|------|
|Coefficient: |Multiplicative coefficient (default 1).|
|Var: |The variable inside the logarithm.|
|Base: |Logarithm base (default 2 for binary algorithms).|


---
#### Property Complexity.LogarithmicComplexity.Coefficient

Multiplicative coefficient (default 1).



---
#### Property Complexity.LogarithmicComplexity.Var

The variable inside the logarithm.



---
#### Property Complexity.LogarithmicComplexity.Base

Logarithm base (default 2 for binary algorithms).



---
## Type Complexity.ExponentialComplexity

 Represents exponential complexity: O(k^n), O(2^n), etc. 



> Exponential complexity indicates algorithms with explosive growth, typically arising from exhaustive enumeration or branching recursive patterns without memoization. 

**Common algorithmic sources:**

- Brute-force subset enumeration: O(2ⁿ)
- Naive recursive Fibonacci: O(φⁿ) ≈ O(1.618ⁿ)
- Traveling salesman (brute force): O(n! × n) ≈ O(nⁿ)
- 3-SAT exhaustive search: O(3ⁿ)


**Growth comparison:** 2¹⁰ = 1,024 but 2²⁰ ≈ 1 million and 2³⁰ ≈ 1 billion. Exponential algorithms become infeasible very quickly. 



|Name | Description |
|-----|------|
|Base: |The exponential base (e.g., 2 for O(2ⁿ)).|
|Var: |The variable in the exponent.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Method Complexity.ExponentialComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents exponential complexity: O(k^n), O(2^n), etc. 



> Exponential complexity indicates algorithms with explosive growth, typically arising from exhaustive enumeration or branching recursive patterns without memoization. 

**Common algorithmic sources:**

- Brute-force subset enumeration: O(2ⁿ)
- Naive recursive Fibonacci: O(φⁿ) ≈ O(1.618ⁿ)
- Traveling salesman (brute force): O(n! × n) ≈ O(nⁿ)
- 3-SAT exhaustive search: O(3ⁿ)


**Growth comparison:** 2¹⁰ = 1,024 but 2²⁰ ≈ 1 million and 2³⁰ ≈ 1 billion. Exponential algorithms become infeasible very quickly. 



|Name | Description |
|-----|------|
|Base: |The exponential base (e.g., 2 for O(2ⁿ)).|
|Var: |The variable in the exponent.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Property Complexity.ExponentialComplexity.Base

The exponential base (e.g., 2 for O(2ⁿ)).



---
#### Property Complexity.ExponentialComplexity.Var

The variable in the exponent.



---
#### Property Complexity.ExponentialComplexity.Coefficient

Optional multiplicative coefficient.



---
## Type Complexity.FactorialComplexity

 Represents factorial complexity: O(n!). 



> Factorial complexity represents the most extreme form of combinatorial explosion, growing faster than exponential. By Stirling's approximation: n! ≈ √(2πn) × (n/e)ⁿ 

**Common algorithmic sources:**

- Generating all permutations: O(n!)
- Traveling salesman brute force: O(n!)
- Determinant by definition: O(n!)


**Growth illustration:** 10! = 3,628,800 while 20! ≈ 2.4 × 10¹⁸. Factorial algorithms are typically only feasible for n ≤ 12. 



|Name | Description |
|-----|------|
|Var: |The variable in the factorial.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Method Complexity.FactorialComplexity.#ctor(ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Represents factorial complexity: O(n!). 



> Factorial complexity represents the most extreme form of combinatorial explosion, growing faster than exponential. By Stirling's approximation: n! ≈ √(2πn) × (n/e)ⁿ 

**Common algorithmic sources:**

- Generating all permutations: O(n!)
- Traveling salesman brute force: O(n!)
- Determinant by definition: O(n!)


**Growth illustration:** 10! = 3,628,800 while 20! ≈ 2.4 × 10¹⁸. Factorial algorithms are typically only feasible for n ≤ 12. 



|Name | Description |
|-----|------|
|Var: |The variable in the factorial.|
|Coefficient: |Optional multiplicative coefficient.|


---
#### Property Complexity.FactorialComplexity.Var

The variable in the factorial.



---
#### Property Complexity.FactorialComplexity.Coefficient

Optional multiplicative coefficient.



---
## Type Complexity.BinaryOperationComplexity

 Binary operation on complexity expressions for compositional analysis. 



> Binary operations form the backbone of complexity composition, mapping code structure to complexity algebra: 

**Operation**: Code Pattern
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Plus]] (T₁ + T₂)**: Sequential code blocks:  `loop1(); loop2();` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Multiply]] (T₁ × T₂)**: Nested loops:  `for(...) { for(...) { } }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Max]] (max(T₁, T₂))**: Branching:  `if(cond) { slow } else { fast }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Min]] (min(T₁, T₂))**: Best-case/early exit analysis


**Simplification Rules:**



######  code

```
    O(n) + O(n²) = O(n²)           // Max dominates in addition
    O(n) × O(m) = O(n·m)           // Multiplication combines
    max(O(n), O(n²)) = O(n²)       // Max selects dominant
    O(1) × O(f(n)) = O(f(n))       // Identity for multiplication
```



|Name | Description |
|-----|------|
|Left: |Left operand expression.|
|Operation: |The binary operation to perform.|
|Right: |Right operand expression.|
**See also**: [`BinaryOp`](BinaryOp)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Method Complexity.BinaryOperationComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.BinaryOp,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Binary operation on complexity expressions for compositional analysis. 



> Binary operations form the backbone of complexity composition, mapping code structure to complexity algebra: 

**Operation**: Code Pattern
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Plus]] (T₁ + T₂)**: Sequential code blocks:  `loop1(); loop2();` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Multiply]] (T₁ × T₂)**: Nested loops:  `for(...) { for(...) { } }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Max]] (max(T₁, T₂))**: Branching:  `if(cond) { slow } else { fast }` 
- **[[|F:ComplexityAnalysis.Core.Complexity.BinaryOp.Min]] (min(T₁, T₂))**: Best-case/early exit analysis


**Simplification Rules:**



######  code

```
    O(n) + O(n²) = O(n²)           // Max dominates in addition
    O(n) × O(m) = O(n·m)           // Multiplication combines
    max(O(n), O(n²)) = O(n²)       // Max selects dominant
    O(1) × O(f(n)) = O(f(n))       // Identity for multiplication
```



|Name | Description |
|-----|------|
|Left: |Left operand expression.|
|Operation: |The binary operation to perform.|
|Right: |Right operand expression.|
**See also**: [`BinaryOp`](BinaryOp)

**See also**: [`ComplexityComposition`](ComplexityComposition)



---
#### Property Complexity.BinaryOperationComplexity.Left

Left operand expression.



---
#### Property Complexity.BinaryOperationComplexity.Operation

The binary operation to perform.



---
#### Property Complexity.BinaryOperationComplexity.Right

Right operand expression.



---
## Type Complexity.BinaryOp

 Binary operations for composing complexity expressions. 



> These operations model how code structure translates to complexity composition: -  `Plus` : Sequential execution (loop₁; loop₂)
-  `Multiply` : Nested execution (for { for { } })
-  `Max` : Worst-case branching (if-else)
-  `Min` : Best-case / early exit






---
#### Field Complexity.BinaryOp.Plus

 Addition: T₁ + T₂ (sequential composition). 



---
#### Field Complexity.BinaryOp.Multiply

 Multiplication: T₁ × T₂ (nested composition). 



---
#### Field Complexity.BinaryOp.Max

 Maximum: max(T₁, T₂) (branching/worst case). 



---
#### Field Complexity.BinaryOp.Min

 Minimum: min(T₁, T₂) (best case/early exit). 



---
## Type Complexity.ConditionalComplexity

 Conditional complexity: represents different complexities based on runtime conditions. 



> Models code branches where different paths have different complexities: 



######  code

```
    if (isSorted) {
        BinarySearch();     // O(log n)
    } else {
        LinearSearch();     // O(n)
    }
    // → ConditionalComplexity("isSorted", O(log n), O(n))
```

**Evaluation Strategy:** For worst-case analysis, [[|M:ComplexityAnalysis.Core.Complexity.ConditionalComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})]] conservatively returns max(TrueBranch, FalseBranch). For best-case or average-case analysis, see the speculative analysis infrastructure. 



|Name | Description |
|-----|------|
|ConditionDescription: |Human-readable description of the condition.|
|TrueBranch: |Complexity when condition is true.|
|FalseBranch: |Complexity when condition is false.|


---
#### Method Complexity.ConditionalComplexity.#ctor(System.String,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Conditional complexity: represents different complexities based on runtime conditions. 



> Models code branches where different paths have different complexities: 



######  code

```
    if (isSorted) {
        BinarySearch();     // O(log n)
    } else {
        LinearSearch();     // O(n)
    }
    // → ConditionalComplexity("isSorted", O(log n), O(n))
```

**Evaluation Strategy:** For worst-case analysis, [[|M:ComplexityAnalysis.Core.Complexity.ConditionalComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})]] conservatively returns max(TrueBranch, FalseBranch). For best-case or average-case analysis, see the speculative analysis infrastructure. 



|Name | Description |
|-----|------|
|ConditionDescription: |Human-readable description of the condition.|
|TrueBranch: |Complexity when condition is true.|
|FalseBranch: |Complexity when condition is false.|


---
#### Property Complexity.ConditionalComplexity.ConditionDescription

Human-readable description of the condition.



---
#### Property Complexity.ConditionalComplexity.TrueBranch

Complexity when condition is true.



---
#### Property Complexity.ConditionalComplexity.FalseBranch

Complexity when condition is false.



---
## Type Complexity.PowerComplexity

 Power of a complexity expression: expr^k. 



---
#### Method Complexity.PowerComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Power of a complexity expression: expr^k. 



---
## Type Complexity.LogOfComplexity

 Logarithm of a complexity expression: log(expr). 



---
#### Method Complexity.LogOfComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Logarithm of a complexity expression: log(expr). 



---
## Type Complexity.ExponentialOfComplexity

 Exponential of a complexity expression: base^expr. 



---
#### Method Complexity.ExponentialOfComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Exponential of a complexity expression: base^expr. 



---
## Type Complexity.FactorialOfComplexity

 Factorial of a complexity expression: expr!. 



---
#### Method Complexity.FactorialOfComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Factorial of a complexity expression: expr!. 



---
## Type Complexity.SourceType

 The type of source for a complexity claim. Ordered from most to least authoritative. 



---
#### Field Complexity.SourceType.Documented

 Documented in official Microsoft docs with explicit complexity. Highest confidence. 



---
#### Field Complexity.SourceType.Attested

 Attested in academic papers, CLRS, or other authoritative sources. High confidence. 



---
#### Field Complexity.SourceType.Empirical

 Measured via benchmarking with verification. Good confidence, but environment-dependent. 



---
#### Field Complexity.SourceType.Inferred

 Inferred from source code analysis. Medium confidence. 



---
#### Field Complexity.SourceType.Heuristic

 Conservative estimate when exact complexity is unknown. Prefer overestimate to underestimate. 



---
#### Field Complexity.SourceType.Unknown

 Unknown source or unverified claim. Lowest confidence. 



---
## Type Complexity.ComplexitySource

 Records the source and confidence level for a complexity claim. Essential for audit trails and conservative estimation. 



---
#### Property Complexity.ComplexitySource.Type

 The type of source for this complexity claim. 



---
#### Property Complexity.ComplexitySource.Citation

 Citation or reference for the source. Examples: - URL to Microsoft docs - "CLRS 4th ed., Chapter 7" - "Measured via BenchmarkDotNet" - "Conservative estimate: worst-case resize" 



---
#### Property Complexity.ComplexitySource.Confidence

 Confidence level in the claim (0.0 to 1.0). - 1.0: Certain (documented, verified) - 0.8-0.9: High confidence (attested, empirical) - 0.5-0.7: Medium confidence (inferred, heuristic) - <0.5: Low confidence (uncertain) 



---
#### Property Complexity.ComplexitySource.IsUpperBound

 Whether this is an upper bound (conservative overestimate). When true, actual complexity may be lower. 



---
#### Property Complexity.ComplexitySource.IsAmortized

 Whether this is an amortized complexity. Individual operations may exceed this bound. 



---
#### Property Complexity.ComplexitySource.IsWorstCase

 Whether this complexity is for the worst case. 



---
#### Property Complexity.ComplexitySource.Notes

 Optional notes about edge cases or assumptions. 



---
#### Property Complexity.ComplexitySource.LastVerified

 Date the source was last verified (if applicable). 



---
#### Method Complexity.ComplexitySource.FromMicrosoftDocs(System.String,System.String)

 Creates a documented source from Microsoft docs. 



---
#### Method Complexity.ComplexitySource.FromAcademic(System.String,System.Double)

 Creates an attested source from academic literature. 



---
#### Method Complexity.ComplexitySource.FromBenchmark(System.String,System.Double)

 Creates an empirical source from benchmarking. 



---
#### Method Complexity.ComplexitySource.Inferred(System.String,System.Double)

 Creates an inferred source from code analysis. 



---
#### Method Complexity.ComplexitySource.ConservativeHeuristic(System.String,System.Double)

 Creates a conservative heuristic estimate. Always marks as upper bound. 



---
#### Method Complexity.ComplexitySource.Unknown

 Creates an unknown source (used when no information is available). 



---
#### Method Complexity.ComplexitySource.Documented(System.String)

 Creates a documented source with citation. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Attested(System.String)

 Creates an attested source with citation. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Empirical(System.String)

 Creates an empirical source with description. Shorthand for BCL mapping declarations. 



---
#### Method Complexity.ComplexitySource.Heuristic(System.String)

 Creates a heuristic source with reasoning. Shorthand for BCL mapping declarations. 



---
## Type Complexity.AttributedComplexity

 A complexity expression paired with its source attribution. 



---
#### Property Complexity.AttributedComplexity.Expression

 The complexity expression. 



---
#### Property Complexity.AttributedComplexity.Source

 The source of the complexity claim. 



---
#### Property Complexity.AttributedComplexity.RequiresReview

 Whether this result requires human review. 



---
#### Property Complexity.AttributedComplexity.ReviewReason

 Reason for requiring review (if applicable). 



---
#### Method Complexity.AttributedComplexity.Documented(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String,System.String)

 Creates an attributed complexity from documented source. 



---
#### Method Complexity.AttributedComplexity.Attested(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates an attributed complexity from academic source. 



---
#### Method Complexity.AttributedComplexity.Heuristic(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates a conservative heuristic complexity. 



---
## Type Complexity.ComplexityResult

 Complete result of complexity analysis for a method or code block. 



---
#### Property Complexity.ComplexityResult.Expression

 The computed complexity expression. 



---
#### Property Complexity.ComplexityResult.Source

 Source attribution for the complexity claim. 



---
#### Property Complexity.ComplexityResult.RequiresReview

 Whether this result requires human review. 



---
#### Property Complexity.ComplexityResult.ReviewReason

 Reason for requiring review (if applicable). 



---
#### Property Complexity.ComplexityResult.Location

 Location in source code where this complexity was computed. 



---
#### Property Complexity.ComplexityResult.SubResults

 Sub-results that contributed to this complexity. Useful for explaining how the total was derived. 



---
#### Method Complexity.ComplexityResult.Create(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexitySource,ComplexityAnalysis.Core.Complexity.SourceLocation)

 Creates a result with automatic review flagging based on source. 



---
## Type Complexity.SourceLocation

 Location in source code. 



---
#### Property Complexity.SourceLocation.FilePath

 File path. 



---
#### Property Complexity.SourceLocation.StartLine

 Starting line number (1-based). 



---
#### Property Complexity.SourceLocation.StartColumn

 Starting column (0-based). 



---
#### Property Complexity.SourceLocation.EndLine

 Ending line number (1-based). 



---
#### Property Complexity.SourceLocation.EndColumn

 Ending column (0-based). 



---
## Type Complexity.ExpressionForm

 Classifies the dominant asymptotic form of complexity expressions. Essential for determining theorem applicability. 



---
#### Field Complexity.ExpressionForm.Constant

O(1) - constant complexity.



---
#### Field Complexity.ExpressionForm.Logarithmic

O(log^k n) - pure logarithmic (no polynomial factor).



---
#### Field Complexity.ExpressionForm.Polynomial

O(n^k) - pure polynomial.



---
#### Field Complexity.ExpressionForm.PolyLog

O(n^k · log^j n) - polylogarithmic.



---
#### Field Complexity.ExpressionForm.Exponential

O(k^n) - exponential.



---
#### Field Complexity.ExpressionForm.Factorial

O(n!) - factorial.



---
#### Field Complexity.ExpressionForm.Unknown

Cannot be classified into standard forms.



---
## Type Complexity.ExpressionClassification

 Result of classifying an expression's asymptotic form. 



---
#### Property Complexity.ExpressionClassification.Form

The dominant asymptotic form.



---
#### Property Complexity.ExpressionClassification.Variable

The variable the classification is with respect to.



---
#### Property Complexity.ExpressionClassification.PrimaryParameter

 For Polynomial/PolyLog: the polynomial degree k in n^k. For Logarithmic: 0. For Exponential: the base. 



---
#### Property Complexity.ExpressionClassification.LogExponent

 For PolyLog/Logarithmic: the log exponent j in log^j n. 



---
#### Property Complexity.ExpressionClassification.Coefficient

 Leading coefficient (preserved for non-asymptotic analysis). 



---
#### Property Complexity.ExpressionClassification.Confidence

 Confidence level in the classification (0.0 to 1.0). Lower for complex composed expressions. 



---
#### Method Complexity.ExpressionClassification.ToPolyLog

 Converts to a normalized PolyLogComplexity if applicable. 



---
#### Method Complexity.ExpressionClassification.CompareDegreeTo(System.Double,System.Double)

 Compares the polynomial degree to a target value. Returns: <0 if degree < target, 0 if equal (within epsilon), >0 if degree > target. 



---
## Type Complexity.IExpressionClassifier

 Interface for classifying complexity expressions into standard forms. Used to determine theorem applicability. 



---
#### Method Complexity.IExpressionClassifier.Classify(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Classifies the dominant asymptotic form of an expression. 



---
#### Method Complexity.IExpressionClassifier.TryExtractPolynomialDegree(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double@)

 Attempts to extract polynomial degree if expression is O(n^k). 



---
#### Method Complexity.IExpressionClassifier.TryExtractPolyLogForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double@,System.Double@)

 Attempts to extract polylog form parameters if expression is O(n^k · log^j n). 



---
#### Method Complexity.IExpressionClassifier.IsBoundedByPolynomial(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Determines if expression is bounded by O(n^d) for given d. 



---
#### Method Complexity.IExpressionClassifier.DominatesPolynomial(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,System.Double)

 Determines if expression dominates Ω(n^d) for given d. 



---
## Type Complexity.StandardExpressionClassifier

 Standard implementation of expression classification. Uses pattern matching and visitor traversal. 



---
## Type Complexity.IComplexityTransformer

 Interface for transforming and simplifying complexity expressions. 



---
#### Method Complexity.IComplexityTransformer.Simplify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Simplify an expression by applying algebraic rules. 



---
#### Method Complexity.IComplexityTransformer.NormalizeForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Normalize to a canonical form for comparison. 



---
#### Method Complexity.IComplexityTransformer.DropConstantFactors(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Drop constant factors for Big-O equivalence. O(3n²) → O(n²) 



---
#### Method Complexity.IComplexityTransformer.DropLowerOrderTerms(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Drop lower-order terms for asymptotic equivalence. O(n² + n + 1) → O(n²) 



---
## Type Complexity.IComplexityComparator

 Compares complexity expressions for asymptotic ordering. 



---
#### Method Complexity.IComplexityComparator.Compare(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Compare two expressions asymptotically. Returns: -1 if left < right, 0 if equal, 1 if left > right. 



---
#### Method Complexity.IComplexityComparator.IsDominated(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Determines if left is dominated by right (left ∈ O(right)). 



---
#### Method Complexity.IComplexityComparator.AreEquivalent(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Determines if two expressions are asymptotically equivalent. 



---
## Type Complexity.ComplexitySimplifier

 Standard implementation of complexity simplification. 



---
#### Method Complexity.ComplexitySimplifier.Simplify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.NormalizeForm(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.DropConstantFactors(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.ComplexitySimplifier.DropLowerOrderTerms(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
## Type Complexity.AsymptoticComparator

 Compares complexity expressions by asymptotic growth rate. 



---
#### Method Complexity.AsymptoticComparator.Compare(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Asymptotic ordering (from slowest to fastest growth): O(1) < O(log n) < O(n) < O(n log n) < O(n²) < O(n³) < O(2ⁿ) < O(n!) 



---
#### Method Complexity.AsymptoticComparator.IsDominated(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.AsymptoticComparator.AreEquivalent(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Method Complexity.AsymptoticComparator.GetAsymptoticOrder(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Gets a numeric order for asymptotic comparison. Higher values = faster growth. 



---
## Type Complexity.ComplexityExpressionExtensions

 Extension methods for complexity expressions. 



---
#### Method Complexity.ComplexityExpressionExtensions.Simplified(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Simplifies the expression using the default simplifier. 



---
#### Method Complexity.ComplexityExpressionExtensions.Normalized(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Normalizes to canonical Big-O form. 



---
#### Method Complexity.ComplexityExpressionExtensions.CompareAsymptotically(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Compares asymptotically to another expression. 



---
#### Method Complexity.ComplexityExpressionExtensions.IsDominatedBy(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Checks if this expression is dominated by another. 



---
#### Method Complexity.ComplexityExpressionExtensions.Dominates(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Checks if this expression dominates another. 



---
## Type Complexity.IComplexityVisitor`1

 Visitor pattern interface for traversing complexity expression trees. Enables operations like simplification, evaluation, and transformation. 



---
#### Method Complexity.IComplexityVisitor`1.VisitUnknown(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Fallback for unknown/unrecognized expression types (e.g., special functions). 



---
## Type Complexity.ComplexityVisitorBase`1

 Base implementation of IComplexityVisitor that returns default values. Override specific methods to handle particular expression types. 



---
## Type Complexity.ComplexityTransformVisitor

 Visitor that recursively transforms complexity expressions. Override methods to modify specific node types during traversal. 



---
## Type Complexity.ParallelComplexity

 Represents complexity of parallel/concurrent algorithms. Parallel complexity considers: - Work: Total operations across all processors (sequential equivalent) - Span/Depth: Longest chain of dependent operations (critical path) - Parallelism: Work / Span ratio (how parallelizable the algorithm is) Examples: - Parallel.For over n items: Work O(n), Span O(1) if independent - Parallel merge sort: Work O(n log n), Span O(log² n) - Parallel prefix sum: Work O(n), Span O(log n) 



---
#### Property Complexity.ParallelComplexity.Work

 Total work across all processors (sequential time complexity). 



---
#### Property Complexity.ParallelComplexity.Span

 Span/depth - the longest chain of dependent operations. Also known as critical path length. 



---
#### Property Complexity.ParallelComplexity.ProcessorCount

 Number of processors/cores assumed. Use Variable.P for parameterized, or a constant for fixed. 



---
#### Property Complexity.ParallelComplexity.PatternType

 The type of parallel pattern detected. 



---
#### Property Complexity.ParallelComplexity.IsTaskBased

 Whether the parallelism is task-based (async/await, Task.Run). 



---
#### Property Complexity.ParallelComplexity.HasSynchronizationOverhead

 Whether the parallel operations have synchronization overhead. 



---
#### Property Complexity.ParallelComplexity.Description

 Description of the parallel pattern. 



---
#### Property Complexity.ParallelComplexity.Parallelism

 Gets the parallelism (Work / Span ratio). Higher values indicate better parallelizability. 



---
#### Property Complexity.ParallelComplexity.ParallelTime

 Gets the parallel time (with p processors): max(Work/p, Span). By Brent's theorem: T_p ≤ (Work - Span)/p + Span 



---
#### Method Complexity.ParallelComplexity.EmbarrassinglyParallel(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a parallel complexity for embarrassingly parallel work. Work = O(n), Span = O(1). 



---
#### Method Complexity.ParallelComplexity.Reduction(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for reduction/aggregation patterns. Work = O(n), Span = O(log n). 



---
#### Method Complexity.ParallelComplexity.DivideAndConquer(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for divide-and-conquer patterns. Work = O(n log n), Span = O(log² n). 



---
#### Method Complexity.ParallelComplexity.PrefixScan(ComplexityAnalysis.Core.Complexity.Variable)

 Creates parallel complexity for prefix/scan operations. Work = O(n), Span = O(log n). 



---
#### Method Complexity.ParallelComplexity.Pipeline(ComplexityAnalysis.Core.Complexity.Variable,System.Int32)

 Creates parallel complexity for pipeline patterns. Work = O(n × stages), Span = O(n + stages). 



---
#### Method Complexity.ParallelComplexity.TaskBased(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates complexity for async/await task-based concurrency. 



---
## Type Complexity.ParallelPatternType

 Types of parallel patterns. 



---
#### Field Complexity.ParallelPatternType.Generic

 Generic parallel pattern. 



---
#### Field Complexity.ParallelPatternType.ParallelFor

 Parallel.For / Parallel.ForEach - data parallelism. 



---
#### Field Complexity.ParallelPatternType.PLINQ

 PLINQ - parallel LINQ. 



---
#### Field Complexity.ParallelPatternType.TaskBased

 Task.Run / Task.WhenAll - task parallelism. 



---
#### Field Complexity.ParallelPatternType.AsyncAwait

 async/await patterns. 



---
#### Field Complexity.ParallelPatternType.Reduction

 Parallel reduction/aggregation. 



---
#### Field Complexity.ParallelPatternType.Scan

 Parallel prefix scan. 



---
#### Field Complexity.ParallelPatternType.DivideAndConquer

 Divide-and-conquer parallelism. 



---
#### Field Complexity.ParallelPatternType.Pipeline

 Pipeline parallelism. 



---
#### Field Complexity.ParallelPatternType.ForkJoin

 Fork-join pattern. 



---
#### Field Complexity.ParallelPatternType.ProducerConsumer

 Producer-consumer pattern. 



---
## Type Complexity.ParallelVariables

 Variable for processor count. 



---
#### Property Complexity.ParallelVariables.P

 Number of processors (p). 



---
#### Method Complexity.ParallelVariables.Processors(System.Int32)

 Creates a processor count variable with a specific value. 



---
#### Property Complexity.ParallelVariables.InfiniteProcessors

 Infinite processors (theoretical analysis). 



---
## Type Complexity.IParallelComplexityVisitor`1

 Extended visitor interface for parallel complexity. 



---
## Type Complexity.ParallelAnalysisResult

 Analysis result for parallel patterns. 



---
#### Property Complexity.ParallelAnalysisResult.Complexity

 The detected parallel complexity. 



---
#### Property Complexity.ParallelAnalysisResult.Speedup

 Speedup factor: T_1 / T_p (sequential time / parallel time). 



---
#### Property Complexity.ParallelAnalysisResult.Efficiency

 Efficiency: Speedup / p (how well processors are utilized). 



---
#### Property Complexity.ParallelAnalysisResult.IsScalable

 Whether the pattern has good scalability. 



---
#### Property Complexity.ParallelAnalysisResult.Warnings

 Potential issues or warnings. 



---
#### Property Complexity.ParallelAnalysisResult.Recommendations

 Recommendations for improving parallelism. 



---
## Type Complexity.ParallelAlgorithms

 Common parallel algorithm complexities. 



---
#### Method Complexity.ParallelAlgorithms.ParallelSum

 Parallel sum/reduction: Work O(n), Span O(log n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelMergeSort

 Parallel merge sort: Work O(n log n), Span O(log² n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelMatrixMultiply

 Parallel matrix multiply (naive): Work O(n³), Span O(log n). 



---
#### Method Complexity.ParallelAlgorithms.ParallelQuickSort

 Parallel quick sort: Work O(n log n), Span O(log² n) expected. 



---
#### Method Complexity.ParallelAlgorithms.ParallelBFS

 Parallel BFS: Work O(V + E), Span O(diameter × log V). 



---
#### Method Complexity.ParallelAlgorithms.PLINQFilter

 PLINQ Where/Select: Work O(n), Span O(n/p + log p). 



---
## Type Complexity.PolyLogComplexity

 Represents polylogarithmic complexity: O(n^k · log^j n). 



>**General Form:** coefficient · n^polyDegree · (log_base n)^logExponent 

 This unified type is essential for representing many common complexity classes: 

**Parameters**: Result
- **k=1, j=1**: O(n log n) - Merge sort, heap sort, optimal comparison sorts
- **k=2, j=0**: O(n²) - Pure polynomial (quadratic)
- **k=0, j=1**: O(log n) - Pure logarithmic (binary search)
- **k=1, j=2**: O(n log² n) - Some advanced algorithms
- **k=0, j=2**: O(log² n) - Iterated binary search


**Master Theorem Connection:**

 Case 2 of the Master Theorem produces polylog solutions. For T(n) = a·T(n/b) + Θ(n^d · log^k n) where d = log_b(a): 



######  code

```
    T(n) = Θ(n^d · log^(k+1) n)
```

 The factory method [[|M:ComplexityAnalysis.Core.Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)]] creates these solutions directly. 

**Algebraic Properties:**



######  code

```
    // Multiplication combines exponents:
    (n^a log^b n) × (n^c log^d n) = n^(a+c) · log^(b+d) n
    
    // Power distributes:
    (n^a log^b n)^k = n^(ak) · log^(bk) n
```



**See also**: [`PolynomialComplexity`](PolynomialComplexity)

**See also**: [`LogarithmicComplexity`](LogarithmicComplexity)



---
#### Method Complexity.PolyLogComplexity.#ctor(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable,System.Double,System.Double)

 Represents polylogarithmic complexity: O(n^k · log^j n). 



>**General Form:** coefficient · n^polyDegree · (log_base n)^logExponent 

 This unified type is essential for representing many common complexity classes: 

**Parameters**: Result
- **k=1, j=1**: O(n log n) - Merge sort, heap sort, optimal comparison sorts
- **k=2, j=0**: O(n²) - Pure polynomial (quadratic)
- **k=0, j=1**: O(log n) - Pure logarithmic (binary search)
- **k=1, j=2**: O(n log² n) - Some advanced algorithms
- **k=0, j=2**: O(log² n) - Iterated binary search


**Master Theorem Connection:**

 Case 2 of the Master Theorem produces polylog solutions. For T(n) = a·T(n/b) + Θ(n^d · log^k n) where d = log_b(a): 



######  code

```
    T(n) = Θ(n^d · log^(k+1) n)
```

 The factory method [[|M:ComplexityAnalysis.Core.Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)]] creates these solutions directly. 

**Algebraic Properties:**



######  code

```
    // Multiplication combines exponents:
    (n^a log^b n) × (n^c log^d n) = n^(a+c) · log^(b+d) n
    
    // Power distributes:
    (n^a log^b n)^k = n^(ak) · log^(bk) n
```



**See also**: [`PolynomialComplexity`](PolynomialComplexity)

**See also**: [`LogarithmicComplexity`](LogarithmicComplexity)



---
#### Property Complexity.PolyLogComplexity.IsPurePolynomial

 True if this is a pure polynomial (no log factor). 



---
#### Property Complexity.PolyLogComplexity.IsPureLogarithmic

 True if this is a pure logarithmic (no polynomial factor). 



---
#### Property Complexity.PolyLogComplexity.IsNLogN

 True if this is the common n log n form. 



---
#### Method Complexity.PolyLogComplexity.NLogN(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n log n) - common for efficient sorting/divide-and-conquer. 



---
#### Method Complexity.PolyLogComplexity.PolyTimesLog(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^k log n) - Master Theorem Case 2 with k=1. 



---
#### Method Complexity.PolyLogComplexity.MasterCase2Solution(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^d · log^(k+1) n) - General Master Theorem Case 2 solution. 



---
#### Method Complexity.PolyLogComplexity.LogPower(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(log^k n) - pure iterated logarithm. 



---
#### Method Complexity.PolyLogComplexity.Polynomial(System.Double,ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n^k) - pure polynomial (for consistency). 



---
#### Method Complexity.PolyLogComplexity.Multiply(ComplexityAnalysis.Core.Complexity.PolyLogComplexity)

 Multiplies two PolyLog expressions: (n^a log^b n) × (n^c log^d n) = n^(a+c) log^(b+d) n 



---
#### Method Complexity.PolyLogComplexity.Power(System.Double)

 Raises to a power: (n^a log^b n)^k = n^(ak) log^(bk) n 



---
## Type Complexity.RandomnessSource

 Specifies the source of randomness in a probabilistic algorithm. 



---
#### Field Complexity.RandomnessSource.InputDistribution

 Randomness comes from the input distribution (average-case analysis). Example: QuickSort with random input permutation. 



---
#### Field Complexity.RandomnessSource.AlgorithmRandomness

 Randomness comes from the algorithm itself (Las Vegas algorithms). Example: Randomized QuickSort with random pivot selection. 



---
#### Field Complexity.RandomnessSource.MonteCarlo

 Monte Carlo algorithms that may produce incorrect results with small probability. Example: Miller-Rabin primality test. 



---
#### Field Complexity.RandomnessSource.HashFunction

 Hash function randomness (universal hashing, expected behavior). Example: Hash table operations assuming uniform hashing. 



---
#### Field Complexity.RandomnessSource.Mixed

 Multiple sources of randomness combined. 



---
## Type Complexity.ProbabilityDistribution

 Specifies the probability distribution of the complexity. 



---
#### Field Complexity.ProbabilityDistribution.Uniform

 Uniform distribution over all inputs. 



---
#### Field Complexity.ProbabilityDistribution.Exponential

 Exponential distribution (common in queueing theory). 



---
#### Field Complexity.ProbabilityDistribution.Geometric

 Geometric distribution (common in randomized algorithms). 



---
#### Field Complexity.ProbabilityDistribution.HighProbabilityBound

 Bounded/concentrated distribution with high probability guarantees. 



---
#### Field Complexity.ProbabilityDistribution.InputDependent

 Distribution determined by specific input characteristics. 



---
#### Field Complexity.ProbabilityDistribution.Unknown

 Unknown or unspecified distribution. 



---
## Type Complexity.ProbabilisticComplexity

 Represents probabilistic complexity analysis for randomized algorithms. Captures expected (average), best-case, and worst-case complexities along with probability distribution information. 



> This is used for analyzing: - Average-case complexity (QuickSort, hash tables) - Randomized algorithms (randomized QuickSort, randomized selection) - Monte Carlo algorithms (primality testing) - Las Vegas algorithms (randomized algorithms that always produce correct results) 



---
#### Property Complexity.ProbabilisticComplexity.ExpectedComplexity

 Gets the expected (average-case) complexity. This represents E[T(n)] - the expected running time. 



---
#### Property Complexity.ProbabilisticComplexity.WorstCaseComplexity

 Gets the worst-case complexity. This is the upper bound that holds for all inputs/random choices. 



---
#### Property Complexity.ProbabilisticComplexity.BestCaseComplexity

 Gets the best-case complexity. Optional - when not specified, defaults to constant. 



---
#### Property Complexity.ProbabilisticComplexity.Source

 Gets the source of randomness in the algorithm. 



---
#### Property Complexity.ProbabilisticComplexity.Distribution

 Gets the probability distribution of the complexity. 



---
#### Property Complexity.ProbabilisticComplexity.Variance

 Gets the variance of the complexity if known. Null indicates unknown variance. 



---
#### Property Complexity.ProbabilisticComplexity.HighProbability

 Gets the high-probability bound if applicable. For algorithms with concentration bounds: Pr[T(n) > bound] ≤ probability. 



---
#### Property Complexity.ProbabilisticComplexity.Assumptions

 Gets any assumptions required for the expected complexity to hold. Example: "uniform random input permutation", "independent hash function" 



---
#### Property Complexity.ProbabilisticComplexity.Description

 Gets an optional description of the probabilistic analysis. 



---
#### Method Complexity.ProbabilisticComplexity.Accept``1(ComplexityAnalysis.Core.Complexity.IComplexityVisitor{``0})

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.Substitute(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

*Inherits documentation from base.*



---
#### Property Complexity.ProbabilisticComplexity.FreeVariables

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.Evaluate(System.Collections.Generic.IReadOnlyDictionary{ComplexityAnalysis.Core.Complexity.Variable,System.Double})

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.ToBigONotation

*Inherits documentation from base.*



---
#### Method Complexity.ProbabilisticComplexity.QuickSortLike(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.RandomnessSource)

 Creates a probabilistic complexity with expected O(n log n) and worst O(n²). Common for randomized sorting algorithms like QuickSort. 



---
#### Method Complexity.ProbabilisticComplexity.HashTableLookup(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for hash table operations. Expected O(1), worst O(n). 



---
#### Method Complexity.ProbabilisticComplexity.RandomizedSelection(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for randomized selection (Quickselect). Expected O(n), worst O(n²). 



---
#### Method Complexity.ProbabilisticComplexity.SkipListOperation(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a probabilistic complexity for skip list operations. Expected O(log n), worst O(n). 



---
#### Method Complexity.ProbabilisticComplexity.BloomFilterLookup(System.Int32)

 Creates a probabilistic complexity for Bloom filter operations. O(k) where k is the number of hash functions, with false positive probability. 



---
#### Method Complexity.ProbabilisticComplexity.MonteCarlo(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double,System.String)

 Creates a Monte Carlo complexity where the result may be incorrect with some probability. 



---
## Type Complexity.HighProbabilityBound

 Represents a high-probability bound: Pr[T(n) ≤ bound] ≥ probability. 



---
#### Property Complexity.HighProbabilityBound.Bound

 Gets the complexity bound that holds with high probability. 



---
#### Property Complexity.HighProbabilityBound.Probability

 Gets the probability that the bound holds. For "with high probability" bounds, this is typically 1 - 1/n^c for some constant c. 



---
#### Property Complexity.HighProbabilityBound.ProbabilityExpression

 Gets an optional expression for the probability as a function of n. Example: 1 - 1/n for bounds that hold "with high probability". 



---
## Type Complexity.IProbabilisticComplexityVisitor`1

 Extension of IComplexityVisitor for probabilistic complexity. 



---
#### Method Complexity.IProbabilisticComplexityVisitor`1.VisitProbabilistic(ComplexityAnalysis.Core.Complexity.ProbabilisticComplexity)

 Visits a probabilistic complexity expression. 



---
## Type Complexity.SpecialFunctionComplexity

 Represents special mathematical functions that arise in complexity analysis, particularly from Akra-Bazzi integral evaluation. These provide symbolic representations when closed-form elementary solutions don't exist, enabling later refinement via numerical methods or CAS integration. 



---
#### Property Complexity.SpecialFunctionComplexity.HasAsymptoticExpansion

 Whether this function has a known asymptotic expansion. 



---
#### Property Complexity.SpecialFunctionComplexity.DominantTerm

 Gets the dominant asymptotic term, if known. 



---
## Type Complexity.PolylogarithmComplexity

 Polylogarithm Li_s(z) = Σₖ₌₁^∞ z^k / k^s Arises when integrating log terms. For |z| ≤ 1: - Li_1(z) = -ln(1-z) - Li_0(z) = z/(1-z) - Li_{-1}(z) = z/(1-z)² For complexity analysis, we often have Li_s(1) = ζ(s) (Riemann zeta). 



---
#### Method Complexity.PolylogarithmComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Polylogarithm Li_s(z) = Σₖ₌₁^∞ z^k / k^s Arises when integrating log terms. For |z| ≤ 1: - Li_1(z) = -ln(1-z) - Li_0(z) = z/(1-z) - Li_{-1}(z) = z/(1-z)² For complexity analysis, we often have Li_s(1) = ζ(s) (Riemann zeta). 



---
## Type Complexity.IncompleteGammaComplexity

 Incomplete Gamma function γ(s, x) = ∫₀ˣ t^(s-1) e^(-t) dt Arises from exponential-polynomial integrals. Asymptotically: - For large x: γ(s, x) → Γ(s) (complete gamma) - For small x: γ(s, x) ≈ x^s / s 



---
#### Method Complexity.IncompleteGammaComplexity.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Incomplete Gamma function γ(s, x) = ∫₀ˣ t^(s-1) e^(-t) dt Arises from exponential-polynomial integrals. Asymptotically: - For large x: γ(s, x) → Γ(s) (complete gamma) - For small x: γ(s, x) ≈ x^s / s 



---
## Type Complexity.IncompleteBetaComplexity

 Incomplete Beta function B(x; a, b) = ∫₀ˣ t^(a-1) (1-t)^(b-1) dt Related to regularized incomplete beta I_x(a,b) = B(x;a,b) / B(a,b). Arises in probability and from polynomial ratio integrals. 



---
#### Method Complexity.IncompleteBetaComplexity.#ctor(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Incomplete Beta function B(x; a, b) = ∫₀ˣ t^(a-1) (1-t)^(b-1) dt Related to regularized incomplete beta I_x(a,b) = B(x;a,b) / B(a,b). Arises in probability and from polynomial ratio integrals. 



---
## Type Complexity.HypergeometricComplexity

 Gauss Hypergeometric function ₂F₁(a, b; c; z) The most general special function needed for Akra-Bazzi integrals. Many special functions are cases of ₂F₁: - log(1+z) = z · ₂F₁(1, 1; 2; -z) - arcsin(z) = z · ₂F₁(1/2, 1/2; 3/2; z²) - (1-z)^(-a) = ₂F₁(a, b; b; z) for any b 



---
#### Method Complexity.HypergeometricComplexity.#ctor(System.Double,System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Gauss Hypergeometric function ₂F₁(a, b; c; z) The most general special function needed for Akra-Bazzi integrals. Many special functions are cases of ₂F₁: - log(1+z) = z · ₂F₁(1, 1; 2; -z) - arcsin(z) = z · ₂F₁(1/2, 1/2; 3/2; z²) - (1-z)^(-a) = ₂F₁(a, b; b; z) for any b 



---
#### Property Complexity.HypergeometricComplexity.SimplifiedForm

 Recognizes if this hypergeometric is actually a simpler function. 



---
## Type Complexity.SymbolicIntegralComplexity

 Represents a symbolic integral that cannot be evaluated in closed form. Preserves the integrand for potential later refinement. 



---
#### Method Complexity.SymbolicIntegralComplexity.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Represents a symbolic integral that cannot be evaluated in closed form. Preserves the integrand for potential later refinement. 



---
#### Method Complexity.SymbolicIntegralComplexity.WithBound(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Creates a symbolic integral with an asymptotic bound estimate. 



---
## Type Complexity.ISpecialFunctionVisitor`1

 Extended visitor interface for special functions. 



---
## Type Complexity.Variable

 Represents a variable in complexity expressions (e.g., n, V, E, degree). 



> Variables are symbolic placeholders for input sizes and algorithm parameters. Unlike mathematical variables, complexity variables carry semantic meaning through their [[|T:ComplexityAnalysis.Core.Complexity.VariableType]] to enable domain-specific analysis. 

**Variable Semantics by Domain:**

**Domain**: Common Variables
- **General**:  `n`  (input size),  `k`  (parameter count)
- **Graphs**:  `V`  (vertices),  `E`  (edges), with relationship E ≤ V²
- **Trees**:  `n`  (nodes),  `h`  (height), with h ∈ [log n, n]
- **Strings**:  `n`  (text length),  `m`  (pattern length)
- **Parallel**:  `n`  (work),  `p`  (processors)


**Multi-Variable Complexity:** Many algorithms have complexity dependent on multiple variables. The system supports this through expression composition: 



######  code

```
    // Graph algorithm: O(V + E)
    var graphComplexity = new BinaryOperationComplexity(
        new VariableComplexity(Variable.V),
        BinaryOp.Plus,
        new VariableComplexity(Variable.E));
        
    // String matching: O(n × m)
    var stringComplexity = new BinaryOperationComplexity(
        new VariableComplexity(Variable.N),
        BinaryOp.Multiply,
        new VariableComplexity(Variable.M));
```

**Implicit Relationships:** Some variables have implicit constraints: 

- In connected graphs: E ≥ V - 1
- In simple graphs: E ≤ V(V-1)/2
- In balanced trees: h = Θ(log n)
- In linked structures: h ≤ n




**See also**: [`VariableType`](VariableType)

**See also**: [`VariableComplexity`](VariableComplexity)



---
#### Property Complexity.Variable.Name

 The symbolic name of the variable (e.g., "n", "V", "E"). 



---
#### Property Complexity.Variable.Type

 The semantic type of the variable, indicating what it represents. 



---
#### Property Complexity.Variable.Description

 Optional description for documentation purposes. 



---
#### Property Complexity.Variable.N

 Creates a standard input size variable named "n". 



---
#### Property Complexity.Variable.V

 Creates a vertex count variable named "V". 



---
#### Property Complexity.Variable.E

 Creates an edge count variable named "E". 



---
#### Property Complexity.Variable.M

 Creates a secondary size variable named "m" (e.g., for pattern length in string search). 



---
#### Property Complexity.Variable.K

 Creates a count parameter variable named "k" (e.g., for Take(k), top-k queries). 



---
#### Property Complexity.Variable.H

 Creates a height/depth variable named "h" (e.g., for tree height). 



---
#### Property Complexity.Variable.P

 Creates a processor count variable named "p" (for parallel complexity). 



---
## Type Complexity.VariableType

 Semantic types for complexity variables, indicating what the variable represents. 



> Variable types enable semantic analysis and validation. For example, the analyzer can verify that graph algorithms use [[|F:ComplexityAnalysis.Core.Complexity.VariableType.VertexCount]] and [[|F:ComplexityAnalysis.Core.Complexity.VariableType.EdgeCount]] appropriately, or flag potential issues when tree algorithms don't account for [[|F:ComplexityAnalysis.Core.Complexity.VariableType.TreeHeight]]. 

**Type Relationships:**

- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.VertexCount]] and [[|F:ComplexityAnalysis.Core.Complexity.VariableType.EdgeCount]] often appear together: O(V + E)
- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.InputSize]] is the default for general algorithms
- [[|F:ComplexityAnalysis.Core.Complexity.VariableType.SecondarySize]] is used when two independent sizes matter (O(n × m))






---
#### Field Complexity.VariableType.InputSize

 General input size (n) - default for most algorithms. 



---
#### Field Complexity.VariableType.DataCount

 Count of data elements in a collection. 



---
#### Field Complexity.VariableType.VertexCount

 Number of vertices in a graph (V). 



---
#### Field Complexity.VariableType.EdgeCount

 Number of edges in a graph (E). 



---
#### Field Complexity.VariableType.DegreeSum

 Sum of vertex degrees in a graph. 



---
#### Field Complexity.VariableType.TreeHeight

 Height or depth of a tree structure. 



---
#### Field Complexity.VariableType.ProcessorCount

 Number of processors/cores (for parallel complexity). 



---
#### Field Complexity.VariableType.Dimensions

 Number of dimensions (for multi-dimensional algorithms). 



---
#### Field Complexity.VariableType.StringLength

 Length of a string or character sequence. 



---
#### Field Complexity.VariableType.SecondarySize

 A secondary size parameter (e.g., m in O(n × m)). 



---
#### Field Complexity.VariableType.Custom

 Custom/user-defined variable type. 



---
## Type Complexity.VariableExtensions

 Extension methods for Variable. 



---
#### Method Complexity.VariableExtensions.ToVariableSet(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Complexity.Variable})

 Creates a variable set from multiple variables. 



---
#### Method Complexity.VariableExtensions.IsGraphVariable(ComplexityAnalysis.Core.Complexity.Variable)

 Determines if a variable represents a graph-related quantity. 



---
## Type Memory.MemoryComplexity

 Represents space/memory complexity analysis result. Space complexity measures memory usage as a function of input size. Components: - Stack space: Recursion depth, local variables - Heap space: Allocated objects, collections - Auxiliary space: Extra space beyond input 



---
#### Property Memory.MemoryComplexity.TotalSpace

 Total space complexity (dominant term). 



---
#### Property Memory.MemoryComplexity.StackSpace

 Stack space complexity (recursion depth). 



---
#### Property Memory.MemoryComplexity.HeapSpace

 Heap space complexity (allocated objects). 



---
#### Property Memory.MemoryComplexity.AuxiliarySpace

 Auxiliary space (extra space beyond input). 



---
#### Property Memory.MemoryComplexity.IsInPlace

 Whether the algorithm is in-place (O(1) auxiliary space). 



---
#### Property Memory.MemoryComplexity.IsTailRecursive

 Whether tail-call optimization can reduce stack space. 



---
#### Property Memory.MemoryComplexity.Description

 Description of memory usage pattern. 



---
#### Property Memory.MemoryComplexity.Allocations

 Breakdown of memory allocations by source. 



---
#### Method Memory.MemoryComplexity.Constant

 Creates O(1) constant space complexity (in-place). 



---
#### Method Memory.MemoryComplexity.Linear(ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Memory.MemorySource)

 Creates O(n) linear space complexity. 



---
#### Method Memory.MemoryComplexity.Logarithmic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(log n) logarithmic space complexity (typical for recursion). 



---
#### Method Memory.MemoryComplexity.Quadratic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates O(n²) quadratic space complexity. 



---
#### Method Memory.MemoryComplexity.FromRecursion(System.Int32,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Creates memory complexity from recursion pattern. 



---
## Type Memory.MemorySource

 Where memory is allocated. 



---
#### Field Memory.MemorySource.Stack

 Stack allocation (local variables, recursion frames). 



---
#### Field Memory.MemorySource.Heap

 Heap allocation (new objects, collections). 



---
#### Field Memory.MemorySource.Both

 Both stack and heap. 



---
## Type Memory.AllocationInfo

 Information about a specific memory allocation. 



---
#### Property Memory.AllocationInfo.Description

 Description of what is being allocated. 



---
#### Property Memory.AllocationInfo.Size

 The size complexity of this allocation. 



---
#### Property Memory.AllocationInfo.Source

 Where the memory is allocated. 



---
#### Property Memory.AllocationInfo.TypeName

 The type being allocated (if known). 



---
#### Property Memory.AllocationInfo.Count

 How many times this allocation occurs. 



---
#### Property Memory.AllocationInfo.TotalSize

 Total memory from this allocation. 



---
## Type Memory.ComplexityAnalysisResult

 Combined time and space complexity result. 



---
#### Property Memory.ComplexityAnalysisResult.TimeComplexity

 Time complexity of the algorithm. 



---
#### Property Memory.ComplexityAnalysisResult.SpaceComplexity

 Space/memory complexity of the algorithm. 



---
#### Property Memory.ComplexityAnalysisResult.Name

 The method or algorithm name. 



---
#### Property Memory.ComplexityAnalysisResult.HasTimeSpaceTradeoff

 Whether time-space tradeoff is possible. 



---
#### Property Memory.ComplexityAnalysisResult.Notes

 Notes about the analysis. 



---
#### Property Memory.ComplexityAnalysisResult.Confidence

 Confidence in the analysis (0-1). 



---
## Type Memory.ComplexityAnalysisResult.CommonAlgorithms

 Common algorithms with their time/space complexities. 



---
## Type Memory.IMemoryComplexityVisitor`1

 Extended visitor interface for memory complexity types. 



---
## Type Memory.SpaceComplexityClass

 Categories of space complexity. 



---
#### Field Memory.SpaceComplexityClass.Constant

 O(1) - Constant space. 



---
#### Field Memory.SpaceComplexityClass.Logarithmic

 O(log n) - Logarithmic space. 



---
#### Field Memory.SpaceComplexityClass.Linear

 O(n) - Linear space. 



---
#### Field Memory.SpaceComplexityClass.Linearithmic

 O(n log n) - Linearithmic space. 



---
#### Field Memory.SpaceComplexityClass.Quadratic

 O(n²) - Quadratic space. 



---
#### Field Memory.SpaceComplexityClass.Cubic

 O(n³) - Cubic space. 



---
#### Field Memory.SpaceComplexityClass.Exponential

 O(2^n) - Exponential space. 



---
#### Field Memory.SpaceComplexityClass.Unknown

 Unknown space complexity. 



---
## Type Memory.SpaceComplexityClassifier

 Utility methods for space complexity classification. 



---
#### Method Memory.SpaceComplexityClassifier.Classify(ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Classifies a complexity expression into a space complexity class. 



---
#### Method Memory.SpaceComplexityClassifier.IsBetterThan(ComplexityAnalysis.Core.Memory.SpaceComplexityClass,ComplexityAnalysis.Core.Memory.SpaceComplexityClass)

 Determines if one space complexity class is better (lower) than another. 



---
#### Method Memory.SpaceComplexityClassifier.GetDescription(ComplexityAnalysis.Core.Memory.SpaceComplexityClass)

 Gets a human-readable description of the space complexity class. 



---
## Type Memory.MemoryTier

 Represents the memory access tier hierarchy with associated performance weights. Each successive tier is approximately 1000x slower than the previous. 



---
#### Field Memory.MemoryTier.CpuCache

 L1/L2 CPU cache - fastest access (~1-10 ns). Typical sizes: L1 64KB, L2 256KB-512KB. 



---
#### Field Memory.MemoryTier.MainMemory

 Main memory (RAM) - fast but slower than cache (~100 ns). Typical sizes: 8GB-128GB. 



---
#### Field Memory.MemoryTier.LocalDisk

 Local disk storage (SSD/HDD) - much slower (~100 µs for SSD). 



---
#### Field Memory.MemoryTier.LocalNetwork

 Local network (LAN, same datacenter) - network latency (~1-10 ms). 



---
#### Field Memory.MemoryTier.FarNetwork

 Far network (WAN, internet, cross-region) - high latency (~100+ ms). 



---
## Type Memory.MemoryTierWeights

 Provides weight values for memory tier access costs. Uses a ~1000x compounding factor between tiers. 



---
#### Field Memory.MemoryTierWeights.CpuCache

 Base weight for CPU cache access (normalized to 1). 



---
#### Field Memory.MemoryTierWeights.MainMemory

 Weight for main memory access (~1000x cache). 



---
#### Field Memory.MemoryTierWeights.LocalDisk

 Weight for local disk access (~1000x memory). 



---
#### Field Memory.MemoryTierWeights.LocalNetwork

 Weight for local network access (~1000x disk). 



---
#### Field Memory.MemoryTierWeights.FarNetwork

 Weight for far network access (~1000x local network). 



---
#### Field Memory.MemoryTierWeights.CompoundingFactor

 The compounding factor between adjacent tiers. 



---
#### Method Memory.MemoryTierWeights.GetWeight(ComplexityAnalysis.Core.Memory.MemoryTier)

 Gets the weight for a given memory tier. 



---
#### Method Memory.MemoryTierWeights.GetWeightByLevel(System.Int32)

 Gets the weight for a tier by its ordinal level. Level 0 = Cache, Level 1 = Memory, etc. 



---
#### Property Memory.MemoryTierWeights.AllTiers

 Gets all tiers and their weights. 



---
## Type Memory.MemoryAccess

 Represents a single memory access with its tier and access count. 



---
#### Property Memory.MemoryAccess.Tier

 The memory tier being accessed. 



---
#### Property Memory.MemoryAccess.AccessCount

 The number of accesses (as a complexity expression). 



---
#### Property Memory.MemoryAccess.Description

 Optional description of what this access represents. 



---
#### Property Memory.MemoryAccess.WeightPerAccess

 Gets the weight per access for this tier. 



---
#### Property Memory.MemoryAccess.TotalCost

 Gets the total weighted cost as a complexity expression. 



---
#### Method Memory.MemoryAccess.Constant(ComplexityAnalysis.Core.Memory.MemoryTier,System.Double,System.String)

 Creates a constant number of accesses to a tier. 



---
#### Method Memory.MemoryAccess.Linear(ComplexityAnalysis.Core.Memory.MemoryTier,ComplexityAnalysis.Core.Complexity.Variable,System.String)

 Creates linear accesses to a tier: O(n) accesses. 



---
#### Method Memory.MemoryAccess.WithComplexity(ComplexityAnalysis.Core.Memory.MemoryTier,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Creates accesses with a given complexity expression. 



---
## Type Memory.AccessPattern

 Represents a pattern of memory access behavior. Used to infer likely memory tier placement. 



---
#### Field Memory.AccessPattern.Sequential

 Sequential access (e.g., array iteration) - cache-friendly. 



---
#### Field Memory.AccessPattern.Random

 Random access (e.g., hash table lookup) - likely main memory. 



---
#### Field Memory.AccessPattern.TemporalLocality

 Temporal locality - same data accessed multiple times. 



---
#### Field Memory.AccessPattern.SpatialLocality

 Spatial locality - nearby data accessed together. 



---
#### Field Memory.AccessPattern.Strided

 Strided access (e.g., matrix column traversal). 



---
#### Field Memory.AccessPattern.FileIO

 File I/O access. 



---
#### Field Memory.AccessPattern.Network

 Network access. 



---
## Type Memory.MemoryHierarchyCost

 Aggregates multiple memory accesses into a hierarchical cost model. 



---
#### Property Memory.MemoryHierarchyCost.Accesses

 All memory accesses in this cost model. 



---
#### Property Memory.MemoryHierarchyCost.TotalCost

 Gets the total weighted cost as a complexity expression. 



---
#### Property Memory.MemoryHierarchyCost.DominantTier

 Gets the dominant tier (the one contributing most to total cost). 



---
#### Property Memory.MemoryHierarchyCost.ByTier

 Groups accesses by tier. 



---
#### Method Memory.MemoryHierarchyCost.Add(ComplexityAnalysis.Core.Memory.MemoryAccess)

 Adds a memory access to this cost model. 



---
#### Method Memory.MemoryHierarchyCost.Combine(ComplexityAnalysis.Core.Memory.MemoryHierarchyCost)

 Combines two memory hierarchy costs. 



---
#### Property Memory.MemoryHierarchyCost.Empty

 Creates an empty cost model. 



---
#### Method Memory.MemoryHierarchyCost.Single(ComplexityAnalysis.Core.Memory.MemoryAccess)

 Creates a cost model with a single access. 



---
## Type Memory.MemoryTierEstimator

 Heuristics for estimating memory tier from access patterns and data sizes. 



---
#### Field Memory.MemoryTierEstimator.L1CacheSize

 Typical L1 cache size in bytes. 



---
#### Field Memory.MemoryTierEstimator.L2CacheSize

 Typical L2 cache size in bytes. 



---
#### Field Memory.MemoryTierEstimator.L3CacheSize

 Typical L3 cache size in bytes. 



---
#### Method Memory.MemoryTierEstimator.EstimateTier(ComplexityAnalysis.Core.Memory.AccessPattern,System.Int64)

 Estimates the memory tier based on access pattern and working set size. 



---
#### Method Memory.MemoryTierEstimator.ConservativeEstimate(ComplexityAnalysis.Core.Memory.AccessPattern)

 Conservative estimate: assumes main memory unless evidence suggests otherwise. 



---
## Type Progress.AnalysisPhase

 The phases of complexity analysis. 



---
#### Field Progress.AnalysisPhase.StaticExtraction

 Phase A: Static complexity extraction from AST/CFG. 



---
#### Field Progress.AnalysisPhase.RecurrenceSolving

 Phase B: Solving recurrence relations. 



---
#### Field Progress.AnalysisPhase.Refinement

 Phase C: Refinement via slack variables and perturbation. 



---
#### Field Progress.AnalysisPhase.SpeculativeAnalysis

 Phase D: Speculative analysis for partial code. 



---
#### Field Progress.AnalysisPhase.Calibration

 Phase E: Hardware calibration and weight adjustment. 



---
## Type Progress.IAnalysisProgress

 Callback interface for receiving progress updates during complexity analysis. Enables real-time feedback, logging, and early termination detection. 



---
#### Method Progress.IAnalysisProgress.OnPhaseStarted(ComplexityAnalysis.Core.Progress.AnalysisPhase)

 Called when an analysis phase begins. 



---
#### Method Progress.IAnalysisProgress.OnPhaseCompleted(ComplexityAnalysis.Core.Progress.AnalysisPhase,ComplexityAnalysis.Core.Progress.PhaseResult)

 Called when an analysis phase completes. 



---
#### Method Progress.IAnalysisProgress.OnMethodAnalyzed(ComplexityAnalysis.Core.Progress.MethodComplexityResult)

 Called when a method's complexity has been analyzed. 



---
#### Method Progress.IAnalysisProgress.OnIntermediateResult(ComplexityAnalysis.Core.Progress.PartialComplexityResult)

 Called with intermediate results during analysis. 



---
#### Method Progress.IAnalysisProgress.OnRecurrenceDetected(ComplexityAnalysis.Core.Progress.RecurrenceDetectionResult)

 Called when a recurrence relation is detected. 



---
#### Method Progress.IAnalysisProgress.OnRecurrenceSolved(ComplexityAnalysis.Core.Progress.RecurrenceSolutionResult)

 Called when a recurrence relation has been solved. 



---
#### Method Progress.IAnalysisProgress.OnWarning(ComplexityAnalysis.Core.Progress.AnalysisWarning)

 Called when a warning or issue is encountered. 



---
#### Method Progress.IAnalysisProgress.OnProgressUpdated(System.Double,System.String)

 Called periodically with overall progress percentage. 



---
## Type Progress.PhaseResult

 Result of a completed analysis phase. 



---
#### Property Progress.PhaseResult.Phase

 The phase that completed. 



---
#### Property Progress.PhaseResult.Success

 Whether the phase completed successfully. 



---
#### Property Progress.PhaseResult.Duration

 Duration of the phase. 



---
#### Property Progress.PhaseResult.ItemsProcessed

 Number of items processed in this phase. 



---
#### Property Progress.PhaseResult.ErrorMessage

 Optional error message if the phase failed. 



---
#### Property Progress.PhaseResult.Metadata

 Additional metadata about the phase result. 



---
## Type Progress.MethodComplexityResult

 Result of analyzing a single method's complexity. 



---
#### Property Progress.MethodComplexityResult.MethodName

 The fully qualified name of the method. 



---
#### Property Progress.MethodComplexityResult.FilePath

 The file path containing the method. 



---
#### Property Progress.MethodComplexityResult.LineNumber

 Line number where the method is defined. 



---
#### Property Progress.MethodComplexityResult.TimeComplexity

 The computed time complexity. 



---
#### Property Progress.MethodComplexityResult.SpaceComplexity

 The computed space complexity (if available). 



---
#### Property Progress.MethodComplexityResult.Confidence

 Confidence level in the result (0.0 to 1.0). 



---
#### Property Progress.MethodComplexityResult.RequiresReview

 Whether this result requires human review. 



---
#### Property Progress.MethodComplexityResult.ReviewReason

 Reason for requiring review (if applicable). 



---
## Type Progress.PartialComplexityResult

 Intermediate complexity result during analysis. 



---
#### Property Progress.PartialComplexityResult.Description

 Description of what was analyzed. 



---
#### Property Progress.PartialComplexityResult.Complexity

 The partial complexity expression. 



---
#### Property Progress.PartialComplexityResult.IsComplete

 Whether this is a complete or partial result. 



---
#### Property Progress.PartialComplexityResult.Context

 Context about where this result comes from. 



---
## Type Progress.RecurrenceDetectionResult

 Result when a recurrence relation is detected. 



---
#### Property Progress.RecurrenceDetectionResult.MethodName

 The method containing the recurrence. 



---
#### Property Progress.RecurrenceDetectionResult.Recurrence

 The detected recurrence pattern. 



---
#### Property Progress.RecurrenceDetectionResult.Type

 Type of recurrence detected. 



---
#### Property Progress.RecurrenceDetectionResult.IsSolvable

 Whether this recurrence can be solved analytically. 



---
#### Property Progress.RecurrenceDetectionResult.RecommendedApproach

 Recommended solving approach. 



---
## Type Progress.RecurrenceType

 Types of recurrence relations. 



---
#### Field Progress.RecurrenceType.Linear

 Linear recursion: T(n) = T(n-1) + f(n). 



---
#### Field Progress.RecurrenceType.DivideAndConquer

 Divide and conquer: T(n) = a·T(n/b) + f(n). 



---
#### Field Progress.RecurrenceType.MultiTerm

 Multi-term: T(n) = Σᵢ aᵢ·T(bᵢ·n) + f(n). 



---
#### Field Progress.RecurrenceType.Mutual

 Mutual recursion between multiple functions. 



---
#### Field Progress.RecurrenceType.NonStandard

 Non-standard recurrence requiring special handling. 



---
## Type Progress.SolvingApproach

 Approaches for solving recurrence relations. 



---
#### Field Progress.SolvingApproach.MasterTheorem

 Master Theorem for standard divide-and-conquer. 



---
#### Field Progress.SolvingApproach.AkraBazzi

 Akra-Bazzi theorem for general multi-term recurrences. 



---
#### Field Progress.SolvingApproach.Expansion

 Direct expansion/substitution. 



---
#### Field Progress.SolvingApproach.Numerical

 Numerical approximation. 



---
#### Field Progress.SolvingApproach.Unsolvable

 Cannot be solved analytically. 



---
## Type Progress.RecurrenceSolutionResult

 Result of solving a recurrence relation. 



---
#### Property Progress.RecurrenceSolutionResult.Input

 The input recurrence that was solved. 



---
#### Property Progress.RecurrenceSolutionResult.Solution

 The closed-form solution. 



---
#### Property Progress.RecurrenceSolutionResult.ApproachUsed

 The approach used to solve it. 



---
#### Property Progress.RecurrenceSolutionResult.Confidence

 Confidence in the solution. 



---
#### Property Progress.RecurrenceSolutionResult.IsExact

 Whether the solution is exact or an approximation. 



---
#### Property Progress.RecurrenceSolutionResult.Notes

 Additional notes about the solution. 



---
## Type Progress.AnalysisWarning

 Warning encountered during analysis. 



---
#### Property Progress.AnalysisWarning.Code

 Unique warning code. 



---
#### Property Progress.AnalysisWarning.Message

 Human-readable warning message. 



---
#### Property Progress.AnalysisWarning.Severity

 Severity of the warning. 



---
#### Property Progress.AnalysisWarning.Location

 Location in source code (if applicable). 



---
#### Property Progress.AnalysisWarning.SuggestedAction

 Suggested action to resolve the warning. 



---
## Type Progress.WarningSeverity

 Severity levels for analysis warnings. 



---
#### Field Progress.WarningSeverity.Info

 Informational message. 



---
#### Field Progress.WarningSeverity.Warning

 Warning that may affect accuracy. 



---
#### Field Progress.WarningSeverity.Error

 Error that prevents accurate analysis. 



---
## Type Progress.NullAnalysisProgress

 Null implementation of IAnalysisProgress that ignores all callbacks. 



---
## Type Progress.CompositeAnalysisProgress

 Aggregates multiple progress handlers. 



---
## Type Progress.ConsoleAnalysisProgress

 Logs progress to console output. 



---
## Type Recurrence.LinearRecurrenceRelation

 Represents a linear recurrence relation: T(n) = Σᵢ aᵢ·T(n-i) + f(n). 



>**General Form:** T(n) = a₁T(n-1) + a₂T(n-2) + ... + aₖT(n-k) + f(n) 

 where: 

- aᵢ = coefficient for the (n-i)th term
- k = order of the recurrence (number of previous terms)
- f(n) = non-homogeneous term (driving function)


**Solution Method:** The characteristic polynomial method: 

-  Form characteristic polynomial: x^k - a₁x^(k-1) - a₂x^(k-2) - ... - aₖ = 0 
-  Find roots (may be real, repeated, or complex) 
-  Build general solution from roots 
-  Handle non-homogeneous term if present 


**Complexity Implications:**

**Root Type**: Solution Form
- **Single root r > 1**: O(rⁿ) - exponential growth
- **Single root r = 1 (with T(n-1))**: Summation: Σf(i)
- **Repeated root r (multiplicity m)**: O(n^(m-1) · rⁿ) - polynomial times exponential
- **Complex roots r·e^(iθ)**: Oscillatory: O(rⁿ) with periodic factor


**Common Patterns:**



######  code

```
    // Fibonacci: T(n) = T(n-1) + T(n-2) → O(φⁿ) where φ ≈ 1.618
    var fib = LinearRecurrenceRelation.Create(new[] { 1.0, 1.0 }, O_1, n);
    
    // Linear summation: T(n) = T(n-1) + O(1) → O(n)
    var linear = LinearRecurrenceRelation.Create(new[] { 1.0 }, O_1, n);
    
    // Exponential doubling: T(n) = 2T(n-1) + O(1) → O(2ⁿ)
    var exp2 = LinearRecurrenceRelation.Create(new[] { 2.0 }, O_1, n);
```



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceComplexity`](RecurrenceComplexity)



---
#### Property Recurrence.LinearRecurrenceRelation.Coefficients

 The coefficients [a₁, a₂, ..., aₖ] for T(n-1), T(n-2), ..., T(n-k). 



> Coefficients[0] is the coefficient of T(n-1), Coefficients[1] is the coefficient of T(n-2), etc. 



---
#### Property Recurrence.LinearRecurrenceRelation.NonRecursiveWork

 The non-homogeneous (driving) function f(n) in T(n) = ... + f(n). 



> If the recurrence is homogeneous (no f(n) term), this should be [[|P:ComplexityAnalysis.Core.Complexity.ConstantComplexity.Zero]]. 



---
#### Property Recurrence.LinearRecurrenceRelation.Variable

 The variable representing the input size (typically n). 



---
#### Property Recurrence.LinearRecurrenceRelation.Order

 The order of the recurrence (k in T(n-k)). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsHomogeneous

 Whether this is a homogeneous recurrence (no f(n) term). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsFirstOrder

 Whether this is a first-order recurrence T(n) = a·T(n-1) + f(n). 



---
#### Property Recurrence.LinearRecurrenceRelation.IsSummation

 Whether this is a simple summation T(n) = T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Create(System.Double[],ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recurrence relation. 

|Name | Description |
|-----|------|
|coefficients: | Coefficients [a₁, a₂, ..., aₖ] where T(n) = a₁T(n-1) + a₂T(n-2) + ... + f(n). |
|nonRecursiveWork: |The non-homogeneous term f(n).|
|variable: |The recurrence variable (typically n).|
**Returns**: A new linear recurrence relation.

[[T:System.ArgumentException|T:System.ArgumentException]]: If coefficients is empty.



---
#### Method Recurrence.LinearRecurrenceRelation.Create(System.Collections.Immutable.ImmutableArray{System.Double},ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recurrence relation with immutable coefficients. 



---
#### Method Recurrence.LinearRecurrenceRelation.Fibonacci(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates the Fibonacci recurrence: T(n) = T(n-1) + T(n-2) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Summation(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple summation recurrence: T(n) = T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.Exponential(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates an exponential recurrence: T(n) = a·T(n-1) + f(n). 



---
#### Method Recurrence.LinearRecurrenceRelation.ToString

 Gets a human-readable representation of the recurrence. 



---
## Type Recurrence.LinearRecurrenceSolution

 Result of solving a linear recurrence relation. 



> Contains both the asymptotic solution and details about how it was derived. 



---
#### Property Recurrence.LinearRecurrenceSolution.Solution

 The closed-form asymptotic solution. 



---
#### Property Recurrence.LinearRecurrenceSolution.Method

 Description of the solution method used. 



---
#### Property Recurrence.LinearRecurrenceSolution.Roots

 The roots of the characteristic polynomial. 



---
#### Property Recurrence.LinearRecurrenceSolution.DominantRoot

 The dominant root (largest magnitude). 



---
#### Property Recurrence.LinearRecurrenceSolution.HasPolynomialFactor

 Whether the solution involves polynomial factors from repeated roots. 



---
#### Property Recurrence.LinearRecurrenceSolution.Explanation

 Detailed explanation of the solution derivation. 



---
## Type Recurrence.CharacteristicRoot

 A root of the characteristic polynomial with its properties. 



> Roots can be real or complex. Complex roots always come in conjugate pairs for recurrences with real coefficients. 



---
#### Property Recurrence.CharacteristicRoot.RealPart

 The real part of the root. 



---
#### Property Recurrence.CharacteristicRoot.ImaginaryPart

 The imaginary part of the root (0 for real roots). 



---
#### Property Recurrence.CharacteristicRoot.Magnitude

 The magnitude |r| = √(a² + b²) for complex root a + bi. 



---
#### Property Recurrence.CharacteristicRoot.Multiplicity

 The multiplicity (how many times this root appears). 



---
#### Property Recurrence.CharacteristicRoot.IsReal

 Whether this is a real root (imaginary part ≈ 0). 



---
#### Property Recurrence.CharacteristicRoot.IsRepeated

 Whether this is a repeated root (multiplicity > 1). 



---
#### Method Recurrence.CharacteristicRoot.Real(System.Double,System.Int32)

 Creates a real root. 



---
#### Method Recurrence.CharacteristicRoot.Complex(System.Double,System.Double,System.Int32)

 Creates a complex root. 



---
## Type Recurrence.MutualRecurrenceSystem

 Represents a system of mutually recursive recurrence relations. For mutually recursive functions A(n) and B(n): - A(n) = T_A(n-1) + f_A(n) where A calls B - B(n) = T_B(n-1) + f_B(n) where B calls A This can be combined into a single recurrence by substitution. 



---
#### Property Recurrence.MutualRecurrenceSystem.Components

 The methods involved in the mutual recursion cycle. The order represents the cycle: methods[0] → methods[1] → ... → methods[0] 



---
#### Property Recurrence.MutualRecurrenceSystem.Variable

 The recurrence variable (typically n). 



---
#### Property Recurrence.MutualRecurrenceSystem.CycleLength

 Number of methods in the cycle. 



---
#### Property Recurrence.MutualRecurrenceSystem.CombinedReduction

 The combined reduction per full cycle through all methods. For A → B → A with each doing -1, this is -2 (or scale 0.99^2 for divide pattern). 



---
#### Property Recurrence.MutualRecurrenceSystem.CombinedWork

 The combined non-recursive work done in one full cycle. 



---
#### Method Recurrence.MutualRecurrenceSystem.ToSingleRecurrence

 Converts the mutual recursion system to an equivalent single recurrence. For a cycle A → B → C → A where each reduces by 1: Combined: T(n) = T(n - cycleLength) + CombinedWork 



---
#### Property Recurrence.MutualRecurrenceSystem.IsSubtractionPattern

 Whether this is a subtraction-based mutual recursion (each step reduces by constant). 



---
#### Property Recurrence.MutualRecurrenceSystem.IsDivisionPattern

 Whether this is a division-based mutual recursion (each step divides by constant). 



---
#### Method Recurrence.MutualRecurrenceSystem.GetDescription

 Gets a human-readable description of the mutual recursion. 



---
## Type Recurrence.MutualRecurrenceComponent

 Represents one method in a mutual recursion cycle. 



---
#### Property Recurrence.MutualRecurrenceComponent.MethodName

 The method name (for diagnostics). 



---
#### Property Recurrence.MutualRecurrenceComponent.NonRecursiveWork

 The non-recursive work done by this method. 



---
#### Property Recurrence.MutualRecurrenceComponent.Reduction

 How much the problem size is reduced when calling the next method. For subtraction: reduction amount (e.g., 1 for n-1). 



---
#### Property Recurrence.MutualRecurrenceComponent.ScaleFactor

 Scale factor for divide-style patterns (1/b in T(n/b)). For subtraction patterns, this is close to 1 (e.g., 0.99). 



---
#### Property Recurrence.MutualRecurrenceComponent.Callees

 The methods this component calls (within the cycle). 



---
## Type Recurrence.MutualRecurrenceSolution

 Result of solving a mutual recursion system. 



---
#### Property Recurrence.MutualRecurrenceSolution.Success

 Whether the system was successfully solved. 



---
#### Property Recurrence.MutualRecurrenceSolution.Solution

 The complexity solution for the first method in the cycle. Since all methods in the cycle have the same asymptotic complexity (differing only by constants), this applies to all. 



---
#### Property Recurrence.MutualRecurrenceSolution.MethodSolutions

 Individual solutions for each method (may differ by constant factors). 



---
#### Property Recurrence.MutualRecurrenceSolution.Method

 The approach used to solve the recurrence. 



---
#### Property Recurrence.MutualRecurrenceSolution.FailureReason

 Diagnostic information if solving failed. 



---
#### Property Recurrence.MutualRecurrenceSolution.EquivalentRecurrence

 The equivalent single recurrence that was solved. 



---
#### Method Recurrence.MutualRecurrenceSolution.Solved(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String,ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Creates a successful solution. 



---
#### Method Recurrence.MutualRecurrenceSolution.Failed(System.String)

 Creates a failed solution. 



---
## Type Recurrence.RecurrenceComplexity

 Represents a recurrence relation for complexity analysis of recursive algorithms. 



>**General Form:** T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n) 

 where: 

- aᵢ = number of recursive calls of type i
- bᵢ = scale factor for subproblem size (0 < bᵢ < 1)
- hᵢ(n) = perturbation function (often 0)
- g(n) = non-recursive work at each level


**Analysis Theorems:**

**Theorem**: Applicability
- **Master Theorem**: Single-term: T(n) = a·T(n/b) + f(n), where a ≥ 1, b > 1
- **Akra-Bazzi**: Multi-term: T(n) = Σᵢ aᵢ·T(bᵢn) + g(n), where aᵢ > 0, 0 < bᵢ < 1
- **Linear Recurrence**: T(n) = T(n-1) + f(n), solved by summation


**Common Patterns:**



######  code

```
    // Merge Sort: T(n) = 2T(n/2) + O(n) → O(n log n)
    var mergeSort = RecurrenceComplexity.DivideAndConquer(2, 2, O_n, n);
    
    // Binary Search: T(n) = T(n/2) + O(1) → O(log n)
    var binarySearch = RecurrenceComplexity.DivideAndConquer(1, 2, O_1, n);
    
    // Strassen: T(n) = 7T(n/2) + O(n²) → O(n^2.807)
    var strassen = RecurrenceComplexity.DivideAndConquer(7, 2, O_n2, n);
```

 See the TheoremApplicabilityAnalyzer in ComplexityAnalysis.Solver for the analysis engine that solves these recurrences. 



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceTerm`](RecurrenceTerm)



---
#### Method Recurrence.RecurrenceComplexity.#ctor(System.Collections.Immutable.ImmutableList{ComplexityAnalysis.Core.Recurrence.RecurrenceTerm},ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Represents a recurrence relation for complexity analysis of recursive algorithms. 



>**General Form:** T(n) = Σᵢ aᵢ·T(bᵢ·n + hᵢ(n)) + g(n) 

 where: 

- aᵢ = number of recursive calls of type i
- bᵢ = scale factor for subproblem size (0 < bᵢ < 1)
- hᵢ(n) = perturbation function (often 0)
- g(n) = non-recursive work at each level


**Analysis Theorems:**

**Theorem**: Applicability
- **Master Theorem**: Single-term: T(n) = a·T(n/b) + f(n), where a ≥ 1, b > 1
- **Akra-Bazzi**: Multi-term: T(n) = Σᵢ aᵢ·T(bᵢn) + g(n), where aᵢ > 0, 0 < bᵢ < 1
- **Linear Recurrence**: T(n) = T(n-1) + f(n), solved by summation


**Common Patterns:**



######  code

```
    // Merge Sort: T(n) = 2T(n/2) + O(n) → O(n log n)
    var mergeSort = RecurrenceComplexity.DivideAndConquer(2, 2, O_n, n);
    
    // Binary Search: T(n) = T(n/2) + O(1) → O(log n)
    var binarySearch = RecurrenceComplexity.DivideAndConquer(1, 2, O_1, n);
    
    // Strassen: T(n) = 7T(n/2) + O(n²) → O(n^2.807)
    var strassen = RecurrenceComplexity.DivideAndConquer(7, 2, O_n2, n);
```

 See the TheoremApplicabilityAnalyzer in ComplexityAnalysis.Solver for the analysis engine that solves these recurrences. 



**See also**: [`RecurrenceRelation`](RecurrenceRelation)

**See also**: [`RecurrenceTerm`](RecurrenceTerm)



---
#### Property Recurrence.RecurrenceComplexity.TotalRecursiveCalls

 Gets the total number of recursive calls (sum of coefficients). For T(n) = 2T(n/2) + O(n), this returns 2. 



---
#### Property Recurrence.RecurrenceComplexity.FitsMasterTheorem

 Determines if this recurrence fits the Master Theorem pattern: T(n) = a·T(n/b) + f(n) where a ≥ 1, b > 1. 



---
#### Property Recurrence.RecurrenceComplexity.FitsAkraBazzi

 Determines if this recurrence fits the Akra-Bazzi pattern (more general than Master Theorem). 



---
#### Method Recurrence.RecurrenceComplexity.DivideAndConquer(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + O(n^d). 



---
#### Method Recurrence.RecurrenceComplexity.Linear(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a linear recursion: T(n) = T(n-1) + O(f(n)). 



---
## Type Recurrence.RecurrenceTerm

 Represents a single term in a recurrence relation. 



> For a recurrence like T(n) = 2·T(n/3) + O(n), the term is: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Coefficient]] = 2 (number of recursive calls)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Argument]] = n/3 (subproblem size expression)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.ScaleFactor]] = 1/3 (reduction ratio)


**Well-formedness:** For theorem applicability, terms must satisfy: 

- Coefficient > 0 (at least one recursive call)
- 0 < ScaleFactor < 1 (subproblem is smaller)




|Name | Description |
|-----|------|
|Coefficient: |The multiplier for this recursive call (a in a·T(f(n))).|
|Argument: |The argument to the recursive call (f(n) in T(f(n))).|
|ScaleFactor: |The scale factor for the subproblem size (1/b in T(n/b)).|


---
#### Method Recurrence.RecurrenceTerm.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Double)

 Represents a single term in a recurrence relation. 



> For a recurrence like T(n) = 2·T(n/3) + O(n), the term is: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Coefficient]] = 2 (number of recursive calls)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.Argument]] = n/3 (subproblem size expression)
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceTerm.ScaleFactor]] = 1/3 (reduction ratio)


**Well-formedness:** For theorem applicability, terms must satisfy: 

- Coefficient > 0 (at least one recursive call)
- 0 < ScaleFactor < 1 (subproblem is smaller)




|Name | Description |
|-----|------|
|Coefficient: |The multiplier for this recursive call (a in a·T(f(n))).|
|Argument: |The argument to the recursive call (f(n) in T(f(n))).|
|ScaleFactor: |The scale factor for the subproblem size (1/b in T(n/b)).|


---
#### Property Recurrence.RecurrenceTerm.Coefficient

The multiplier for this recursive call (a in a·T(f(n))).



---
#### Property Recurrence.RecurrenceTerm.Argument

The argument to the recursive call (f(n) in T(f(n))).



---
#### Property Recurrence.RecurrenceTerm.ScaleFactor

The scale factor for the subproblem size (1/b in T(n/b)).



---
#### Property Recurrence.RecurrenceTerm.IsReducing

 Determines if this term represents a proper reduction (subproblem smaller than original). 



---
## Type Recurrence.RecurrenceRelationTerm

 A term in a recurrence relation with coefficient and scale factor. 



---
#### Method Recurrence.RecurrenceRelationTerm.#ctor(System.Double,System.Double)

 A term in a recurrence relation with coefficient and scale factor. 



---
## Type Recurrence.RecurrenceRelation

 Represents a fully specified recurrence relation with explicit terms for analysis. 



> This is the normalized form used as input to recurrence solvers. It extracts the essential mathematical components from [[|T:ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity]]: 

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.Terms]]: The recursive structure [(aᵢ, bᵢ)]
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.NonRecursiveWork]]: The g(n) function
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.BaseCase]]: The T(1) boundary condition


**Theorem Selection:**

- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.FitsMasterTheorem]]: Single term with a ≥ 1, b > 1 
- [[|P:ComplexityAnalysis.Core.Recurrence.RecurrenceRelation.FitsAkraBazzi]]: All terms have aᵢ > 0 and 0 < bᵢ < 1 


**Convenience Factories:**



######  code

```
    // Standard divide-and-conquer
    var rec = RecurrenceRelation.DivideAndConquer(2, 2, O_n, Variable.N);
    
    // From existing RecurrenceComplexity
    var rel = RecurrenceRelation.FromComplexity(recurrence);
```



**See also**: [`RecurrenceComplexity`](RecurrenceComplexity)



---
#### Property Recurrence.RecurrenceRelation.Terms

 The recursive terms: [(aᵢ, bᵢ)] where T(n) contains aᵢ·T(bᵢ·n). 



---
#### Property Recurrence.RecurrenceRelation.NonRecursiveWork

 The non-recursive work function g(n). 



---
#### Property Recurrence.RecurrenceRelation.BaseCase

 The base case complexity T(1). 



---
#### Property Recurrence.RecurrenceRelation.Variable

 The recurrence variable (typically n). 



---
#### Method Recurrence.RecurrenceRelation.#ctor(System.Collections.Generic.IEnumerable{ComplexityAnalysis.Core.Recurrence.RecurrenceRelationTerm},ComplexityAnalysis.Core.Complexity.Variable,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Creates a recurrence relation from explicit terms. 



---
#### Property Recurrence.RecurrenceRelation.FitsMasterTheorem

 Checks if this recurrence fits the Master Theorem form. 



---
#### Property Recurrence.RecurrenceRelation.FitsAkraBazzi

 Checks if this recurrence fits the Akra-Bazzi pattern. 



---
#### Property Recurrence.RecurrenceRelation.A

 For Master Theorem: a in T(n) = a·T(n/b) + f(n). 



---
#### Property Recurrence.RecurrenceRelation.B

 For Master Theorem: b in T(n) = a·T(n/b) + f(n). 



---
#### Method Recurrence.RecurrenceRelation.FromComplexity(ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity)

 Creates a RecurrenceRelation from a RecurrenceComplexity. 



---
#### Method Recurrence.RecurrenceRelation.DivideAndConquer(System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.Variable)

 Creates a standard divide-and-conquer recurrence: T(n) = a·T(n/b) + f(n). 



---
## Type Recurrence.TheoremApplicability

 Base type for theorem applicability results. Captures which theorem applies (or not) and all relevant parameters. 



---
#### Property Recurrence.TheoremApplicability.IsApplicable

Whether any theorem successfully applies.



---
#### Property Recurrence.TheoremApplicability.Solution

The recommended solution if applicable.



---
#### Property Recurrence.TheoremApplicability.Explanation

Human-readable explanation of the result.



---
## Type Recurrence.MasterTheoremApplicable

 Master Theorem applies successfully. 



---
#### Method Recurrence.MasterTheoremApplicable.#ctor(ComplexityAnalysis.Core.Recurrence.MasterTheoremCase,System.Double,System.Double,System.Double,ComplexityAnalysis.Core.Complexity.ExpressionClassification,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Master Theorem applies successfully. 



---
#### Property Recurrence.MasterTheoremApplicable.Epsilon

 For Case 1: the ε such that f(n) = O(n^(log_b(a) - ε)). For Case 3: the ε such that f(n) = Ω(n^(log_b(a) + ε)). For Case 2: 0 (exact match). 



---
#### Property Recurrence.MasterTheoremApplicable.LogExponentK

For Case 2: the k in f(n) = Θ(n^d · log^k n).



---
#### Property Recurrence.MasterTheoremApplicable.RegularityVerified

For Case 3: whether the regularity condition was verified.



---
## Type Recurrence.MasterTheoremCase

 The three cases of the Master Theorem. 



---
#### Field Recurrence.MasterTheoremCase.Case1

 f(n) = O(n^(log_b(a) - ε)) for some ε > 0. Work at leaves dominates. Solution: Θ(n^(log_b a)). 



---
#### Field Recurrence.MasterTheoremCase.Case2

 f(n) = Θ(n^(log_b a) · log^k n) for some k ≥ 0. Work balanced across levels. Solution: Θ(n^(log_b a) · log^(k+1) n). 



---
#### Field Recurrence.MasterTheoremCase.Case3

 f(n) = Ω(n^(log_b(a) + ε)) for some ε > 0, AND regularity holds. Work at root dominates. Solution: Θ(f(n)). 



---
#### Field Recurrence.MasterTheoremCase.Gap

 Falls between cases (Master Theorem gap). Use Akra-Bazzi or other methods. 



---
## Type Recurrence.AkraBazziApplicable

 Akra-Bazzi Theorem applies successfully. 



---
#### Method Recurrence.AkraBazziApplicable.#ctor(System.Double,ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Akra-Bazzi Theorem applies successfully. 



---
#### Property Recurrence.AkraBazziApplicable.Terms

The recurrence terms used.



---
#### Property Recurrence.AkraBazziApplicable.GClassification

Classification of g(n).



---
## Type Recurrence.LinearRecurrenceSolved

 Linear recurrence T(n) = T(n-1) + f(n) solved directly. 



---
#### Method Recurrence.LinearRecurrenceSolved.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.String)

 Linear recurrence T(n) = T(n-1) + f(n) solved directly. 



---
## Type Recurrence.TheoremNotApplicable

 No standard theorem applies. 



---
#### Method Recurrence.TheoremNotApplicable.#ctor(System.String,System.Collections.Immutable.ImmutableList{System.String})

 No standard theorem applies. 



---
#### Property Recurrence.TheoremNotApplicable.Suggestions

Suggested alternative approaches.



---
## Type Recurrence.ITheoremApplicabilityAnalyzer

 Analyzer that determines which theorem applies to a recurrence. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.Analyze(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Analyzes a recurrence and determines which theorem applies. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.CheckMasterTheorem(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Specifically checks Master Theorem applicability. 



---
#### Method Recurrence.ITheoremApplicabilityAnalyzer.CheckAkraBazzi(ComplexityAnalysis.Core.Recurrence.RecurrenceRelation)

 Specifically checks Akra-Bazzi applicability. 



---
## Type Recurrence.TheoremApplicabilityExtensions

 Extension methods for working with theorem applicability. 



---
#### Method Recurrence.TheoremApplicabilityExtensions.AnalyzeRecurrence(ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity,ComplexityAnalysis.Core.Recurrence.ITheoremApplicabilityAnalyzer)

 Tries Master Theorem first, then Akra-Bazzi, then reports failure. 



---




# ComplexityAnalysis.Calibration #

## Type BCLCalibrator

 Calibrates BCL method complexities through runtime verification. Generates benchmark code, runs it, and analyzes results to determine constant factors for complexity expressions. 



---
#### Method BCLCalibrator.CalibrateListAdd

 Calibrates List<T>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateListContains

 Calibrates List<T>.Contains method. Expected: O(n). 



---
#### Method BCLCalibrator.CalibrateListSort

 Calibrates List<T>.Sort method. Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateListBinarySearch

 Calibrates List<T>.BinarySearch method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateDictionaryAdd

 Calibrates Dictionary<K,V>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateDictionaryTryGetValue

 Calibrates Dictionary<K,V>.TryGetValue method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateDictionaryContainsKey

 Calibrates Dictionary<K,V>.ContainsKey method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateHashSetAdd

 Calibrates HashSet<T>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateHashSetContains

 Calibrates HashSet<T>.Contains method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateSortedSetAdd

 Calibrates SortedSet<T>.Add method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateSortedDictionaryAdd

 Calibrates SortedDictionary<K,V>.Add method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateStringContains

 Calibrates String.Contains method. Expected: O(n*m) worst case, often O(n) average. 



---
#### Method BCLCalibrator.CalibrateStringIndexOf

 Calibrates String.IndexOf method. Expected: O(n*m) worst case. 



---
#### Method BCLCalibrator.CalibrateArraySort

 Calibrates Array.Sort method. Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateArrayBinarySearch

 Calibrates Array.BinarySearch method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateLinqWhere

 Calibrates LINQ Enumerable.Where (iteration). Expected: O(n) for full iteration. 



---
#### Method BCLCalibrator.CalibrateLinqOrderBy

 Calibrates LINQ Enumerable.OrderBy (full sort). Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateStringBuilderAppend

 Calibrates StringBuilder.Append method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateRegexIsMatch

 Calibrates Regex.IsMatch for simple pattern. Expected: O(n) for simple patterns. 



---
#### Method BCLCalibrator.RunFullCalibration(System.IProgress{System.ValueTuple{System.String,System.Int32,System.Int32}})

 Runs all standard BCL calibrations. 

|Name | Description |
|-----|------|
|progress: |Optional progress callback.|
**Returns**: Complete calibration data.



---
#### Method BCLCalibrator.CalibrateMethod``1(System.String,System.String,System.String,System.Func{System.Int32,``0},System.Action{``0})

 Generic method calibration helper. 



---
#### Method BCLCalibrator.EstimateError(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult},System.Double)

 Estimates error in constant factor from measurement variance. 



---
## Type BenchmarkResult

 Result of a single micro-benchmark measurement. 



---
#### Property BenchmarkResult.InputSize

 The input size N used for this measurement. 



---
#### Property BenchmarkResult.MeanNanoseconds

 Mean time per operation in nanoseconds. 



---
#### Property BenchmarkResult.StdDevNanoseconds

 Standard deviation in nanoseconds. 



---
#### Property BenchmarkResult.Iterations

 Number of iterations performed. 



---
#### Property BenchmarkResult.MinNanoseconds

 Minimum time observed in nanoseconds. 



---
#### Property BenchmarkResult.MaxNanoseconds

 Maximum time observed in nanoseconds. 



---
#### Property BenchmarkResult.AllocatedBytes

 Memory allocated per operation in bytes (if tracked). 



---
#### Property BenchmarkResult.CoefficientOfVariation

 Coefficient of variation (StdDev / Mean). Lower values indicate more stable measurements. 



---
## Type ComplexityVerificationResult

 Result of complexity verification through runtime measurement. 



---
#### Property ComplexityVerificationResult.ClaimedComplexity

 The claimed complexity class. 



---
#### Property ComplexityVerificationResult.MeasuredComplexity

 The best-fit complexity class from measurement. 



---
#### Property ComplexityVerificationResult.Verified

 Whether the measured complexity matches the claim. 



---
#### Property ComplexityVerificationResult.ConstantFactor

 Estimated constant factor (c in c*f(n)). 



---
#### Property ComplexityVerificationResult.RSquared

 R-squared value for the complexity fit (0.0 to 1.0). Higher values indicate better fit. 



---
#### Property ComplexityVerificationResult.BenchmarkResults

 Individual benchmark results used for verification. 



---
#### Property ComplexityVerificationResult.Confidence

 Confidence level in the verification (0.0 to 1.0). 



---
#### Property ComplexityVerificationResult.Notes

 Any warnings or notes about the verification. 



---
## Type BCLCalibrationResult

 Result of calibrating a specific BCL method. 



---
#### Property BCLCalibrationResult.MethodName

 The fully qualified method name. 



---
#### Property BCLCalibrationResult.TypeName

 The type containing the method. 



---
#### Property BCLCalibrationResult.Complexity

 The verified/calibrated complexity. 



---
#### Property BCLCalibrationResult.ConstantFactorNs

 Estimated constant factor in nanoseconds per base operation. 



---
#### Property BCLCalibrationResult.ConstantFactorError

 Standard error of the constant factor estimate. 



---
#### Property BCLCalibrationResult.Success

 Whether the calibration was successful. 



---
#### Property BCLCalibrationResult.DataPoints

 Number of data points used for calibration. 



---
#### Property BCLCalibrationResult.RSquared

 R-squared value for the fit. 



---
#### Property BCLCalibrationResult.Timestamp

 Timestamp when calibration was performed. 



---
#### Property BCLCalibrationResult.HardwareProfile

 Hardware profile at time of calibration. 



---
#### Property BCLCalibrationResult.ErrorMessage

 Any error message if calibration failed. 



---
## Type HardwareProfile

 Hardware profile for a calibration environment. 



---
#### Property HardwareProfile.ProfileId

 Unique identifier for this hardware profile. 



---
#### Property HardwareProfile.MachineName

 Machine name. 



---
#### Property HardwareProfile.ProcessorDescription

 Processor description. 



---
#### Property HardwareProfile.ProcessorCount

 Number of logical processors. 



---
#### Property HardwareProfile.PhysicalMemoryBytes

 Physical memory in bytes. 



---
#### Property HardwareProfile.OSDescription

 Operating system description. 



---
#### Property HardwareProfile.RuntimeVersion

 .NET runtime version. 



---
#### Property HardwareProfile.Is64BitProcess

 Whether the process is running in 64-bit mode. 



---
#### Property HardwareProfile.ReferenceBenchmarkScore

 Reference benchmark score for normalization across machines. Higher is faster. 



---
#### Property HardwareProfile.CapturedAt

 Timestamp when this profile was captured. 



---
#### Method HardwareProfile.Current

 Creates a hardware profile for the current machine. 



---
## Type CalibrationData

 Complete calibration data for a machine, containing all BCL calibration results. 



---
#### Property CalibrationData.Version

 Version of the calibration data format. 



---
#### Property CalibrationData.HardwareProfile

 Hardware profile for this calibration data. 



---
#### Property CalibrationData.MethodCalibrations

 All BCL method calibration results. 



---
#### Property CalibrationData.StartedAt

 When the calibration was started. 



---
#### Property CalibrationData.CompletedAt

 When the calibration was completed. 



---
#### Property CalibrationData.Duration

 Total duration of calibration. 



---
#### Property CalibrationData.SuccessfulCalibrations

 Number of methods successfully calibrated. 



---
#### Property CalibrationData.FailedCalibrations

 Number of methods that failed calibration. 



---
## Type BenchmarkOptions

 Options for benchmark configuration. 



---
#### Property BenchmarkOptions.Quick

 Default benchmark options for quick calibration. 



---
#### Property BenchmarkOptions.Standard

 Standard benchmark options for typical calibration. 



---
#### Property BenchmarkOptions.Thorough

 Thorough benchmark options for high-precision calibration. 



---
#### Property BenchmarkOptions.WarmupIterations

 Number of warmup iterations before measurement. 



---
#### Property BenchmarkOptions.MeasurementIterations

 Number of measurement iterations. 



---
#### Property BenchmarkOptions.MinIterationTimeMs

 Minimum time per iteration in milliseconds. 



---
#### Property BenchmarkOptions.MaxIterationTimeMs

 Maximum time per iteration in milliseconds. 



---
#### Property BenchmarkOptions.InputSizes

 Input sizes to benchmark at. 



---
#### Property BenchmarkOptions.TrackAllocations

 Whether to track memory allocations. 



---
#### Property BenchmarkOptions.ForceGC

 Whether to force garbage collection between iterations. 



---
## Type CalibrationStore

 Persists and loads calibration data to/from disk. Supports JSON format with optional compression. 



---
#### Method CalibrationStore.#ctor(System.String)

 Creates a calibration store with the specified base path. 

|Name | Description |
|-----|------|
|basePath: |Directory to store calibration data. Defaults to user's local app data.|


---
#### Method CalibrationStore.SaveAsync(ComplexityAnalysis.Calibration.CalibrationData,System.Threading.CancellationToken)

 Saves calibration data for the current machine. 



---
#### Method CalibrationStore.LoadLatestAsync(System.Threading.CancellationToken)

 Loads the most recent calibration data. 



---
#### Method CalibrationStore.LoadAsync(System.String,System.Threading.CancellationToken)

 Loads calibration data for a specific hardware profile. 



---
#### Method CalibrationStore.ListProfiles

 Lists all available calibration profiles. 



---
#### Method CalibrationStore.IsCalibrationValidAsync(System.TimeSpan,System.Threading.CancellationToken)

 Checks if calibration data exists and is recent enough. 



---
#### Method CalibrationStore.Delete(System.String)

 Deletes calibration data for a profile. 



---
#### Method CalibrationStore.GenerateReport(ComplexityAnalysis.Calibration.CalibrationData)

 Exports calibration data as a summary report. 



---
## Type CalibratedComplexityLookup

 Provides lookup of calibrated constant factors for BCL methods. 



---
#### Property CalibratedComplexityLookup.HasCalibration

 Whether calibration data is available. 



---
#### Method CalibratedComplexityLookup.GetConstantFactor(System.String,System.String)

 Gets the calibrated constant factor for a method, if available. 



---
#### Method CalibratedComplexityLookup.EstimateTime(System.String,System.String,System.String,System.Int32)

 Gets the estimated time for an operation with given complexity and input size. 

|Name | Description |
|-----|------|
|typeName: |Type containing the method.|
|methodName: |Method name.|
|complexity: |Complexity class (e.g., "O(n)", "O(log n)").|
|inputSize: |Input size N.|
**Returns**: Estimated time in nanoseconds, or null if not calibrated.



---
#### Method CalibratedComplexityLookup.GetAllCalibrations

 Gets all calibrated methods. 



---
## Type ComplexityVerifier

 Verifies claimed complexity classes against runtime measurements. Uses curve fitting to determine the best-fit complexity class. 



---
#### Method ComplexityVerifier.Verify``1(System.String,System.Func{System.Int32,``0},System.Action{``0})

 Verifies that an operation matches its claimed complexity. 

|Name | Description |
|-----|------|
|T: |Type of setup data.|
|Name | Description |
|-----|------|
|claimedComplexity: |The claimed complexity (e.g., "O(n)", "O(log n)").|
|setup: |Setup function that creates data for input size N.|
|action: |The action to verify.|
**Returns**: Verification result with measured complexity and constant factor.



---
#### Method ComplexityVerifier.AnalyzeResults(System.String,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Analyzes benchmark results to determine complexity class. 



---
#### Method ComplexityVerifier.DetectComplexity(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Detects the complexity class from benchmark results without a prior claim. 



---
#### Method ComplexityVerifier.EstimateConstantFactor(System.String,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Estimates the constant factor for a known complexity class. 



---
#### Method ComplexityVerifier.FitConstant(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(1) - constant time. 



---
#### Method ComplexityVerifier.FitLogarithmic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(log n) - logarithmic time. 



---
#### Method ComplexityVerifier.FitLinear(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n) - linear time. 



---
#### Method ComplexityVerifier.FitLinearithmic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n log n) - linearithmic time. 



---
#### Method ComplexityVerifier.FitQuadratic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n²) - quadratic time. 



---
#### Method ComplexityVerifier.FitCubic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n³) - cubic time. 



---
#### Method ComplexityVerifier.FitExponential(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(2^n) - exponential time. 



---
#### Method ComplexityVerifier.LinearRegression(System.Collections.Generic.IReadOnlyList{System.Double},System.Collections.Generic.IReadOnlyList{System.Double})

 Performs linear regression without intercept: y = c*x Returns R² and coefficient c. 



---
#### Method ComplexityVerifier.NormalizeComplexity(System.String)

 Normalizes complexity notation to a standard form. 



---
#### Method ComplexityVerifier.ComputeConfidence(System.Double,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Computes confidence based on R² and measurement stability. 



---
## Type MicroBenchmarkRunner

 Runs micro-benchmarks to measure operation timing with high precision. Uses careful warmup, statistical analysis, and outlier removal. 



---
#### Method MicroBenchmarkRunner.Run``1(System.Func{System.Int32,``0},System.Action{``0})

 Runs a benchmark on an action that operates on input of size N. 

|Name | Description |
|-----|------|
|setup: |Setup function that returns data for input size N.|
|action: |The action to benchmark, receives setup data.|
|T: |Type of setup data.|
**Returns**: List of benchmark results for each input size.



---
#### Method MicroBenchmarkRunner.MeasureAtSize``1(System.Func{System.Int32,``0},System.Action{``0},System.Int32)

 Runs a benchmark at a specific input size. 



---
#### Method MicroBenchmarkRunner.CalibrateOperationsPerIteration``1(System.Action{``0},``0)

 Calibrates the number of operations to run per iteration to meet timing requirements. 



---
#### Method MicroBenchmarkRunner.MeasureOperations``1(System.Action{``0},``0,System.Int32)

 Measures the time to execute a specific number of operations. 



---
#### Method MicroBenchmarkRunner.RemoveOutliers(System.Collections.Generic.List{System.Double})

 Removes outliers using the IQR method. 



---
#### Method MicroBenchmarkRunner.ComputeStdDev(System.Collections.Generic.List{System.Double},System.Double)

 Computes standard deviation. 



---


