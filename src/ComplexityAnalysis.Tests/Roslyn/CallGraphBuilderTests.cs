using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ComplexityAnalysis.Roslyn.Analysis;
using Xunit;

namespace ComplexityAnalysis.Tests.Roslyn;

public class CallGraphBuilderTests
{
    private static Compilation CreateCompilation(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        return CSharpCompilation.Create("TestAssembly")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location))
            .AddSyntaxTrees(tree);
    }

    [Fact]
    public void Build_SingleMethod_NoCallees()
    {
        var code = @"
class Test {
    void Method() {
        int x = 1;
    }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var methods = graph.AllMethods.ToList();
        Assert.Single(methods);
    }

    [Fact]
    public void Build_TwoMethods_DetectsCall()
    {
        var code = @"
class Test {
    void Caller() {
        Callee();
    }

    void Callee() {
        int x = 1;
    }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var methods = graph.AllMethods.ToList();
        Assert.Equal(2, methods.Count);

        var caller = methods.First(m => m.Name == "Caller");
        var callees = graph.GetCallees(caller);
        Assert.Single(callees);
        Assert.Equal("Callee", callees.First().Name);
    }

    [Fact]
    public void Build_RecursiveMethod_DetectsRecursion()
    {
        var code = @"
class Test {
    int Factorial(int n) {
        if (n <= 1) return 1;
        return n * Factorial(n - 1);
    }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var methods = graph.AllMethods.ToList();
        Assert.Single(methods);

        var factorial = methods.First();
        Assert.True(graph.IsRecursive(factorial));
    }

    [Fact]
    public void Build_MutualRecursion_DetectsCycle()
    {
        var code = @"
class Test {
    void A(int n) {
        if (n > 0) B(n - 1);
    }

    void B(int n) {
        if (n > 0) A(n - 1);
    }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var methodA = graph.AllMethods.First(m => m.Name == "A");
        var methodB = graph.AllMethods.First(m => m.Name == "B");

        Assert.True(graph.IsRecursive(methodA));
        Assert.True(graph.IsRecursive(methodB));
    }

    [Fact]
    public void TopologicalSort_AcyclicGraph_ReturnsOrder()
    {
        var code = @"
class Test {
    void A() { B(); C(); }
    void B() { D(); }
    void C() { D(); }
    void D() { }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var sorted = graph.TopologicalSort();

        Assert.NotNull(sorted);
        Assert.Equal(4, sorted.Count);

        // D should come before B and C, which should come before A
        var sortedList = sorted.ToList();
        var indexD = sortedList.FindIndex(m => m.Name == "D");
        var indexB = sortedList.FindIndex(m => m.Name == "B");
        var indexC = sortedList.FindIndex(m => m.Name == "C");
        var indexA = sortedList.FindIndex(m => m.Name == "A");

        Assert.True(indexD < indexB);
        Assert.True(indexD < indexC);
        Assert.True(indexB < indexA);
        Assert.True(indexC < indexA);
    }

    [Fact]
    public void TopologicalSort_CyclicGraph_ReturnsNull()
    {
        var code = @"
class Test {
    void A(int n) {
        if (n > 0) B(n - 1);
    }

    void B(int n) {
        if (n > 0) A(n - 1);
    }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var sorted = graph.TopologicalSort();

        Assert.Null(sorted); // Cycle detected
    }

    [Fact]
    public void FindStronglyConnectedComponents_MutualRecursion_FindsSCC()
    {
        var code = @"
class Test {
    void A(int n) {
        if (n > 0) B(n - 1);
    }

    void B(int n) {
        if (n > 0) A(n - 1);
    }

    void C() { }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var sccs = builder.FindStronglyConnectedComponents();

        // Should have at least 2 SCCs: one containing A and B, one containing C
        Assert.True(sccs.Count >= 2);

        // Find the SCC containing A and B
        var mutualSCC = sccs.FirstOrDefault(scc => scc.Any(m => m.Name == "A"));
        Assert.NotNull(mutualSCC);
        Assert.Equal(2, mutualSCC.Count);
        Assert.Contains(mutualSCC, m => m.Name == "A");
        Assert.Contains(mutualSCC, m => m.Name == "B");
    }

    [Fact]
    public void FindEntryPoints_ReturnsMethodsWithNoCallers()
    {
        var code = @"
class Test {
    void Entry1() { Helper(); }
    void Entry2() { Helper(); }
    void Helper() { }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var entryPoints = graph.FindEntryPoints().ToList();

        Assert.Equal(2, entryPoints.Count);
        Assert.Contains(entryPoints, m => m.Name == "Entry1");
        Assert.Contains(entryPoints, m => m.Name == "Entry2");
    }

    [Fact]
    public void FindLeafMethods_ReturnsMethodsWithNoCallees()
    {
        var code = @"
class Test {
    void Caller() { Leaf1(); Leaf2(); }
    void Leaf1() { int x = 1; }
    void Leaf2() { int y = 2; }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var leaves = graph.FindLeafMethods().ToList();

        Assert.Equal(2, leaves.Count);
        Assert.Contains(leaves, m => m.Name == "Leaf1");
        Assert.Contains(leaves, m => m.Name == "Leaf2");
    }

    [Fact]
    public void FindMaxCallDepth_ReturnsCorrectDepth()
    {
        var code = @"
class Test {
    void A() { B(); }
    void B() { C(); }
    void C() { D(); }
    void D() { }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var methodA = graph.AllMethods.First(m => m.Name == "A");
        var depth = graph.FindMaxCallDepth(methodA);

        Assert.Equal(4, depth); // A -> B -> C -> D
    }

    [Fact]
    public void SetAndGetComplexity_WorksCorrectly()
    {
        var code = @"
class Test {
    void Method() { }
}";

        var compilation = CreateCompilation(code);
        var builder = new CallGraphBuilder(compilation);
        var graph = builder.Build();

        var method = graph.AllMethods.First();
        var complexity = new ComplexityAnalysis.Core.Complexity.ConstantComplexity(1);

        graph.SetComplexity(method, complexity);
        var retrieved = graph.GetComplexity(method);

        Assert.NotNull(retrieved);
        Assert.Equal(complexity, retrieved);
    }
}
