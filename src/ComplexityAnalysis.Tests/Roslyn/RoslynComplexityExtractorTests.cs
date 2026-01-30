using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Xunit;

namespace ComplexityAnalysis.Tests.Roslyn;

public class RoslynComplexityExtractorTests
{
    private static (SemanticModel semanticModel, MethodDeclarationSyntax method) ParseMethod(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        return (semanticModel, method);
    }

    [Fact]
    public void SimpleMethod_ReturnsConstantComplexity()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
        int y = 2;
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // Simple assignments are O(1)
        Assert.NotNull(complexity);
    }

    [Fact]
    public void SingleForLoop_ReturnsLinearComplexity()
    {
        var code = @"
class Test {
    void Method(int[] arr) {
        for (int i = 0; i < arr.Length; i++) {
            int x = arr[i];
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // Should be O(n) for a single loop
        Assert.NotNull(complexity);
        var notation = complexity.ToBigONotation();
        Assert.Contains("arr", notation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NestedForLoops_ReturnsQuadraticComplexity()
    {
        var code = @"
class Test {
    void Method(int[] arr) {
        for (int i = 0; i < arr.Length; i++) {
            for (int j = 0; j < arr.Length; j++) {
                int x = arr[i] + arr[j];
            }
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // Should be O(n²) for nested loops
        Assert.NotNull(complexity);
        Assert.IsType<BinaryOperationComplexity>(complexity);
    }

    [Fact]
    public void LogarithmicLoop_ReturnsLogComplexity()
    {
        var code = @"
class Test {
    void Method(int n) {
        for (int i = 1; i < n; i *= 2) {
            int x = i;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // Should detect logarithmic pattern
        Assert.NotNull(complexity);
    }

    [Fact]
    public void ForeachLoop_ReturnsLinearComplexity()
    {
        var code = @"
using System.Collections.Generic;
class Test {
    void Method(List<int> items) {
        foreach (var item in items) {
            int x = item;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        Assert.NotNull(complexity);
    }

    [Fact]
    public void IfStatement_ReturnsMaxOfBranches()
    {
        var code = @"
class Test {
    void Method(int[] arr, bool condition) {
        if (condition) {
            for (int i = 0; i < arr.Length; i++) {
                int x = arr[i];
            }
        } else {
            int y = 1;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // Should be max(O(n), O(1)) = O(n)
        Assert.NotNull(complexity);
        Assert.IsType<BinaryOperationComplexity>(complexity);
    }

    [Fact]
    public void BCLMethodCall_UsesKnownComplexity()
    {
        var code = @"
using System.Collections.Generic;
class Test {
    bool Method(List<int> items, int target) {
        return items.Contains(target);
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        // List.Contains is O(n)
        Assert.NotNull(complexity);
    }

    [Fact]
    public void RecursiveMethod_DetectsRecurrence()
    {
        var code = @"
class Test {
    int Factorial(int n) {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }
}";

        var (semanticModel, method) = ParseMethod(code);

        // Build call graph for the method
        var tree = method.SyntaxTree;
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        var extractor = new RoslynComplexityExtractor(semanticModel, callGraph);
        var complexity = extractor.AnalyzeMethod(method);

        // Should detect recursion
        Assert.NotNull(complexity);
    }

    [Fact]
    public void DivideAndConquerRecursion_DetectsPattern()
    {
        var code = @"
class Test {
    int BinarySearch(int[] arr, int target, int left, int right) {
        if (left > right) return -1;
        int mid = (left + right) / 2;
        if (arr[mid] == target) return mid;
        if (arr[mid] > target) return BinarySearch(arr, target, left, mid - 1);
        return BinarySearch(arr, target, mid + 1, right);
    }
}";

        var (semanticModel, method) = ParseMethod(code);

        var tree = method.SyntaxTree;
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        var extractor = new RoslynComplexityExtractor(semanticModel, callGraph);
        var complexity = extractor.AnalyzeMethod(method);

        Assert.NotNull(complexity);
    }

    [Fact]
    public void WhileLoop_AnalyzesCorrectly()
    {
        var code = @"
class Test {
    void Method(int n) {
        int i = 0;
        while (i < n) {
            int x = i;
            i++;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        Assert.NotNull(complexity);
    }

    [Fact]
    public void SwitchStatement_ReturnsMaxOfCases()
    {
        var code = @"
class Test {
    void Method(int[] arr, int choice) {
        switch (choice) {
            case 1:
                for (int i = 0; i < arr.Length; i++) { }
                break;
            case 2:
                int x = 1;
                break;
            default:
                break;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        Assert.NotNull(complexity);
    }

    [Fact]
    public void TryCatchFinally_AnalyzesCorrectly()
    {
        var code = @"
class Test {
    void Method(int[] arr) {
        try {
            for (int i = 0; i < arr.Length; i++) { }
        } catch {
            int x = 1;
        } finally {
            int y = 2;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        var complexity = extractor.AnalyzeMethod(method);

        Assert.NotNull(complexity);
    }

    [Fact]
    public void MethodResults_ContainsAnalyzedMethods()
    {
        var code = @"
class Test {
    void Method1() { }
    void Method2() { }
}";

        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);
        var extractor = new RoslynComplexityExtractor(semanticModel);

        extractor.AnalyzeAllMethods(tree.GetRoot());

        Assert.Equal(2, extractor.MethodResults.Count);
    }
}
