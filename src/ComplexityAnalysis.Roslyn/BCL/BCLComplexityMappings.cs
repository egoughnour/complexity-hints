using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Roslyn.BCL;

/// <summary>
/// Central registry for BCL method complexity mappings.
///
/// Source attribution levels:
/// - Documented: Official Microsoft documentation explicitly states complexity
/// - Attested: Widely accepted, verified through source inspection (e.g., .NET runtime source)
/// - Empirical: Measured through benchmarking
/// - Heuristic: Conservative estimate based on algorithm analysis
///
/// Strategy: When in doubt, we overestimate (prefer false positives to false negatives).
/// </summary>
public sealed class BCLComplexityMappings
{
    private readonly ImmutableDictionary<MethodSignature, ComplexityMapping> _mappings;
    private readonly ComplexityMapping _defaultMapping;

    private BCLComplexityMappings(ImmutableDictionary<MethodSignature, ComplexityMapping> mappings)
    {
        _mappings = mappings;
        _defaultMapping = new ComplexityMapping(
            new VariableComplexity(Variable.N),
            ComplexitySource.Heuristic("Unknown method - conservative O(n) estimate"),
            ComplexityNotes.Unknown);
    }

    /// <summary>
    /// Gets the complexity mapping for a method, or a conservative default.
    /// </summary>
    public ComplexityMapping GetComplexity(string typeName, string methodName, int argCount = -1)
    {
        var sig = new MethodSignature(typeName, methodName, argCount);

        // Try exact match first
        if (_mappings.TryGetValue(sig, out var mapping))
            return mapping;

        // Try without arg count
        sig = sig with { ArgumentCount = -1 };
        if (_mappings.TryGetValue(sig, out mapping))
            return mapping;

        // Try base type mappings (e.g., IList<T> for List<T>)
        foreach (var baseType in GetBaseTypes(typeName))
        {
            sig = new MethodSignature(baseType, methodName, argCount);
            if (_mappings.TryGetValue(sig, out mapping))
                return mapping;
        }

        return _defaultMapping;
    }

    private static IEnumerable<string> GetBaseTypes(string typeName)
    {
        // Common interface hierarchies
        if (typeName.Contains("List"))
            yield return "IList`1";
        if (typeName.Contains("Dictionary") || typeName.Contains("HashSet"))
            yield return "ICollection`1";
        if (typeName.Contains("Enumerable") || typeName.EndsWith("[]"))
            yield return "IEnumerable`1";
    }

    /// <summary>
    /// Creates the complete BCL mappings registry.
    /// </summary>
    public static BCLComplexityMappings Create()
    {
        var builder = ImmutableDictionary.CreateBuilder<MethodSignature, ComplexityMapping>();

        // Add all mappings
        AddCollectionsGeneric(builder);
        AddLinqMappings(builder);
        AddStringMappings(builder);
        AddConcurrentCollections(builder);
        AddRegexMappings(builder);
        AddArrayMappings(builder);
        AddSpanMappings(builder);

        return new BCLComplexityMappings(builder.ToImmutable());
    }

    public static BCLComplexityMappings Instance { get; } = Create();

    #region Collections.Generic Mappings

    private static void AddCollectionsGeneric(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        // === List<T> ===
        var listType = "List`1";

        // O(1) operations
        builder.Add(new MethodSignature(listType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: List<T>.Count property is O(1)")));
        builder.Add(new MethodSignature(listType, "get_Item"),
            O1(ComplexitySource.Documented("MSDN: List<T> indexer is O(1)")));
        builder.Add(new MethodSignature(listType, "set_Item"),
            O1(ComplexitySource.Documented("MSDN: List<T> indexer is O(1)")));
        builder.Add(new MethodSignature(listType, "Add"),
            Amortized(O1(ComplexitySource.Documented("MSDN: List<T>.Add is O(1) amortized"))));
        builder.Add(new MethodSignature(listType, "Clear"),
            On(ComplexitySource.Documented("MSDN: List<T>.Clear is O(n)")));

        // O(n) operations
        builder.Add(new MethodSignature(listType, "Contains"),
            On(ComplexitySource.Documented("MSDN: List<T>.Contains is O(n)")));
        builder.Add(new MethodSignature(listType, "IndexOf"),
            On(ComplexitySource.Documented("MSDN: List<T>.IndexOf is O(n)")));
        builder.Add(new MethodSignature(listType, "LastIndexOf"),
            On(ComplexitySource.Documented("MSDN: List<T>.LastIndexOf is O(n)")));
        builder.Add(new MethodSignature(listType, "Remove"),
            On(ComplexitySource.Documented("MSDN: List<T>.Remove is O(n)")));
        builder.Add(new MethodSignature(listType, "RemoveAt"),
            On(ComplexitySource.Documented("MSDN: List<T>.RemoveAt is O(n)")));
        builder.Add(new MethodSignature(listType, "Insert"),
            On(ComplexitySource.Documented("MSDN: List<T>.Insert is O(n)")));
        builder.Add(new MethodSignature(listType, "InsertRange"),
            OnPlusM(ComplexitySource.Documented("MSDN: O(n + count)")));
        builder.Add(new MethodSignature(listType, "RemoveAll"),
            On(ComplexitySource.Documented("MSDN: List<T>.RemoveAll is O(n)")));
        builder.Add(new MethodSignature(listType, "RemoveRange"),
            On(ComplexitySource.Documented("MSDN: List<T>.RemoveRange is O(n)")));
        builder.Add(new MethodSignature(listType, "Reverse"),
            On(ComplexitySource.Documented("MSDN: List<T>.Reverse is O(n)")));
        builder.Add(new MethodSignature(listType, "ToArray"),
            On(ComplexitySource.Documented("MSDN: List<T>.ToArray is O(n)")));
        builder.Add(new MethodSignature(listType, "CopyTo"),
            On(ComplexitySource.Documented("MSDN: List<T>.CopyTo is O(n)")));
        builder.Add(new MethodSignature(listType, "Find"),
            On(ComplexitySource.Documented("MSDN: List<T>.Find is O(n)")));
        builder.Add(new MethodSignature(listType, "FindAll"),
            On(ComplexitySource.Documented("MSDN: List<T>.FindAll is O(n)")));
        builder.Add(new MethodSignature(listType, "FindIndex"),
            On(ComplexitySource.Documented("MSDN: List<T>.FindIndex is O(n)")));
        builder.Add(new MethodSignature(listType, "Exists"),
            On(ComplexitySource.Documented("MSDN: List<T>.Exists is O(n)")));
        builder.Add(new MethodSignature(listType, "TrueForAll"),
            On(ComplexitySource.Documented("MSDN: List<T>.TrueForAll is O(n)")));
        builder.Add(new MethodSignature(listType, "ForEach"),
            On(ComplexitySource.Attested("Iterates all elements")));
        builder.Add(new MethodSignature(listType, "GetRange"),
            On(ComplexitySource.Documented("MSDN: O(count)")));
        builder.Add(new MethodSignature(listType, "AddRange"),
            On(ComplexitySource.Documented("MSDN: O(count)")));

        // O(n log n) operations
        builder.Add(new MethodSignature(listType, "Sort"),
            ONLogN(ComplexitySource.Documented("MSDN: List<T>.Sort is O(n log n) on average")));
        builder.Add(new MethodSignature(listType, "BinarySearch"),
            OLogN(ComplexitySource.Documented("MSDN: List<T>.BinarySearch is O(log n)")));

        // === Dictionary<TKey, TValue> ===
        var dictType = "Dictionary`2";

        // O(1) amortized operations (hash-based)
        builder.Add(new MethodSignature(dictType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: Dictionary.Count is O(1)")));
        builder.Add(new MethodSignature(dictType, "get_Item"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary indexer is O(1) approaching"))));
        builder.Add(new MethodSignature(dictType, "set_Item"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary indexer is O(1) approaching"))));
        builder.Add(new MethodSignature(dictType, "Add"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary.Add is O(1) approaching"))));
        builder.Add(new MethodSignature(dictType, "TryAdd"),
            Amortized(O1(ComplexitySource.Attested("Same as Add"))));
        builder.Add(new MethodSignature(dictType, "ContainsKey"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary.ContainsKey is O(1) approaching"))));
        builder.Add(new MethodSignature(dictType, "TryGetValue"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary.TryGetValue is O(1) approaching"))));
        builder.Add(new MethodSignature(dictType, "Remove"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Dictionary.Remove is O(1) approaching"))));

        // O(n) operations
        builder.Add(new MethodSignature(dictType, "ContainsValue"),
            On(ComplexitySource.Documented("MSDN: Dictionary.ContainsValue is O(n)")));
        builder.Add(new MethodSignature(dictType, "Clear"),
            On(ComplexitySource.Documented("MSDN: Dictionary.Clear is O(n)")));
        builder.Add(new MethodSignature(dictType, "get_Keys"),
            On(ComplexitySource.Heuristic("Creates snapshot of keys")));
        builder.Add(new MethodSignature(dictType, "get_Values"),
            On(ComplexitySource.Heuristic("Creates snapshot of values")));

        // === HashSet<T> ===
        var hashSetType = "HashSet`1";

        builder.Add(new MethodSignature(hashSetType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: HashSet.Count is O(1)")));
        builder.Add(new MethodSignature(hashSetType, "Add"),
            Amortized(O1(ComplexitySource.Documented("MSDN: HashSet.Add is O(1)"))));
        builder.Add(new MethodSignature(hashSetType, "Contains"),
            Amortized(O1(ComplexitySource.Documented("MSDN: HashSet.Contains is O(1)"))));
        builder.Add(new MethodSignature(hashSetType, "Remove"),
            Amortized(O1(ComplexitySource.Documented("MSDN: HashSet.Remove is O(1)"))));
        builder.Add(new MethodSignature(hashSetType, "Clear"),
            On(ComplexitySource.Documented("MSDN: HashSet.Clear is O(n)")));

        // Set operations - O(n + m)
        builder.Add(new MethodSignature(hashSetType, "UnionWith"),
            OnPlusM(ComplexitySource.Documented("MSDN: O(n + m)")));
        builder.Add(new MethodSignature(hashSetType, "IntersectWith"),
            OnPlusM(ComplexitySource.Documented("MSDN: O(n + m)")));
        builder.Add(new MethodSignature(hashSetType, "ExceptWith"),
            On(ComplexitySource.Documented("MSDN: O(n)")));
        builder.Add(new MethodSignature(hashSetType, "SymmetricExceptWith"),
            OnPlusM(ComplexitySource.Documented("MSDN: O(n + m)")));
        builder.Add(new MethodSignature(hashSetType, "IsSubsetOf"),
            On(ComplexitySource.Documented("MSDN: O(n)")));
        builder.Add(new MethodSignature(hashSetType, "IsSupersetOf"),
            On(ComplexitySource.Documented("MSDN: O(m)")));
        builder.Add(new MethodSignature(hashSetType, "Overlaps"),
            On(ComplexitySource.Documented("MSDN: O(n)")));
        builder.Add(new MethodSignature(hashSetType, "SetEquals"),
            On(ComplexitySource.Documented("MSDN: O(n)")));

        // === SortedSet<T> ===
        var sortedSetType = "SortedSet`1";

        builder.Add(new MethodSignature(sortedSetType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: SortedSet.Count is O(1)")));
        builder.Add(new MethodSignature(sortedSetType, "get_Min"),
            OLogN(ComplexitySource.Documented("MSDN: SortedSet.Min is O(log n)")));
        builder.Add(new MethodSignature(sortedSetType, "get_Max"),
            OLogN(ComplexitySource.Documented("MSDN: SortedSet.Max is O(log n)")));
        builder.Add(new MethodSignature(sortedSetType, "Add"),
            OLogN(ComplexitySource.Documented("MSDN: SortedSet.Add is O(log n)")));
        builder.Add(new MethodSignature(sortedSetType, "Contains"),
            OLogN(ComplexitySource.Documented("MSDN: SortedSet.Contains is O(log n)")));
        builder.Add(new MethodSignature(sortedSetType, "Remove"),
            OLogN(ComplexitySource.Documented("MSDN: SortedSet.Remove is O(log n)")));
        builder.Add(new MethodSignature(sortedSetType, "Clear"),
            On(ComplexitySource.Attested("Must deallocate tree nodes")));
        builder.Add(new MethodSignature(sortedSetType, "GetViewBetween"),
            OLogN(ComplexitySource.Documented("MSDN: O(log n)")));

        // === SortedDictionary<TKey, TValue> ===
        var sortedDictType = "SortedDictionary`2";

        builder.Add(new MethodSignature(sortedDictType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: SortedDictionary.Count is O(1)")));
        builder.Add(new MethodSignature(sortedDictType, "get_Item"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary indexer is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "set_Item"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary indexer is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "Add"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary.Add is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "ContainsKey"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary.ContainsKey is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "TryGetValue"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary.TryGetValue is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "Remove"),
            OLogN(ComplexitySource.Documented("MSDN: SortedDictionary.Remove is O(log n)")));
        builder.Add(new MethodSignature(sortedDictType, "ContainsValue"),
            On(ComplexitySource.Documented("MSDN: SortedDictionary.ContainsValue is O(n)")));
        builder.Add(new MethodSignature(sortedDictType, "Clear"),
            On(ComplexitySource.Attested("Must deallocate tree nodes")));

        // === SortedList<TKey, TValue> ===
        var sortedListType = "SortedList`2";

        builder.Add(new MethodSignature(sortedListType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: SortedList.Count is O(1)")));
        builder.Add(new MethodSignature(sortedListType, "get_Item"),
            OLogN(ComplexitySource.Documented("MSDN: SortedList indexer get is O(log n)")));
        builder.Add(new MethodSignature(sortedListType, "set_Item"),
            On(ComplexitySource.Documented("MSDN: SortedList indexer set is O(n) for new keys")));
        builder.Add(new MethodSignature(sortedListType, "Add"),
            On(ComplexitySource.Documented("MSDN: SortedList.Add is O(n)")));
        builder.Add(new MethodSignature(sortedListType, "ContainsKey"),
            OLogN(ComplexitySource.Documented("MSDN: SortedList.ContainsKey is O(log n)")));
        builder.Add(new MethodSignature(sortedListType, "ContainsValue"),
            On(ComplexitySource.Documented("MSDN: SortedList.ContainsValue is O(n)")));
        builder.Add(new MethodSignature(sortedListType, "TryGetValue"),
            OLogN(ComplexitySource.Documented("MSDN: SortedList.TryGetValue is O(log n)")));
        builder.Add(new MethodSignature(sortedListType, "Remove"),
            On(ComplexitySource.Documented("MSDN: SortedList.Remove is O(n)")));
        builder.Add(new MethodSignature(sortedListType, "RemoveAt"),
            On(ComplexitySource.Documented("MSDN: SortedList.RemoveAt is O(n)")));
        builder.Add(new MethodSignature(sortedListType, "IndexOfKey"),
            OLogN(ComplexitySource.Documented("MSDN: SortedList.IndexOfKey is O(log n)")));
        builder.Add(new MethodSignature(sortedListType, "IndexOfValue"),
            On(ComplexitySource.Documented("MSDN: SortedList.IndexOfValue is O(n)")));

        // === Queue<T> ===
        var queueType = "Queue`1";

        builder.Add(new MethodSignature(queueType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: Queue.Count is O(1)")));
        builder.Add(new MethodSignature(queueType, "Enqueue"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Queue.Enqueue is O(1) amortized"))));
        builder.Add(new MethodSignature(queueType, "Dequeue"),
            O1(ComplexitySource.Documented("MSDN: Queue.Dequeue is O(1)")));
        builder.Add(new MethodSignature(queueType, "Peek"),
            O1(ComplexitySource.Documented("MSDN: Queue.Peek is O(1)")));
        builder.Add(new MethodSignature(queueType, "TryDequeue"),
            O1(ComplexitySource.Attested("Same as Dequeue")));
        builder.Add(new MethodSignature(queueType, "TryPeek"),
            O1(ComplexitySource.Attested("Same as Peek")));
        builder.Add(new MethodSignature(queueType, "Contains"),
            On(ComplexitySource.Documented("MSDN: Queue.Contains is O(n)")));
        builder.Add(new MethodSignature(queueType, "Clear"),
            On(ComplexitySource.Documented("MSDN: Queue.Clear is O(n)")));
        builder.Add(new MethodSignature(queueType, "ToArray"),
            On(ComplexitySource.Documented("MSDN: Queue.ToArray is O(n)")));

        // === Stack<T> ===
        var stackType = "Stack`1";

        builder.Add(new MethodSignature(stackType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: Stack.Count is O(1)")));
        builder.Add(new MethodSignature(stackType, "Push"),
            Amortized(O1(ComplexitySource.Documented("MSDN: Stack.Push is O(1) amortized"))));
        builder.Add(new MethodSignature(stackType, "Pop"),
            O1(ComplexitySource.Documented("MSDN: Stack.Pop is O(1)")));
        builder.Add(new MethodSignature(stackType, "Peek"),
            O1(ComplexitySource.Documented("MSDN: Stack.Peek is O(1)")));
        builder.Add(new MethodSignature(stackType, "TryPop"),
            O1(ComplexitySource.Attested("Same as Pop")));
        builder.Add(new MethodSignature(stackType, "TryPeek"),
            O1(ComplexitySource.Attested("Same as Peek")));
        builder.Add(new MethodSignature(stackType, "Contains"),
            On(ComplexitySource.Documented("MSDN: Stack.Contains is O(n)")));
        builder.Add(new MethodSignature(stackType, "Clear"),
            On(ComplexitySource.Documented("MSDN: Stack.Clear is O(n)")));
        builder.Add(new MethodSignature(stackType, "ToArray"),
            On(ComplexitySource.Documented("MSDN: Stack.ToArray is O(n)")));

        // === LinkedList<T> ===
        var linkedListType = "LinkedList`1";

        builder.Add(new MethodSignature(linkedListType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.Count is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "get_First"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.First is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "get_Last"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.Last is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "AddFirst"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.AddFirst is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "AddLast"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.AddLast is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "AddBefore"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.AddBefore is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "AddAfter"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.AddAfter is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "Remove"),
            On(ComplexitySource.Documented("MSDN: LinkedList.Remove(T) is O(n)")));
        builder.Add(new MethodSignature(linkedListType, "RemoveFirst"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.RemoveFirst is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "RemoveLast"),
            O1(ComplexitySource.Documented("MSDN: LinkedList.RemoveLast is O(1)")));
        builder.Add(new MethodSignature(linkedListType, "Contains"),
            On(ComplexitySource.Documented("MSDN: LinkedList.Contains is O(n)")));
        builder.Add(new MethodSignature(linkedListType, "Find"),
            On(ComplexitySource.Documented("MSDN: LinkedList.Find is O(n)")));
        builder.Add(new MethodSignature(linkedListType, "FindLast"),
            On(ComplexitySource.Documented("MSDN: LinkedList.FindLast is O(n)")));
        builder.Add(new MethodSignature(linkedListType, "Clear"),
            On(ComplexitySource.Documented("MSDN: LinkedList.Clear is O(n)")));

        // === PriorityQueue<TElement, TPriority> (.NET 6+) ===
        var priorityQueueType = "PriorityQueue`2";

        builder.Add(new MethodSignature(priorityQueueType, "get_Count"),
            O1(ComplexitySource.Documented("MSDN: PriorityQueue.Count is O(1)")));
        builder.Add(new MethodSignature(priorityQueueType, "Enqueue"),
            OLogN(ComplexitySource.Documented("MSDN: PriorityQueue.Enqueue is O(log n)")));
        builder.Add(new MethodSignature(priorityQueueType, "Dequeue"),
            OLogN(ComplexitySource.Documented("MSDN: PriorityQueue.Dequeue is O(log n)")));
        builder.Add(new MethodSignature(priorityQueueType, "Peek"),
            O1(ComplexitySource.Documented("MSDN: PriorityQueue.Peek is O(1)")));
        builder.Add(new MethodSignature(priorityQueueType, "TryDequeue"),
            OLogN(ComplexitySource.Attested("Same as Dequeue")));
        builder.Add(new MethodSignature(priorityQueueType, "TryPeek"),
            O1(ComplexitySource.Attested("Same as Peek")));
        builder.Add(new MethodSignature(priorityQueueType, "EnqueueDequeue"),
            OLogN(ComplexitySource.Documented("MSDN: PriorityQueue.EnqueueDequeue is O(log n)")));
        builder.Add(new MethodSignature(priorityQueueType, "Clear"),
            On(ComplexitySource.Attested("Clears backing array")));
    }

    #endregion

    #region LINQ Mappings

    private static void AddLinqMappings(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        var enumerable = "Enumerable";

        // === Deferred execution (O(1) to create, O(n) to enumerate) ===
        // These return immediately but cost O(n) when materialized

        builder.Add(new MethodSignature(enumerable, "Where"),
            Deferred(On(ComplexitySource.Documented("LINQ: Where is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Select"),
            Deferred(On(ComplexitySource.Documented("LINQ: Select is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "SelectMany"),
            Deferred(OnTimesM(ComplexitySource.Documented("LINQ: SelectMany is O(n×m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Take"),
            Deferred(Ok(ComplexitySource.Documented("LINQ: Take is O(k) deferred"))));
        builder.Add(new MethodSignature(enumerable, "TakeLast"),
            Deferred(Ok(ComplexitySource.Documented("LINQ: TakeLast is O(k) deferred (buffered)"))));
        builder.Add(new MethodSignature(enumerable, "TakeWhile"),
            Deferred(On(ComplexitySource.Documented("LINQ: TakeWhile is O(n) worst case deferred"))));
        builder.Add(new MethodSignature(enumerable, "Skip"),
            Deferred(On(ComplexitySource.Documented("LINQ: Skip is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "SkipLast"),
            Deferred(On(ComplexitySource.Documented("LINQ: SkipLast is O(n) deferred (buffered)"))));
        builder.Add(new MethodSignature(enumerable, "SkipWhile"),
            Deferred(On(ComplexitySource.Documented("LINQ: SkipWhile is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Distinct"),
            Deferred(On(ComplexitySource.Documented("LINQ: Distinct is O(n) deferred (hash set)"))));
        builder.Add(new MethodSignature(enumerable, "DistinctBy"),
            Deferred(On(ComplexitySource.Attested("Same as Distinct"))));
        builder.Add(new MethodSignature(enumerable, "Concat"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: Concat is O(n+m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Append"),
            Deferred(OnPlusOne(ComplexitySource.Documented("LINQ: Append is O(n+1) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Prepend"),
            Deferred(OnPlusOne(ComplexitySource.Documented("LINQ: Prepend is O(n+1) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Zip"),
            Deferred(OMinNM(ComplexitySource.Documented("LINQ: Zip is O(min(n,m)) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Cast"),
            Deferred(On(ComplexitySource.Documented("LINQ: Cast is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "OfType"),
            Deferred(On(ComplexitySource.Documented("LINQ: OfType is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "DefaultIfEmpty"),
            Deferred(On(ComplexitySource.Documented("LINQ: DefaultIfEmpty is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Reverse"),
            Deferred(On(ComplexitySource.Documented("LINQ: Reverse is O(n) deferred (buffered)"))));
        builder.Add(new MethodSignature(enumerable, "Chunk"),
            Deferred(On(ComplexitySource.Attested("LINQ: Chunk is O(n) deferred"))));

        // === Set operations (deferred, but require full enumeration of one source) ===
        builder.Add(new MethodSignature(enumerable, "Union"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: Union is O(n+m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "UnionBy"),
            Deferred(OnPlusM(ComplexitySource.Attested("Same as Union"))));
        builder.Add(new MethodSignature(enumerable, "Intersect"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: Intersect is O(n+m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "IntersectBy"),
            Deferred(OnPlusM(ComplexitySource.Attested("Same as Intersect"))));
        builder.Add(new MethodSignature(enumerable, "Except"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: Except is O(n+m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "ExceptBy"),
            Deferred(OnPlusM(ComplexitySource.Attested("Same as Except"))));

        // === Ordering (deferred, but O(n log n) when materialized) ===
        builder.Add(new MethodSignature(enumerable, "OrderBy"),
            Deferred(ONLogN(ComplexitySource.Documented("LINQ: OrderBy is O(n log n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "OrderByDescending"),
            Deferred(ONLogN(ComplexitySource.Documented("LINQ: OrderByDescending is O(n log n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "ThenBy"),
            Deferred(ONLogN(ComplexitySource.Documented("LINQ: ThenBy is O(n log n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "ThenByDescending"),
            Deferred(ONLogN(ComplexitySource.Documented("LINQ: ThenByDescending is O(n log n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Order"),
            Deferred(ONLogN(ComplexitySource.Attested(".NET 7+ Order is O(n log n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "OrderDescending"),
            Deferred(ONLogN(ComplexitySource.Attested(".NET 7+ OrderDescending is O(n log n) deferred"))));

        // === Grouping/Join (deferred but buffered) ===
        builder.Add(new MethodSignature(enumerable, "GroupBy"),
            Deferred(On(ComplexitySource.Documented("LINQ: GroupBy is O(n) deferred"))));
        builder.Add(new MethodSignature(enumerable, "GroupJoin"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: GroupJoin is O(n+m) deferred"))));
        builder.Add(new MethodSignature(enumerable, "Join"),
            Deferred(OnPlusM(ComplexitySource.Documented("LINQ: Join is O(n+m) deferred"))));

        // === Immediate execution (O(n) or O(n log n)) ===

        builder.Add(new MethodSignature(enumerable, "ToList"),
            On(ComplexitySource.Documented("LINQ: ToList is O(n) immediate")));
        builder.Add(new MethodSignature(enumerable, "ToArray"),
            On(ComplexitySource.Documented("LINQ: ToArray is O(n) immediate")));
        builder.Add(new MethodSignature(enumerable, "ToDictionary"),
            On(ComplexitySource.Documented("LINQ: ToDictionary is O(n) immediate")));
        builder.Add(new MethodSignature(enumerable, "ToHashSet"),
            On(ComplexitySource.Documented("LINQ: ToHashSet is O(n) immediate")));
        builder.Add(new MethodSignature(enumerable, "ToLookup"),
            On(ComplexitySource.Documented("LINQ: ToLookup is O(n) immediate")));

        // Aggregations
        builder.Add(new MethodSignature(enumerable, "Count"),
            On(ComplexitySource.Documented("LINQ: Count is O(n) unless ICollection")));
        builder.Add(new MethodSignature(enumerable, "LongCount"),
            On(ComplexitySource.Documented("LINQ: LongCount is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Sum"),
            On(ComplexitySource.Documented("LINQ: Sum is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Average"),
            On(ComplexitySource.Documented("LINQ: Average is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Min"),
            On(ComplexitySource.Documented("LINQ: Min is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Max"),
            On(ComplexitySource.Documented("LINQ: Max is O(n)")));
        builder.Add(new MethodSignature(enumerable, "MinBy"),
            On(ComplexitySource.Attested("LINQ: MinBy is O(n)")));
        builder.Add(new MethodSignature(enumerable, "MaxBy"),
            On(ComplexitySource.Attested("LINQ: MaxBy is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Aggregate"),
            On(ComplexitySource.Documented("LINQ: Aggregate is O(n)")));

        // Predicates
        builder.Add(new MethodSignature(enumerable, "Any"),
            On(ComplexitySource.Documented("LINQ: Any is O(n) worst case")));
        builder.Add(new MethodSignature(enumerable, "All"),
            On(ComplexitySource.Documented("LINQ: All is O(n)")));
        builder.Add(new MethodSignature(enumerable, "Contains"),
            On(ComplexitySource.Documented("LINQ: Contains is O(n)")));
        builder.Add(new MethodSignature(enumerable, "SequenceEqual"),
            OMinNM(ComplexitySource.Documented("LINQ: SequenceEqual is O(min(n,m))")));

        // Element retrieval
        builder.Add(new MethodSignature(enumerable, "First"),
            O1(ComplexitySource.Documented("LINQ: First is O(1) if IList, else O(n)")));
        builder.Add(new MethodSignature(enumerable, "FirstOrDefault"),
            O1(ComplexitySource.Documented("LINQ: FirstOrDefault is O(1) if IList, else O(n)")));
        builder.Add(new MethodSignature(enumerable, "Last"),
            On(ComplexitySource.Documented("LINQ: Last is O(1) if IList, else O(n)")));
        builder.Add(new MethodSignature(enumerable, "LastOrDefault"),
            On(ComplexitySource.Documented("LINQ: LastOrDefault is O(1) if IList, else O(n)")));
        builder.Add(new MethodSignature(enumerable, "Single"),
            On(ComplexitySource.Documented("LINQ: Single is O(n)")));
        builder.Add(new MethodSignature(enumerable, "SingleOrDefault"),
            On(ComplexitySource.Documented("LINQ: SingleOrDefault is O(n)")));
        builder.Add(new MethodSignature(enumerable, "ElementAt"),
            On(ComplexitySource.Documented("LINQ: ElementAt is O(1) if IList, else O(n)")));
        builder.Add(new MethodSignature(enumerable, "ElementAtOrDefault"),
            On(ComplexitySource.Documented("LINQ: ElementAtOrDefault is O(1) if IList, else O(n)")));
    }

    #endregion

    #region String Mappings

    private static void AddStringMappings(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        var stringType = "String";

        // O(1) operations
        builder.Add(new MethodSignature(stringType, "get_Length"),
            O1(ComplexitySource.Documented("String.Length is O(1)")));
        builder.Add(new MethodSignature(stringType, "get_Chars"),
            O1(ComplexitySource.Documented("String indexer is O(1)")));
        builder.Add(new MethodSignature(stringType, "IsNullOrEmpty"),
            O1(ComplexitySource.Documented("String.IsNullOrEmpty is O(1)")));
        builder.Add(new MethodSignature(stringType, "IsNullOrWhiteSpace"),
            On(ComplexitySource.Attested("Must scan for whitespace")));

        // O(n) operations
        builder.Add(new MethodSignature(stringType, "ToLower"),
            On(ComplexitySource.Attested("String.ToLower is O(n)")));
        builder.Add(new MethodSignature(stringType, "ToLowerInvariant"),
            On(ComplexitySource.Attested("String.ToLowerInvariant is O(n)")));
        builder.Add(new MethodSignature(stringType, "ToUpper"),
            On(ComplexitySource.Attested("String.ToUpper is O(n)")));
        builder.Add(new MethodSignature(stringType, "ToUpperInvariant"),
            On(ComplexitySource.Attested("String.ToUpperInvariant is O(n)")));
        builder.Add(new MethodSignature(stringType, "Trim"),
            On(ComplexitySource.Attested("String.Trim is O(n)")));
        builder.Add(new MethodSignature(stringType, "TrimStart"),
            On(ComplexitySource.Attested("String.TrimStart is O(n)")));
        builder.Add(new MethodSignature(stringType, "TrimEnd"),
            On(ComplexitySource.Attested("String.TrimEnd is O(n)")));
        builder.Add(new MethodSignature(stringType, "Substring"),
            On(ComplexitySource.Attested("String.Substring is O(n) for copy")));
        builder.Add(new MethodSignature(stringType, "ToCharArray"),
            On(ComplexitySource.Attested("String.ToCharArray is O(n)")));
        builder.Add(new MethodSignature(stringType, "GetHashCode"),
            On(ComplexitySource.Attested("String.GetHashCode is O(n)")));

        // Search operations - O(n×m) worst case (naive algorithm)
        // Note: .NET uses optimized algorithms but worst case is still O(n×m)
        builder.Add(new MethodSignature(stringType, "IndexOf"),
            OnTimesM(ComplexitySource.Heuristic("String.IndexOf is O(n×m) worst case, typically O(n+m)")));
        builder.Add(new MethodSignature(stringType, "LastIndexOf"),
            OnTimesM(ComplexitySource.Heuristic("String.LastIndexOf is O(n×m) worst case")));
        builder.Add(new MethodSignature(stringType, "IndexOfAny"),
            On(ComplexitySource.Attested("String.IndexOfAny is O(n)")));
        builder.Add(new MethodSignature(stringType, "LastIndexOfAny"),
            On(ComplexitySource.Attested("String.LastIndexOfAny is O(n)")));
        builder.Add(new MethodSignature(stringType, "Contains"),
            OnTimesM(ComplexitySource.Heuristic("String.Contains is O(n×m) worst case")));
        builder.Add(new MethodSignature(stringType, "StartsWith"),
            Om(ComplexitySource.Attested("String.StartsWith is O(m) where m is pattern length")));
        builder.Add(new MethodSignature(stringType, "EndsWith"),
            Om(ComplexitySource.Attested("String.EndsWith is O(m) where m is pattern length")));
        builder.Add(new MethodSignature(stringType, "Equals"),
            On(ComplexitySource.Attested("String.Equals is O(n)")));
        builder.Add(new MethodSignature(stringType, "Compare"),
            On(ComplexitySource.Attested("String.Compare is O(n)")));
        builder.Add(new MethodSignature(stringType, "CompareTo"),
            On(ComplexitySource.Attested("String.CompareTo is O(n)")));

        // Replace/Split - O(n×m) or O(n)
        builder.Add(new MethodSignature(stringType, "Replace"),
            OnTimesM(ComplexitySource.Heuristic("String.Replace is O(n×m) worst case")));
        builder.Add(new MethodSignature(stringType, "Split"),
            On(ComplexitySource.Attested("String.Split is O(n)")));
        builder.Add(new MethodSignature(stringType, "Join"),
            On(ComplexitySource.Attested("String.Join is O(n) total characters")));
        builder.Add(new MethodSignature(stringType, "Concat"),
            On(ComplexitySource.Attested("String.Concat is O(n) total characters")));
        builder.Add(new MethodSignature(stringType, "Format"),
            On(ComplexitySource.Attested("String.Format is O(n)")));
        builder.Add(new MethodSignature(stringType, "PadLeft"),
            On(ComplexitySource.Attested("String.PadLeft is O(n)")));
        builder.Add(new MethodSignature(stringType, "PadRight"),
            On(ComplexitySource.Attested("String.PadRight is O(n)")));
        builder.Add(new MethodSignature(stringType, "Remove"),
            On(ComplexitySource.Attested("String.Remove is O(n)")));
        builder.Add(new MethodSignature(stringType, "Insert"),
            On(ComplexitySource.Attested("String.Insert is O(n)")));

        // === StringBuilder ===
        var sbType = "StringBuilder";

        builder.Add(new MethodSignature(sbType, "get_Length"),
            O1(ComplexitySource.Attested("StringBuilder.Length is O(1)")));
        builder.Add(new MethodSignature(sbType, "get_Capacity"),
            O1(ComplexitySource.Attested("StringBuilder.Capacity is O(1)")));
        builder.Add(new MethodSignature(sbType, "get_Chars"),
            O1(ComplexitySource.Attested("StringBuilder indexer is O(1)")));
        builder.Add(new MethodSignature(sbType, "Append"),
            Amortized(Om(ComplexitySource.Attested("StringBuilder.Append is O(m) amortized"))));
        builder.Add(new MethodSignature(sbType, "AppendLine"),
            Amortized(O1(ComplexitySource.Attested("StringBuilder.AppendLine is O(1) amortized"))));
        builder.Add(new MethodSignature(sbType, "Insert"),
            On(ComplexitySource.Attested("StringBuilder.Insert is O(n)")));
        builder.Add(new MethodSignature(sbType, "Remove"),
            On(ComplexitySource.Attested("StringBuilder.Remove is O(n)")));
        builder.Add(new MethodSignature(sbType, "Replace"),
            OnTimesM(ComplexitySource.Attested("StringBuilder.Replace is O(n×m)")));
        builder.Add(new MethodSignature(sbType, "Clear"),
            O1(ComplexitySource.Attested("StringBuilder.Clear is O(1)")));
        builder.Add(new MethodSignature(sbType, "ToString"),
            On(ComplexitySource.Attested("StringBuilder.ToString is O(n)")));
    }

    #endregion

    #region Concurrent Collections Mappings

    private static void AddConcurrentCollections(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        // === ConcurrentDictionary<TKey, TValue> ===
        var concDictType = "ConcurrentDictionary`2";

        builder.Add(new MethodSignature(concDictType, "get_Count"),
            On(ComplexitySource.Documented("ConcurrentDictionary.Count is O(n) - requires lock acquisition")));
        builder.Add(new MethodSignature(concDictType, "get_Item"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary indexer is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "TryGetValue"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.TryGetValue is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "TryAdd"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.TryAdd is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "TryRemove"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.TryRemove is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "TryUpdate"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.TryUpdate is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "GetOrAdd"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.GetOrAdd is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "AddOrUpdate"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.AddOrUpdate is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "ContainsKey"),
            Amortized(O1(ComplexitySource.Documented("ConcurrentDictionary.ContainsKey is O(1)"))));
        builder.Add(new MethodSignature(concDictType, "Clear"),
            On(ComplexitySource.Attested("ConcurrentDictionary.Clear is O(n)")));

        // === ConcurrentQueue<T> ===
        var concQueueType = "ConcurrentQueue`1";

        builder.Add(new MethodSignature(concQueueType, "get_Count"),
            On(ComplexitySource.Attested("ConcurrentQueue.Count may require enumeration")));
        builder.Add(new MethodSignature(concQueueType, "Enqueue"),
            O1(ComplexitySource.Documented("ConcurrentQueue.Enqueue is O(1)")));
        builder.Add(new MethodSignature(concQueueType, "TryDequeue"),
            O1(ComplexitySource.Documented("ConcurrentQueue.TryDequeue is O(1)")));
        builder.Add(new MethodSignature(concQueueType, "TryPeek"),
            O1(ComplexitySource.Documented("ConcurrentQueue.TryPeek is O(1)")));
        builder.Add(new MethodSignature(concQueueType, "Clear"),
            On(ComplexitySource.Attested("ConcurrentQueue.Clear is O(n)")));

        // === ConcurrentStack<T> ===
        var concStackType = "ConcurrentStack`1";

        builder.Add(new MethodSignature(concStackType, "get_Count"),
            On(ComplexitySource.Attested("ConcurrentStack.Count may require enumeration")));
        builder.Add(new MethodSignature(concStackType, "Push"),
            O1(ComplexitySource.Documented("ConcurrentStack.Push is O(1)")));
        builder.Add(new MethodSignature(concStackType, "TryPop"),
            O1(ComplexitySource.Documented("ConcurrentStack.TryPop is O(1)")));
        builder.Add(new MethodSignature(concStackType, "TryPeek"),
            O1(ComplexitySource.Documented("ConcurrentStack.TryPeek is O(1)")));
        builder.Add(new MethodSignature(concStackType, "PushRange"),
            Ok(ComplexitySource.Documented("ConcurrentStack.PushRange is O(k)")));
        builder.Add(new MethodSignature(concStackType, "TryPopRange"),
            Ok(ComplexitySource.Documented("ConcurrentStack.TryPopRange is O(k)")));
        builder.Add(new MethodSignature(concStackType, "Clear"),
            On(ComplexitySource.Attested("ConcurrentStack.Clear is O(n)")));

        // === ConcurrentBag<T> ===
        var concBagType = "ConcurrentBag`1";

        builder.Add(new MethodSignature(concBagType, "get_Count"),
            On(ComplexitySource.Attested("ConcurrentBag.Count requires enumeration")));
        builder.Add(new MethodSignature(concBagType, "Add"),
            O1(ComplexitySource.Documented("ConcurrentBag.Add is O(1)")));
        builder.Add(new MethodSignature(concBagType, "TryTake"),
            O1(ComplexitySource.Documented("ConcurrentBag.TryTake is O(1) for local, O(n) for stealing")));
        builder.Add(new MethodSignature(concBagType, "TryPeek"),
            O1(ComplexitySource.Attested("ConcurrentBag.TryPeek is O(1)")));
        builder.Add(new MethodSignature(concBagType, "Clear"),
            On(ComplexitySource.Attested("ConcurrentBag.Clear is O(n)")));

        // === BlockingCollection<T> ===
        var blockingType = "BlockingCollection`1";

        builder.Add(new MethodSignature(blockingType, "get_Count"),
            O1(ComplexitySource.Attested("BlockingCollection.Count is O(1)")));
        builder.Add(new MethodSignature(blockingType, "Add"),
            O1(ComplexitySource.Documented("BlockingCollection.Add is O(1) excluding wait")));
        builder.Add(new MethodSignature(blockingType, "Take"),
            O1(ComplexitySource.Documented("BlockingCollection.Take is O(1) excluding wait")));
        builder.Add(new MethodSignature(blockingType, "TryAdd"),
            O1(ComplexitySource.Documented("BlockingCollection.TryAdd is O(1)")));
        builder.Add(new MethodSignature(blockingType, "TryTake"),
            O1(ComplexitySource.Documented("BlockingCollection.TryTake is O(1)")));
    }

    #endregion

    #region Regex Mappings

    private static void AddRegexMappings(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        var regexType = "Regex";

        // WARNING: Regex complexity is highly pattern-dependent!
        // Backtracking can lead to catastrophic O(2^n) in worst cases.
        // These are conservative estimates.

        builder.Add(new MethodSignature(regexType, "IsMatch"),
            new ComplexityMapping(
                new ExponentialComplexity(2, Variable.N),
                ComplexitySource.Heuristic("Regex.IsMatch: O(n) best case, O(2^n) worst case with backtracking"),
                ComplexityNotes.BacktrackingWarning));

        builder.Add(new MethodSignature(regexType, "Match"),
            new ComplexityMapping(
                new ExponentialComplexity(2, Variable.N),
                ComplexitySource.Heuristic("Regex.Match: O(n) best case, O(2^n) worst case with backtracking"),
                ComplexityNotes.BacktrackingWarning));

        builder.Add(new MethodSignature(regexType, "Matches"),
            new ComplexityMapping(
                new ExponentialComplexity(2, Variable.N),
                ComplexitySource.Heuristic("Regex.Matches: O(n×k) best case where k is match count, O(2^n) worst case"),
                ComplexityNotes.BacktrackingWarning));

        builder.Add(new MethodSignature(regexType, "Replace"),
            new ComplexityMapping(
                new ExponentialComplexity(2, Variable.N),
                ComplexitySource.Heuristic("Regex.Replace: O(n) best case, O(2^n) worst case with backtracking"),
                ComplexityNotes.BacktrackingWarning));

        builder.Add(new MethodSignature(regexType, "Split"),
            new ComplexityMapping(
                new ExponentialComplexity(2, Variable.N),
                ComplexitySource.Heuristic("Regex.Split: O(n) best case, O(2^n) worst case with backtracking"),
                ComplexityNotes.BacktrackingWarning));

        // Compiled regex has same worst-case but better constant factors
        builder.Add(new MethodSignature(regexType, ".ctor"),
            new ComplexityMapping(
                new PolynomialComplexity(new[] { 0.0, 0.0, 1.0 }, Variable.M), // O(m²) for pattern length m
                ComplexitySource.Heuristic("Regex construction: O(m) to O(m²) depending on pattern"),
                ComplexityNotes.None));
    }

    #endregion

    #region Array Mappings

    private static void AddArrayMappings(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        var arrayType = "Array";

        // O(1) operations
        builder.Add(new MethodSignature(arrayType, "get_Length"),
            O1(ComplexitySource.Documented("Array.Length is O(1)")));
        builder.Add(new MethodSignature(arrayType, "GetLength"),
            O1(ComplexitySource.Documented("Array.GetLength is O(1)")));
        builder.Add(new MethodSignature(arrayType, "get_Rank"),
            O1(ComplexitySource.Documented("Array.Rank is O(1)")));
        builder.Add(new MethodSignature(arrayType, "GetValue"),
            O1(ComplexitySource.Documented("Array.GetValue is O(1)")));
        builder.Add(new MethodSignature(arrayType, "SetValue"),
            O1(ComplexitySource.Documented("Array.SetValue is O(1)")));

        // O(n) operations
        builder.Add(new MethodSignature(arrayType, "Clear"),
            On(ComplexitySource.Documented("Array.Clear is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Copy"),
            On(ComplexitySource.Documented("Array.Copy is O(n)")));
        builder.Add(new MethodSignature(arrayType, "CopyTo"),
            On(ComplexitySource.Documented("Array.CopyTo is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Clone"),
            On(ComplexitySource.Documented("Array.Clone is O(n)")));
        builder.Add(new MethodSignature(arrayType, "IndexOf"),
            On(ComplexitySource.Documented("Array.IndexOf is O(n)")));
        builder.Add(new MethodSignature(arrayType, "LastIndexOf"),
            On(ComplexitySource.Documented("Array.LastIndexOf is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Find"),
            On(ComplexitySource.Documented("Array.Find is O(n)")));
        builder.Add(new MethodSignature(arrayType, "FindIndex"),
            On(ComplexitySource.Documented("Array.FindIndex is O(n)")));
        builder.Add(new MethodSignature(arrayType, "FindAll"),
            On(ComplexitySource.Documented("Array.FindAll is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Exists"),
            On(ComplexitySource.Documented("Array.Exists is O(n)")));
        builder.Add(new MethodSignature(arrayType, "TrueForAll"),
            On(ComplexitySource.Documented("Array.TrueForAll is O(n)")));
        builder.Add(new MethodSignature(arrayType, "ForEach"),
            On(ComplexitySource.Documented("Array.ForEach is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Reverse"),
            On(ComplexitySource.Documented("Array.Reverse is O(n)")));
        builder.Add(new MethodSignature(arrayType, "ConvertAll"),
            On(ComplexitySource.Documented("Array.ConvertAll is O(n)")));
        builder.Add(new MethodSignature(arrayType, "ConstrainedCopy"),
            On(ComplexitySource.Documented("Array.ConstrainedCopy is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Fill"),
            On(ComplexitySource.Attested("Array.Fill is O(n)")));
        builder.Add(new MethodSignature(arrayType, "Resize"),
            On(ComplexitySource.Documented("Array.Resize is O(n)")));

        // O(n log n) operations
        builder.Add(new MethodSignature(arrayType, "Sort"),
            ONLogN(ComplexitySource.Documented("Array.Sort is O(n log n)")));
        builder.Add(new MethodSignature(arrayType, "BinarySearch"),
            OLogN(ComplexitySource.Documented("Array.BinarySearch is O(log n)")));
    }

    #endregion

    #region Span Mappings

    private static void AddSpanMappings(
        ImmutableDictionary<MethodSignature, ComplexityMapping>.Builder builder)
    {
        // Span<T> and ReadOnlySpan<T> have similar complexities
        foreach (var spanType in new[] { "Span`1", "ReadOnlySpan`1" })
        {
            builder.Add(new MethodSignature(spanType, "get_Length"),
                O1(ComplexitySource.Documented("Span.Length is O(1)")));
            builder.Add(new MethodSignature(spanType, "get_Item"),
                O1(ComplexitySource.Documented("Span indexer is O(1)")));
            builder.Add(new MethodSignature(spanType, "Slice"),
                O1(ComplexitySource.Documented("Span.Slice is O(1) - no copy")));
            builder.Add(new MethodSignature(spanType, "ToArray"),
                On(ComplexitySource.Documented("Span.ToArray is O(n)")));
            builder.Add(new MethodSignature(spanType, "CopyTo"),
                On(ComplexitySource.Documented("Span.CopyTo is O(n)")));
            builder.Add(new MethodSignature(spanType, "Fill"),
                On(ComplexitySource.Documented("Span.Fill is O(n)")));
            builder.Add(new MethodSignature(spanType, "Clear"),
                On(ComplexitySource.Documented("Span.Clear is O(n)")));
            builder.Add(new MethodSignature(spanType, "IndexOf"),
                On(ComplexitySource.Attested("Span.IndexOf is O(n)")));
            builder.Add(new MethodSignature(spanType, "LastIndexOf"),
                On(ComplexitySource.Attested("Span.LastIndexOf is O(n)")));
            builder.Add(new MethodSignature(spanType, "Contains"),
                On(ComplexitySource.Attested("Span.Contains is O(n)")));
            builder.Add(new MethodSignature(spanType, "SequenceEqual"),
                On(ComplexitySource.Attested("Span.SequenceEqual is O(n)")));
        }

        // Memory<T> and ReadOnlyMemory<T>
        foreach (var memType in new[] { "Memory`1", "ReadOnlyMemory`1" })
        {
            builder.Add(new MethodSignature(memType, "get_Length"),
                O1(ComplexitySource.Attested("Memory.Length is O(1)")));
            builder.Add(new MethodSignature(memType, "get_Span"),
                O1(ComplexitySource.Attested("Memory.Span is O(1)")));
            builder.Add(new MethodSignature(memType, "Slice"),
                O1(ComplexitySource.Attested("Memory.Slice is O(1)")));
            builder.Add(new MethodSignature(memType, "ToArray"),
                On(ComplexitySource.Attested("Memory.ToArray is O(n)")));
            builder.Add(new MethodSignature(memType, "CopyTo"),
                On(ComplexitySource.Attested("Memory.CopyTo is O(n)")));
        }
    }

    #endregion

    #region Helper Methods

    private static ComplexityMapping O1(ComplexitySource source) =>
        new(new ConstantComplexity(1), source, ComplexityNotes.None);

    private static ComplexityMapping On(ComplexitySource source) =>
        new(new LinearComplexity(1.0, Variable.N), source, ComplexityNotes.None);

    private static ComplexityMapping Om(ComplexitySource source) =>
        new(new LinearComplexity(1.0, Variable.M), source, ComplexityNotes.None);

    private static ComplexityMapping Ok(ComplexitySource source) =>
        new(new LinearComplexity(1.0, Variable.K), source, ComplexityNotes.None);

    private static ComplexityMapping OLogN(ComplexitySource source) =>
        new(new LogarithmicComplexity(1.0, Variable.N), source, ComplexityNotes.None);

    private static ComplexityMapping ONLogN(ComplexitySource source) =>
        new(PolyLogComplexity.NLogN(Variable.N), source, ComplexityNotes.None);

    private static ComplexityMapping OnPlusM(ComplexitySource source) =>
        new(new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Plus,
            new LinearComplexity(1.0, Variable.M)),
            source, ComplexityNotes.None);

    private static ComplexityMapping OnPlusOne(ComplexitySource source) =>
        new(new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Plus,
            new ConstantComplexity(1)),
            source, ComplexityNotes.None);

    private static ComplexityMapping OnTimesM(ComplexitySource source) =>
        new(new BinaryOperationComplexity(
            new LinearComplexity(1.0, Variable.N),
            BinaryOp.Multiply,
            new LinearComplexity(1.0, Variable.M)),
            source, ComplexityNotes.None);

    private static ComplexityMapping OMinNM(ComplexitySource source) =>
        new(new ConditionalComplexity(
            "n < m",
            new LinearComplexity(1.0, Variable.N),
            new LinearComplexity(1.0, Variable.M)),
            source, ComplexityNotes.None);

    private static ComplexityMapping Amortized(ComplexityMapping mapping) =>
        mapping with { Notes = mapping.Notes | ComplexityNotes.Amortized };

    private static ComplexityMapping Deferred(ComplexityMapping mapping) =>
        mapping with { Notes = mapping.Notes | ComplexityNotes.DeferredExecution };

    #endregion
}

/// <summary>
/// Signature for method lookup in the mappings registry.
/// </summary>
public readonly record struct MethodSignature(
    string TypeName,
    string MethodName,
    int ArgumentCount = -1);

/// <summary>
/// A complexity mapping with source attribution and notes.
/// </summary>
public sealed record ComplexityMapping(
    ComplexityExpression Complexity,
    ComplexitySource Source,
    ComplexityNotes Notes);

/// <summary>
/// Additional notes about complexity characteristics.
/// </summary>
[Flags]
public enum ComplexityNotes
{
    None = 0,

    /// <summary>Complexity is amortized (occasional expensive operations)</summary>
    Amortized = 1 << 0,

    /// <summary>LINQ deferred execution - O(1) to create, full cost on enumeration</summary>
    DeferredExecution = 1 << 1,

    /// <summary>Regex backtracking warning - can be exponential</summary>
    BacktrackingWarning = 1 << 2,

    /// <summary>Complexity depends on input characteristics</summary>
    InputDependent = 1 << 3,

    /// <summary>Thread-safe but may have contention overhead</summary>
    ThreadSafe = 1 << 4,

    /// <summary>Unknown method - conservative estimate</summary>
    Unknown = 1 << 5
}
