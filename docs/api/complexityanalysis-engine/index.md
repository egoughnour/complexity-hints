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



# ComplexityAnalysis.Roslyn #

## Type Analysis.AmortizedAnalyzer

 Analyzes code patterns to detect amortized complexity scenarios. Detects patterns like: - Dynamic array resizing (doubling strategy) - Hash table rehashing - Binary counter increment - Stack with multipop - Union-Find with path compression 



---
#### Method Analysis.AmortizedAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for amortized complexity patterns. Returns an AmortizedComplexity if an amortized pattern is detected, or null if the complexity should be treated as worst-case. 



---
#### Method Analysis.AmortizedAnalyzer.AnalyzeOperationSequence(System.Collections.Generic.IReadOnlyList{System.ValueTuple{Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,System.Int32}})

 Analyzes a sequence of operations for aggregate amortized complexity. 



---
#### Method Analysis.AmortizedAnalyzer.DetectDoublingResizePattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects the doubling resize pattern common in dynamic arrays. Pattern: if (count == capacity) resize to capacity * 2 



---
#### Method Analysis.AmortizedAnalyzer.DetectRehashPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects hash table rehash pattern. Pattern: if (load > threshold) rehash to larger table 



---
#### Method Analysis.AmortizedAnalyzer.DetectBinaryCounterPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects binary counter increment pattern. Pattern: while (bit[i] == 1) flip to 0; flip next to 1 



---
#### Method Analysis.AmortizedAnalyzer.DetectUnionFindPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Union-Find pattern with path compression. Pattern: recursive Find with _parent[x] = Find(_parent[x]) 



---
#### Method Analysis.AmortizedAnalyzer.DetectMultipopPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects multipop stack pattern. Pattern: pop k items in a loop 



---
## Type Analysis.AmortizedAnalysisExtensions

 Extends RoslynComplexityExtractor with amortized analysis capability. 



---
#### Method Analysis.AmortizedAnalysisExtensions.AnalyzeWithAmortization(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Analyzes a method with amortized complexity detection. Returns AmortizedComplexity if a pattern is detected, otherwise falls back to worst-case. 



---
#### Method Analysis.AmortizedAnalysisExtensions.AnalyzeLoopWithAmortization(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Analyzes a loop containing BCL calls with amortized complexity. 



---
## Type Analysis.AnalysisContext

 Context for complexity analysis, providing access to semantic model and scope information. 



---
#### Property Analysis.AnalysisContext.SemanticModel

 The semantic model for the current syntax tree. 



---
#### Property Analysis.AnalysisContext.CurrentMethod

 The current method being analyzed (if any). 



---
#### Property Analysis.AnalysisContext.VariableMap

 Variables in scope with their complexity interpretations. 



---
#### Property Analysis.AnalysisContext.LoopBounds

 Known loop variables and their bounds. 



---
#### Property Analysis.AnalysisContext.CallGraph

 Call graph for inter-procedural analysis. 



---
#### Property Analysis.AnalysisContext.AnalyzeRecursion

 Whether to analyze recursion. 



---
#### Property Analysis.AnalysisContext.MaxCallDepth

 Maximum recursion depth for inter-procedural analysis. 



---
#### Property Analysis.AnalysisContext.CanonicalVarCounter

 Counter for generating canonical variable names (n, m, k, ...). 



---
#### Field Analysis.AnalysisContext.CanonicalNames

 Canonical variable name sequence for clean Big-O notation. 



---
#### Method Analysis.AnalysisContext.GetNextCanonicalName

 Gets the next canonical variable name. 



---
#### Method Analysis.AnalysisContext.WithMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Creates a child context for a nested scope. 



---
#### Method Analysis.AnalysisContext.WithVariable(Microsoft.CodeAnalysis.ISymbol,ComplexityAnalysis.Core.Complexity.Variable)

 Adds a variable to the context. 



---
#### Method Analysis.AnalysisContext.WithLoopBound(Microsoft.CodeAnalysis.ISymbol,ComplexityAnalysis.Roslyn.Analysis.LoopBound)

 Adds a loop bound to the context. 



---
#### Method Analysis.AnalysisContext.GetVariable(Microsoft.CodeAnalysis.ISymbol)

 Gets the complexity variable for a symbol, if known. 



---
#### Method Analysis.AnalysisContext.GetLoopBound(Microsoft.CodeAnalysis.ISymbol)

 Gets the loop bound for a variable, if known. 



---
#### Method Analysis.AnalysisContext.InferParameterVariableWithContext(Microsoft.CodeAnalysis.IParameterSymbol)

 Infers the complexity variable for a parameter. Uses canonical variable names (n, m, etc.) for cleaner Big-O notation. Returns a tuple of (Variable, UpdatedContext) to track name allocation. 



---
#### Method Analysis.AnalysisContext.InferParameterVariable(Microsoft.CodeAnalysis.IParameterSymbol)

 Infers the complexity variable for a parameter. Uses canonical variable names (n, m, etc.) for cleaner Big-O notation. Note: This method doesn't track which names have been used; prefer InferParameterVariableWithContext. 



---
## Type Analysis.LoopBound

 Represents a loop iteration bound. 



---
#### Property Analysis.LoopBound.LowerBound

 The lower bound expression. 



---
#### Property Analysis.LoopBound.UpperBound

 The upper bound expression. 



---
#### Property Analysis.LoopBound.Step

 The step (increment/decrement) per iteration. 



---
#### Property Analysis.LoopBound.IsExact

 Whether the bound is exact or an estimate. 



---
#### Property Analysis.LoopBound.Pattern

 The type of iteration pattern. 



---
#### Property Analysis.LoopBound.IterationCount

 Computes the number of iterations. 



---
#### Method Analysis.LoopBound.ZeroToN(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple 0 to n bound. 



---
#### Method Analysis.LoopBound.Logarithmic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a logarithmic bound (i *= 2 or i /= 2). 



---
## Type Analysis.IterationPattern

 Types of iteration patterns. 



---
#### Field Analysis.IterationPattern.Linear

 Linear iteration: i++, i--, i += k. 



---
#### Field Analysis.IterationPattern.Logarithmic

 Logarithmic iteration: i *= k, i /= k. 



---
#### Field Analysis.IterationPattern.Quadratic

 Quadratic iteration: dependent on another loop. 



---
#### Field Analysis.IterationPattern.Unknown

 Unknown pattern. 



---
## Type Analysis.CallGraph

 Represents a call graph for inter-procedural analysis. 



---
#### Method Analysis.CallGraph.AddMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Registers a method in the call graph (even if it has no calls). 



---
#### Method Analysis.CallGraph.AddCall(Microsoft.CodeAnalysis.IMethodSymbol,Microsoft.CodeAnalysis.IMethodSymbol)

 Adds a call edge from caller to callee. 



---
#### Method Analysis.CallGraph.GetCallees(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets all methods called by the given method. 



---
#### Method Analysis.CallGraph.GetCallers(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets all methods that call the given method. 



---
#### Method Analysis.CallGraph.IsRecursive(Microsoft.CodeAnalysis.IMethodSymbol)

 Checks if the method is recursive (directly or indirectly). 



---
#### Method Analysis.CallGraph.IsReachable(Microsoft.CodeAnalysis.IMethodSymbol,Microsoft.CodeAnalysis.IMethodSymbol,System.Collections.Generic.HashSet{Microsoft.CodeAnalysis.IMethodSymbol})

 Checks if there's a path from source to target. 



---
#### Method Analysis.CallGraph.SetComplexity(Microsoft.CodeAnalysis.IMethodSymbol,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Sets the computed complexity for a method. 



---
#### Method Analysis.CallGraph.GetComplexity(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets the computed complexity for a method, if available. 



---
#### Property Analysis.CallGraph.AllMethods

 Gets all methods in the call graph. 



---
#### Method Analysis.CallGraph.TopologicalSort

 Gets methods in topological order (callees before callers). Returns null if there's a cycle. 



---
#### Method Analysis.CallGraph.FindCycles

 Finds all cycles (strongly connected components with more than one node) in the call graph. Uses Tarjan's algorithm for O(V+E) complexity. 



---
## Type Analysis.CallGraphBuilder

 Builds a call graph from Roslyn compilation for inter-procedural analysis. 



---
#### Method Analysis.CallGraphBuilder.Build

 Builds the complete call graph from the compilation. 



---
#### Method Analysis.CallGraphBuilder.BuildForMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Builds a call graph for a single method and its transitive callees. 



---
#### Method Analysis.CallGraphBuilder.FindStronglyConnectedComponents

 Detects strongly connected components (SCCs) for handling mutual recursion. 



---
## Type Analysis.CallGraphBuilder.CallGraphWalker

 Walker that builds the complete call graph. 



---
## Type Analysis.CallGraphBuilder.MethodCallWalker

 Walker that finds all methods called from a specific method. 



---
## Type Analysis.MethodCallInfo

 Analysis result for a method including its call context. 



---
#### Property Analysis.MethodCallInfo.Method

 The method being called. 



---
#### Property Analysis.MethodCallInfo.Invocation

 The invocation syntax. 



---
#### Property Analysis.MethodCallInfo.Arguments

 Arguments passed to the method. 



---
#### Property Analysis.MethodCallInfo.IsRecursive

 Whether this is a recursive call. 



---
#### Property Analysis.MethodCallInfo.Caller

 The containing method. 



---
## Type Analysis.ArgumentInfo

 Information about a method argument. 



---
#### Property Analysis.ArgumentInfo.Parameter

 The parameter this argument corresponds to. 



---
#### Property Analysis.ArgumentInfo.Expression

 The argument expression. 



---
#### Property Analysis.ArgumentInfo.ComplexityVariable

 The complexity variable associated with this argument (if known). 



---
#### Property Analysis.ArgumentInfo.Relation

 How the argument relates to the caller's parameter (if derivable). 



---
#### Property Analysis.ArgumentInfo.ScaleFactor

 The scale factor if this is a scaled argument (e.g., n/2 has scale 0.5). 



---
## Type Analysis.ArgumentRelation

 Relationship between caller's parameter and callee's argument. 



---
#### Field Analysis.ArgumentRelation.Unknown

 Unknown relationship. 



---
#### Field Analysis.ArgumentRelation.Direct

 Direct pass-through (same variable). 



---
#### Field Analysis.ArgumentRelation.Scaled

 Scaled version (e.g., n/2, n-1). 



---
#### Field Analysis.ArgumentRelation.Derived

 Derived from multiple variables. 



---
#### Field Analysis.ArgumentRelation.Constant

 Constant value. 



---
## Type Analysis.CallGraphExtensions

 Extension methods for call graph analysis. 



---
#### Method Analysis.CallGraphExtensions.FindRecursiveMethods(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Finds all recursive methods in the call graph. 



---
#### Method Analysis.CallGraphExtensions.FindMaxCallDepth(ComplexityAnalysis.Roslyn.Analysis.CallGraph,Microsoft.CodeAnalysis.IMethodSymbol)

 Finds the longest call chain from a method. 



---
#### Method Analysis.CallGraphExtensions.FindEntryPoints(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Gets methods that have no callers (entry points). 



---
#### Method Analysis.CallGraphExtensions.FindLeafMethods(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Gets methods that have no callees (leaf methods). 



---
## Type Analysis.ControlFlowAnalysis

 Builds and analyzes control flow graphs for complexity analysis. 



---
#### Method Analysis.ControlFlowAnalysis.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes the control flow of a method body. 



---
#### Method Analysis.ControlFlowAnalysis.BuildControlFlowGraph(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Builds a simplified control flow graph. 



---
#### Method Analysis.ControlFlowAnalysis.IsReducible(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Checks if the CFG is reducible (has structured control flow). 



---
#### Method Analysis.ControlFlowAnalysis.ComputeLoopNestingDepth(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes the maximum loop nesting depth. Uses both CFG-based analysis and AST-based fallback for accuracy. 



---
#### Method Analysis.ControlFlowAnalysis.ComputeLoopNestingDepthFromSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Computes loop nesting depth directly from AST (more reliable than CFG analysis). 



---
#### Method Analysis.ControlFlowAnalysis.ComputeCyclomaticComplexity(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes cyclomatic complexity: E - N + 2P 



---
#### Method Analysis.ControlFlowAnalysis.ComputeBranchingFactor(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes the average branching factor. 



---
## Type Analysis.ControlFlowAnalysis.ManualCFGBuilder

 Manual CFG builder for when Roslyn's CFG is unavailable. 



---
## Type Analysis.ControlFlowResult

 Result of control flow analysis. 



---
#### Property Analysis.ControlFlowResult.Success

 Whether the analysis was successful. 



---
#### Property Analysis.ControlFlowResult.Graph

 The control flow graph. 



---
#### Property Analysis.ControlFlowResult.IsReducible

 Whether the CFG is reducible (structured control flow). 



---
#### Property Analysis.ControlFlowResult.LoopNestingDepth

 Maximum loop nesting depth. 



---
#### Property Analysis.ControlFlowResult.CyclomaticComplexity

 Cyclomatic complexity (E - N + 2P). 



---
#### Property Analysis.ControlFlowResult.BranchingFactor

 Average branching factor. 



---
#### Property Analysis.ControlFlowResult.ErrorMessage

 Error message if analysis failed. 



---
## Type Analysis.SimplifiedCFG

 Simplified control flow graph representation. 



---
#### Property Analysis.SimplifiedCFG.EntryBlock

 The entry block. 



---
#### Property Analysis.SimplifiedCFG.ExitBlock

 The exit block. 



---
#### Property Analysis.SimplifiedCFG.Blocks

 All basic blocks. 



---
#### Property Analysis.SimplifiedCFG.Edges

 All edges between blocks. 



---
#### Method Analysis.SimplifiedCFG.GetSuccessors(ComplexityAnalysis.Roslyn.Analysis.CFGBlock)

 Gets successors of a block. 



---
#### Method Analysis.SimplifiedCFG.GetPredecessors(ComplexityAnalysis.Roslyn.Analysis.CFGBlock)

 Gets predecessors of a block. 



---
#### Property Analysis.SimplifiedCFG.LoopHeaders

 Finds all loop headers. 



---
## Type Analysis.CFGBlock

 A basic block in the CFG. 



---
## Type Analysis.CFGBlockKind

 Kind of CFG block. 



---
## Type Analysis.CFGEdge

 An edge in the CFG. 



---
#### Method Analysis.CFGEdge.#ctor(System.Int32,System.Int32,ComplexityAnalysis.Roslyn.Analysis.CFGEdgeKind)

 An edge in the CFG. 



---
## Type Analysis.CFGEdgeKind

 Kind of CFG edge. 



---
## Type Analysis.LoopAnalyzer

 Analyzes loop constructs to extract iteration bounds and patterns. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeForLoop(Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a for loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeWhileLoop(Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a while loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeForeachLoop(Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a foreach loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeDoWhileLoop(Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a do-while loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.TraceLocalVariableDefinition(Microsoft.CodeAnalysis.ILocalSymbol,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Uses DFA to trace a local variable back to its definition and extract complexity. 



---
#### Method Analysis.LoopAnalyzer.ExtractDominantTermFromBinary(Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Extracts the dominant term from a binary expression like (n - i) or (array.Length - 1). For complexity analysis, subtraction and division don't change asymptotic behavior. 



---
## Type Analysis.LoopAnalyzer.IncrementFinder

 Helper walker to find increment patterns in while/do-while bodies. 



---
## Type Analysis.LoopAnalysisResult

 Result of loop analysis. 



---
#### Property Analysis.LoopAnalysisResult.Success

 Whether the analysis was successful. 



---
#### Property Analysis.LoopAnalysisResult.LoopVariable

 The loop variable symbol (if identified). 



---
#### Property Analysis.LoopAnalysisResult.Bound

 The computed loop bound. 



---
#### Property Analysis.LoopAnalysisResult.IterationCount

 The number of iterations as a complexity expression. 



---
#### Property Analysis.LoopAnalysisResult.Pattern

 The iteration pattern detected. 



---
#### Property Analysis.LoopAnalysisResult.Notes

 Additional notes about the analysis. 



---
#### Property Analysis.LoopAnalysisResult.ErrorMessage

 Error message if analysis failed. 



---
#### Method Analysis.LoopAnalysisResult.Unknown(System.String)

 Creates an unknown/failed result. 



---
## Type Analysis.BoundType

 Type of bound determined from analysis. 



---
#### Field Analysis.BoundType.Exact

 Exact bound known. 



---
#### Field Analysis.BoundType.Estimated

 Estimated bound (conservative). 



---
#### Field Analysis.BoundType.Unknown

 Unknown bound. 



---
## Type Analysis.MemoryAnalyzer

 Analyzes code to determine memory/space complexity. Detects: - Stack space from recursion depth - Heap allocations (arrays, collections, objects) - Auxiliary space usage - In-place algorithms - Tail recursion optimization potential 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method's memory complexity. 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeRecursion(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes recursion depth and patterns. 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeAllocations(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes heap allocations in a method. 



---
## Type Analysis.RecursionAnalysisResult

 Result of recursion analysis. 



---
## Type Analysis.RecursionPattern

 Patterns of recursion. 



---
#### Field Analysis.RecursionPattern.None

 No recursion. 



---
#### Field Analysis.RecursionPattern.Linear

 Single recursive call with n-1 or similar. 



---
#### Field Analysis.RecursionPattern.DivideByConstant

 Single recursive call with n/k. 



---
#### Field Analysis.RecursionPattern.DecrementByConstant

 Single recursive call decrementing by constant. 



---
#### Field Analysis.RecursionPattern.DivideAndConquer

 Two calls with halving (like merge sort). 



---
#### Field Analysis.RecursionPattern.TreeRecursion

 Two calls without halving (like Fibonacci). 



---
#### Field Analysis.RecursionPattern.Multiple

 More than two recursive calls. 



---
## Type Analysis.AllocationAnalysisResult

 Result of allocation analysis. 



---
## Type Analysis.MemoryAnalysisExtensions

 Extension methods for memory analysis. 



---
#### Method Analysis.MemoryAnalysisExtensions.AnalyzeComplete(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Analyzes a method for both time and space complexity. 



---
## Type Analysis.MutualRecursionDetector

 Detects mutual recursion patterns in code using call graph analysis. Mutual recursion occurs when two or more methods call each other in a cycle: - A() calls B(), B() calls A() - A() calls B(), B() calls C(), C() calls A() Detection uses Tarjan's algorithm for strongly connected components (SCCs). 



---
#### Method Analysis.MutualRecursionDetector.DetectCycles

 Detects all mutual recursion cycles in the call graph. 



---
#### Method Analysis.MutualRecursionDetector.IsInMutualRecursion(Microsoft.CodeAnalysis.IMethodSymbol)

 Checks if a specific method is part of a mutual recursion cycle. 



---
#### Method Analysis.MutualRecursionDetector.GetCycleContaining(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets the mutual recursion cycle containing a specific method, if any. 



---
#### Method Analysis.MutualRecursionDetector.AnalyzeCycle(System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Analyzes a strongly connected component to extract mutual recursion details. 



---
#### Method Analysis.MutualRecursionDetector.OrderCycle(System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Orders methods in a cycle by their call relationships. Returns methods in the order they call each other: A → B → C → A 



---
#### Method Analysis.MutualRecursionDetector.AnalyzeMethod(Microsoft.CodeAnalysis.IMethodSymbol,System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Analyzes a single method's contribution to the mutual recursion. 



---
## Type Analysis.MutualRecursionDetector.MethodBodyAnalyzer

 Analyzes method body to find cycle calls and non-recursive work. 



---
## Type Analysis.MutualRecursionCycle

 Represents a detected mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionCycle.Methods

 Information about each method in the cycle. 



---
#### Property Analysis.MutualRecursionCycle.CycleOrder

 The order of methods in the cycle (by name). 



---
#### Property Analysis.MutualRecursionCycle.Length

 Number of methods in the cycle. 



---
#### Method Analysis.MutualRecursionCycle.ToRecurrenceSystem(ComplexityAnalysis.Core.Complexity.Variable)

 Converts to a mutual recurrence system for solving. 



---
#### Method Analysis.MutualRecursionCycle.GetDescription

 Gets a human-readable description of the cycle. 



---
## Type Analysis.MutualRecursionMethodInfo

 Information about a single method in a mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionMethodInfo.Method

 The method symbol. 



---
#### Property Analysis.MutualRecursionMethodInfo.MethodName

 The method name. 



---
#### Property Analysis.MutualRecursionMethodInfo.NonRecursiveWork

 The non-recursive work done by this method. 



---
#### Property Analysis.MutualRecursionMethodInfo.CycleCalls

 Calls to other methods in the cycle. 



---
## Type Analysis.MutualRecursionCall

 Information about a call to another method in the mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionCall.TargetMethod

 The target method being called. 



---
#### Property Analysis.MutualRecursionCall.TargetMethodName

 The target method name. 



---
#### Property Analysis.MutualRecursionCall.Reduction

 How much the problem size is reduced (for subtraction patterns). 



---
#### Property Analysis.MutualRecursionCall.ScaleFactor

 Scale factor (for division patterns). 



---
#### Property Analysis.MutualRecursionCall.InvocationSyntax

 The invocation syntax. 



---
## Type Analysis.ParallelPatternAnalyzer

 Analyzes code patterns to detect parallel complexity scenarios. Detects patterns like: - Parallel.For / Parallel.ForEach (data parallelism) - PLINQ (AsParallel, parallel LINQ) - Task.Run / Task.WhenAll / Task.WhenAny (task parallelism) - async/await patterns - Parallel invoke 



---
#### Method Analysis.ParallelPatternAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for parallel complexity patterns. Returns a ParallelComplexity if a parallel pattern is detected, or null if no parallel pattern is found. 



---
#### Method Analysis.ParallelPatternAnalyzer.AnalyzeBlock(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)

 Analyzes a block of code for parallel patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectParallelForPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Parallel.For and Parallel.ForEach patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectPLINQPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects PLINQ patterns (AsParallel(), parallel LINQ). 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectTaskWhenPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Task.WhenAll / Task.WhenAny patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectTaskRunPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Task.Run patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectParallelInvokePattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Parallel.Invoke patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectAsyncAwaitPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects async/await patterns in async methods. 



---
## Type Analysis.ParallelAnalysisExtensions

 Extension methods for parallel pattern analysis. 



---
#### Method Analysis.ParallelAnalysisExtensions.AnalyzeWithParallelism(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method with parallel complexity detection. Returns ParallelComplexity if a pattern is detected, otherwise falls back to sequential analysis. 



---
#### Method Analysis.ParallelAnalysisExtensions.ContainsParallelPatterns(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Determines if a method contains any parallel patterns. 



---
#### Method Analysis.ParallelAnalysisExtensions.GetParallelPatternSummary(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Gets a summary of parallel patterns in a method. 



---
## Type Analysis.ParallelPatternSummary

 Summary of parallel patterns in a method. 



---
## Type Analysis.ProbabilisticAnalyzer

 Detects probabilistic patterns in code and produces probabilistic complexity analysis. 



---
#### Method Analysis.ProbabilisticAnalyzer.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method for probabilistic complexity patterns. 



---
#### Method Analysis.ProbabilisticAnalyzer.AnalyzeExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a specific expression for probabilistic characteristics. 



---
## Type Analysis.ProbabilisticAnalyzer.ProbabilisticPatternWalker

 Walker to find probabilistic patterns in code. 



---
## Type Analysis.ProbabilisticAnalysisResult

 Result of probabilistic complexity analysis. 



---
#### Property Analysis.ProbabilisticAnalysisResult.Success

 Whether the analysis found probabilistic patterns. 



---
#### Property Analysis.ProbabilisticAnalysisResult.ProbabilisticComplexity

 The combined probabilistic complexity. 



---
#### Property Analysis.ProbabilisticAnalysisResult.DetectedPatterns

 All detected probabilistic patterns. 



---
#### Property Analysis.ProbabilisticAnalysisResult.Notes

 Additional notes about the analysis. 



---
#### Property Analysis.ProbabilisticAnalysisResult.ErrorMessage

 Error message if analysis failed. 



---
#### Method Analysis.ProbabilisticAnalysisResult.NoProbabilisticPatterns

 Creates a result indicating no probabilistic patterns were found. 



---
## Type Analysis.ProbabilisticPattern

 A detected probabilistic pattern in code. 



---
#### Property Analysis.ProbabilisticPattern.Type

 The type of probabilistic pattern detected. 



---
#### Property Analysis.ProbabilisticPattern.Source

 The source of randomness in this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Distribution

 The probability distribution of this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Location

 The location in code where this pattern was detected. 



---
#### Property Analysis.ProbabilisticPattern.Description

 Description of the pattern. 



---
#### Property Analysis.ProbabilisticPattern.ExpectedComplexity

 The expected complexity for this pattern. 



---
#### Property Analysis.ProbabilisticPattern.WorstCaseComplexity

 The worst-case complexity for this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Assumptions

 Assumptions required for the expected complexity. 



---
## Type Analysis.ProbabilisticPatternType

 Types of probabilistic patterns that can be detected. 



---
#### Field Analysis.ProbabilisticPatternType.RandomNumberGeneration

 Random number generation (Random.Next, etc.) 



---
#### Field Analysis.ProbabilisticPatternType.HashFunction

 Hash function computation (GetHashCode, HashCode.Combine) 



---
#### Field Analysis.ProbabilisticPatternType.HashTableOperation

 Hash table operations (Dictionary, HashSet access) 



---
#### Field Analysis.ProbabilisticPatternType.Shuffle

 Random shuffle operations (Fisher-Yates, etc.) 



---
#### Field Analysis.ProbabilisticPatternType.PivotSelection

 Random pivot selection (QuickSort-like) 



---
#### Field Analysis.ProbabilisticPatternType.RandomizedSelection

 Randomized selection (Quickselect) 



---
#### Field Analysis.ProbabilisticPatternType.SkipList

 Skip list operations 



---
#### Field Analysis.ProbabilisticPatternType.BloomFilter

 Bloom filter operations 



---
#### Field Analysis.ProbabilisticPatternType.MonteCarlo

 Monte Carlo algorithm patterns 



---
#### Field Analysis.ProbabilisticPatternType.RandomizedLoop

 Loop with randomized iteration count 



---
#### Field Analysis.ProbabilisticPatternType.Other

 Other probabilistic pattern 



---
## Type Analysis.RoslynComplexityExtractor

 Extracts complexity expressions from C# source code using Roslyn. 



---
#### Property Analysis.RoslynComplexityExtractor.MethodResults

 Gets the results of method analysis. 



---
#### Property Analysis.RoslynComplexityExtractor.MethodComplexities

 Gets computed complexities for methods. 



---
#### Method Analysis.RoslynComplexityExtractor.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a single method and returns its complexity. 



---
#### Method Analysis.RoslynComplexityExtractor.TryDetectMutualRecursion(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.IMethodSymbol,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Attempts to detect and solve mutual recursion for a method. Returns null if the method is not part of a mutual recursion cycle. 



---
## Type Analysis.RoslynComplexityExtractorExtensions

 Extension methods for the complexity extractor. 



---
#### Method Analysis.RoslynComplexityExtractorExtensions.AnalyzeAllMethods(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.SyntaxNode)

 Analyzes all methods in a syntax tree. 



---
#### Method Analysis.RoslynComplexityExtractorExtensions.AnalyzeInTopologicalOrder(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.SyntaxNode,ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Analyzes methods in topological order based on call graph. 



---
## Type BCL.BCLComplexityMappings

 Central registry for Base Class Library (BCL) method complexity mappings. 



> This registry provides complexity information for .NET BCL methods, enabling accurate complexity analysis without requiring source code inspection. 

**Source Attribution Levels:**

**Level**: Meaning
- **Documented**: Official Microsoft documentation explicitly states complexity (MSDN)
- **Attested**: Verified through .NET runtime source code inspection (github.com/dotnet/runtime)
- **Empirical**: Measured through systematic benchmarking
- **Heuristic**: Conservative estimate based on algorithm analysis


**Coverage:**

- **System.Collections.Generic**: List, Dictionary, HashSet, SortedSet, Queue, Stack, LinkedList, PriorityQueue
- **System.Linq**: All Enumerable extension methods with deferred/immediate distinction
- **System.String**: String manipulation, search, comparison operations
- **System.Collections.Concurrent**: Thread-safe collections
- **System.Text.RegularExpressions**: Regex with backtracking warnings
- **System.Threading.Tasks**: TPL, Parallel, PLINQ operations


**Design Philosophy:** When in doubt, we overestimate complexity. False positives (warning about performance that's actually fine) are preferable to false negatives (missing actual performance problems). 

**Usage:**



######  code

```
    var mappings = BCLComplexityMappings.Instance;
    var complexity = mappings.GetComplexity("List`1", "Contains");
    // Returns: O(n) with source "MSDN: List<T>.Contains is O(n)"
```



**See also**: [`ComplexityMapping`](ComplexityMapping)

**See also**: [`ComplexitySource`](ComplexitySource)



---
#### Method BCL.BCLComplexityMappings.GetComplexity(System.String,System.String,System.Int32)

 Gets the complexity mapping for a method, or a conservative default. 



---
#### Method BCL.BCLComplexityMappings.Create

 Creates the complete BCL mappings registry. 



---
#### Method BCL.BCLComplexityMappings.AmortizedO1(ComplexityAnalysis.Core.Complexity.ComplexitySource)

 Creates an amortized O(1) complexity with O(n) worst case. Used for operations like List.Add, HashSet.Add, Dictionary.Add. 



---
## Type BCL.MethodSignature

 Signature for method lookup in the mappings registry. 



---
#### Method BCL.MethodSignature.#ctor(System.String,System.String,System.Int32)

 Signature for method lookup in the mappings registry. 



---
## Type BCL.ComplexityMapping

 A complexity mapping with source attribution and notes. 



---
#### Method BCL.ComplexityMapping.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexitySource,ComplexityAnalysis.Roslyn.BCL.ComplexityNotes,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 A complexity mapping with source attribution and notes. 



---
## Type BCL.ComplexityNotes

 Additional notes about complexity characteristics. 



---
#### Field BCL.ComplexityNotes.Amortized

Complexity is amortized (occasional expensive operations)



---
#### Field BCL.ComplexityNotes.DeferredExecution

LINQ deferred execution - O(1) to create, full cost on enumeration



---
#### Field BCL.ComplexityNotes.BacktrackingWarning

Regex backtracking warning - can be exponential



---
#### Field BCL.ComplexityNotes.InputDependent

Complexity depends on input characteristics



---
#### Field BCL.ComplexityNotes.ThreadSafe

Thread-safe but may have contention overhead



---
#### Field BCL.ComplexityNotes.Unknown

Unknown method - conservative estimate



---
#### Field BCL.ComplexityNotes.Probabilistic

Probabilistic complexity - expected vs worst case may differ



---
## Type Speculative.ComplexityContract

 Complexity contract information from attributes or XML docs. 



---
## Type Speculative.ComplexityContractReader

 Reads complexity contracts from: - [Complexity("O(n)")] attributes - XML documentation with complexity info 



---
#### Method Speculative.ComplexityContractReader.ReadContract(Microsoft.CodeAnalysis.IMethodSymbol)

 Reads complexity contract from a method symbol. 



---
#### Method Speculative.ComplexityContractReader.ParseComplexityString(System.String)

 Parses a complexity string like "O(n)", "O(n log n)", "O(n^2)". 



---
## Type Speculative.IncompleteCodeResult

 Result of incomplete code detection. 



---
## Type Speculative.IncompleteCodeDetector

 Detects incomplete code patterns: - throw new NotImplementedException() - throw new NotSupportedException() - TODO/FIXME/HACK comments - Empty method bodies 



---
#### Method Speculative.IncompleteCodeDetector.Detect(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects incomplete code patterns in a method. 



---
## Type Speculative.IncrementalComplexityAnalyzer

 Provides incremental complexity analysis for code being actively edited. Designed for real-time feedback in IDE scenarios where code may be incomplete or syntactically invalid during typing. Key features: - Parses incomplete/malformed syntax gracefully - Caches analysis results for unchanged code regions - Streams progress callbacks during analysis - Provides confidence-weighted estimates for partial constructs 



---
#### Method Speculative.IncrementalComplexityAnalyzer.#ctor(ComplexityAnalysis.Roslyn.Speculative.IOnlineAnalysisCallback,ComplexityAnalysis.Roslyn.Speculative.AnalysisOptions)

 Creates a new incremental analyzer with optional callback. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.AnalyzeAsync(System.String,System.Int32,System.Threading.CancellationToken)

 Analyzes code text incrementally, reporting progress via callbacks. Handles incomplete syntax gracefully. 

|Name | Description |
|-----|------|
|sourceText: |The current source text (may be incomplete)|
|position: |Caret position in the text|
|cancellationToken: |Cancellation token for async operation|


---
#### Method Speculative.IncrementalComplexityAnalyzer.AnalyzeMethodAsync(System.String,System.String,System.Threading.CancellationToken)

 Analyzes a specific method by name, useful for targeted analysis. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.GetCachedAnalysis(System.String)

 Gets cached analysis for a code region, or null if not cached. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.ClearCache

 Clears the analysis cache. 



---
## Type Speculative.AnalysisOptions

 Options for online analysis. 



---
#### Property Speculative.AnalysisOptions.Timeout

 Maximum time to spend on analysis before returning partial results. 



---
#### Property Speculative.AnalysisOptions.UseCache

 Whether to use cached results when available. 



---
#### Property Speculative.AnalysisOptions.MinConfidence

 Minimum confidence to report a result. 



---
#### Property Speculative.AnalysisOptions.MaxMethodsPerPass

 Maximum number of methods to analyze in one pass. 



---
## Type Speculative.OnlineAnalysisPhase

 Phases of online analysis. 



---
## Type Speculative.ScopeType

 Types of analysis scope. 



---
## Type Speculative.IncompleteReason

 Reasons for incomplete code. 



---
## Type Speculative.ParseResult

 Result of parsing with recovery. 



---
## Type Speculative.IncompleteNode

 An incomplete node in the syntax tree. 



---
## Type Speculative.AnalysisScope

 Analysis scope definition. 



---
## Type Speculative.FragmentAnalysisResult

 Result of analyzing a code fragment. 



---
## Type Speculative.MethodAnalysisSnapshot

 Snapshot of a method's complexity analysis. 



---
## Type Speculative.LoopSnapshot

 Snapshot of a loop's analysis. 



---
## Type Speculative.MethodComplexitySnapshot

 Per-method complexity snapshot in online results. 



---
## Type Speculative.ParseDiagnostic

 Parse diagnostic for reporting to UI. 



---
## Type Speculative.IncompleteRegion

 Region of incomplete code. 



---
## Type Speculative.CachedAnalysis

 Cached analysis result. 



---
## Type Speculative.OnlineAnalysisResult

 Overall result of online analysis. 



---
## Type Speculative.IOnlineAnalysisCallback

 Callback interface for online/incremental analysis progress. Implementations receive real-time updates during code analysis, suitable for IDE integration and live feedback. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnAnalysisStarted(System.Int32)

 Called when analysis begins. 

|Name | Description |
|-----|------|
|sourceLength: |Length of source text being analyzed.|


---
#### Method Speculative.IOnlineAnalysisCallback.OnPhaseStarted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisPhase)

 Called when an analysis phase begins. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnPhaseCompleted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisPhase)

 Called when an analysis phase completes. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnProgress(System.Int32,System.Int32,System.String)

 Called to report analysis progress. 

|Name | Description |
|-----|------|
|completed: |Number of items completed.|
|total: |Total number of items.|
|currentItem: |Name of current item being processed.|


---
#### Method Speculative.IOnlineAnalysisCallback.OnAnalysisCompleted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisResult,System.TimeSpan)

 Called when analysis completes successfully. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnError(System.Exception)

 Called when an error occurs during analysis. 



---
## Type Speculative.NullOnlineAnalysisCallback

 Null implementation that does nothing. 



---
## Type Speculative.ConsoleOnlineAnalysisCallback

 Console-based callback for debugging and testing. 



---
## Type Speculative.BufferedOnlineAnalysisCallback

 Callback that buffers events for later processing. Useful for testing and batch processing. 



---
## Type Speculative.AnalysisEvent

 Base class for analysis events. 



---
#### Method Speculative.AnalysisEvent.#ctor(System.DateTime)

 Base class for analysis events. 



---
## Type Speculative.CompositeOnlineAnalysisCallback

 Aggregates multiple callbacks into one. 



---
## Type Speculative.SpeculativeAnalysisResult

 Result of speculative analysis for incomplete or partial code. 



---
#### Property Speculative.SpeculativeAnalysisResult.Complexity

Best-effort complexity estimate.



---
#### Property Speculative.SpeculativeAnalysisResult.LowerBound

Lower bound complexity (what we know for certain).



---
#### Property Speculative.SpeculativeAnalysisResult.UpperBound

Upper bound complexity (conservative estimate).



---
#### Property Speculative.SpeculativeAnalysisResult.Confidence

Confidence in the result (0.0 to 1.0).



---
#### Property Speculative.SpeculativeAnalysisResult.IsIncomplete

Whether the code appears incomplete (NIE, TODO, etc.).



---
#### Property Speculative.SpeculativeAnalysisResult.IsStub

Whether the code appears to be a stub.



---
#### Property Speculative.SpeculativeAnalysisResult.HasTodoMarker

Whether the code contains TODO/FIXME markers.



---
#### Property Speculative.SpeculativeAnalysisResult.HasUncertainty

Whether there's unresolved uncertainty from abstract/interface calls.



---
#### Property Speculative.SpeculativeAnalysisResult.UsedContract

Whether a complexity contract was used.



---
#### Property Speculative.SpeculativeAnalysisResult.UncertaintySource

Source of uncertainty (e.g., "IProcessor.Process").



---
#### Property Speculative.SpeculativeAnalysisResult.DependsOn

Methods this analysis depends on (for uncertainty tracking).



---
#### Property Speculative.SpeculativeAnalysisResult.DetectedPatterns

Detected code patterns that inform the analysis.



---
#### Property Speculative.SpeculativeAnalysisResult.Explanation

Explanation of the analysis.



---
## Type Speculative.CodePattern

 Detected code pattern that informs speculative analysis. 



---
#### Field Speculative.CodePattern.ThrowsNotImplementedException

throw new NotImplementedException()



---
#### Field Speculative.CodePattern.ThrowsNotSupportedException

throw new NotSupportedException()



---
#### Field Speculative.CodePattern.HasTodoComment

Contains TODO/FIXME/HACK comment



---
#### Field Speculative.CodePattern.ReturnsDefault

Returns default/null/empty



---
#### Field Speculative.CodePattern.EmptyBody

Method body is empty or just returns



---
#### Field Speculative.CodePattern.CounterOnly

Only increments counter (mock pattern)



---
#### Field Speculative.CodePattern.ReturnsConstant

Returns constant value



---
#### Field Speculative.CodePattern.CallsAbstract

Calls abstract method



---
#### Field Speculative.CodePattern.CallsInterface

Calls interface method



---
#### Field Speculative.CodePattern.CallsVirtual

Calls virtual method that may be overridden



---
#### Field Speculative.CodePattern.HasComplexityAttribute

Has [Complexity] attribute



---
#### Field Speculative.CodePattern.HasComplexityXmlDoc

Has XML doc with complexity info



---
## Type Speculative.SpeculativeAnalyzer

 Analyzes partial, incomplete, or abstract code to produce speculative complexity estimates. This is Phase D of the analysis pipeline, handling: - Incomplete implementations (NotImplementedException, TODO) - Abstract method calls - Interface method calls - Stub detection - Complexity contracts (attributes, XML docs) 



---
#### Method Speculative.SpeculativeAnalyzer.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for speculative complexity, handling incomplete code. 



---
#### Method Speculative.SpeculativeAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.SyntaxTree,System.String)

 Analyzes a method by name in the compilation. 



---
## Type Speculative.StubDetectionResult

 Result of stub detection. 



---
## Type Speculative.StubDetector

 Detects stub implementations: - Returns default/null/empty - Counter-only implementations (mocks) - Returns constant value with no logic 



---
#### Method Speculative.StubDetector.Detect(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Detects if a method is a stub implementation. 



---
## Type Speculative.SyntaxFragmentAnalyzer

 Analyzes syntax fragments, including incomplete code during active editing. Provides best-effort complexity estimates with confidence values. 



---
#### Method Speculative.SyntaxFragmentAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,System.Boolean)

 Analyzes a method, handling incomplete syntax gracefully. 



---
#### Method Speculative.SyntaxFragmentAnalyzer.AnalyzeStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax)

 Analyzes a single statement, useful for incremental updates. 



---
## Type Speculative.StatementAnalysisResult

 Result of analyzing a single statement. 



---
## Type Speculative.UncertaintyResult

 Result of uncertainty tracking. 



---
## Type Speculative.UncertaintyTracker

 Tracks uncertainty from abstract, virtual, and interface method calls. When complexity depends on runtime polymorphism, we track the dependency rather than making potentially incorrect assumptions. 



---
#### Method Speculative.UncertaintyTracker.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for uncertainty from polymorphic calls. 



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



# ComplexityAnalysis.Roslyn #

## Type Analysis.AmortizedAnalyzer

 Analyzes code patterns to detect amortized complexity scenarios. Detects patterns like: - Dynamic array resizing (doubling strategy) - Hash table rehashing - Binary counter increment - Stack with multipop - Union-Find with path compression 



---
#### Method Analysis.AmortizedAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for amortized complexity patterns. Returns an AmortizedComplexity if an amortized pattern is detected, or null if the complexity should be treated as worst-case. 



---
#### Method Analysis.AmortizedAnalyzer.AnalyzeOperationSequence(System.Collections.Generic.IReadOnlyList{System.ValueTuple{Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,System.Int32}})

 Analyzes a sequence of operations for aggregate amortized complexity. 



---
#### Method Analysis.AmortizedAnalyzer.DetectDoublingResizePattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects the doubling resize pattern common in dynamic arrays. Pattern: if (count == capacity) resize to capacity * 2 



---
#### Method Analysis.AmortizedAnalyzer.DetectRehashPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects hash table rehash pattern. Pattern: if (load > threshold) rehash to larger table 



---
#### Method Analysis.AmortizedAnalyzer.DetectBinaryCounterPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects binary counter increment pattern. Pattern: while (bit[i] == 1) flip to 0; flip next to 1 



---
#### Method Analysis.AmortizedAnalyzer.DetectUnionFindPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Union-Find pattern with path compression. Pattern: recursive Find with _parent[x] = Find(_parent[x]) 



---
#### Method Analysis.AmortizedAnalyzer.DetectMultipopPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects multipop stack pattern. Pattern: pop k items in a loop 



---
## Type Analysis.AmortizedAnalysisExtensions

 Extends RoslynComplexityExtractor with amortized analysis capability. 



---
#### Method Analysis.AmortizedAnalysisExtensions.AnalyzeWithAmortization(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Analyzes a method with amortized complexity detection. Returns AmortizedComplexity if a pattern is detected, otherwise falls back to worst-case. 



---
#### Method Analysis.AmortizedAnalysisExtensions.AnalyzeLoopWithAmortization(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexityExpression,System.Boolean)

 Analyzes a loop containing BCL calls with amortized complexity. 



---
## Type Analysis.AnalysisContext

 Context for complexity analysis, providing access to semantic model and scope information. 



---
#### Property Analysis.AnalysisContext.SemanticModel

 The semantic model for the current syntax tree. 



---
#### Property Analysis.AnalysisContext.CurrentMethod

 The current method being analyzed (if any). 



---
#### Property Analysis.AnalysisContext.VariableMap

 Variables in scope with their complexity interpretations. 



---
#### Property Analysis.AnalysisContext.LoopBounds

 Known loop variables and their bounds. 



---
#### Property Analysis.AnalysisContext.CallGraph

 Call graph for inter-procedural analysis. 



---
#### Property Analysis.AnalysisContext.AnalyzeRecursion

 Whether to analyze recursion. 



---
#### Property Analysis.AnalysisContext.MaxCallDepth

 Maximum recursion depth for inter-procedural analysis. 



---
#### Property Analysis.AnalysisContext.CanonicalVarCounter

 Counter for generating canonical variable names (n, m, k, ...). 



---
#### Field Analysis.AnalysisContext.CanonicalNames

 Canonical variable name sequence for clean Big-O notation. 



---
#### Method Analysis.AnalysisContext.GetNextCanonicalName

 Gets the next canonical variable name. 



---
#### Method Analysis.AnalysisContext.WithMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Creates a child context for a nested scope. 



---
#### Method Analysis.AnalysisContext.WithVariable(Microsoft.CodeAnalysis.ISymbol,ComplexityAnalysis.Core.Complexity.Variable)

 Adds a variable to the context. 



---
#### Method Analysis.AnalysisContext.WithLoopBound(Microsoft.CodeAnalysis.ISymbol,ComplexityAnalysis.Roslyn.Analysis.LoopBound)

 Adds a loop bound to the context. 



---
#### Method Analysis.AnalysisContext.GetVariable(Microsoft.CodeAnalysis.ISymbol)

 Gets the complexity variable for a symbol, if known. 



---
#### Method Analysis.AnalysisContext.GetLoopBound(Microsoft.CodeAnalysis.ISymbol)

 Gets the loop bound for a variable, if known. 



---
#### Method Analysis.AnalysisContext.InferParameterVariableWithContext(Microsoft.CodeAnalysis.IParameterSymbol)

 Infers the complexity variable for a parameter. Uses canonical variable names (n, m, etc.) for cleaner Big-O notation. Returns a tuple of (Variable, UpdatedContext) to track name allocation. 



---
#### Method Analysis.AnalysisContext.InferParameterVariable(Microsoft.CodeAnalysis.IParameterSymbol)

 Infers the complexity variable for a parameter. Uses canonical variable names (n, m, etc.) for cleaner Big-O notation. Note: This method doesn't track which names have been used; prefer InferParameterVariableWithContext. 



---
## Type Analysis.LoopBound

 Represents a loop iteration bound. 



---
#### Property Analysis.LoopBound.LowerBound

 The lower bound expression. 



---
#### Property Analysis.LoopBound.UpperBound

 The upper bound expression. 



---
#### Property Analysis.LoopBound.Step

 The step (increment/decrement) per iteration. 



---
#### Property Analysis.LoopBound.IsExact

 Whether the bound is exact or an estimate. 



---
#### Property Analysis.LoopBound.Pattern

 The type of iteration pattern. 



---
#### Property Analysis.LoopBound.IterationCount

 Computes the number of iterations. 



---
#### Method Analysis.LoopBound.ZeroToN(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a simple 0 to n bound. 



---
#### Method Analysis.LoopBound.Logarithmic(ComplexityAnalysis.Core.Complexity.Variable)

 Creates a logarithmic bound (i *= 2 or i /= 2). 



---
## Type Analysis.IterationPattern

 Types of iteration patterns. 



---
#### Field Analysis.IterationPattern.Linear

 Linear iteration: i++, i--, i += k. 



---
#### Field Analysis.IterationPattern.Logarithmic

 Logarithmic iteration: i *= k, i /= k. 



---
#### Field Analysis.IterationPattern.Quadratic

 Quadratic iteration: dependent on another loop. 



---
#### Field Analysis.IterationPattern.Unknown

 Unknown pattern. 



---
## Type Analysis.CallGraph

 Represents a call graph for inter-procedural analysis. 



---
#### Method Analysis.CallGraph.AddMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Registers a method in the call graph (even if it has no calls). 



---
#### Method Analysis.CallGraph.AddCall(Microsoft.CodeAnalysis.IMethodSymbol,Microsoft.CodeAnalysis.IMethodSymbol)

 Adds a call edge from caller to callee. 



---
#### Method Analysis.CallGraph.GetCallees(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets all methods called by the given method. 



---
#### Method Analysis.CallGraph.GetCallers(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets all methods that call the given method. 



---
#### Method Analysis.CallGraph.IsRecursive(Microsoft.CodeAnalysis.IMethodSymbol)

 Checks if the method is recursive (directly or indirectly). 



---
#### Method Analysis.CallGraph.IsReachable(Microsoft.CodeAnalysis.IMethodSymbol,Microsoft.CodeAnalysis.IMethodSymbol,System.Collections.Generic.HashSet{Microsoft.CodeAnalysis.IMethodSymbol})

 Checks if there's a path from source to target. 



---
#### Method Analysis.CallGraph.SetComplexity(Microsoft.CodeAnalysis.IMethodSymbol,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Sets the computed complexity for a method. 



---
#### Method Analysis.CallGraph.GetComplexity(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets the computed complexity for a method, if available. 



---
#### Property Analysis.CallGraph.AllMethods

 Gets all methods in the call graph. 



---
#### Method Analysis.CallGraph.TopologicalSort

 Gets methods in topological order (callees before callers). Returns null if there's a cycle. 



---
#### Method Analysis.CallGraph.FindCycles

 Finds all cycles (strongly connected components with more than one node) in the call graph. Uses Tarjan's algorithm for O(V+E) complexity. 



---
## Type Analysis.CallGraphBuilder

 Builds a call graph from Roslyn compilation for inter-procedural analysis. 



---
#### Method Analysis.CallGraphBuilder.Build

 Builds the complete call graph from the compilation. 



---
#### Method Analysis.CallGraphBuilder.BuildForMethod(Microsoft.CodeAnalysis.IMethodSymbol)

 Builds a call graph for a single method and its transitive callees. 



---
#### Method Analysis.CallGraphBuilder.FindStronglyConnectedComponents

 Detects strongly connected components (SCCs) for handling mutual recursion. 



---
## Type Analysis.CallGraphBuilder.CallGraphWalker

 Walker that builds the complete call graph. 



---
## Type Analysis.CallGraphBuilder.MethodCallWalker

 Walker that finds all methods called from a specific method. 



---
## Type Analysis.MethodCallInfo

 Analysis result for a method including its call context. 



---
#### Property Analysis.MethodCallInfo.Method

 The method being called. 



---
#### Property Analysis.MethodCallInfo.Invocation

 The invocation syntax. 



---
#### Property Analysis.MethodCallInfo.Arguments

 Arguments passed to the method. 



---
#### Property Analysis.MethodCallInfo.IsRecursive

 Whether this is a recursive call. 



---
#### Property Analysis.MethodCallInfo.Caller

 The containing method. 



---
## Type Analysis.ArgumentInfo

 Information about a method argument. 



---
#### Property Analysis.ArgumentInfo.Parameter

 The parameter this argument corresponds to. 



---
#### Property Analysis.ArgumentInfo.Expression

 The argument expression. 



---
#### Property Analysis.ArgumentInfo.ComplexityVariable

 The complexity variable associated with this argument (if known). 



---
#### Property Analysis.ArgumentInfo.Relation

 How the argument relates to the caller's parameter (if derivable). 



---
#### Property Analysis.ArgumentInfo.ScaleFactor

 The scale factor if this is a scaled argument (e.g., n/2 has scale 0.5). 



---
## Type Analysis.ArgumentRelation

 Relationship between caller's parameter and callee's argument. 



---
#### Field Analysis.ArgumentRelation.Unknown

 Unknown relationship. 



---
#### Field Analysis.ArgumentRelation.Direct

 Direct pass-through (same variable). 



---
#### Field Analysis.ArgumentRelation.Scaled

 Scaled version (e.g., n/2, n-1). 



---
#### Field Analysis.ArgumentRelation.Derived

 Derived from multiple variables. 



---
#### Field Analysis.ArgumentRelation.Constant

 Constant value. 



---
## Type Analysis.CallGraphExtensions

 Extension methods for call graph analysis. 



---
#### Method Analysis.CallGraphExtensions.FindRecursiveMethods(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Finds all recursive methods in the call graph. 



---
#### Method Analysis.CallGraphExtensions.FindMaxCallDepth(ComplexityAnalysis.Roslyn.Analysis.CallGraph,Microsoft.CodeAnalysis.IMethodSymbol)

 Finds the longest call chain from a method. 



---
#### Method Analysis.CallGraphExtensions.FindEntryPoints(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Gets methods that have no callers (entry points). 



---
#### Method Analysis.CallGraphExtensions.FindLeafMethods(ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Gets methods that have no callees (leaf methods). 



---
## Type Analysis.ControlFlowAnalysis

 Builds and analyzes control flow graphs for complexity analysis. 



---
#### Method Analysis.ControlFlowAnalysis.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes the control flow of a method body. 



---
#### Method Analysis.ControlFlowAnalysis.BuildControlFlowGraph(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Builds a simplified control flow graph. 



---
#### Method Analysis.ControlFlowAnalysis.IsReducible(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Checks if the CFG is reducible (has structured control flow). 



---
#### Method Analysis.ControlFlowAnalysis.ComputeLoopNestingDepth(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes the maximum loop nesting depth. Uses both CFG-based analysis and AST-based fallback for accuracy. 



---
#### Method Analysis.ControlFlowAnalysis.ComputeLoopNestingDepthFromSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Computes loop nesting depth directly from AST (more reliable than CFG analysis). 



---
#### Method Analysis.ControlFlowAnalysis.ComputeCyclomaticComplexity(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes cyclomatic complexity: E - N + 2P 



---
#### Method Analysis.ControlFlowAnalysis.ComputeBranchingFactor(ComplexityAnalysis.Roslyn.Analysis.SimplifiedCFG)

 Computes the average branching factor. 



---
## Type Analysis.ControlFlowAnalysis.ManualCFGBuilder

 Manual CFG builder for when Roslyn's CFG is unavailable. 



---
## Type Analysis.ControlFlowResult

 Result of control flow analysis. 



---
#### Property Analysis.ControlFlowResult.Success

 Whether the analysis was successful. 



---
#### Property Analysis.ControlFlowResult.Graph

 The control flow graph. 



---
#### Property Analysis.ControlFlowResult.IsReducible

 Whether the CFG is reducible (structured control flow). 



---
#### Property Analysis.ControlFlowResult.LoopNestingDepth

 Maximum loop nesting depth. 



---
#### Property Analysis.ControlFlowResult.CyclomaticComplexity

 Cyclomatic complexity (E - N + 2P). 



---
#### Property Analysis.ControlFlowResult.BranchingFactor

 Average branching factor. 



---
#### Property Analysis.ControlFlowResult.ErrorMessage

 Error message if analysis failed. 



---
## Type Analysis.SimplifiedCFG

 Simplified control flow graph representation. 



---
#### Property Analysis.SimplifiedCFG.EntryBlock

 The entry block. 



---
#### Property Analysis.SimplifiedCFG.ExitBlock

 The exit block. 



---
#### Property Analysis.SimplifiedCFG.Blocks

 All basic blocks. 



---
#### Property Analysis.SimplifiedCFG.Edges

 All edges between blocks. 



---
#### Method Analysis.SimplifiedCFG.GetSuccessors(ComplexityAnalysis.Roslyn.Analysis.CFGBlock)

 Gets successors of a block. 



---
#### Method Analysis.SimplifiedCFG.GetPredecessors(ComplexityAnalysis.Roslyn.Analysis.CFGBlock)

 Gets predecessors of a block. 



---
#### Property Analysis.SimplifiedCFG.LoopHeaders

 Finds all loop headers. 



---
## Type Analysis.CFGBlock

 A basic block in the CFG. 



---
## Type Analysis.CFGBlockKind

 Kind of CFG block. 



---
## Type Analysis.CFGEdge

 An edge in the CFG. 



---
#### Method Analysis.CFGEdge.#ctor(System.Int32,System.Int32,ComplexityAnalysis.Roslyn.Analysis.CFGEdgeKind)

 An edge in the CFG. 



---
## Type Analysis.CFGEdgeKind

 Kind of CFG edge. 



---
## Type Analysis.LoopAnalyzer

 Analyzes loop constructs to extract iteration bounds and patterns. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeForLoop(Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a for loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeWhileLoop(Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a while loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeForeachLoop(Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a foreach loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.AnalyzeDoWhileLoop(Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a do-while loop to extract its iteration bound. 



---
#### Method Analysis.LoopAnalyzer.TraceLocalVariableDefinition(Microsoft.CodeAnalysis.ILocalSymbol,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Uses DFA to trace a local variable back to its definition and extract complexity. 



---
#### Method Analysis.LoopAnalyzer.ExtractDominantTermFromBinary(Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Extracts the dominant term from a binary expression like (n - i) or (array.Length - 1). For complexity analysis, subtraction and division don't change asymptotic behavior. 



---
## Type Analysis.LoopAnalyzer.IncrementFinder

 Helper walker to find increment patterns in while/do-while bodies. 



---
## Type Analysis.LoopAnalysisResult

 Result of loop analysis. 



---
#### Property Analysis.LoopAnalysisResult.Success

 Whether the analysis was successful. 



---
#### Property Analysis.LoopAnalysisResult.LoopVariable

 The loop variable symbol (if identified). 



---
#### Property Analysis.LoopAnalysisResult.Bound

 The computed loop bound. 



---
#### Property Analysis.LoopAnalysisResult.IterationCount

 The number of iterations as a complexity expression. 



---
#### Property Analysis.LoopAnalysisResult.Pattern

 The iteration pattern detected. 



---
#### Property Analysis.LoopAnalysisResult.Notes

 Additional notes about the analysis. 



---
#### Property Analysis.LoopAnalysisResult.ErrorMessage

 Error message if analysis failed. 



---
#### Method Analysis.LoopAnalysisResult.Unknown(System.String)

 Creates an unknown/failed result. 



---
## Type Analysis.BoundType

 Type of bound determined from analysis. 



---
#### Field Analysis.BoundType.Exact

 Exact bound known. 



---
#### Field Analysis.BoundType.Estimated

 Estimated bound (conservative). 



---
#### Field Analysis.BoundType.Unknown

 Unknown bound. 



---
## Type Analysis.MemoryAnalyzer

 Analyzes code to determine memory/space complexity. Detects: - Stack space from recursion depth - Heap allocations (arrays, collections, objects) - Auxiliary space usage - In-place algorithms - Tail recursion optimization potential 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method's memory complexity. 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeRecursion(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes recursion depth and patterns. 



---
#### Method Analysis.MemoryAnalyzer.AnalyzeAllocations(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes heap allocations in a method. 



---
## Type Analysis.RecursionAnalysisResult

 Result of recursion analysis. 



---
## Type Analysis.RecursionPattern

 Patterns of recursion. 



---
#### Field Analysis.RecursionPattern.None

 No recursion. 



---
#### Field Analysis.RecursionPattern.Linear

 Single recursive call with n-1 or similar. 



---
#### Field Analysis.RecursionPattern.DivideByConstant

 Single recursive call with n/k. 



---
#### Field Analysis.RecursionPattern.DecrementByConstant

 Single recursive call decrementing by constant. 



---
#### Field Analysis.RecursionPattern.DivideAndConquer

 Two calls with halving (like merge sort). 



---
#### Field Analysis.RecursionPattern.TreeRecursion

 Two calls without halving (like Fibonacci). 



---
#### Field Analysis.RecursionPattern.Multiple

 More than two recursive calls. 



---
## Type Analysis.AllocationAnalysisResult

 Result of allocation analysis. 



---
## Type Analysis.MemoryAnalysisExtensions

 Extension methods for memory analysis. 



---
#### Method Analysis.MemoryAnalysisExtensions.AnalyzeComplete(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Analyzes a method for both time and space complexity. 



---
## Type Analysis.MutualRecursionDetector

 Detects mutual recursion patterns in code using call graph analysis. Mutual recursion occurs when two or more methods call each other in a cycle: - A() calls B(), B() calls A() - A() calls B(), B() calls C(), C() calls A() Detection uses Tarjan's algorithm for strongly connected components (SCCs). 



---
#### Method Analysis.MutualRecursionDetector.DetectCycles

 Detects all mutual recursion cycles in the call graph. 



---
#### Method Analysis.MutualRecursionDetector.IsInMutualRecursion(Microsoft.CodeAnalysis.IMethodSymbol)

 Checks if a specific method is part of a mutual recursion cycle. 



---
#### Method Analysis.MutualRecursionDetector.GetCycleContaining(Microsoft.CodeAnalysis.IMethodSymbol)

 Gets the mutual recursion cycle containing a specific method, if any. 



---
#### Method Analysis.MutualRecursionDetector.AnalyzeCycle(System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Analyzes a strongly connected component to extract mutual recursion details. 



---
#### Method Analysis.MutualRecursionDetector.OrderCycle(System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Orders methods in a cycle by their call relationships. Returns methods in the order they call each other: A → B → C → A 



---
#### Method Analysis.MutualRecursionDetector.AnalyzeMethod(Microsoft.CodeAnalysis.IMethodSymbol,System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.IMethodSymbol})

 Analyzes a single method's contribution to the mutual recursion. 



---
## Type Analysis.MutualRecursionDetector.MethodBodyAnalyzer

 Analyzes method body to find cycle calls and non-recursive work. 



---
## Type Analysis.MutualRecursionCycle

 Represents a detected mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionCycle.Methods

 Information about each method in the cycle. 



---
#### Property Analysis.MutualRecursionCycle.CycleOrder

 The order of methods in the cycle (by name). 



---
#### Property Analysis.MutualRecursionCycle.Length

 Number of methods in the cycle. 



---
#### Method Analysis.MutualRecursionCycle.ToRecurrenceSystem(ComplexityAnalysis.Core.Complexity.Variable)

 Converts to a mutual recurrence system for solving. 



---
#### Method Analysis.MutualRecursionCycle.GetDescription

 Gets a human-readable description of the cycle. 



---
## Type Analysis.MutualRecursionMethodInfo

 Information about a single method in a mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionMethodInfo.Method

 The method symbol. 



---
#### Property Analysis.MutualRecursionMethodInfo.MethodName

 The method name. 



---
#### Property Analysis.MutualRecursionMethodInfo.NonRecursiveWork

 The non-recursive work done by this method. 



---
#### Property Analysis.MutualRecursionMethodInfo.CycleCalls

 Calls to other methods in the cycle. 



---
## Type Analysis.MutualRecursionCall

 Information about a call to another method in the mutual recursion cycle. 



---
#### Property Analysis.MutualRecursionCall.TargetMethod

 The target method being called. 



---
#### Property Analysis.MutualRecursionCall.TargetMethodName

 The target method name. 



---
#### Property Analysis.MutualRecursionCall.Reduction

 How much the problem size is reduced (for subtraction patterns). 



---
#### Property Analysis.MutualRecursionCall.ScaleFactor

 Scale factor (for division patterns). 



---
#### Property Analysis.MutualRecursionCall.InvocationSyntax

 The invocation syntax. 



---
## Type Analysis.ParallelPatternAnalyzer

 Analyzes code patterns to detect parallel complexity scenarios. Detects patterns like: - Parallel.For / Parallel.ForEach (data parallelism) - PLINQ (AsParallel, parallel LINQ) - Task.Run / Task.WhenAll / Task.WhenAny (task parallelism) - async/await patterns - Parallel invoke 



---
#### Method Analysis.ParallelPatternAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for parallel complexity patterns. Returns a ParallelComplexity if a parallel pattern is detected, or null if no parallel pattern is found. 



---
#### Method Analysis.ParallelPatternAnalyzer.AnalyzeBlock(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax)

 Analyzes a block of code for parallel patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectParallelForPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Parallel.For and Parallel.ForEach patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectPLINQPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects PLINQ patterns (AsParallel(), parallel LINQ). 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectTaskWhenPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Task.WhenAll / Task.WhenAny patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectTaskRunPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Task.Run patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectParallelInvokePattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects Parallel.Invoke patterns. 



---
#### Method Analysis.ParallelPatternAnalyzer.DetectAsyncAwaitPattern(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects async/await patterns in async methods. 



---
## Type Analysis.ParallelAnalysisExtensions

 Extension methods for parallel pattern analysis. 



---
#### Method Analysis.ParallelAnalysisExtensions.AnalyzeWithParallelism(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method with parallel complexity detection. Returns ParallelComplexity if a pattern is detected, otherwise falls back to sequential analysis. 



---
#### Method Analysis.ParallelAnalysisExtensions.ContainsParallelPatterns(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Determines if a method contains any parallel patterns. 



---
#### Method Analysis.ParallelAnalysisExtensions.GetParallelPatternSummary(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Gets a summary of parallel patterns in a method. 



---
## Type Analysis.ParallelPatternSummary

 Summary of parallel patterns in a method. 



---
## Type Analysis.ProbabilisticAnalyzer

 Detects probabilistic patterns in code and produces probabilistic complexity analysis. 



---
#### Method Analysis.ProbabilisticAnalyzer.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a method for probabilistic complexity patterns. 



---
#### Method Analysis.ProbabilisticAnalyzer.AnalyzeExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax,ComplexityAnalysis.Roslyn.Analysis.AnalysisContext)

 Analyzes a specific expression for probabilistic characteristics. 



---
## Type Analysis.ProbabilisticAnalyzer.ProbabilisticPatternWalker

 Walker to find probabilistic patterns in code. 



---
## Type Analysis.ProbabilisticAnalysisResult

 Result of probabilistic complexity analysis. 



---
#### Property Analysis.ProbabilisticAnalysisResult.Success

 Whether the analysis found probabilistic patterns. 



---
#### Property Analysis.ProbabilisticAnalysisResult.ProbabilisticComplexity

 The combined probabilistic complexity. 



---
#### Property Analysis.ProbabilisticAnalysisResult.DetectedPatterns

 All detected probabilistic patterns. 



---
#### Property Analysis.ProbabilisticAnalysisResult.Notes

 Additional notes about the analysis. 



---
#### Property Analysis.ProbabilisticAnalysisResult.ErrorMessage

 Error message if analysis failed. 



---
#### Method Analysis.ProbabilisticAnalysisResult.NoProbabilisticPatterns

 Creates a result indicating no probabilistic patterns were found. 



---
## Type Analysis.ProbabilisticPattern

 A detected probabilistic pattern in code. 



---
#### Property Analysis.ProbabilisticPattern.Type

 The type of probabilistic pattern detected. 



---
#### Property Analysis.ProbabilisticPattern.Source

 The source of randomness in this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Distribution

 The probability distribution of this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Location

 The location in code where this pattern was detected. 



---
#### Property Analysis.ProbabilisticPattern.Description

 Description of the pattern. 



---
#### Property Analysis.ProbabilisticPattern.ExpectedComplexity

 The expected complexity for this pattern. 



---
#### Property Analysis.ProbabilisticPattern.WorstCaseComplexity

 The worst-case complexity for this pattern. 



---
#### Property Analysis.ProbabilisticPattern.Assumptions

 Assumptions required for the expected complexity. 



---
## Type Analysis.ProbabilisticPatternType

 Types of probabilistic patterns that can be detected. 



---
#### Field Analysis.ProbabilisticPatternType.RandomNumberGeneration

 Random number generation (Random.Next, etc.) 



---
#### Field Analysis.ProbabilisticPatternType.HashFunction

 Hash function computation (GetHashCode, HashCode.Combine) 



---
#### Field Analysis.ProbabilisticPatternType.HashTableOperation

 Hash table operations (Dictionary, HashSet access) 



---
#### Field Analysis.ProbabilisticPatternType.Shuffle

 Random shuffle operations (Fisher-Yates, etc.) 



---
#### Field Analysis.ProbabilisticPatternType.PivotSelection

 Random pivot selection (QuickSort-like) 



---
#### Field Analysis.ProbabilisticPatternType.RandomizedSelection

 Randomized selection (Quickselect) 



---
#### Field Analysis.ProbabilisticPatternType.SkipList

 Skip list operations 



---
#### Field Analysis.ProbabilisticPatternType.BloomFilter

 Bloom filter operations 



---
#### Field Analysis.ProbabilisticPatternType.MonteCarlo

 Monte Carlo algorithm patterns 



---
#### Field Analysis.ProbabilisticPatternType.RandomizedLoop

 Loop with randomized iteration count 



---
#### Field Analysis.ProbabilisticPatternType.Other

 Other probabilistic pattern 



---
## Type Analysis.RoslynComplexityExtractor

 Extracts complexity expressions from C# source code using Roslyn. 



---
#### Property Analysis.RoslynComplexityExtractor.MethodResults

 Gets the results of method analysis. 



---
#### Property Analysis.RoslynComplexityExtractor.MethodComplexities

 Gets computed complexities for methods. 



---
#### Method Analysis.RoslynComplexityExtractor.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a single method and returns its complexity. 



---
#### Method Analysis.RoslynComplexityExtractor.TryDetectMutualRecursion(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.IMethodSymbol,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 Attempts to detect and solve mutual recursion for a method. Returns null if the method is not part of a mutual recursion cycle. 



---
## Type Analysis.RoslynComplexityExtractorExtensions

 Extension methods for the complexity extractor. 



---
#### Method Analysis.RoslynComplexityExtractorExtensions.AnalyzeAllMethods(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.SyntaxNode)

 Analyzes all methods in a syntax tree. 



---
#### Method Analysis.RoslynComplexityExtractorExtensions.AnalyzeInTopologicalOrder(ComplexityAnalysis.Roslyn.Analysis.RoslynComplexityExtractor,Microsoft.CodeAnalysis.SyntaxNode,ComplexityAnalysis.Roslyn.Analysis.CallGraph)

 Analyzes methods in topological order based on call graph. 



---
## Type BCL.BCLComplexityMappings

 Central registry for Base Class Library (BCL) method complexity mappings. 



> This registry provides complexity information for .NET BCL methods, enabling accurate complexity analysis without requiring source code inspection. 

**Source Attribution Levels:**

**Level**: Meaning
- **Documented**: Official Microsoft documentation explicitly states complexity (MSDN)
- **Attested**: Verified through .NET runtime source code inspection (github.com/dotnet/runtime)
- **Empirical**: Measured through systematic benchmarking
- **Heuristic**: Conservative estimate based on algorithm analysis


**Coverage:**

- **System.Collections.Generic**: List, Dictionary, HashSet, SortedSet, Queue, Stack, LinkedList, PriorityQueue
- **System.Linq**: All Enumerable extension methods with deferred/immediate distinction
- **System.String**: String manipulation, search, comparison operations
- **System.Collections.Concurrent**: Thread-safe collections
- **System.Text.RegularExpressions**: Regex with backtracking warnings
- **System.Threading.Tasks**: TPL, Parallel, PLINQ operations


**Design Philosophy:** When in doubt, we overestimate complexity. False positives (warning about performance that's actually fine) are preferable to false negatives (missing actual performance problems). 

**Usage:**



######  code

```
    var mappings = BCLComplexityMappings.Instance;
    var complexity = mappings.GetComplexity("List`1", "Contains");
    // Returns: O(n) with source "MSDN: List<T>.Contains is O(n)"
```



**See also**: [`ComplexityMapping`](ComplexityMapping)

**See also**: [`ComplexitySource`](ComplexitySource)



---
#### Method BCL.BCLComplexityMappings.GetComplexity(System.String,System.String,System.Int32)

 Gets the complexity mapping for a method, or a conservative default. 



---
#### Method BCL.BCLComplexityMappings.Create

 Creates the complete BCL mappings registry. 



---
#### Method BCL.BCLComplexityMappings.AmortizedO1(ComplexityAnalysis.Core.Complexity.ComplexitySource)

 Creates an amortized O(1) complexity with O(n) worst case. Used for operations like List.Add, HashSet.Add, Dictionary.Add. 



---
## Type BCL.MethodSignature

 Signature for method lookup in the mappings registry. 



---
#### Method BCL.MethodSignature.#ctor(System.String,System.String,System.Int32)

 Signature for method lookup in the mappings registry. 



---
## Type BCL.ComplexityMapping

 A complexity mapping with source attribution and notes. 



---
#### Method BCL.ComplexityMapping.#ctor(ComplexityAnalysis.Core.Complexity.ComplexityExpression,ComplexityAnalysis.Core.Complexity.ComplexitySource,ComplexityAnalysis.Roslyn.BCL.ComplexityNotes,ComplexityAnalysis.Core.Complexity.ComplexityExpression)

 A complexity mapping with source attribution and notes. 



---
## Type BCL.ComplexityNotes

 Additional notes about complexity characteristics. 



---
#### Field BCL.ComplexityNotes.Amortized

Complexity is amortized (occasional expensive operations)



---
#### Field BCL.ComplexityNotes.DeferredExecution

LINQ deferred execution - O(1) to create, full cost on enumeration



---
#### Field BCL.ComplexityNotes.BacktrackingWarning

Regex backtracking warning - can be exponential



---
#### Field BCL.ComplexityNotes.InputDependent

Complexity depends on input characteristics



---
#### Field BCL.ComplexityNotes.ThreadSafe

Thread-safe but may have contention overhead



---
#### Field BCL.ComplexityNotes.Unknown

Unknown method - conservative estimate



---
#### Field BCL.ComplexityNotes.Probabilistic

Probabilistic complexity - expected vs worst case may differ



---
## Type Speculative.ComplexityContract

 Complexity contract information from attributes or XML docs. 



---
## Type Speculative.ComplexityContractReader

 Reads complexity contracts from: - [Complexity("O(n)")] attributes - XML documentation with complexity info 



---
#### Method Speculative.ComplexityContractReader.ReadContract(Microsoft.CodeAnalysis.IMethodSymbol)

 Reads complexity contract from a method symbol. 



---
#### Method Speculative.ComplexityContractReader.ParseComplexityString(System.String)

 Parses a complexity string like "O(n)", "O(n log n)", "O(n^2)". 



---
## Type Speculative.IncompleteCodeResult

 Result of incomplete code detection. 



---
## Type Speculative.IncompleteCodeDetector

 Detects incomplete code patterns: - throw new NotImplementedException() - throw new NotSupportedException() - TODO/FIXME/HACK comments - Empty method bodies 



---
#### Method Speculative.IncompleteCodeDetector.Detect(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Detects incomplete code patterns in a method. 



---
## Type Speculative.IncrementalComplexityAnalyzer

 Provides incremental complexity analysis for code being actively edited. Designed for real-time feedback in IDE scenarios where code may be incomplete or syntactically invalid during typing. Key features: - Parses incomplete/malformed syntax gracefully - Caches analysis results for unchanged code regions - Streams progress callbacks during analysis - Provides confidence-weighted estimates for partial constructs 



---
#### Method Speculative.IncrementalComplexityAnalyzer.#ctor(ComplexityAnalysis.Roslyn.Speculative.IOnlineAnalysisCallback,ComplexityAnalysis.Roslyn.Speculative.AnalysisOptions)

 Creates a new incremental analyzer with optional callback. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.AnalyzeAsync(System.String,System.Int32,System.Threading.CancellationToken)

 Analyzes code text incrementally, reporting progress via callbacks. Handles incomplete syntax gracefully. 

|Name | Description |
|-----|------|
|sourceText: |The current source text (may be incomplete)|
|position: |Caret position in the text|
|cancellationToken: |Cancellation token for async operation|


---
#### Method Speculative.IncrementalComplexityAnalyzer.AnalyzeMethodAsync(System.String,System.String,System.Threading.CancellationToken)

 Analyzes a specific method by name, useful for targeted analysis. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.GetCachedAnalysis(System.String)

 Gets cached analysis for a code region, or null if not cached. 



---
#### Method Speculative.IncrementalComplexityAnalyzer.ClearCache

 Clears the analysis cache. 



---
## Type Speculative.AnalysisOptions

 Options for online analysis. 



---
#### Property Speculative.AnalysisOptions.Timeout

 Maximum time to spend on analysis before returning partial results. 



---
#### Property Speculative.AnalysisOptions.UseCache

 Whether to use cached results when available. 



---
#### Property Speculative.AnalysisOptions.MinConfidence

 Minimum confidence to report a result. 



---
#### Property Speculative.AnalysisOptions.MaxMethodsPerPass

 Maximum number of methods to analyze in one pass. 



---
## Type Speculative.OnlineAnalysisPhase

 Phases of online analysis. 



---
## Type Speculative.ScopeType

 Types of analysis scope. 



---
## Type Speculative.IncompleteReason

 Reasons for incomplete code. 



---
## Type Speculative.ParseResult

 Result of parsing with recovery. 



---
## Type Speculative.IncompleteNode

 An incomplete node in the syntax tree. 



---
## Type Speculative.AnalysisScope

 Analysis scope definition. 



---
## Type Speculative.FragmentAnalysisResult

 Result of analyzing a code fragment. 



---
## Type Speculative.MethodAnalysisSnapshot

 Snapshot of a method's complexity analysis. 



---
## Type Speculative.LoopSnapshot

 Snapshot of a loop's analysis. 



---
## Type Speculative.MethodComplexitySnapshot

 Per-method complexity snapshot in online results. 



---
## Type Speculative.ParseDiagnostic

 Parse diagnostic for reporting to UI. 



---
## Type Speculative.IncompleteRegion

 Region of incomplete code. 



---
## Type Speculative.CachedAnalysis

 Cached analysis result. 



---
## Type Speculative.OnlineAnalysisResult

 Overall result of online analysis. 



---
## Type Speculative.IOnlineAnalysisCallback

 Callback interface for online/incremental analysis progress. Implementations receive real-time updates during code analysis, suitable for IDE integration and live feedback. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnAnalysisStarted(System.Int32)

 Called when analysis begins. 

|Name | Description |
|-----|------|
|sourceLength: |Length of source text being analyzed.|


---
#### Method Speculative.IOnlineAnalysisCallback.OnPhaseStarted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisPhase)

 Called when an analysis phase begins. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnPhaseCompleted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisPhase)

 Called when an analysis phase completes. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnProgress(System.Int32,System.Int32,System.String)

 Called to report analysis progress. 

|Name | Description |
|-----|------|
|completed: |Number of items completed.|
|total: |Total number of items.|
|currentItem: |Name of current item being processed.|


---
#### Method Speculative.IOnlineAnalysisCallback.OnAnalysisCompleted(ComplexityAnalysis.Roslyn.Speculative.OnlineAnalysisResult,System.TimeSpan)

 Called when analysis completes successfully. 



---
#### Method Speculative.IOnlineAnalysisCallback.OnError(System.Exception)

 Called when an error occurs during analysis. 



---
## Type Speculative.NullOnlineAnalysisCallback

 Null implementation that does nothing. 



---
## Type Speculative.ConsoleOnlineAnalysisCallback

 Console-based callback for debugging and testing. 



---
## Type Speculative.BufferedOnlineAnalysisCallback

 Callback that buffers events for later processing. Useful for testing and batch processing. 



---
## Type Speculative.AnalysisEvent

 Base class for analysis events. 



---
#### Method Speculative.AnalysisEvent.#ctor(System.DateTime)

 Base class for analysis events. 



---
## Type Speculative.CompositeOnlineAnalysisCallback

 Aggregates multiple callbacks into one. 



---
## Type Speculative.SpeculativeAnalysisResult

 Result of speculative analysis for incomplete or partial code. 



---
#### Property Speculative.SpeculativeAnalysisResult.Complexity

Best-effort complexity estimate.



---
#### Property Speculative.SpeculativeAnalysisResult.LowerBound

Lower bound complexity (what we know for certain).



---
#### Property Speculative.SpeculativeAnalysisResult.UpperBound

Upper bound complexity (conservative estimate).



---
#### Property Speculative.SpeculativeAnalysisResult.Confidence

Confidence in the result (0.0 to 1.0).



---
#### Property Speculative.SpeculativeAnalysisResult.IsIncomplete

Whether the code appears incomplete (NIE, TODO, etc.).



---
#### Property Speculative.SpeculativeAnalysisResult.IsStub

Whether the code appears to be a stub.



---
#### Property Speculative.SpeculativeAnalysisResult.HasTodoMarker

Whether the code contains TODO/FIXME markers.



---
#### Property Speculative.SpeculativeAnalysisResult.HasUncertainty

Whether there's unresolved uncertainty from abstract/interface calls.



---
#### Property Speculative.SpeculativeAnalysisResult.UsedContract

Whether a complexity contract was used.



---
#### Property Speculative.SpeculativeAnalysisResult.UncertaintySource

Source of uncertainty (e.g., "IProcessor.Process").



---
#### Property Speculative.SpeculativeAnalysisResult.DependsOn

Methods this analysis depends on (for uncertainty tracking).



---
#### Property Speculative.SpeculativeAnalysisResult.DetectedPatterns

Detected code patterns that inform the analysis.



---
#### Property Speculative.SpeculativeAnalysisResult.Explanation

Explanation of the analysis.



---
## Type Speculative.CodePattern

 Detected code pattern that informs speculative analysis. 



---
#### Field Speculative.CodePattern.ThrowsNotImplementedException

throw new NotImplementedException()



---
#### Field Speculative.CodePattern.ThrowsNotSupportedException

throw new NotSupportedException()



---
#### Field Speculative.CodePattern.HasTodoComment

Contains TODO/FIXME/HACK comment



---
#### Field Speculative.CodePattern.ReturnsDefault

Returns default/null/empty



---
#### Field Speculative.CodePattern.EmptyBody

Method body is empty or just returns



---
#### Field Speculative.CodePattern.CounterOnly

Only increments counter (mock pattern)



---
#### Field Speculative.CodePattern.ReturnsConstant

Returns constant value



---
#### Field Speculative.CodePattern.CallsAbstract

Calls abstract method



---
#### Field Speculative.CodePattern.CallsInterface

Calls interface method



---
#### Field Speculative.CodePattern.CallsVirtual

Calls virtual method that may be overridden



---
#### Field Speculative.CodePattern.HasComplexityAttribute

Has [Complexity] attribute



---
#### Field Speculative.CodePattern.HasComplexityXmlDoc

Has XML doc with complexity info



---
## Type Speculative.SpeculativeAnalyzer

 Analyzes partial, incomplete, or abstract code to produce speculative complexity estimates. This is Phase D of the analysis pipeline, handling: - Incomplete implementations (NotImplementedException, TODO) - Abstract method calls - Interface method calls - Stub detection - Complexity contracts (attributes, XML docs) 



---
#### Method Speculative.SpeculativeAnalyzer.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for speculative complexity, handling incomplete code. 



---
#### Method Speculative.SpeculativeAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.SyntaxTree,System.String)

 Analyzes a method by name in the compilation. 



---
## Type Speculative.StubDetectionResult

 Result of stub detection. 



---
## Type Speculative.StubDetector

 Detects stub implementations: - Returns default/null/empty - Counter-only implementations (mocks) - Returns constant value with no logic 



---
#### Method Speculative.StubDetector.Detect(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,Microsoft.CodeAnalysis.SemanticModel)

 Detects if a method is a stub implementation. 



---
## Type Speculative.SyntaxFragmentAnalyzer

 Analyzes syntax fragments, including incomplete code during active editing. Provides best-effort complexity estimates with confidence values. 



---
#### Method Speculative.SyntaxFragmentAnalyzer.AnalyzeMethod(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax,System.Boolean)

 Analyzes a method, handling incomplete syntax gracefully. 



---
#### Method Speculative.SyntaxFragmentAnalyzer.AnalyzeStatement(Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax)

 Analyzes a single statement, useful for incremental updates. 



---
## Type Speculative.StatementAnalysisResult

 Result of analyzing a single statement. 



---
## Type Speculative.UncertaintyResult

 Result of uncertainty tracking. 



---
## Type Speculative.UncertaintyTracker

 Tracks uncertainty from abstract, virtual, and interface method calls. When complexity depends on runtime polymorphism, we track the dependency rather than making potentially incorrect assumptions. 



---
#### Method Speculative.UncertaintyTracker.Analyze(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax)

 Analyzes a method for uncertainty from polymorphic calls. 



---




# ComplexityAnalysis.Engine #



