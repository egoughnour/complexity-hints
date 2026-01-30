using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Roslyn.BCL;
using ComplexityAnalysis.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace ComplexityAnalysis.Tests.Roslyn;

/// <summary>
/// Comprehensive data-driven tests for BCL complexity mappings.
/// Verifies that all documented BCL method complexities are correctly mapped.
/// </summary>
public class BCLComplexityMappingTests
{
    private readonly ITestOutputHelper _output;
    private readonly BCLComplexityMappings _mappings = BCLComplexityMappings.Instance;

    public BCLComplexityMappingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region List<T> Operations

    [Theory]
    [InlineData("List`1", "get_Count", "O(1)", false)]
    [InlineData("List`1", "get_Item", "O(1)", false)]
    [InlineData("List`1", "set_Item", "O(1)", false)]
    [InlineData("List`1", "Add", "O(1)", true)]
    [InlineData("List`1", "Clear", "O(n)", false)]
    [InlineData("List`1", "Contains", "O(n)", false)]
    [InlineData("List`1", "IndexOf", "O(n)", false)]
    [InlineData("List`1", "LastIndexOf", "O(n)", false)]
    [InlineData("List`1", "Remove", "O(n)", false)]
    [InlineData("List`1", "RemoveAt", "O(n)", false)]
    [InlineData("List`1", "Insert", "O(n)", false)]
    [InlineData("List`1", "RemoveAll", "O(n)", false)]
    [InlineData("List`1", "RemoveRange", "O(n)", false)]
    [InlineData("List`1", "Reverse", "O(n)", false)]
    [InlineData("List`1", "ToArray", "O(n)", false)]
    [InlineData("List`1", "CopyTo", "O(n)", false)]
    [InlineData("List`1", "Find", "O(n)", false)]
    [InlineData("List`1", "FindAll", "O(n)", false)]
    [InlineData("List`1", "FindIndex", "O(n)", false)]
    [InlineData("List`1", "Exists", "O(n)", false)]
    [InlineData("List`1", "TrueForAll", "O(n)", false)]
    [InlineData("List`1", "ForEach", "O(n)", false)]
    [InlineData("List`1", "Sort", "O(n log n)", false)]
    [InlineData("List`1", "BinarySearch", "O(log n)", false)]
    public void List_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity, bool isAmortized)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);

        if (isAmortized)
        {
            Assert.True(mapping.Notes.HasFlag(ComplexityNotes.Amortized),
                $"{methodName} should be marked as amortized");
        }
    }

    #endregion

    #region Dictionary<K,V> Operations

    [Theory]
    [InlineData("Dictionary`2", "get_Count", "O(1)", false)]
    [InlineData("Dictionary`2", "get_Item", "O(1)", true)]
    [InlineData("Dictionary`2", "set_Item", "O(1)", true)]
    [InlineData("Dictionary`2", "Add", "O(1)", true)]
    [InlineData("Dictionary`2", "TryAdd", "O(1)", true)]
    [InlineData("Dictionary`2", "ContainsKey", "O(1)", true)]
    [InlineData("Dictionary`2", "TryGetValue", "O(1)", true)]
    [InlineData("Dictionary`2", "Remove", "O(1)", true)]
    [InlineData("Dictionary`2", "ContainsValue", "O(n)", false)]
    [InlineData("Dictionary`2", "Clear", "O(n)", false)]
    public void Dictionary_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity, bool isAmortized)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);

        if (isAmortized)
        {
            Assert.True(mapping.Notes.HasFlag(ComplexityNotes.Amortized),
                $"{methodName} should be marked as amortized");
        }
    }

    #endregion

    #region HashSet<T> Operations

    [Theory]
    [InlineData("HashSet`1", "get_Count", "O(1)", false)]
    [InlineData("HashSet`1", "Add", "O(1)", true)]
    [InlineData("HashSet`1", "Contains", "O(1)", true)]
    [InlineData("HashSet`1", "Remove", "O(1)", true)]
    [InlineData("HashSet`1", "Clear", "O(n)", false)]
    [InlineData("HashSet`1", "UnionWith", "O(n)", false)]
    [InlineData("HashSet`1", "IntersectWith", "O(n)", false)]
    [InlineData("HashSet`1", "ExceptWith", "O(n)", false)]
    public void HashSet_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity, bool isAmortized)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region SortedSet<T> Operations

    [Theory]
    [InlineData("SortedSet`1", "get_Count", "O(1)")]
    [InlineData("SortedSet`1", "Add", "O(log n)")]
    [InlineData("SortedSet`1", "Contains", "O(log n)")]
    [InlineData("SortedSet`1", "Remove", "O(log n)")]
    [InlineData("SortedSet`1", "get_Min", "O(log n)")]
    [InlineData("SortedSet`1", "get_Max", "O(log n)")]
    public void SortedSet_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region Queue and Stack Operations

    [Theory]
    [InlineData("Queue`1", "get_Count", "O(1)")]
    [InlineData("Queue`1", "Enqueue", "O(1)")]
    [InlineData("Queue`1", "Dequeue", "O(1)")]
    [InlineData("Queue`1", "Peek", "O(1)")]
    [InlineData("Queue`1", "Contains", "O(n)")]
    [InlineData("Queue`1", "Clear", "O(n)")]
    [InlineData("Stack`1", "get_Count", "O(1)")]
    [InlineData("Stack`1", "Push", "O(1)")]
    [InlineData("Stack`1", "Pop", "O(1)")]
    [InlineData("Stack`1", "Peek", "O(1)")]
    [InlineData("Stack`1", "Contains", "O(n)")]
    [InlineData("Stack`1", "Clear", "O(n)")]
    public void QueueStack_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region LinkedList<T> Operations

    [Theory]
    [InlineData("LinkedList`1", "get_Count", "O(1)")]
    [InlineData("LinkedList`1", "get_First", "O(1)")]
    [InlineData("LinkedList`1", "get_Last", "O(1)")]
    [InlineData("LinkedList`1", "AddFirst", "O(1)")]
    [InlineData("LinkedList`1", "AddLast", "O(1)")]
    [InlineData("LinkedList`1", "RemoveFirst", "O(1)")]
    [InlineData("LinkedList`1", "RemoveLast", "O(1)")]
    [InlineData("LinkedList`1", "Contains", "O(n)")]
    [InlineData("LinkedList`1", "Find", "O(n)")]
    [InlineData("LinkedList`1", "Clear", "O(n)")]
    public void LinkedList_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region LINQ Operations

    [Theory]
    [InlineData("Enumerable", "Where", "O(n)", true)]
    [InlineData("Enumerable", "Select", "O(n)", true)]
    [InlineData("Enumerable", "SelectMany", "O(n)", true)]
    [InlineData("Enumerable", "Take", "O(n)", true)]
    [InlineData("Enumerable", "Skip", "O(n)", true)]
    [InlineData("Enumerable", "First", "O(1)", false)]
    [InlineData("Enumerable", "FirstOrDefault", "O(1)", false)]
    [InlineData("Enumerable", "Last", "O(n)", false)]
    [InlineData("Enumerable", "LastOrDefault", "O(n)", false)]
    [InlineData("Enumerable", "Single", "O(n)", false)]
    [InlineData("Enumerable", "SingleOrDefault", "O(n)", false)]
    [InlineData("Enumerable", "Count", "O(n)", false)]
    [InlineData("Enumerable", "Any", "O(n)", false)]
    [InlineData("Enumerable", "All", "O(n)", false)]
    [InlineData("Enumerable", "Sum", "O(n)", false)]
    [InlineData("Enumerable", "Average", "O(n)", false)]
    [InlineData("Enumerable", "Min", "O(n)", false)]
    [InlineData("Enumerable", "Max", "O(n)", false)]
    [InlineData("Enumerable", "Aggregate", "O(n)", false)]
    [InlineData("Enumerable", "ToList", "O(n)", false)]
    [InlineData("Enumerable", "ToArray", "O(n)", false)]
    [InlineData("Enumerable", "ToDictionary", "O(n)", false)]
    [InlineData("Enumerable", "ToHashSet", "O(n)", false)]
    [InlineData("Enumerable", "OrderBy", "O(n log n)", true)]
    [InlineData("Enumerable", "OrderByDescending", "O(n log n)", true)]
    [InlineData("Enumerable", "ThenBy", "O(n log n)", true)]
    [InlineData("Enumerable", "Distinct", "O(n)", true)]
    [InlineData("Enumerable", "Union", "O(n)", true)]
    [InlineData("Enumerable", "Intersect", "O(n)", true)]
    [InlineData("Enumerable", "Except", "O(n)", true)]
    [InlineData("Enumerable", "GroupBy", "O(n)", true)]
    [InlineData("Enumerable", "Join", "O(n)", true)]
    [InlineData("Enumerable", "GroupJoin", "O(n)", true)]
    [InlineData("Enumerable", "Reverse", "O(n)", false)]
    [InlineData("Enumerable", "Concat", "O(1)", true)]
    [InlineData("Enumerable", "Append", "O(1)", true)]
    [InlineData("Enumerable", "Prepend", "O(1)", true)]
    public void Linq_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity, bool isDeferred)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()} (deferred={isDeferred})");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);

        if (isDeferred)
        {
            Assert.True(mapping.Notes.HasFlag(ComplexityNotes.DeferredExecution),
                $"{methodName} should be marked as deferred execution");
        }
    }

    #endregion

    #region String Operations

    [Theory]
    [InlineData("String", "get_Length", "O(1)")]
    [InlineData("String", "get_Chars", "O(1)")]
    [InlineData("String", "IndexOf", "O(n)")]
    [InlineData("String", "LastIndexOf", "O(n)")]
    [InlineData("String", "Contains", "O(n)")]
    [InlineData("String", "StartsWith", "O(n)")]
    [InlineData("String", "EndsWith", "O(n)")]
    [InlineData("String", "Substring", "O(n)")]
    [InlineData("String", "ToLower", "O(n)")]
    [InlineData("String", "ToUpper", "O(n)")]
    [InlineData("String", "Trim", "O(n)")]
    [InlineData("String", "TrimStart", "O(n)")]
    [InlineData("String", "TrimEnd", "O(n)")]
    [InlineData("String", "Split", "O(n)")]
    [InlineData("String", "Replace", "O(n)")]
    [InlineData("String", "Concat", "O(n)")]
    [InlineData("String", "Join", "O(n)")]
    public void String_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region StringBuilder Operations

    [Theory]
    [InlineData("StringBuilder", "get_Length", "O(1)")]
    [InlineData("StringBuilder", "get_Capacity", "O(1)")]
    [InlineData("StringBuilder", "Append", "O(1)")]
    [InlineData("StringBuilder", "AppendLine", "O(1)")]
    [InlineData("StringBuilder", "Insert", "O(n)")]
    [InlineData("StringBuilder", "Remove", "O(n)")]
    [InlineData("StringBuilder", "Replace", "O(n)")]
    [InlineData("StringBuilder", "Clear", "O(1)")]
    [InlineData("StringBuilder", "ToString", "O(n)")]
    public void StringBuilder_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region Array Operations

    [Theory]
    [InlineData("Array", "get_Length", "O(1)")]
    [InlineData("Array", "Sort", "O(n log n)")]
    [InlineData("Array", "BinarySearch", "O(log n)")]
    [InlineData("Array", "IndexOf", "O(n)")]
    [InlineData("Array", "LastIndexOf", "O(n)")]
    [InlineData("Array", "Copy", "O(n)")]
    [InlineData("Array", "Clear", "O(n)")]
    [InlineData("Array", "Fill", "O(n)")]
    [InlineData("Array", "Reverse", "O(n)")]
    [InlineData("Array", "Resize", "O(n)")]
    public void Array_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region Concurrent Collections

    [Theory]
    [InlineData("ConcurrentDictionary`2", "TryAdd", "O(1)")]
    [InlineData("ConcurrentDictionary`2", "TryGetValue", "O(1)")]
    [InlineData("ConcurrentDictionary`2", "TryRemove", "O(1)")]
    [InlineData("ConcurrentDictionary`2", "ContainsKey", "O(1)")]
    [InlineData("ConcurrentDictionary`2", "get_Count", "O(n)")]
    [InlineData("ConcurrentDictionary`2", "Clear", "O(n)")]
    [InlineData("ConcurrentQueue`1", "Enqueue", "O(1)")]
    [InlineData("ConcurrentQueue`1", "TryDequeue", "O(1)")]
    [InlineData("ConcurrentQueue`1", "TryPeek", "O(1)")]
    [InlineData("ConcurrentStack`1", "Push", "O(1)")]
    [InlineData("ConcurrentStack`1", "TryPop", "O(1)")]
    [InlineData("ConcurrentStack`1", "TryPeek", "O(1)")]
    [InlineData("ConcurrentBag`1", "Add", "O(1)")]
    [InlineData("ConcurrentBag`1", "TryTake", "O(1)")]
    public void ConcurrentCollections_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);

        // Concurrent collections should be thread-safe
        if (!typeName.StartsWith("ConcurrentDictionary") || methodName != "get_Count")
        {
            Assert.True(mapping.Notes.HasFlag(ComplexityNotes.ThreadSafe),
                $"{typeName}.{methodName} should be marked as thread-safe");
        }
    }

    #endregion

    #region Span and Memory Operations

    [Theory]
    [InlineData("Span`1", "get_Length", "O(1)")]
    [InlineData("Span`1", "get_Item", "O(1)")]
    [InlineData("Span`1", "Slice", "O(1)")]
    [InlineData("Span`1", "CopyTo", "O(n)")]
    [InlineData("Span`1", "Fill", "O(n)")]
    [InlineData("Span`1", "Clear", "O(n)")]
    [InlineData("ReadOnlySpan`1", "get_Length", "O(1)")]
    [InlineData("ReadOnlySpan`1", "get_Item", "O(1)")]
    [InlineData("ReadOnlySpan`1", "Slice", "O(1)")]
    [InlineData("ReadOnlySpan`1", "CopyTo", "O(n)")]
    [InlineData("Memory`1", "get_Length", "O(1)")]
    [InlineData("Memory`1", "Slice", "O(1)")]
    [InlineData("Memory`1", "get_Span", "O(1)")]
    public void SpanMemory_MethodComplexity_MatchesDocumented(
        string typeName, string methodName, string expectedComplexity)
    {
        var mapping = _mappings.GetComplexity(typeName, methodName);

        _output.WriteLine($"{typeName}.{methodName}: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        AssertComplexityMatches(mapping.Expression, expectedComplexity);
    }

    #endregion

    #region Regex (with Backtracking Warning)

    [Fact]
    public void Regex_Match_HasBacktrackingWarning()
    {
        var mapping = _mappings.GetComplexity("Regex", "Match");

        _output.WriteLine($"Regex.Match: {mapping.Expression.ToBigONotation()}");
        _output.WriteLine($"Notes: {mapping.Notes}");

        Assert.NotNull(mapping);
        Assert.True(mapping.Notes.HasFlag(ComplexityNotes.BacktrackingWarning),
            "Regex.Match should have backtracking warning");
    }

    [Fact]
    public void Regex_Replace_HasBacktrackingWarning()
    {
        var mapping = _mappings.GetComplexity("Regex", "Replace");

        _output.WriteLine($"Regex.Replace: {mapping.Expression.ToBigONotation()}");

        Assert.NotNull(mapping);
        Assert.True(mapping.Notes.HasFlag(ComplexityNotes.BacktrackingWarning),
            "Regex.Replace should have backtracking warning");
    }

    #endregion

    #region Default/Fallback Behavior

    [Fact]
    public void UnknownMethod_ReturnsConservativeDefault()
    {
        var mapping = _mappings.GetComplexity("SomeUnknownType", "SomeUnknownMethod");

        _output.WriteLine($"Unknown method: {mapping.Expression.ToBigONotation()}");
        _output.WriteLine($"Source: {mapping.Source.Type}");

        Assert.NotNull(mapping);
        // Should return conservative O(n) estimate
        AssertComplexityMatches(mapping.Expression, "O(n)");
        Assert.Equal(SourceType.Heuristic, mapping.Source.Type);
        Assert.True(mapping.Notes.HasFlag(ComplexityNotes.Unknown));
    }

    [Fact]
    public void SourceAttribution_IsProperlySet()
    {
        // Documented source
        var listAdd = _mappings.GetComplexity("List`1", "Add");
        Assert.Equal(SourceType.Documented, listAdd.Source.Type);
        Assert.Contains("MSDN", listAdd.Source.Citation);

        // Check that we can identify source types
        var dictAdd = _mappings.GetComplexity("Dictionary`2", "Add");
        Assert.NotNull(dictAdd.Source.Citation);
    }

    #endregion

    #region Helper Methods

    private static void AssertComplexityMatches(ComplexityExpression actual, string expected)
    {
        var actualBigO = actual.ToBigONotation().ToLower().Replace(" ", "");
        var expectedNorm = expected.ToLower().Replace(" ", "");

        // Handle common variations
        var variations = new[]
        {
            expectedNorm,
            expectedNorm.Replace("log", "log₂"),
            expectedNorm.Replace("nlogn", "n·logn"),
            expectedNorm.Replace("nlogn", "n·log₂n"),
        };

        var matches = variations.Any(v =>
            actualBigO.Contains(v.Replace("o(", "").Replace(")", "")) ||
            actualBigO == v);

        Assert.True(matches,
            $"Expected complexity {expected} but got {actual.ToBigONotation()}");
    }

    #endregion
}
