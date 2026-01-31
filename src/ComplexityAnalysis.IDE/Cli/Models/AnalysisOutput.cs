using System.Text.Json.Serialization;

namespace ComplexityAnalysis.IDE.Cli.Models;

/// <summary>
/// JSON output model for complexity analysis results.
/// </summary>
public sealed class AnalysisOutput
{
    [JsonPropertyName("success")]
    public bool Success { get; init; } = true;

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("document")]
    public string? Document { get; init; }

    [JsonPropertyName("methods")]
    public List<MethodHint> Methods { get; init; } = new();

    [JsonPropertyName("elapsed_ms")]
    public long ElapsedMs { get; init; }
}

/// <summary>
/// Complexity hint for a single method - matches VS Code extension types.
/// </summary>
public sealed class MethodHint
{
    /// <summary>
    /// Unique identifier for the method within the document.
    /// Format: MethodDeclaration::{MethodName}::{SpanStart}
    /// </summary>
    [JsonPropertyName("methodId")]
    public required string MethodId { get; init; }

    /// <summary>
    /// The method name (for display).
    /// </summary>
    [JsonPropertyName("methodName")]
    public required string MethodName { get; init; }

    /// <summary>
    /// 1-based line number where the method starts.
    /// </summary>
    [JsonPropertyName("line")]
    public required int Line { get; init; }

    /// <summary>
    /// 0-based character offset within the line.
    /// </summary>
    [JsonPropertyName("character")]
    public required int Character { get; init; }

    /// <summary>
    /// Time complexity in Big-O notation, e.g., "O(n log n)".
    /// </summary>
    [JsonPropertyName("timeComplexity")]
    public required string TimeComplexity { get; init; }

    /// <summary>
    /// Space complexity in Big-O notation (if computed).
    /// </summary>
    [JsonPropertyName("spaceComplexity")]
    public string? SpaceComplexity { get; init; }

    /// <summary>
    /// Confidence score from 0.0 to 1.0.
    /// </summary>
    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }

    /// <summary>
    /// Whether this result requires human review.
    /// </summary>
    [JsonPropertyName("requiresReview")]
    public bool RequiresReview { get; init; }

    /// <summary>
    /// Reason for requiring review (if applicable).
    /// </summary>
    [JsonPropertyName("reviewReason")]
    public string? ReviewReason { get; init; }
}

/// <summary>
/// Version information output.
/// </summary>
public sealed class VersionOutput
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("roslyn_version")]
    public required string RoslynVersion { get; init; }

    [JsonPropertyName("runtime")]
    public required string Runtime { get; init; }
}

/// <summary>
/// Environment probe output.
/// </summary>
public sealed class ProbeOutput
{
    [JsonPropertyName("dotnet")]
    public ToolInfo Dotnet { get; init; } = new();

    [JsonPropertyName("python")]
    public ToolInfo? Python { get; init; }

    [JsonPropertyName("uv")]
    public ToolInfo? Uv { get; init; }
}

/// <summary>
/// Tool availability information.
/// </summary>
public sealed class ToolInfo
{
    [JsonPropertyName("available")]
    public bool Available { get; init; }

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("path")]
    public string? Path { get; init; }
}
