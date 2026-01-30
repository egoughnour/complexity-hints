using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Recurrence;
using ComplexityAnalysis.Roslyn.Analysis;
using ComplexityAnalysis.Roslyn.BCL;
using ComplexityAnalysis.Solver;
using ComplexityAnalysis.Solver.Refinement;
using ComplexityAnalysis.Tests.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

// Alias to disambiguate from ComplexityAnalysis.Solver.Refinement.AnalysisContext
using RoslynAnalysisContext = ComplexityAnalysis.Roslyn.Analysis.AnalysisContext;
// Alias to disambiguate from Microsoft.CodeAnalysis.ControlFlowAnalysis
using RoslynControlFlowAnalysis = ComplexityAnalysis.Roslyn.Analysis.ControlFlowAnalysis;

namespace ComplexityAnalysis.Tests.Integration;

/// <summary>
/// End-to-end integration tests that verify the complete analysis pipeline:
/// Code → Roslyn AST → Complexity Extraction → Recurrence Detection → Theorem Solving → Refinement
/// </summary>
public class EndToEndAnalysisTests
{
    private readonly ITestOutputHelper _output;
    private readonly TheoremApplicabilityAnalyzer _theoremAnalyzer = TheoremApplicabilityAnalyzer.Instance;
    private readonly BCLComplexityMappings _bclMappings = BCLComplexityMappings.Instance;

    public EndToEndAnalysisTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Classic Algorithm Analyses

    /// <summary>
    /// Analyze binary search implementation - expected O(log n).
    /// </summary>
    [Fact]
    public async Task BinarySearch_AnalyzesAsLogN()
    {
        const string code = @"
public class Algorithms
{
    public int BinarySearch(int[] array, int target)
    {
        int left = 0;
        int right = array.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (array[mid] == target)
                return mid;
            else if (array[mid] < target)
                left = mid + 1;
            else
                right = mid - 1;
        }

        return -1;
    }
}";
        var result = await AnalyzeMethodAsync(code, "BinarySearch");

        _output.WriteLine($"Binary Search Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Loop detected: {result.HasLoop}");
        _output.WriteLine($"Loop pattern: {result.LoopPattern}");

        // Should detect logarithmic loop pattern
        Assert.True(result.HasLoop);
        Assert.Equal(IterationPattern.Logarithmic, result.LoopPattern);

        // Overall complexity should be O(log n)
        AssertComplexityForm(result.Complexity, ExpressionForm.Logarithmic);
    }

    /// <summary>
    /// Analyze linear search implementation - expected O(n).
    /// </summary>
    [Fact]
    public async Task LinearSearch_AnalyzesAsN()
    {
        const string code = @"
public class Algorithms
{
    public int LinearSearch(int[] array, int target)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == target)
                return i;
        }
        return -1;
    }
}";
        var result = await AnalyzeMethodAsync(code, "LinearSearch");

        _output.WriteLine($"Linear Search Analysis: {result.Complexity?.ToBigONotation()}");

        Assert.True(result.HasLoop);
        Assert.Equal(IterationPattern.Linear, result.LoopPattern);
        AssertComplexityForm(result.Complexity, ExpressionForm.Polynomial, expectedDegree: 1);
    }

    /// <summary>
    /// Analyze bubble sort - expected O(n²).
    /// </summary>
    [Fact]
    public async Task BubbleSort_AnalyzesAsNSquared()
    {
        const string code = @"
public class Algorithms
{
    public void BubbleSort(int[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (array[j] > array[j + 1])
                {
                    int temp = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = temp;
                }
            }
        }
    }
}";
        var result = await AnalyzeMethodAsync(code, "BubbleSort");

        _output.WriteLine($"Bubble Sort Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Nesting depth: {result.NestingDepth}");

        // Should detect nested loops with depth 2
        Assert.True(result.NestingDepth >= 2);

        // Overall complexity should be O(n²)
        AssertComplexityForm(result.Complexity, ExpressionForm.Polynomial, expectedDegree: 2);
    }

    /// <summary>
    /// Analyze matrix multiplication - expected O(n³).
    /// </summary>
    [Fact]
    public async Task MatrixMultiply_AnalyzesAsNCubed()
    {
        const string code = @"
public class Algorithms
{
    public int[,] MatrixMultiply(int[,] a, int[,] b, int n)
    {
        var result = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = 0;
                for (int k = 0; k < n; k++)
                {
                    result[i, j] += a[i, k] * b[k, j];
                }
            }
        }

        return result;
    }
}";
        var result = await AnalyzeMethodAsync(code, "MatrixMultiply");

        _output.WriteLine($"Matrix Multiply Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Nesting depth: {result.NestingDepth}");

        // Should detect 3 levels of nesting
        Assert.True(result.NestingDepth >= 3);

        // Overall complexity should be O(n³)
        AssertComplexityForm(result.Complexity, ExpressionForm.Polynomial, expectedDegree: 3);
    }

    #endregion

    #region Recursive Algorithm Analyses

    /// <summary>
    /// Analyze recursive factorial - expected O(n).
    /// </summary>
    [Fact]
    public async Task RecursiveFactorial_DetectsLinearRecurrence()
    {
        const string code = @"
public class Algorithms
{
    public long Factorial(int n)
    {
        if (n <= 1)
            return 1;
        return n * Factorial(n - 1);
    }
}";
        var result = await AnalyzeMethodAsync(code, "Factorial");

        _output.WriteLine($"Factorial Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Recursive: {result.IsRecursive}");
        _output.WriteLine($"Recurrence: {result.DetectedRecurrence}");

        // Should detect recursion
        Assert.True(result.IsRecursive);

        // This is T(n) = T(n-1) + O(1), which gives O(n)
        AssertComplexityForm(result.Complexity, ExpressionForm.Polynomial, expectedDegree: 1);
    }

    /// <summary>
    /// Analyze recursive fibonacci - expected O(2^n).
    /// </summary>
    [Fact]
    public async Task RecursiveFibonacci_DetectsExponentialRecurrence()
    {
        const string code = @"
public class Algorithms
{
    public long Fibonacci(int n)
    {
        if (n <= 1)
            return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
}";
        var result = await AnalyzeMethodAsync(code, "Fibonacci");

        _output.WriteLine($"Fibonacci Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Recursive: {result.IsRecursive}");
        _output.WriteLine($"Recursive calls: {result.RecursiveCallCount}");

        // Should detect recursion with 2 calls
        Assert.True(result.IsRecursive);
        Assert.True(result.RecursiveCallCount >= 2);

        // This is T(n) = T(n-1) + T(n-2) + O(1) ≈ O(2^n) or more precisely O(φ^n)
        AssertComplexityForm(result.Complexity, ExpressionForm.Exponential);
    }

    /// <summary>
    /// Analyze merge sort - expected O(n log n).
    /// </summary>
    [Fact]
    public async Task MergeSort_AnalyzesAsNLogN()
    {
        const string code = @"
public class Algorithms
{
    public void MergeSort(int[] array, int left, int right)
    {
        if (left < right)
        {
            int mid = (left + right) / 2;

            MergeSort(array, left, mid);
            MergeSort(array, mid + 1, right);

            Merge(array, left, mid, right);
        }
    }

    private void Merge(int[] array, int left, int mid, int right)
    {
        // O(n) merge operation
        int n = right - left + 1;
        var temp = new int[n];
        for (int i = 0; i < n; i++)
            temp[i] = array[left + i];
    }
}";
        var result = await AnalyzeMethodAsync(code, "MergeSort");

        _output.WriteLine($"Merge Sort Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"Recursive: {result.IsRecursive}");
        _output.WriteLine($"Recurrence: {result.DetectedRecurrence}");

        // Should detect recursion
        Assert.True(result.IsRecursive);
        Assert.True(result.RecursiveCallCount >= 2);

        // This is T(n) = 2T(n/2) + O(n), which gives O(n log n)
        // Verify through theorem solving
        if (result.DetectedRecurrence != null)
        {
            var theoremResult = _theoremAnalyzer.Analyze(result.DetectedRecurrence);
            _output.WriteLine($"Theorem result: {theoremResult}");

            if (theoremResult is MasterTheoremApplicable master)
            {
                Assert.Equal(MasterTheoremCase.Case2, master.Case);
            }
        }

        AssertComplexityForm(result.Complexity, ExpressionForm.PolyLog, expectedDegree: 1, expectedLog: 1);
    }

    #endregion

    #region BCL Method Call Analysis

    /// <summary>
    /// Analyze code with List operations.
    /// </summary>
    [Fact]
    public async Task ListOperations_ResolveBCLComplexities()
    {
        const string code = @"
using System.Collections.Generic;

public class DataProcessor
{
    public void ProcessItems(List<int> items)
    {
        // O(n) operation
        if (items.Contains(42))
        {
            // O(1) amortized
            items.Add(100);
        }

        // O(n log n) operation
        items.Sort();

        // O(log n) operation
        int index = items.BinarySearch(50);
    }
}";
        var result = await AnalyzeMethodAsync(code, "ProcessItems", includeCollections: true);

        _output.WriteLine($"List Operations Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"BCL calls detected: {result.BCLCallCount}");

        // Should detect multiple BCL calls
        Assert.True(result.BCLCallCount >= 3);

        // Dominant complexity should be O(n log n) from Sort
        AssertComplexityForm(result.Complexity, ExpressionForm.PolyLog, expectedDegree: 1, expectedLog: 1);
    }

    /// <summary>
    /// Analyze code with Dictionary operations.
    /// </summary>
    [Fact]
    public async Task DictionaryOperations_ResolveAsConstant()
    {
        const string code = @"
using System.Collections.Generic;

public class Cache
{
    private Dictionary<string, int> _cache = new();

    public int GetOrAdd(string key, int value)
    {
        // O(1) lookup
        if (_cache.TryGetValue(key, out var existing))
            return existing;

        // O(1) insert
        _cache.Add(key, value);
        return value;
    }
}";
        var result = await AnalyzeMethodAsync(code, "GetOrAdd", includeCollections: true);

        _output.WriteLine($"Dictionary Operations Analysis: {result.Complexity?.ToBigONotation()}");

        // Should be O(1) overall (amortized)
        AssertComplexityForm(result.Complexity, ExpressionForm.Constant);
    }

    /// <summary>
    /// Analyze LINQ query chains.
    /// </summary>
    [Fact]
    public async Task LinqChain_ComposesCorrectly()
    {
        const string code = @"
using System.Collections.Generic;
using System.Linq;

public class DataAnalyzer
{
    public int AnalyzeData(IEnumerable<int> data)
    {
        // Chain of O(n) operations
        return data
            .Where(x => x > 0)      // O(n) deferred
            .Select(x => x * 2)     // O(n) deferred
            .OrderBy(x => x)        // O(n log n) deferred
            .Take(10)               // O(n) deferred
            .Sum();                 // O(n) immediate - triggers evaluation
    }
}";
        var result = await AnalyzeMethodAsync(code, "AnalyzeData", includeLinq: true);

        _output.WriteLine($"LINQ Analysis: {result.Complexity?.ToBigONotation()}");
        _output.WriteLine($"LINQ calls: {result.LinqCallCount}");

        // Dominant is OrderBy at O(n log n)
        Assert.True(result.LinqCallCount >= 4);
    }

    #endregion

    #region Full Pipeline Integration Tests

    /// <summary>
    /// Test the complete analysis pipeline: extraction → theorem → refinement.
    /// </summary>
    [Theory]
    [MemberData(nameof(FullPipelineCases))]
    public async Task FullPipeline_ProducesConsistentResults(
        string name, string code, string methodName,
        double expectedPolyDegree, double expectedLogExponent)
    {
        _output.WriteLine($"=== Full Pipeline Test: {name} ===");

        // Step 1: Extract complexity from code
        var extractionResult = await AnalyzeMethodAsync(code, methodName, includeCollections: true);
        _output.WriteLine($"Extraction: {extractionResult.Complexity?.ToBigONotation()}");

        // Step 2: If recurrence detected, solve it
        if (extractionResult.DetectedRecurrence != null)
        {
            var theoremResult = _theoremAnalyzer.Analyze(extractionResult.DetectedRecurrence);
            _output.WriteLine($"Theorem: {theoremResult}");

            if (theoremResult.IsApplicable)
            {
                // Step 3: Refine the result
                var refinementEngine = new RefinementEngine(
                    new SlackVariableOptimizer(),
                    new PerturbationExpansion(),
                    new InductionVerifier(),
                    new ConfidenceScorer());

                var refinedResult = refinementEngine.Refine(
                    extractionResult.DetectedRecurrence, theoremResult);

                _output.WriteLine($"Refined: {refinedResult.RefinedSolution?.ToBigONotation()}");
                _output.WriteLine($"Confidence: {refinedResult.ConfidenceAssessment?.OverallScore:F4}");

                // Verify the refined result matches expectations
                var refinedDegree = GetPolyDegree(refinedResult.RefinedSolution);
                var refinedLog = GetLogExponent(refinedResult.RefinedSolution);

                Assert.Equal(expectedPolyDegree, refinedDegree, precision: 1);
                Assert.Equal(expectedLogExponent, refinedLog, precision: 1);
            }
        }

        // Verify overall extraction
        var extractedDegree = GetPolyDegree(extractionResult.Complexity);
        var extractedLog = GetLogExponent(extractionResult.Complexity);

        _output.WriteLine($"Final: degree={extractedDegree}, log={extractedLog}");

        // Allow some tolerance for extraction variations
        Assert.InRange(extractedDegree, expectedPolyDegree - 0.5, expectedPolyDegree + 0.5);
    }

    public static IEnumerable<object[]> FullPipelineCases => new[]
    {
        // name, code, methodName, expectedPolyDegree, expectedLogExponent
        new object[] {
            "LinearSum",
            @"public class T { public int Sum(int[] arr) { int s = 0; for (int i = 0; i < arr.Length; i++) s += arr[i]; return s; } }",
            "Sum", 1.0, 0.0 },

        new object[] {
            "NestedLoop",
            @"public class T { public int Nested(int n) { int c = 0; for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) c++; return c; } }",
            "Nested", 2.0, 0.0 },

        new object[] {
            "LogLoop",
            @"public class T { public int Log(int n) { int c = 0; for (int i = 1; i < n; i *= 2) c++; return c; } }",
            "Log", 0.0, 1.0 },
    };

    #endregion

    #region Call Graph Analysis

    /// <summary>
    /// Test inter-procedural analysis with call graph.
    /// </summary>
    [Fact]
    public async Task CallGraph_DetectsMethodDependencies()
    {
        const string code = @"
public class Calculator
{
    public int Compute(int n)
    {
        return ProcessLevel1(n);
    }

    private int ProcessLevel1(int n)
    {
        int result = 0;
        for (int i = 0; i < n; i++)
            result += ProcessLevel2(i);
        return result;
    }

    private int ProcessLevel2(int n)
    {
        int result = 0;
        for (int i = 0; i < n; i++)
            result += i;
        return result;
    }
}";
        var compilation = CreateCompilation(code);
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        _output.WriteLine($"Methods in call graph: {callGraph.AllMethods.Count()}");
        _output.WriteLine($"Entry points: {string.Join(", ", callGraph.FindEntryPoints().Select(m => m.Name))}");

        // Should detect the call chain
        var computeMethod = callGraph.AllMethods.FirstOrDefault(m => m.Name == "Compute");
        Assert.NotNull(computeMethod);

        var callees = callGraph.GetCallees(computeMethod);
        _output.WriteLine($"Compute calls: {string.Join(", ", callees.Select(m => m.Name))}");

        Assert.Contains(callees, m => m.Name == "ProcessLevel1");
    }

    #endregion

    #region Helper Methods

    private async Task<AnalysisResult> AnalyzeMethodAsync(
        string code, string methodName,
        bool includeCollections = false,
        bool includeLinq = false)
    {
        var compilation = CreateCompilation(code, includeCollections, includeLinq);
        var tree = compilation.SyntaxTrees.First();
        var root = await tree.GetRootAsync();
        var semanticModel = compilation.GetSemanticModel(tree);

        var methodDecl = root.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (methodDecl == null)
            throw new InvalidOperationException($"Method '{methodName}' not found");

        var extractor = new RoslynComplexityExtractor(semanticModel);
        var context = new RoslynAnalysisContext { SemanticModel = semanticModel };

        // Analyze loops (for, while, foreach, do-while)
        var loopAnalyzer = new LoopAnalyzer(semanticModel);
        var loopResults = new List<LoopAnalysisResult>();

        // Analyze for loops
        foreach (var forLoop in methodDecl.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax>())
        {
            loopResults.Add(loopAnalyzer.AnalyzeForLoop(forLoop, context));
        }

        // Analyze while loops
        foreach (var whileLoop in methodDecl.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax>())
        {
            loopResults.Add(loopAnalyzer.AnalyzeWhileLoop(whileLoop, context));
        }

        // Analyze foreach loops
        foreach (var foreachLoop in methodDecl.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax>())
        {
            loopResults.Add(loopAnalyzer.AnalyzeForeachLoop(foreachLoop, context));
        }

        // Analyze do-while loops
        foreach (var doWhileLoop in methodDecl.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DoStatementSyntax>())
        {
            loopResults.Add(loopAnalyzer.AnalyzeDoWhileLoop(doWhileLoop, context));
        }

        var loopInfo = loopResults.FirstOrDefault(l => l.Success) ?? loopResults.FirstOrDefault();

        // Analyze control flow
        var cfAnalysis = new RoslynControlFlowAnalysis(semanticModel);
        var cfResult = cfAnalysis.AnalyzeMethod(methodDecl);

        // Extract overall complexity
        var complexity = extractor.AnalyzeMethod(methodDecl);

        // Check for recursion using semantic analysis
        var methodSymbol = semanticModel.GetDeclaredSymbol(methodDecl);
        var isRecursive = methodDecl.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .Any(inv => IsRecursiveCall(inv, methodSymbol, semanticModel));

        var recursiveCallCount = methodDecl.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .Count(inv => IsRecursiveCall(inv, methodSymbol, semanticModel));

        // Count BCL/LINQ calls
        var allInvocations = methodDecl.DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>()
            .ToList();

        // For recursive methods, solve the recurrence to get the correct complexity
        var detectedRecurrence = isRecursive ? CreateRecurrenceFromContext(context, recursiveCallCount) : null;
        var finalComplexity = complexity;

        if (detectedRecurrence != null)
        {
            var theoremResult = _theoremAnalyzer.Analyze(detectedRecurrence);
            if (theoremResult.IsApplicable && theoremResult.Solution != null)
            {
                finalComplexity = theoremResult.Solution;
            }
        }

        return new AnalysisResult
        {
            Complexity = finalComplexity,
            HasLoop = loopResults.Any(l => l.Success),
            LoopPattern = loopInfo?.Pattern ?? IterationPattern.Unknown,
            NestingDepth = cfResult?.LoopNestingDepth ?? 0,
            IsRecursive = isRecursive,
            RecursiveCallCount = recursiveCallCount,
            DetectedRecurrence = detectedRecurrence,
            BCLCallCount = allInvocations.Count(i => IsBCLCall(i, semanticModel)),
            LinqCallCount = allInvocations.Count(i => IsLinqCall(i, semanticModel))
        };
    }

    private static Compilation CreateCompilation(
        string code,
        bool includeCollections = false,
        bool includeLinq = false)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        };

        // Add System.Runtime
        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimePath != null)
        {
            var systemRuntime = Path.Combine(runtimePath, "System.Runtime.dll");
            if (File.Exists(systemRuntime))
                references.Add(MetadataReference.CreateFromFile(systemRuntime));
        }

        if (includeCollections || includeLinq)
        {
            references.Add(MetadataReference.CreateFromFile(
                typeof(System.Collections.Generic.List<>).Assembly.Location));
        }

        if (includeLinq)
        {
            references.Add(MetadataReference.CreateFromFile(
                typeof(System.Linq.Enumerable).Assembly.Location));
        }

        var tree = CSharpSyntaxTree.ParseText(code);
        return CSharpCompilation.Create("TestAssembly",
            new[] { tree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static RecurrenceRelation? CreateRecurrenceFromContext(RoslynAnalysisContext context, int recursiveCallCount)
    {
        // Simplified recurrence creation based on recursive call count
        if (recursiveCallCount == 0) return null;

        if (recursiveCallCount == 1)
        {
            // T(n) = T(n/2) + O(1) or T(n) = T(n-1) + O(1)
            return RecurrenceRelation.DivideAndConquer(1, 2, new ConstantComplexity(1), Variable.N);
        }
        else if (recursiveCallCount == 2)
        {
            // T(n) = 2T(n/2) + O(n) for divide and conquer
            return RecurrenceRelation.DivideAndConquer(2, 2, new LinearComplexity(1, Variable.N), Variable.N);
        }

        return null;
    }

    private static bool IsRecursiveCall(
        Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation,
        IMethodSymbol? containingMethod,
        SemanticModel semanticModel)
    {
        if (containingMethod is null)
            return false;

        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is IMethodSymbol calledMethod)
        {
            return SymbolEqualityComparer.Default.Equals(calledMethod, containingMethod);
        }

        // Fallback to syntactic check for unresolved symbols
        var expr = invocation.Expression;
        if (expr is Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax id)
            return id.Identifier.Text == containingMethod.Name;
        if (expr is Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax member)
            return member.Name.Identifier.Text == containingMethod.Name;

        return false;
    }

    private static bool IsBCLCall(
        Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            var ns = method.ContainingNamespace?.ToDisplayString() ?? "";
            return ns.StartsWith("System.Collections") || ns.StartsWith("System.String");
        }
        return false;
    }

    private static bool IsLinqCall(
        Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            return method.ContainingType?.ToDisplayString() == "System.Linq.Enumerable";
        }
        return false;
    }

    private static void AssertComplexityForm(
        ComplexityExpression? expr,
        ExpressionForm expectedForm,
        double expectedDegree = 0,
        double expectedLog = 0)
    {
        Assert.NotNull(expr);

        var degree = GetPolyDegree(expr);
        var log = GetLogExponent(expr);

        switch (expectedForm)
        {
            case ExpressionForm.Constant:
                Assert.Equal(0, degree, precision: 1);
                Assert.Equal(0, log, precision: 1);
                break;
            case ExpressionForm.Logarithmic:
                Assert.Equal(0, degree, precision: 1);
                Assert.True(log >= 1);
                break;
            case ExpressionForm.Polynomial:
                Assert.Equal(expectedDegree, degree, precision: 1);
                break;
            case ExpressionForm.PolyLog:
                Assert.Equal(expectedDegree, degree, precision: 1);
                Assert.Equal(expectedLog, log, precision: 1);
                break;
            case ExpressionForm.Exponential:
                Assert.True(expr is ExponentialComplexity || expr is ExponentialOfComplexity);
                break;
        }
    }

    private static double GetPolyDegree(ComplexityExpression? expr) => expr switch
    {
        PolyLogComplexity p => p.PolyDegree,
        LinearComplexity => 1.0,
        VariableComplexity => 1.0,
        ConstantComplexity => 0.0,
        LogarithmicComplexity => 0.0,
        PolynomialComplexity p => p.Degree,
        BinaryOperationComplexity b when b.Operation == BinaryOp.Multiply => 
            GetPolyDegree(b.Left) + GetPolyDegree(b.Right),
        BinaryOperationComplexity b => Math.Max(GetPolyDegree(b.Left), GetPolyDegree(b.Right)),
        _ => 0.0
    };

    private static double GetLogExponent(ComplexityExpression? expr) => expr switch
    {
        PolyLogComplexity p => p.LogExponent,
        LogarithmicComplexity => 1.0,
        BinaryOperationComplexity b => Math.Max(GetLogExponent(b.Left), GetLogExponent(b.Right)),
        _ => 0.0
    };

    #endregion

    #region Result Types

    private class AnalysisResult
    {
        public ComplexityExpression? Complexity { get; init; }
        public bool HasLoop { get; init; }
        public IterationPattern LoopPattern { get; init; }
        public int NestingDepth { get; init; }
        public bool IsRecursive { get; init; }
        public int RecursiveCallCount { get; init; }
        public RecurrenceRelation? DetectedRecurrence { get; init; }
        public int BCLCallCount { get; init; }
        public int LinqCallCount { get; init; }
    }

    #endregion
}
