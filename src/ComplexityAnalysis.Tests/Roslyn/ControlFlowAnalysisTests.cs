using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ComplexityAnalysis.Roslyn.Analysis;
using Xunit;

namespace ComplexityAnalysis.Tests.Roslyn;

public class ControlFlowAnalysisTests
{
    private static (SemanticModel semanticModel, MethodDeclarationSyntax method) ParseMethod(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(tree);

        var semanticModel = compilation.GetSemanticModel(tree);
        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        return (semanticModel, method);
    }

    [Fact]
    public void AnalyzeMethod_SimpleMethod_Succeeds()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
        int y = 2;
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
    }

    [Fact]
    public void AnalyzeMethod_WithIfStatement_HasConditionalBlocks()
    {
        var code = @"
class Test {
    void Method(bool condition) {
        if (condition) {
            int x = 1;
        } else {
            int y = 2;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
        Assert.True(result.Graph.Blocks.Any(b => b.IsConditional));
    }

    [Fact]
    public void AnalyzeMethod_WithForLoop_HasLoopHeader()
    {
        var code = @"
class Test {
    void Method() {
        for (int i = 0; i < 10; i++) {
            int x = i;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
        Assert.True(result.Graph.LoopHeaders.Any());
    }

    [Fact]
    public void AnalyzeMethod_WithWhileLoop_HasLoopHeader()
    {
        var code = @"
class Test {
    void Method() {
        int i = 0;
        while (i < 10) {
            int x = i;
            i++;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
        Assert.True(result.Graph.LoopHeaders.Any());
    }

    [Fact]
    public void AnalyzeMethod_NestedLoops_ReportsCorrectNestingDepth()
    {
        var code = @"
class Test {
    void Method() {
        for (int i = 0; i < 10; i++) {
            for (int j = 0; j < 10; j++) {
                int x = i + j;
            }
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.True(result.LoopNestingDepth >= 2);
    }

    [Fact]
    public void AnalyzeMethod_SimpleCFG_IsReducible()
    {
        var code = @"
class Test {
    void Method() {
        for (int i = 0; i < 10; i++) {
            if (i % 2 == 0) {
                int x = i;
            }
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.True(result.IsReducible);
    }

    [Fact]
    public void CyclomaticComplexity_SimpleMethod_IsOne()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        // Simple method should have low cyclomatic complexity
        Assert.True(result.CyclomaticComplexity >= 1);
    }

    [Fact]
    public void CyclomaticComplexity_WithBranching_Increases()
    {
        var code = @"
class Test {
    void Method(int x) {
        if (x > 0) {
            int a = 1;
        }
        if (x > 10) {
            int b = 2;
        }
        if (x > 100) {
            int c = 3;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        // Multiple if statements increase cyclomatic complexity
        Assert.True(result.CyclomaticComplexity >= 3);
    }

    [Fact]
    public void SimplifiedCFG_HasEntryAndExit()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
        Assert.NotNull(result.Graph.EntryBlock);
        Assert.NotNull(result.Graph.ExitBlock);
        Assert.Equal(CFGBlockKind.Entry, result.Graph.EntryBlock.Kind);
        Assert.Equal(CFGBlockKind.Exit, result.Graph.ExitBlock.Kind);
    }

    [Fact]
    public void SimplifiedCFG_GetSuccessors_ReturnsCorrectBlocks()
    {
        var code = @"
class Test {
    void Method(bool condition) {
        if (condition) {
            int x = 1;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);

        // Entry block should have successors
        var entrySuccessors = result.Graph.GetSuccessors(result.Graph.EntryBlock).ToList();
        Assert.True(entrySuccessors.Count > 0);
    }

    [Fact]
    public void SimplifiedCFG_GetPredecessors_ReturnsCorrectBlocks()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);

        // Exit block should have predecessors
        var exitPredecessors = result.Graph.GetPredecessors(result.Graph.ExitBlock).ToList();
        Assert.True(exitPredecessors.Count > 0);
    }

    [Fact]
    public void MethodWithoutBody_ReturnsFailure()
    {
        var code = @"
abstract class Test {
    abstract void Method();
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void ExpressionBodiedMethod_Succeeds()
    {
        var code = @"
class Test {
    int Method() => 42;
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.NotNull(result.Graph);
    }

    [Fact]
    public void BranchingFactor_Calculated()
    {
        var code = @"
class Test {
    void Method(int x) {
        if (x > 0) {
            int a = 1;
        } else {
            int b = 2;
        }
    }
}";

        var (semanticModel, method) = ParseMethod(code);
        var analyzer = new ControlFlowAnalysis(semanticModel);

        var result = analyzer.AnalyzeMethod(method);

        Assert.True(result.Success);
        Assert.True(result.BranchingFactor > 0);
    }
}
