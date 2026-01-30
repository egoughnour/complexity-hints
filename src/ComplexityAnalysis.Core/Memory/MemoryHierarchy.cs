using System.Collections.Immutable;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Core.Memory;

/// <summary>
/// Represents the memory access tier hierarchy with associated performance weights.
/// Each successive tier is approximately 1000x slower than the previous.
/// </summary>
public enum MemoryTier
{
    /// <summary>
    /// L1/L2 CPU cache - fastest access (~1-10 ns).
    /// Typical sizes: L1 64KB, L2 256KB-512KB.
    /// </summary>
    CpuCache = 0,

    /// <summary>
    /// Main memory (RAM) - fast but slower than cache (~100 ns).
    /// Typical sizes: 8GB-128GB.
    /// </summary>
    MainMemory = 1,

    /// <summary>
    /// Local disk storage (SSD/HDD) - much slower (~100 µs for SSD).
    /// </summary>
    LocalDisk = 2,

    /// <summary>
    /// Local network (LAN, same datacenter) - network latency (~1-10 ms).
    /// </summary>
    LocalNetwork = 3,

    /// <summary>
    /// Far network (WAN, internet, cross-region) - high latency (~100+ ms).
    /// </summary>
    FarNetwork = 4
}

/// <summary>
/// Provides weight values for memory tier access costs.
/// Uses a ~1000x compounding factor between tiers.
/// </summary>
public static class MemoryTierWeights
{
    /// <summary>
    /// Base weight for CPU cache access (normalized to 1).
    /// </summary>
    public const double CpuCache = 1.0;

    /// <summary>
    /// Weight for main memory access (~1000x cache).
    /// </summary>
    public const double MainMemory = 1_000.0;

    /// <summary>
    /// Weight for local disk access (~1000x memory).
    /// </summary>
    public const double LocalDisk = 1_000_000.0;

    /// <summary>
    /// Weight for local network access (~1000x disk).
    /// </summary>
    public const double LocalNetwork = 1_000_000_000.0;

    /// <summary>
    /// Weight for far network access (~1000x local network).
    /// </summary>
    public const double FarNetwork = 1_000_000_000_000.0;

    /// <summary>
    /// The compounding factor between adjacent tiers.
    /// </summary>
    public const double CompoundingFactor = 1_000.0;

    /// <summary>
    /// Gets the weight for a given memory tier.
    /// </summary>
    public static double GetWeight(MemoryTier tier) => tier switch
    {
        MemoryTier.CpuCache => CpuCache,
        MemoryTier.MainMemory => MainMemory,
        MemoryTier.LocalDisk => LocalDisk,
        MemoryTier.LocalNetwork => LocalNetwork,
        MemoryTier.FarNetwork => FarNetwork,
        _ => throw new ArgumentOutOfRangeException(nameof(tier))
    };

    /// <summary>
    /// Gets the weight for a tier by its ordinal level.
    /// Level 0 = Cache, Level 1 = Memory, etc.
    /// </summary>
    public static double GetWeightByLevel(int level) =>
        Math.Pow(CompoundingFactor, level);

    /// <summary>
    /// Gets all tiers and their weights.
    /// </summary>
    public static IEnumerable<(MemoryTier Tier, double Weight)> AllTiers =>
        Enum.GetValues<MemoryTier>().Select(t => (t, GetWeight(t)));
}

/// <summary>
/// Represents a single memory access with its tier and access count.
/// </summary>
public sealed record MemoryAccess
{
    /// <summary>
    /// The memory tier being accessed.
    /// </summary>
    public required MemoryTier Tier { get; init; }

    /// <summary>
    /// The number of accesses (as a complexity expression).
    /// </summary>
    public required ComplexityExpression AccessCount { get; init; }

    /// <summary>
    /// Optional description of what this access represents.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the weight per access for this tier.
    /// </summary>
    public double WeightPerAccess => MemoryTierWeights.GetWeight(Tier);

    /// <summary>
    /// Gets the total weighted cost as a complexity expression.
    /// </summary>
    public ComplexityExpression TotalCost =>
        new BinaryOperationComplexity(
            AccessCount,
            BinaryOp.Multiply,
            new ConstantComplexity(WeightPerAccess));

    /// <summary>
    /// Creates a constant number of accesses to a tier.
    /// </summary>
    public static MemoryAccess Constant(MemoryTier tier, double count, string? description = null) =>
        new()
        {
            Tier = tier,
            AccessCount = new ConstantComplexity(count),
            Description = description
        };

    /// <summary>
    /// Creates linear accesses to a tier: O(n) accesses.
    /// </summary>
    public static MemoryAccess Linear(MemoryTier tier, Variable variable, string? description = null) =>
        new()
        {
            Tier = tier,
            AccessCount = new VariableComplexity(variable),
            Description = description
        };

    /// <summary>
    /// Creates accesses with a given complexity expression.
    /// </summary>
    public static MemoryAccess WithComplexity(MemoryTier tier, ComplexityExpression count, string? description = null) =>
        new()
        {
            Tier = tier,
            AccessCount = count,
            Description = description
        };
}

/// <summary>
/// Represents a pattern of memory access behavior.
/// Used to infer likely memory tier placement.
/// </summary>
public enum AccessPattern
{
    /// <summary>
    /// Sequential access (e.g., array iteration) - cache-friendly.
    /// </summary>
    Sequential,

    /// <summary>
    /// Random access (e.g., hash table lookup) - likely main memory.
    /// </summary>
    Random,

    /// <summary>
    /// Temporal locality - same data accessed multiple times.
    /// </summary>
    TemporalLocality,

    /// <summary>
    /// Spatial locality - nearby data accessed together.
    /// </summary>
    SpatialLocality,

    /// <summary>
    /// Strided access (e.g., matrix column traversal).
    /// </summary>
    Strided,

    /// <summary>
    /// File I/O access.
    /// </summary>
    FileIO,

    /// <summary>
    /// Network access.
    /// </summary>
    Network
}

/// <summary>
/// Aggregates multiple memory accesses into a hierarchical cost model.
/// </summary>
public sealed record MemoryHierarchyCost
{
    /// <summary>
    /// All memory accesses in this cost model.
    /// </summary>
    public ImmutableList<MemoryAccess> Accesses { get; init; } = ImmutableList<MemoryAccess>.Empty;

    /// <summary>
    /// Gets the total weighted cost as a complexity expression.
    /// </summary>
    public ComplexityExpression TotalCost
    {
        get
        {
            if (Accesses.IsEmpty)
                return ConstantComplexity.Zero;

            return Accesses
                .Select(a => a.TotalCost)
                .Aggregate((acc, next) =>
                    new BinaryOperationComplexity(acc, BinaryOp.Plus, next));
        }
    }

    /// <summary>
    /// Gets the dominant tier (the one contributing most to total cost).
    /// </summary>
    public MemoryTier? DominantTier
    {
        get
        {
            if (Accesses.IsEmpty) return null;

            // The highest tier in the hierarchy dominates due to exponential weights
            return Accesses.Max(a => a.Tier);
        }
    }

    /// <summary>
    /// Groups accesses by tier.
    /// </summary>
    public ImmutableDictionary<MemoryTier, ImmutableList<MemoryAccess>> ByTier =>
        Accesses
            .GroupBy(a => a.Tier)
            .ToImmutableDictionary(
                g => g.Key,
                g => g.ToImmutableList());

    /// <summary>
    /// Adds a memory access to this cost model.
    /// </summary>
    public MemoryHierarchyCost Add(MemoryAccess access) =>
        this with { Accesses = Accesses.Add(access) };

    /// <summary>
    /// Combines two memory hierarchy costs.
    /// </summary>
    public MemoryHierarchyCost Combine(MemoryHierarchyCost other) =>
        this with { Accesses = Accesses.AddRange(other.Accesses) };

    /// <summary>
    /// Creates an empty cost model.
    /// </summary>
    public static MemoryHierarchyCost Empty => new();

    /// <summary>
    /// Creates a cost model with a single access.
    /// </summary>
    public static MemoryHierarchyCost Single(MemoryAccess access) =>
        new() { Accesses = ImmutableList.Create(access) };
}

/// <summary>
/// Heuristics for estimating memory tier from access patterns and data sizes.
/// </summary>
public static class MemoryTierEstimator
{
    /// <summary>
    /// Typical L1 cache size in bytes.
    /// </summary>
    public const int L1CacheSize = 64 * 1024; // 64 KB

    /// <summary>
    /// Typical L2 cache size in bytes.
    /// </summary>
    public const int L2CacheSize = 512 * 1024; // 512 KB

    /// <summary>
    /// Typical L3 cache size in bytes.
    /// </summary>
    public const int L3CacheSize = 8 * 1024 * 1024; // 8 MB

    /// <summary>
    /// Estimates the memory tier based on access pattern and working set size.
    /// </summary>
    public static MemoryTier EstimateTier(AccessPattern pattern, long workingSetBytes)
    {
        // Network access patterns always use network tiers
        if (pattern == AccessPattern.Network)
            return MemoryTier.FarNetwork;

        if (pattern == AccessPattern.FileIO)
            return MemoryTier.LocalDisk;

        // For compute patterns, estimate based on working set size
        return pattern switch
        {
            AccessPattern.Sequential when workingSetBytes <= L2CacheSize => MemoryTier.CpuCache,
            AccessPattern.Sequential => MemoryTier.MainMemory,

            AccessPattern.TemporalLocality when workingSetBytes <= L1CacheSize => MemoryTier.CpuCache,
            AccessPattern.TemporalLocality when workingSetBytes <= L3CacheSize => MemoryTier.CpuCache,
            AccessPattern.TemporalLocality => MemoryTier.MainMemory,

            AccessPattern.SpatialLocality when workingSetBytes <= L2CacheSize => MemoryTier.CpuCache,
            AccessPattern.SpatialLocality => MemoryTier.MainMemory,

            AccessPattern.Strided when workingSetBytes <= L3CacheSize => MemoryTier.CpuCache,
            AccessPattern.Strided => MemoryTier.MainMemory,

            // Random access is typically main memory unless very small
            AccessPattern.Random when workingSetBytes <= L1CacheSize => MemoryTier.CpuCache,
            AccessPattern.Random => MemoryTier.MainMemory,

            _ => MemoryTier.MainMemory
        };
    }

    /// <summary>
    /// Conservative estimate: assumes main memory unless evidence suggests otherwise.
    /// </summary>
    public static MemoryTier ConservativeEstimate(AccessPattern pattern) =>
        pattern switch
        {
            AccessPattern.Network => MemoryTier.FarNetwork,
            AccessPattern.FileIO => MemoryTier.LocalDisk,
            _ => MemoryTier.MainMemory
        };
}
