# BCL Complexity Mappings

The Complexity Analysis System includes pre-mapped complexity information for over 150 .NET Base Class Library (BCL) methods. This guide documents the mappings and their sources.

## System.Collections.Generic

### List&lt;T&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Add` | O(1) amortized | O(n) when resizing |
| `AddRange` | O(k) | k = items being added |
| `Insert` | O(n) | Shifts elements |
| `Remove` | O(n) | Linear search + shift |
| `RemoveAt` | O(n) | Shifts elements |
| `Contains` | O(n) | Linear search |
| `IndexOf` | O(n) | Linear search |
| `BinarySearch` | O(log n) | Requires sorted list |
| `Sort` | O(n log n) | Introsort |
| `Reverse` | O(n) | In-place |
| `Clear` | O(n) | Clears references |
| `get_Item` | O(1) | Index access |
| `set_Item` | O(1) | Index access |

### Dictionary&lt;K,V&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Add` | O(1) amortized | O(n) on rehash |
| `TryGetValue` | O(1) expected | O(n) worst-case with collisions |
| `ContainsKey` | O(1) expected | O(n) worst-case |
| `ContainsValue` | O(n) | Linear scan |
| `Remove` | O(1) expected | O(n) worst-case |
| `get_Item` | O(1) expected | O(n) worst-case |
| `set_Item` | O(1) amortized | O(n) worst-case |
| `Clear` | O(n) | Clears buckets |

### HashSet&lt;T&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Add` | O(1) amortized | O(n) on resize |
| `Contains` | O(1) expected | O(n) worst-case |
| `Remove` | O(1) expected | O(n) worst-case |
| `UnionWith` | O(n + m) | n = this, m = other |
| `IntersectWith` | O(n) | Or O(m) if smaller |
| `ExceptWith` | O(m) | m = items to remove |
| `IsSubsetOf` | O(n) | Checks all elements |

### SortedSet&lt;T&gt; / SortedDictionary&lt;K,V&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Add` | O(log n) | Red-black tree |
| `Contains` | O(log n) | Binary search |
| `Remove` | O(log n) | With rebalancing |
| `Min` / `Max` | O(log n) | Tree traversal |

### Queue&lt;T&gt; / Stack&lt;T&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Enqueue` / `Push` | O(1) amortized | O(n) on resize |
| `Dequeue` / `Pop` | O(1) | |
| `Peek` | O(1) | |
| `Contains` | O(n) | Linear search |

### PriorityQueue&lt;T,P&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Enqueue` | O(log n) | Heap insert |
| `Dequeue` | O(log n) | Heap extract |
| `Peek` | O(1) | |

### LinkedList&lt;T&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `AddFirst` / `AddLast` | O(1) | |
| `RemoveFirst` / `RemoveLast` | O(1) | |
| `Find` | O(n) | Linear search |
| `Contains` | O(n) | Linear search |

---

## System.Linq (Enumerable)

### Deferred Execution (O(1) to call)

These methods return immediately; complexity applies when enumerated:

| Method | Enumeration Complexity | Notes |
|--------|----------------------|-------|
| `Where` | O(n) | Filter predicate |
| `Select` | O(n) | Projection |
| `SelectMany` | O(n×m) | Flattening |
| `Take` / `Skip` | O(k) | k items accessed |
| `TakeWhile` / `SkipWhile` | O(k) | Until predicate fails |
| `Concat` | O(n + m) | |
| `Zip` | O(min(n, m)) | |
| `OfType` / `Cast` | O(n) | Type filtering |
| `Distinct` | O(n) | Hash-based |
| `Union` / `Intersect` / `Except` | O(n + m) | Hash-based |

### Immediate Execution

| Method | Complexity | Notes |
|--------|------------|-------|
| `ToList` / `ToArray` | O(n) | Materializes |
| `ToDictionary` | O(n) | With hashing |
| `ToHashSet` | O(n) | |
| `Count()` | O(1) or O(n) | O(1) if ICollection |
| `First` / `Single` | O(1) or O(n) | May scan |
| `Last` | O(1) or O(n) | O(1) if IList |
| `ElementAt` | O(1) or O(n) | O(1) if IList |
| `Any` | O(1) to O(n) | Short-circuit |
| `All` | O(n) | Must check all |
| `Contains` | O(n) | Linear search |
| `Sum` / `Average` / `Min` / `Max` | O(n) | Full scan |
| `Aggregate` | O(n) | |

### Sorting Operations

| Method | Complexity | Notes |
|--------|------------|-------|
| `OrderBy` / `OrderByDescending` | O(n log n) | Quicksort variant |
| `ThenBy` / `ThenByDescending` | O(n log n) | Stable sort |
| `Reverse` | O(n) | Buffers all |

### Grouping / Joining

| Method | Complexity | Notes |
|--------|------------|-------|
| `GroupBy` | O(n) | Hash-based |
| `Join` | O(n + m) | Hash join |
| `GroupJoin` | O(n + m) | |

---

## System.String

| Method | Complexity | Notes |
|--------|------------|-------|
| `Length` | O(1) | Property access |
| `get_Chars` | O(1) | Index access |
| `Substring` | O(k) | k = length extracted |
| `Contains` | O(n×m) | n = string, m = pattern |
| `IndexOf` | O(n×m) | Pattern search |
| `LastIndexOf` | O(n×m) | Reverse search |
| `StartsWith` / `EndsWith` | O(m) | m = prefix/suffix length |
| `Split` | O(n) | Plus allocation |
| `Join` | O(Σ lengths) | |
| `Concat` | O(Σ lengths) | |
| `Replace` | O(n×m) | n = string, m = pattern |
| `Trim` / `ToUpper` / `ToLower` | O(n) | |
| `Format` | O(n + Σ arg lengths) | |

---

## System.Text.RegularExpressions

| Method | Complexity | Notes |
|--------|------------|-------|
| `IsMatch` | O(n) to **O(2^n)** | ⚠️ Backtracking danger |
| `Match` / `Matches` | O(n) to **O(2^n)** | Depends on pattern |
| `Replace` | O(n) to **O(2^n)** | |

> **Warning**: Regex with backtracking can exhibit exponential time complexity on pathological inputs. Use non-backtracking mode (`RegexOptions.NonBacktracking` in .NET 7+) for untrusted input.

---

## System.Array

| Method | Complexity | Notes |
|--------|------------|-------|
| `Sort` | O(n log n) | Introsort |
| `BinarySearch` | O(log n) | Requires sorted |
| `IndexOf` | O(n) | Linear search |
| `Copy` | O(n) | |
| `Reverse` | O(n) | In-place |
| `Clear` | O(n) | |
| `Resize` | O(n) | Allocates new |
| `Fill` | O(n) | |

---

## Concurrent Collections

| Type | Method | Complexity | Notes |
|------|--------|------------|-------|
| `ConcurrentDictionary` | `TryAdd` | O(1) expected | Lock-free |
| `ConcurrentDictionary` | `TryGetValue` | O(1) expected | |
| `ConcurrentQueue` | `Enqueue` | O(1) | Lock-free |
| `ConcurrentQueue` | `TryDequeue` | O(1) | |
| `ConcurrentStack` | `Push` | O(1) | Lock-free |
| `ConcurrentStack` | `TryPop` | O(1) | |
| `ConcurrentBag` | `Add` | O(1) | Thread-local |
| `ConcurrentBag` | `TryTake` | O(1) amortized | May steal |

---

## Span&lt;T&gt; / Memory&lt;T&gt;

| Method | Complexity | Notes |
|--------|------------|-------|
| `Slice` | O(1) | No allocation |
| `CopyTo` | O(n) | |
| `Fill` | O(n) | |
| `IndexOf` | O(n) | Vectorized when possible |
| `SequenceEqual` | O(n) | |
| `BinarySearch` | O(log n) | Requires sorted |
| `Sort` | O(n log n) | |
| `Reverse` | O(n) | In-place |

---

## Source Attribution

Each mapping includes attribution:

- **Documented**: Official Microsoft documentation
- **Attested**: Referenced from .NET source code or academic sources
- **Empirical**: Verified through benchmarking
- **Heuristic**: Conservative estimate based on implementation pattern

When in doubt, the system uses conservative (worst-case) estimates.
