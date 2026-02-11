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


