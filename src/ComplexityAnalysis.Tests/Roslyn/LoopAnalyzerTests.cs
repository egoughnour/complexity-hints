using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.Analysis;
using Xunit;

namespace ComplexityAnalysis.Tests.Roslyn;

public class LoopAnalyzerTests
{
    private static (SemanticModel semanticModel, LoopAnalyzer analyzer, AnalysisContext context) Setup(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);
        var analyzer = new LoopAnalyzer(semanticModel);
        var context = new AnalysisContext { SemanticModel = semanticModel };

        return (semanticModel, analyzer, context);
    }

    [Fact]
    public void ForLoop_ZeroToN_ReturnsLinearBound()
    {
        var code = @"
class Test {
    void Method(int n) {
        for (int i = 0; i < n; i++) {
            int x = i;
        }
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        // Add the parameter to context
        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var forLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ForStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeForLoop(forLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Linear, result.Pattern);
        Assert.NotNull(result.IterationCount);
    }

    [Fact]
    public void ForLoop_Logarithmic_ReturnsLogBound()
    {
        var code = @"
class Test {
    void Method(int n) {
        for (int i = 1; i < n; i *= 2) {
            int x = i;
        }
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var forLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ForStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeForLoop(forLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Logarithmic, result.Pattern);
    }

    [Fact]
    public void ForLoop_ArrayLength_RecognizesBound()
    {
        var code = @"
class Test {
    void Method(int[] arr) {
        for (int i = 0; i < arr.Length; i++) {
            int x = arr[i];
        }
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            var variable = context.InferParameterVariable(paramSymbol);
            context = context.WithVariable(paramSymbol, variable);
        }

        var forLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ForStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeForLoop(forLoop, context);

        Assert.True(result.Success);
        Assert.NotNull(result.IterationCount);
    }

    [Fact]
    public void ForEachLoop_ReturnsCollectionSize()
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

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            var variable = context.InferParameterVariable(paramSymbol);
            context = context.WithVariable(paramSymbol, variable);
        }

        var foreachLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ForEachStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeForeachLoop(foreachLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Linear, result.Pattern);
    }

    [Fact]
    public void WhileLoop_WithIncrement_AnalyzesCorrectly()
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

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var whileLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<WhileStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeWhileLoop(whileLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Linear, result.Pattern);
    }

    [Fact]
    public void WhileLoop_Logarithmic_DetectsPattern()
    {
        var code = @"
class Test {
    void Method(int n) {
        int i = n;
        while (i > 1) {
            int x = i;
            i /= 2;
        }
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var whileLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<WhileStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeWhileLoop(whileLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Logarithmic, result.Pattern);
    }

    [Fact]
    public void DoWhileLoop_AnalyzesCorrectly()
    {
        var code = @"
class Test {
    void Method(int n) {
        int i = 0;
        do {
            int x = i;
            i++;
        } while (i < n);
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var doWhile = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<DoStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeDoWhileLoop(doWhile, context);

        Assert.True(result.Success);
        Assert.NotNull(result.Notes);
        Assert.Contains("at least once", result.Notes);
    }

    [Fact]
    public void ForLoop_StepByTwo_RecognizesLinear()
    {
        var code = @"
class Test {
    void Method(int n) {
        for (int i = 0; i < n; i += 2) {
            int x = i;
        }
    }
}";

        var (semanticModel, analyzer, context) = Setup(code);

        var methodSymbol = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var paramSymbol = semanticModel.GetDeclaredSymbol(methodSymbol)?.Parameters.First();
        if (paramSymbol != null)
        {
            context = context.WithVariable(paramSymbol, Variable.N);
        }

        var forLoop = semanticModel.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<ForStatementSyntax>()
            .First();

        var result = analyzer.AnalyzeForLoop(forLoop, context);

        Assert.True(result.Success);
        Assert.Equal(IterationPattern.Linear, result.Pattern);
        Assert.NotNull(result.Bound);
        Assert.Equal(2, ((ConstantComplexity)result.Bound.Step).Value);
    }

    [Fact]
    public void LoopBound_ZeroToN_ComputesIterationCount()
    {
        var bound = LoopBound.ZeroToN(Variable.N);

        Assert.Equal(IterationPattern.Linear, bound.Pattern);
        Assert.IsType<VariableComplexity>(bound.UpperBound);
    }

    [Fact]
    public void LoopBound_Logarithmic_ComputesIterationCount()
    {
        var bound = LoopBound.Logarithmic(Variable.N);

        Assert.Equal(IterationPattern.Logarithmic, bound.Pattern);
    }
}
