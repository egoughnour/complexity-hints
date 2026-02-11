# Complexity Analysis System

**First-class algorithmic complexity analysis for .NET — powered by Roslyn, the Master Theorem, and Akra-Bazzi.**

See Big-O time and space complexity directly in your editor, backed by a rigorous five-phase analysis pipeline that extracts recurrences from C# source code and solves them symbolically.

## Overview

The system is organized into five core libraries:

| Library | Purpose |
|---------|---------|
| **Core** | Complexity types, expressions, confidence scoring, and progress tracking |
| **Roslyn** | AST/CFG extraction from C# source via Roslyn semantic analysis |
| **Solver** | Recurrence solving: Master Theorem, Akra-Bazzi, characteristic polynomials |
| **Calibration** | Hardware micro-benchmarking and curve fitting to verify theoretical predictions |
| **Engine** | Orchestration layer connecting analysis, solving, and calibration |

## Quick Start

```csharp
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis.CSharp;

var tree = CSharpSyntaxTree.ParseText(sourceCode);
var compilation = CSharpCompilation.Create("Analysis", new[] { tree }, references);

var extractor = new RoslynComplexityExtractor(compilation.GetSemanticModel(tree));
var complexity = extractor.AnalyzeMethod(methodDeclaration);

Console.WriteLine($"Complexity: {complexity.ToBigONotation()}");
// => "Complexity: O(n log n)"
```

## Documentation

- **[Guides](articles/architecture.md)** — Architecture overview, expression types, recurrence theory
- **[API Reference](api/complexityanalysis-core/index.md)** — Generated API docs for each library
