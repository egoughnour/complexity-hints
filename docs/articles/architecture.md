# System Architecture

The Complexity Analysis System is designed around a five-phase pipeline, each phase building on the previous to provide increasingly refined complexity estimates.

## Five-Phase Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  Phase A: Static Complexity Extraction                               │
│  ├── AST/CST analysis via Roslyn                                    │
│  ├── Control Flow Graph (CFG) construction                          │
│  ├── Call graph building and cycle detection                        │
│  └── BCL method complexity lookup                                   │
├─────────────────────────────────────────────────────────────────────┤
│  Phase B: Recurrence Solving                                        │
│  ├── Master Theorem (standard divide-and-conquer)                   │
│  ├── Akra-Bazzi Theorem (general multi-term)                        │
│  └── Linear recurrence detection                                    │
├─────────────────────────────────────────────────────────────────────┤
│  Phase C: Refinement                                                │
│  ├── Slack variable optimization                                    │
│  ├── Perturbation expansion for boundary cases                      │
│  ├── Induction-based verification                                   │
│  └── Confidence scoring                                             │
├─────────────────────────────────────────────────────────────────────┤
│  Phase D: Speculative Analysis (IDE Integration)                    │
│  ├── Incremental parsing with caching                               │
│  ├── Incomplete code handling                                       │
│  ├── Stub detection and uncertainty tracking                        │
│  └── Real-time complexity hints                                     │
├─────────────────────────────────────────────────────────────────────┤
│  Phase E: Hardware Calibration                                      │
│  ├── Micro-benchmarking with warmup                                 │
│  ├── Curve fitting for complexity verification                      │
│  ├── Constant factor estimation                                     │
│  └── Hardware profile capture                                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Component Libraries

### ComplexityAnalysis.Core

The foundation library containing:

- **Complexity Types**: Expression hierarchy (`ComplexityExpression` and subclasses)
- **Variables**: Input size variables (`Variable`, `VariableType`)
- **Recurrence Types**: `RecurrenceComplexity`, `RecurrenceTerm`
- **Specialized Types**: `AmortizedComplexity`, `ParallelComplexity`, `MemoryComplexity`, `ProbabilisticComplexity`
- **Visitor Pattern**: `IComplexityVisitor<T>` for traversal

### ComplexityAnalysis.Roslyn

Roslyn-based code analysis:

- **RoslynComplexityExtractor**: Main syntax walker for complexity extraction
- **LoopAnalyzer**: Loop bound detection (for, while, foreach, do-while)
- **CallGraphBuilder**: Inter-procedural call analysis with SCC detection
- **ControlFlowAnalysis**: CFG construction, cyclomatic complexity
- **BCLComplexityMappings**: 150+ pre-mapped .NET method complexities
- **Speculative/**: IDE integration components

### ComplexityAnalysis.Solver

Mathematical solving:

- **TheoremApplicabilityAnalyzer**: Main coordinator for theorem selection
- **CriticalExponentSolver**: Newton-Raphson for Akra-Bazzi $\sum a_i b_i^p = 1$
- **AkraBazziIntegralEvaluator**: Closed-form integral evaluation
- **RegularityChecker**: Master Theorem Case 3 verification
- **Refinement/**: Slack variables, perturbation, induction, confidence

### ComplexityAnalysis.Calibration

Runtime verification:

- **MicroBenchmarkRunner**: Precise timing with outlier removal
- **ComplexityVerifier**: Curve fitting to verify complexity claims
- **BCLCalibrator**: Calibrates BCL methods with constant factors
- **CalibrationStore**: JSON persistence of calibration data

## Memory Hierarchy Model

For online calibration, the system models memory access costs:

| Tier | Weight Factor | Typical Latency |
|------|---------------|-----------------|
| L1/L2 Cache | 1× | ~1-10 ns |
| Main Memory | 1,000× | ~100 ns |
| Local Disk (SSD) | 1,000,000× | ~100 µs |
| Local Network | 1,000,000,000× | ~1-10 ms |
| Far Network | 1,000,000,000,000× | ~100+ ms |

## Source Attribution

Complexity mappings are attributed by confidence level:

| Level | Description | Example |
|-------|-------------|---------|
| **Documented** | Official Microsoft docs | MSDN complexity guarantees |
| **Attested** | Academic papers | CLRS algorithm analysis |
| **Empirical** | Benchmarking-verified | Measured O(n log n) behavior |
| **Inferred** | Code analysis derived | Detected nested loops |
| **Heuristic** | Conservative estimate | Unknown → assume worst |

**Conservative Principle**: When uncertain, assume the worst reasonable case.

## Data Flow

```
Source Code
    │
    ▼
┌───────────────────────┐
│  Roslyn Syntax Tree   │
│  + Semantic Model     │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│  Phase A: Extract     │
│  - Loop bounds        │
│  - BCL method calls   │
│  - Recursive calls    │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│  Phase B: Solve       │
│  - Detect recurrence  │
│  - Apply theorem      │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│  Phase C: Refine      │
│  - Tighten bounds     │
│  - Verify solution    │
│  - Score confidence   │
└───────────┬───────────┘
            │
            ▼
    ComplexityExpression
    + ConfidenceScore
```

## Extension Points

### Custom Visitors

Implement `IComplexityVisitor<T>` to traverse expression trees:

```csharp
public class MyVisitor : IComplexityVisitor<string>
{
    public string Visit(ConstantComplexity c) => "constant";
    public string Visit(LinearComplexity c) => "linear";
    // ... other Visit methods
}
```

### Custom BCL Mappings

Add mappings for domain-specific libraries:

```csharp
BCLComplexityMappings.Instance.RegisterCustomMapping(
    typeName: "MyLibrary.MyCollection",
    methodName: "Find",
    complexity: new LinearComplexity(1, Variable.N),
    source: ComplexitySource.Documented("...")
);
```

### Progress Callbacks

Monitor analysis progress:

```csharp
extractor.OnMethodAnalyzed += (method, complexity) => 
    Console.WriteLine($"{method.Name}: {complexity.ToBigONotation()}");

extractor.OnRecurrenceDetected += (recurrence) =>
    Console.WriteLine($"Found recurrence: {recurrence}");
```
