using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents a variable in complexity expressions (e.g., n, V, E, degree).
/// Variables are the symbolic placeholders for input sizes and other parameters.
/// </summary>
public sealed record Variable
{
    /// <summary>
    /// The symbolic name of the variable (e.g., "n", "V", "E").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The semantic type of the variable, indicating what it represents.
    /// </summary>
    public VariableType Type { get; }

    /// <summary>
    /// Optional description for documentation purposes.
    /// </summary>
    public string? Description { get; init; }

    public Variable(string name, VariableType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Creates a standard input size variable named "n".
    /// </summary>
    public static Variable N => new("n", VariableType.InputSize);

    /// <summary>
    /// Creates a vertex count variable named "V".
    /// </summary>
    public static Variable V => new("V", VariableType.VertexCount);

    /// <summary>
    /// Creates an edge count variable named "E".
    /// </summary>
    public static Variable E => new("E", VariableType.EdgeCount);

    /// <summary>
    /// Creates a secondary size variable named "m" (e.g., for pattern length in string search).
    /// </summary>
    public static Variable M => new("m", VariableType.SecondarySize);

    /// <summary>
    /// Creates a count parameter variable named "k" (e.g., for Take(k), top-k queries).
    /// </summary>
    public static Variable K => new("k", VariableType.Custom) { Description = "Count parameter" };

    /// <summary>
    /// Creates a height/depth variable named "h" (e.g., for tree height).
    /// </summary>
    public static Variable H => new("h", VariableType.TreeHeight);

    public override string ToString() => Name;

    public bool Equals(Variable? other) =>
        other is not null && Name == other.Name && Type == other.Type;

    public override int GetHashCode() => HashCode.Combine(Name, Type);
}

/// <summary>
/// Semantic types for complexity variables, indicating what the variable represents.
/// </summary>
public enum VariableType
{
    /// <summary>
    /// General input size (n) - default for most algorithms.
    /// </summary>
    InputSize,

    /// <summary>
    /// Count of data elements in a collection.
    /// </summary>
    DataCount,

    /// <summary>
    /// Number of vertices in a graph (V).
    /// </summary>
    VertexCount,

    /// <summary>
    /// Number of edges in a graph (E).
    /// </summary>
    EdgeCount,

    /// <summary>
    /// Sum of vertex degrees in a graph.
    /// </summary>
    DegreeSum,

    /// <summary>
    /// Height or depth of a tree structure.
    /// </summary>
    TreeHeight,

    /// <summary>
    /// Number of dimensions (for multi-dimensional algorithms).
    /// </summary>
    Dimensions,

    /// <summary>
    /// Length of a string or character sequence.
    /// </summary>
    StringLength,

    /// <summary>
    /// A secondary size parameter (e.g., m in O(n × m)).
    /// </summary>
    SecondarySize,

    /// <summary>
    /// Custom/user-defined variable type.
    /// </summary>
    Custom
}

/// <summary>
/// Extension methods for Variable.
/// </summary>
public static class VariableExtensions
{
    /// <summary>
    /// Creates a variable set from multiple variables.
    /// </summary>
    public static ImmutableHashSet<Variable> ToVariableSet(this IEnumerable<Variable> variables) =>
        variables.ToImmutableHashSet();

    /// <summary>
    /// Determines if a variable represents a graph-related quantity.
    /// </summary>
    public static bool IsGraphVariable(this Variable variable) =>
        variable.Type is VariableType.VertexCount
            or VariableType.EdgeCount
            or VariableType.DegreeSum;
}
