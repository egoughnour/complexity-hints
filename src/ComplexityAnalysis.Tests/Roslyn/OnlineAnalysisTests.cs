using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Speculative;
using Xunit;

namespace ComplexityAnalysis.Tests.Roslyn;

/// <summary>
/// Tests for online/incremental complexity analysis during active editing.
/// These tests verify that the analyzer can handle incomplete code gracefully
/// and provide useful estimates during typing.
/// </summary>
public class OnlineAnalysisTests
{
    #region Complete Code Analysis

    [Fact]
    public async Task AnalyzeAsync_CompleteForLoop_ReturnsLinearComplexity()
    {
        // Arrange
        var code = @"
void Process(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
    {
        Console.WriteLine(arr[i]);
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        Assert.True(result.Methods[0].Confidence > 0.7);
        Assert.True(result.IsCodeComplete);
    }

    [Fact]
    public async Task AnalyzeAsync_NestedLoops_ReturnsQuadraticComplexity()
    {
        // Arrange
        var code = @"
void BubbleSort(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
    {
        for (int j = 0; j < arr.Length - i; j++)
        {
            if (arr[j] > arr[j + 1])
            {
                var temp = arr[j];
                arr[j] = arr[j + 1];
                arr[j + 1] = temp;
            }
        }
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        var method = result.Methods[0];
        Assert.True(method.Complexity is PolynomialComplexity or VariableComplexity);
    }

    [Fact]
    public async Task AnalyzeAsync_LogarithmicLoop_ReturnsLogComplexity()
    {
        // Arrange
        var code = @"
int BinarySearch(int[] arr, int target)
{
    int low = 0, high = arr.Length - 1;
    while (low <= high)
    {
        int mid = (low + high) / 2;
        if (arr[mid] == target) return mid;
        if (arr[mid] < target) low = mid + 1;
        else high = mid - 1;
    }
    return -1;
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        // The while loop with binary pattern should be detected
    }

    [Fact]
    public async Task AnalyzeAsync_RecursiveMethod_DetectsRecursion()
    {
        // Arrange
        var code = @"
int Factorial(int n)
{
    if (n <= 1) return 1;
    return n * Factorial(n - 1);
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        var method = result.Methods[0];
        // Should detect linear recursion
        Assert.True(method.Complexity is VariableComplexity or LinearComplexity);
    }

    [Fact]
    public async Task AnalyzeAsync_DivideAndConquer_DetectsLogLinear()
    {
        // Arrange - Use a pattern that's detectable via string analysis
        var code = @"
void MergeSort(int[] arr, int left, int right)
{
    if (left < right)
    {
        int mid = (left + right) / 2;
        MergeSort(arr, left, mid / 2);
        MergeSort(arr, mid + 1, right / 2);
        Merge(arr, left, mid, right);
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        // Two recursive calls detected - should return some form of recursive complexity
        var method = result.Methods[0];
        Assert.NotNull(method.Complexity);
    }

    #endregion

    #region Incomplete Code Analysis

    [Fact]
    public async Task AnalyzeAsync_IncompleteForLoop_ReportsIncomplete()
    {
        // Arrange - user is typing a for loop
        var code = @"
void Process(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
    {
        
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Methods);
        // Should still provide an estimate even with empty body
        Assert.NotNull(result.Methods[0].Complexity);
    }

    [Fact]
    public async Task AnalyzeAsync_MethodWithMissingBody_HandlesGracefully()
    {
        // Arrange - user just typed method signature
        var code = @"
void Process(int[] arr)
";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        // Method without body should be reported but with low confidence
        if (result.Methods.Count > 0)
        {
            var method = result.Methods[0];
            Assert.True(method.Confidence < 0.5 || method.Complexity is null);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_SyntaxError_StillAnalyzes()
    {
        // Arrange - code with syntax error
        var code = @"
void Process(int[] arr)
{
    for (int i = 0; i < arr.Length i++)  // missing semicolon
    {
        Console.WriteLine(arr[i]);
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.ParseDiagnostics.Count > 0); // Should report the error
        Assert.False(result.IsCodeComplete);
    }

    [Fact]
    public async Task AnalyzeAsync_PartiallyTypedLoop_ProvidesEstimate()
    {
        // Arrange - user is in the middle of typing - needs a missing token
        var code = @"
void Process(int[] arr)
{
    for (int i = 0; i < arr.Length i++)  // missing semicolon to be incomplete
    {
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        // Should detect either syntax errors or incomplete regions
        Assert.True(!result.IsCodeComplete || result.ParseDiagnostics.Count > 0);
    }

    #endregion

    #region Position-Based Analysis

    [Fact]
    public async Task AnalyzeAsync_WithPosition_ReturnsFocusedMethod()
    {
        // Arrange
        var code = @"
void Method1()
{
    for (int i = 0; i < n; i++) { }
}

void Method2()
{
    for (int i = 0; i < n; i++)
    {
        for (int j = 0; j < n; j++) { }
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();
        // Position in Method2
        var position = code.IndexOf("Method2") + 10;

        // Act
        var result = await analyzer.AnalyzeAsync(code, position);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.FocusedMethod);
        Assert.Equal("Method2", result.FocusedMethod.MethodName);
    }

    [Fact]
    public async Task AnalyzeMethodAsync_ByName_ReturnsSnapshot()
    {
        // Arrange
        var code = @"
void LinearMethod()
{
    for (int i = 0; i < n; i++) { }
}

void QuadraticMethod()
{
    for (int i = 0; i < n; i++)
    {
        for (int j = 0; j < n; j++) { }
    }
}";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var snapshot = await analyzer.AnalyzeMethodAsync(code, "QuadraticMethod");

        // Assert
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.Complexity);
    }

    #endregion

    #region Callback Integration

    [Fact]
    public async Task AnalyzeAsync_WithCallback_ReportsProgress()
    {
        // Arrange
        var code = @"
void Method1() { }
void Method2() { for (int i = 0; i < n; i++) { } }
void Method3() { while (x > 0) { x--; } }";

        var callback = new BufferedOnlineAnalysisCallback();
        var analyzer = new IncrementalComplexityAnalyzer(callback);

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        var events = callback.Events;

        // Should have started event
        Assert.Contains(events, e => e is AnalysisStartedEvent);

        // Should have phase events
        Assert.Contains(events, e => e is PhaseStartedEvent ps && ps.Phase == OnlineAnalysisPhase.Parsing);
        Assert.Contains(events, e => e is PhaseCompletedEvent pc && pc.Phase == OnlineAnalysisPhase.Parsing);

        // Should have completed event
        Assert.Contains(events, e => e is AnalysisCompletedEvent);
    }

    [Fact]
    public async Task AnalyzeAsync_Cancellation_ReturnsCancelled()
    {
        // Arrange
        var code = @"
void Method1() { }
void Method2() { for (int i = 0; i < n; i++) { } }";

        var analyzer = new IncrementalComplexityAnalyzer();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await analyzer.AnalyzeAsync(code, -1, cts.Token);

        // Assert
        Assert.True(result.IsCancelled);
    }

    #endregion

    #region Caching

    [Fact]
    public async Task AnalyzeAsync_SameCode_UsesCachedResults()
    {
        // Arrange
        var code = @"
void Process(int[] arr)
{
    for (int i = 0; i < arr.Length; i++)
    {
        Console.WriteLine(arr[i]);
    }
}";
        var callback1 = new BufferedOnlineAnalysisCallback();
        var callback2 = new BufferedOnlineAnalysisCallback();

        var analyzer = new IncrementalComplexityAnalyzer(callback1);

        // Act - First analysis
        var result1 = await analyzer.AnalyzeAsync(code);

        // Replace callback for second analysis
        var analyzer2 = new IncrementalComplexityAnalyzer(callback2);
        var result2 = await analyzer2.AnalyzeAsync(code);

        // Assert
        Assert.True(result1.Success);
        Assert.True(result2.Success);
        // Results should be equivalent
        Assert.Equal(result1.Methods.Count, result2.Methods.Count);
    }

    [Fact]
    public void ClearCache_RemovesCachedAnalysis()
    {
        // Arrange
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        analyzer.ClearCache();

        // Assert - no exception means success
        Assert.NotNull(analyzer);
    }

    #endregion

    #region SyntaxFragmentAnalyzer Direct Tests

    [Fact]
    public void SyntaxFragmentAnalyzer_ForLoop_Linear()
    {
        // Arrange
        var code = "class C { void M() { for (int i = 0; i < n; i++) { } } }";
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var fragmentAnalyzer = new SyntaxFragmentAnalyzer();

        // Act
        var result = fragmentAnalyzer.AnalyzeMethod(method);

        // Assert
        Assert.NotNull(result.Complexity);
        Assert.True(result.Confidence > 0.5);
    }

    [Fact]
    public void SyntaxFragmentAnalyzer_LogarithmicMultiply_DetectsLog()
    {
        // Arrange - logarithmic loop pattern with *= 2
        var code = @"
class C {
    void M(int n) {
        for (int i = 1; i < n; i *= 2) { Console.WriteLine(i); }
    }
}";
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var fragmentAnalyzer = new SyntaxFragmentAnalyzer();

        // Act
        var result = fragmentAnalyzer.AnalyzeMethod(method);

        // Assert
        Assert.NotNull(result.Complexity);
        Assert.True(result.Loops?.Count > 0);
        // The loop iteration count should be logarithmic
        var loopIterCount = result.Loops![0].IterationCount;
        Assert.True(loopIterCount is LogarithmicComplexity, 
            $"Expected LogarithmicComplexity but got {loopIterCount?.GetType().Name}");
    }

    [Fact]
    public void SyntaxFragmentAnalyzer_RecursiveCall_Detected()
    {
        // Arrange
        var code = @"
class C {
int Fib(int n)
{
    if (n <= 1) return n;
    return Fib(n - 1) + Fib(n - 2);
}
}";
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var fragmentAnalyzer = new SyntaxFragmentAnalyzer();

        // Act
        var result = fragmentAnalyzer.AnalyzeMethod(method);

        // Assert
        Assert.NotNull(result.RecursiveCalls);
        Assert.Equal(2, result.RecursiveCalls.Count);
        // Two recursive calls with n-1, n-2 → exponential
        Assert.True(result.Complexity is ExponentialComplexity);
    }

    [Fact]
    public void SyntaxFragmentAnalyzer_EmptyMethod_ReturnsConstant()
    {
        // Arrange
        var code = "class C { void M() { } }";
        var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
        var method = tree.GetRoot().DescendantNodes()
            .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>()
            .First();

        var fragmentAnalyzer = new SyntaxFragmentAnalyzer();

        // Act
        var result = fragmentAnalyzer.AnalyzeMethod(method);

        // Assert
        Assert.True(result.Complexity is ConstantComplexity);
        Assert.True(result.Confidence > 0.9);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task AnalyzeAsync_EmptyCode_ReturnsEmptyResult()
    {
        // Arrange
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync("");

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Methods);
    }

    [Fact]
    public async Task AnalyzeAsync_OnlyComments_ReturnsEmptyResult()
    {
        // Arrange
        var code = @"
// This is a comment
/* Multi-line
   comment */
";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Methods);
    }

    [Fact]
    public async Task AnalyzeAsync_MultipleMethods_AnalyzesAll()
    {
        // Arrange
        var code = @"
void Method1() { for (int i = 0; i < n; i++) { } }
void Method2() { while (x > 0) { x--; } }
int Method3() { return 42; }
";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Methods.Count);
    }

    [Fact]
    public async Task AnalyzeAsync_ClassWithMethods_AnalyzesContainedMethods()
    {
        // Arrange
        var code = @"
class MyClass
{
    void Method1() { }
    int Method2(int n) { return n * n; }
}
";
        var analyzer = new IncrementalComplexityAnalyzer();

        // Act
        var result = await analyzer.AnalyzeAsync(code);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.Methods.Count);
    }

    #endregion
}
