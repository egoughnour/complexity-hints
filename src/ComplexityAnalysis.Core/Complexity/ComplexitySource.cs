namespace ComplexityAnalysis.Core.Complexity;

/// <summary>
/// The type of source for a complexity claim.
/// Ordered from most to least authoritative.
/// </summary>
public enum SourceType
{
    /// <summary>
    /// Documented in official Microsoft docs with explicit complexity.
    /// Highest confidence.
    /// </summary>
    Documented = 0,

    /// <summary>
    /// Attested in academic papers, CLRS, or other authoritative sources.
    /// High confidence.
    /// </summary>
    Attested = 1,

    /// <summary>
    /// Measured via benchmarking with verification.
    /// Good confidence, but environment-dependent.
    /// </summary>
    Empirical = 2,

    /// <summary>
    /// Inferred from source code analysis.
    /// Medium confidence.
    /// </summary>
    Inferred = 3,

    /// <summary>
    /// Conservative estimate when exact complexity is unknown.
    /// Prefer overestimate to underestimate.
    /// </summary>
    Heuristic = 4,

    /// <summary>
    /// Unknown source or unverified claim.
    /// Lowest confidence.
    /// </summary>
    Unknown = 5
}

/// <summary>
/// Records the source and confidence level for a complexity claim.
/// Essential for audit trails and conservative estimation.
/// </summary>
public sealed record ComplexitySource
{
    /// <summary>
    /// The type of source for this complexity claim.
    /// </summary>
    public required SourceType Type { get; init; }

    /// <summary>
    /// Citation or reference for the source.
    /// Examples:
    /// - URL to Microsoft docs
    /// - "CLRS 4th ed., Chapter 7"
    /// - "Measured via BenchmarkDotNet"
    /// - "Conservative estimate: worst-case resize"
    /// </summary>
    public required string Citation { get; init; }

    /// <summary>
    /// Confidence level in the claim (0.0 to 1.0).
    /// - 1.0: Certain (documented, verified)
    /// - 0.8-0.9: High confidence (attested, empirical)
    /// - 0.5-0.7: Medium confidence (inferred, heuristic)
    /// - &lt;0.5: Low confidence (uncertain)
    /// </summary>
    public double Confidence { get; init; } = 1.0;

    /// <summary>
    /// Whether this is an upper bound (conservative overestimate).
    /// When true, actual complexity may be lower.
    /// </summary>
    public bool IsUpperBound { get; init; }

    /// <summary>
    /// Whether this is an amortized complexity.
    /// Individual operations may exceed this bound.
    /// </summary>
    public bool IsAmortized { get; init; }

    /// <summary>
    /// Whether this complexity is for the worst case.
    /// </summary>
    public bool IsWorstCase { get; init; } = true;

    /// <summary>
    /// Optional notes about edge cases or assumptions.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Date the source was last verified (if applicable).
    /// </summary>
    public DateOnly? LastVerified { get; init; }

    /// <summary>
    /// Creates a documented source from Microsoft docs.
    /// </summary>
    public static ComplexitySource FromMicrosoftDocs(string url, string? notes = null) =>
        new()
        {
            Type = SourceType.Documented,
            Citation = url,
            Confidence = 1.0,
            Notes = notes,
            LastVerified = DateOnly.FromDateTime(DateTime.Today)
        };

    /// <summary>
    /// Creates an attested source from academic literature.
    /// </summary>
    public static ComplexitySource FromAcademic(string citation, double confidence = 0.95) =>
        new()
        {
            Type = SourceType.Attested,
            Citation = citation,
            Confidence = confidence
        };

    /// <summary>
    /// Creates an empirical source from benchmarking.
    /// </summary>
    public static ComplexitySource FromBenchmark(string description, double confidence = 0.85) =>
        new()
        {
            Type = SourceType.Empirical,
            Citation = description,
            Confidence = confidence,
            Notes = "Environment-dependent"
        };

    /// <summary>
    /// Creates an inferred source from code analysis.
    /// </summary>
    public static ComplexitySource Inferred(string reasoning, double confidence = 0.7) =>
        new()
        {
            Type = SourceType.Inferred,
            Citation = $"Inferred: {reasoning}",
            Confidence = confidence
        };

    /// <summary>
    /// Creates a conservative heuristic estimate.
    /// Always marks as upper bound.
    /// </summary>
    public static ComplexitySource ConservativeHeuristic(string reasoning, double confidence = 0.5) =>
        new()
        {
            Type = SourceType.Heuristic,
            Citation = $"Conservative estimate: {reasoning}",
            Confidence = confidence,
            IsUpperBound = true,
            Notes = "Prefer overestimate to avoid misleading underestimates"
        };

    /// <summary>
    /// Creates an unknown source (used when no information is available).
    /// </summary>
    public static ComplexitySource Unknown() =>
        new()
        {
            Type = SourceType.Unknown,
            Citation = "Unknown - requires investigation",
            Confidence = 0.3,
            IsUpperBound = true
        };

    /// <summary>
    /// Creates a documented source with citation.
    /// Shorthand for BCL mapping declarations.
    /// </summary>
    public static ComplexitySource Documented(string citation) =>
        new()
        {
            Type = SourceType.Documented,
            Citation = citation,
            Confidence = 1.0
        };

    /// <summary>
    /// Creates an attested source with citation.
    /// Shorthand for BCL mapping declarations.
    /// </summary>
    public static ComplexitySource Attested(string citation) =>
        new()
        {
            Type = SourceType.Attested,
            Citation = citation,
            Confidence = 0.95
        };

    /// <summary>
    /// Creates an empirical source with description.
    /// Shorthand for BCL mapping declarations.
    /// </summary>
    public static ComplexitySource Empirical(string description) =>
        new()
        {
            Type = SourceType.Empirical,
            Citation = description,
            Confidence = 0.85
        };

    /// <summary>
    /// Creates a heuristic source with reasoning.
    /// Shorthand for BCL mapping declarations.
    /// </summary>
    public static ComplexitySource Heuristic(string reasoning) =>
        new()
        {
            Type = SourceType.Heuristic,
            Citation = reasoning,
            Confidence = 0.5,
            IsUpperBound = true
        };
}

/// <summary>
/// A complexity expression paired with its source attribution.
/// </summary>
public sealed record AttributedComplexity
{
    /// <summary>
    /// The complexity expression.
    /// </summary>
    public required ComplexityExpression Expression { get; init; }

    /// <summary>
    /// The source of the complexity claim.
    /// </summary>
    public required ComplexitySource Source { get; init; }

    /// <summary>
    /// Whether this result requires human review.
    /// </summary>
    public bool RequiresReview =>
        Source.Type >= SourceType.Heuristic ||
        Source.Confidence < 0.7;

    /// <summary>
    /// Reason for requiring review (if applicable).
    /// </summary>
    public string? ReviewReason =>
        RequiresReview
            ? Source.Type switch
            {
                SourceType.Heuristic => "Heuristic estimate - verify actual complexity",
                SourceType.Unknown => "Unknown complexity - requires investigation",
                _ when Source.Confidence < 0.7 => "Low confidence estimate",
                _ => null
            }
            : null;

    /// <summary>
    /// Creates an attributed complexity from documented source.
    /// </summary>
    public static AttributedComplexity Documented(
        ComplexityExpression expression,
        string url,
        string? notes = null) =>
        new()
        {
            Expression = expression,
            Source = ComplexitySource.FromMicrosoftDocs(url, notes)
        };

    /// <summary>
    /// Creates an attributed complexity from academic source.
    /// </summary>
    public static AttributedComplexity Attested(
        ComplexityExpression expression,
        string citation) =>
        new()
        {
            Expression = expression,
            Source = ComplexitySource.FromAcademic(citation)
        };

    /// <summary>
    /// Creates a conservative heuristic complexity.
    /// </summary>
    public static AttributedComplexity Heuristic(
        ComplexityExpression expression,
        string reasoning) =>
        new()
        {
            Expression = expression,
            Source = ComplexitySource.ConservativeHeuristic(reasoning)
        };
}

/// <summary>
/// Complete result of complexity analysis for a method or code block.
/// </summary>
public sealed record ComplexityResult
{
    /// <summary>
    /// The computed complexity expression.
    /// </summary>
    public required ComplexityExpression Expression { get; init; }

    /// <summary>
    /// Source attribution for the complexity claim.
    /// </summary>
    public required ComplexitySource Source { get; init; }

    /// <summary>
    /// Whether this result requires human review.
    /// </summary>
    public bool RequiresReview { get; init; }

    /// <summary>
    /// Reason for requiring review (if applicable).
    /// </summary>
    public string? ReviewReason { get; init; }

    /// <summary>
    /// Location in source code where this complexity was computed.
    /// </summary>
    public SourceLocation? Location { get; init; }

    /// <summary>
    /// Sub-results that contributed to this complexity.
    /// Useful for explaining how the total was derived.
    /// </summary>
    public IReadOnlyList<ComplexityResult>? SubResults { get; init; }

    /// <summary>
    /// Creates a result with automatic review flagging based on source.
    /// </summary>
    public static ComplexityResult Create(
        ComplexityExpression expression,
        ComplexitySource source,
        SourceLocation? location = null)
    {
        var requiresReview = source.Type >= SourceType.Heuristic || source.Confidence < 0.7;
        var reviewReason = requiresReview
            ? source.Type switch
            {
                SourceType.Heuristic => "Heuristic estimate",
                SourceType.Unknown => "Unknown complexity",
                _ when source.Confidence < 0.7 => "Low confidence",
                _ => null
            }
            : null;

        return new ComplexityResult
        {
            Expression = expression,
            Source = source,
            RequiresReview = requiresReview,
            ReviewReason = reviewReason,
            Location = location
        };
    }
}

/// <summary>
/// Location in source code.
/// </summary>
public sealed record SourceLocation
{
    /// <summary>
    /// File path.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Starting line number (1-based).
    /// </summary>
    public required int StartLine { get; init; }

    /// <summary>
    /// Starting column (0-based).
    /// </summary>
    public int StartColumn { get; init; }

    /// <summary>
    /// Ending line number (1-based).
    /// </summary>
    public int EndLine { get; init; }

    /// <summary>
    /// Ending column (0-based).
    /// </summary>
    public int EndColumn { get; init; }

    public override string ToString() =>
        EndLine > StartLine
            ? $"{FilePath}({StartLine},{StartColumn})-({EndLine},{EndColumn})"
            : $"{FilePath}({StartLine},{StartColumn})";
}
