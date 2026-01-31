# Complexity Analysis System

Welcome to the **Complexity Analysis System** documentation. This system provides first-class algorithmic complexity analysis for .NET applications using Roslyn-based static analysis.

## Quick Start

```csharp
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis.CSharp;

// Parse your code
var tree = CSharpSyntaxTree.ParseText(sourceCode);
var compilation = CSharpCompilation.Create("Analysis", new[] { tree }, references);

// Analyze
var extractor = new RoslynComplexityExtractor(compilation.GetSemanticModel(tree));
var complexity = extractor.AnalyzeMethod(methodDeclaration);

Console.WriteLine($"Complexity: {complexity.ToBigONotation()}");
```

## Architecture Overview

The system is organized into five phases:

| Phase | Component | Description |
|-------|-----------|-------------|
| A | `ComplexityAnalysis.Roslyn` | Static AST/CFG analysis via Roslyn |
| B | `ComplexityAnalysis.Solver` | Recurrence solving (Master Theorem, Akra-Bazzi) |
| C | `ComplexityAnalysis.Solver.Refinement` | Bound tightening and verification |
| D | `ComplexityAnalysis.Roslyn.Speculative` | Real-time IDE analysis |
| E | `ComplexityAnalysis.Calibration` | Hardware benchmarking and calibration |

## Core Concepts

### Complexity Expression Types

The system represents complexity using an expression tree hierarchy:

- **`ConstantComplexity`** - O(1)
- **`LinearComplexity`** - O(n)
- **`PolynomialComplexity`** - O(n²), O(n³)
- **`LogarithmicComplexity`** - O(log n)
- **`PolyLogComplexity`** - O(n log n), O(n log² n)
- **`ExponentialComplexity`** - O(2ⁿ)
- **`FactorialComplexity`** - O(n!)
- **`RecurrenceComplexity`** - T(n) = aT(n/b) + f(n)

### Recurrence Theorem Solving

The solver implements:

- **Master Theorem** for standard divide-and-conquer: T(n) = aT(n/b) + f(n)
- **Akra-Bazzi Theorem** for general multi-term recurrences: T(n) = Σᵢ aᵢT(bᵢn) + g(n)

### BCL Method Mappings

Over 150 .NET BCL methods are pre-mapped with documented complexities, including:

- `System.Collections.Generic` (List, Dictionary, HashSet, etc.)
- `System.Linq` (all Enumerable methods)
- `System.String` operations
- Concurrent collections
- Span/Memory operations

## API Reference

See the [API Documentation](api/index.md) for detailed type and method references.

## Learn More

- [Expression Types Guide](docs/articles/expression-types.md)
- [Recurrence Theorems](docs/articles/recurrence-theorems.md)
- [Architecture Design](docs/articles/architecture.md)
