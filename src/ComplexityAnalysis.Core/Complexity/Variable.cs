using System.Collections.Immutable;

namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// Represents a variable in complexity expressions (e.g., n, V, E, degree).
/// </summary>
/// <remarks>
/// <para>
/// Variables are symbolic placeholders for input sizes and algorithm parameters.
/// Unlike mathematical variables, complexity variables carry semantic meaning
/// through their <see cref="VariableType"/> to enable domain-specific analysis.
/// </para>
/// 
/// <para>
/// <b>Variable Semantics by Domain:</b>
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Domain</term>
///     <description>Common Variables</description>
///   </listheader>
///   <item>
///     <term>General</term>
///     <description><c>n</c> (input size), <c>k</c> (parameter count)</description>
///   </item>
///   <item>
///     <term>Graphs</term>
///     <description><c>V</c> (vertices), <c>E</c> (edges), with relationship E ≤ V²</description>
///   </item>
///   <item>
///     <term>Trees</term>
///     <description><c>n</c> (nodes), <c>h</c> (height), with h ∈ [log n, n]</description>
///   </item>
///   <item>
///     <term>Strings</term>
///     <description><c>n</c> (text length), <c>m</c> (pattern length)</description>
///   </item>
///   <item>
///     <term>Parallel</term>
///     <description><c>n</c> (work), <c>p</c> (processors)</description>
///   </item>
/// </list>
/// 
/// <para>
/// <b>Multi-Variable Complexity:</b> Many algorithms have complexity dependent on
/// multiple variables. The system supports this through expression composition:
/// </para>
/// <code>
/// // Graph algorithm: O(V + E)
/// var graphComplexity = new BinaryOperationComplexity(
///     new VariableComplexity(Variable.V),
///     BinaryOp.Plus,
///     new VariableComplexity(Variable.E));
///     
/// // String matching: O(n × m)
/// var stringComplexity = new BinaryOperationComplexity(
///     new VariableComplexity(Variable.N),
///     BinaryOp.Multiply,
///     new VariableComplexity(Variable.M));
/// </code>
/// 
/// <para>
/// <b>Implicit Relationships:</b> Some variables have implicit constraints:
/// </para>
/// <list type="bullet">
///   <item><description>In connected graphs: E ≥ V - 1</description></item>
///   <item><description>In simple graphs: E ≤ V(V-1)/2</description></item>
///   <item><description>In balanced trees: h = Θ(log n)</description></item>
///   <item><description>In linked structures: h ≤ n</description></item>
/// </list>
/// </remarks>
/// <seealso cref="VariableType"/>
/// <seealso cref="VariableComplexity"/>
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

    /// <summary>
    /// Creates a processor count variable named "p" (for parallel complexity).
    /// </summary>
    public static Variable P => new("p", VariableType.ProcessorCount) { Description = "Number of processors" };

    public override string ToString() => Name;

    public bool Equals(Variable? other) =>
        other is not null && Name == other.Name && Type == other.Type;

    public override int GetHashCode() => HashCode.Combine(Name, Type);
}

/// <summary>
/// Semantic types for complexity variables, indicating what the variable represents.
/// </summary>
/// <remarks>
/// <para>
/// Variable types enable semantic analysis and validation. For example, the analyzer
/// can verify that graph algorithms use <see cref="VertexCount"/> and <see cref="EdgeCount"/>
/// appropriately, or flag potential issues when tree algorithms don't account for
/// <see cref="TreeHeight"/>.
/// </para>
/// <para>
/// <b>Type Relationships:</b>
/// </para>
/// <list type="bullet">
///   <item><description><see cref="VertexCount"/> and <see cref="EdgeCount"/> often appear together: O(V + E)</description></item>
///   <item><description><see cref="InputSize"/> is the default for general algorithms</description></item>
///   <item><description><see cref="SecondarySize"/> is used when two independent sizes matter (O(n × m))</description></item>
/// </list>
/// </remarks>
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
    /// Number of processors/cores (for parallel complexity).
    /// </summary>
    ProcessorCount,

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
