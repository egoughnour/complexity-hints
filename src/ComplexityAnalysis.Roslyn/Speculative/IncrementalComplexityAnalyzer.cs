using System.Collections.Concurrent;
using ComplexityAnalysis.Core.Complexity;
using ComplexityAnalysis.Core.Progress;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Provides incremental complexity analysis for code being actively edited.
/// Designed for real-time feedback in IDE scenarios where code may be incomplete
/// or syntactically invalid during typing.
/// 
/// Key features:
/// - Parses incomplete/malformed syntax gracefully
/// - Caches analysis results for unchanged code regions
/// - Streams progress callbacks during analysis
/// - Provides confidence-weighted estimates for partial constructs
/// </summary>
public sealed class IncrementalComplexityAnalyzer
{
    private readonly SyntaxFragmentAnalyzer _fragmentAnalyzer;
    private readonly ConcurrentDictionary<string, CachedAnalysis> _cache;
    private readonly IOnlineAnalysisCallback? _callback;
    private readonly AnalysisOptions _options;

    /// <summary>
    /// Creates a new incremental analyzer with optional callback.
    /// </summary>
    public IncrementalComplexityAnalyzer(
        IOnlineAnalysisCallback? callback = null,
        AnalysisOptions? options = null)
    {
        _callback = callback;
        _options = options ?? AnalysisOptions.Default;
        _fragmentAnalyzer = new SyntaxFragmentAnalyzer();
        _cache = new ConcurrentDictionary<string, CachedAnalysis>();
    }

    /// <summary>
    /// Analyzes code text incrementally, reporting progress via callbacks.
    /// Handles incomplete syntax gracefully.
    /// </summary>
    /// <param name="sourceText">The current source text (may be incomplete)</param>
    /// <param name="position">Caret position in the text</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    public async Task<OnlineAnalysisResult> AnalyzeAsync(
        string sourceText,
        int position = -1,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _callback?.OnAnalysisStarted(sourceText.Length);

        try
        {
            // Phase 1: Parse with recovery
            _callback?.OnPhaseStarted(OnlineAnalysisPhase.Parsing);
            var parseResult = await ParseWithRecoveryAsync(sourceText, cancellationToken);
            _callback?.OnPhaseCompleted(OnlineAnalysisPhase.Parsing);

            if (cancellationToken.IsCancellationRequested)
                return OnlineAnalysisResult.Cancelled();

            // Phase 2: Identify analysis scope
            _callback?.OnPhaseStarted(OnlineAnalysisPhase.ScopeDetection);
            var scope = DetermineAnalysisScope(parseResult.SyntaxTree, position);
            _callback?.OnPhaseCompleted(OnlineAnalysisPhase.ScopeDetection);

            if (cancellationToken.IsCancellationRequested)
                return OnlineAnalysisResult.Cancelled();

            // Phase 3: Analyze fragments
            _callback?.OnPhaseStarted(OnlineAnalysisPhase.FragmentAnalysis);
            var fragmentResults = await AnalyzeFragmentsAsync(
                parseResult, scope, cancellationToken);
            _callback?.OnPhaseCompleted(OnlineAnalysisPhase.FragmentAnalysis);

            if (cancellationToken.IsCancellationRequested)
                return OnlineAnalysisResult.Cancelled();

            // Phase 4: Compose results
            _callback?.OnPhaseStarted(OnlineAnalysisPhase.Composition);
            var result = ComposeResults(fragmentResults, parseResult, position);
            _callback?.OnPhaseCompleted(OnlineAnalysisPhase.Composition);

            var elapsed = DateTime.UtcNow - startTime;
            _callback?.OnAnalysisCompleted(result, elapsed);

            return result;
        }
        catch (OperationCanceledException)
        {
            return OnlineAnalysisResult.Cancelled();
        }
        catch (Exception ex)
        {
            _callback?.OnError(ex);
            return OnlineAnalysisResult.Error(ex.Message);
        }
    }

    /// <summary>
    /// Analyzes a specific method by name, useful for targeted analysis.
    /// </summary>
    public async Task<MethodAnalysisSnapshot?> AnalyzeMethodAsync(
        string sourceText,
        string methodName,
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseWithRecoveryAsync(sourceText, cancellationToken);
        var root = await parseResult.SyntaxTree.GetRootAsync(cancellationToken);

        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        if (method is null)
            return null;

        return AnalyzeMethodFragment(method, parseResult);
    }

    /// <summary>
    /// Gets cached analysis for a code region, or null if not cached.
    /// </summary>
    public CachedAnalysis? GetCachedAnalysis(string codeHash)
    {
        return _cache.TryGetValue(codeHash, out var cached) ? cached : null;
    }

    /// <summary>
    /// Clears the analysis cache.
    /// </summary>
    public void ClearCache() => _cache.Clear();

    #region Parsing

    private async Task<ParseResult> ParseWithRecoveryAsync(
        string sourceText,
        CancellationToken cancellationToken)
    {
        var options = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest)
            .WithKind(SourceCodeKind.Script); // More lenient parsing

        var tree = CSharpSyntaxTree.ParseText(
            sourceText,
            options,
            cancellationToken: cancellationToken);

        var root = await tree.GetRootAsync(cancellationToken);
        var diagnostics = tree.GetDiagnostics().ToList();

        // Identify incomplete constructs
        var incompleteNodes = FindIncompleteNodes(root);

        return new ParseResult
        {
            SyntaxTree = tree,
            Root = root,
            Diagnostics = diagnostics,
            IncompleteNodes = incompleteNodes,
            HasErrors = diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            IsComplete = incompleteNodes.Count == 0 && !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)
        };
    }

    private List<IncompleteNode> FindIncompleteNodes(SyntaxNode root)
    {
        var incomplete = new List<IncompleteNode>();

        foreach (var node in root.DescendantNodesAndTokens())
        {
            // Missing tokens indicate incomplete syntax
            if (node.IsToken && node.AsToken().IsMissing)
            {
                incomplete.Add(new IncompleteNode
                {
                    Node = node.AsToken().Parent,
                    Reason = IncompleteReason.MissingToken,
                    MissingElement = node.AsToken().Kind().ToString()
                });
            }

            // Skipped tokens indicate parser recovery
            if (node.IsToken && node.AsToken().Kind() == SyntaxKind.SkippedTokensTrivia)
            {
                incomplete.Add(new IncompleteNode
                {
                    Node = node.AsToken().Parent,
                    Reason = IncompleteReason.SkippedTokens
                });
            }

            // Methods without bodies
            if (node.IsNode && node.AsNode() is MethodDeclarationSyntax method)
            {
                if (method.Body is null && method.ExpressionBody is null && 
                    !method.Modifiers.Any(SyntaxKind.AbstractKeyword) &&
                    !method.Modifiers.Any(SyntaxKind.ExternKeyword))
                {
                    incomplete.Add(new IncompleteNode
                    {
                        Node = method,
                        Reason = IncompleteReason.MissingBody
                    });
                }
            }

            // For loops with missing parts
            if (node.IsNode && node.AsNode() is ForStatementSyntax forStmt)
            {
                if (forStmt.Statement is EmptyStatementSyntax or null)
                {
                    incomplete.Add(new IncompleteNode
                    {
                        Node = forStmt,
                        Reason = IncompleteReason.IncompleteLoop
                    });
                }
            }
        }

        return incomplete;
    }

    #endregion

    #region Scope Detection

    private AnalysisScope DetermineAnalysisScope(SyntaxTree tree, int position)
    {
        var root = tree.GetRoot();

        // If position specified, focus on containing construct
        if (position >= 0 && position < root.FullSpan.Length)
        {
            var token = root.FindToken(position);
            var containingMethod = token.Parent?.AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (containingMethod is not null)
            {
                return new AnalysisScope
                {
                    ScopeType = ScopeType.Method,
                    FocusNode = containingMethod,
                    Position = position
                };
            }

            var containingClass = token.Parent?.AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (containingClass is not null)
            {
                return new AnalysisScope
                {
                    ScopeType = ScopeType.Class,
                    FocusNode = containingClass,
                    Position = position
                };
            }
        }

        // Default: analyze all methods
        return new AnalysisScope
        {
            ScopeType = ScopeType.File,
            FocusNode = root,
            Position = position
        };
    }

    #endregion

    #region Fragment Analysis

    private async Task<List<FragmentAnalysisResult>> AnalyzeFragmentsAsync(
        ParseResult parseResult,
        AnalysisScope scope,
        CancellationToken cancellationToken)
    {
        var results = new List<FragmentAnalysisResult>();
        var methods = GetMethodsInScope(parseResult.Root, scope);
        var total = methods.Count;
        var processed = 0;

        foreach (var method in methods)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var snapshot = AnalyzeMethodFragment(method, parseResult);
            results.Add(new FragmentAnalysisResult
            {
                MethodName = method.Identifier.Text,
                Snapshot = snapshot,
                Span = method.Span
            });

            processed++;
            _callback?.OnProgress(processed, total, method.Identifier.Text);
        }

        return results;
    }

    private List<MethodDeclarationSyntax> GetMethodsInScope(SyntaxNode root, AnalysisScope scope)
    {
        return scope.ScopeType switch
        {
            ScopeType.Method when scope.FocusNode is MethodDeclarationSyntax m => new List<MethodDeclarationSyntax> { m },
            ScopeType.Class when scope.FocusNode is ClassDeclarationSyntax c =>
                c.Members.OfType<MethodDeclarationSyntax>().ToList(),
            _ => root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList()
        };
    }

    private MethodAnalysisSnapshot AnalyzeMethodFragment(
        MethodDeclarationSyntax method,
        ParseResult parseResult)
    {
        var isIncomplete = parseResult.IncompleteNodes
            .Any(n => n.Node?.Span.IntersectsWith(method.Span) == true);

        // Check cache
        var cacheKey = ComputeMethodHash(method);
        if (_cache.TryGetValue(cacheKey, out var cached) && !isIncomplete)
        {
            return cached.Snapshot;
        }

        // Analyze the method
        var result = _fragmentAnalyzer.AnalyzeMethod(method, isIncomplete);

        // Cache if complete
        if (!isIncomplete && result.Confidence > 0.8)
        {
            _cache[cacheKey] = new CachedAnalysis
            {
                Snapshot = result,
                Timestamp = DateTime.UtcNow
            };
        }

        return result;
    }

    private static string ComputeMethodHash(MethodDeclarationSyntax method)
    {
        // Simple hash based on method text
        return method.NormalizeWhitespace().ToFullString().GetHashCode().ToString("X8");
    }

    #endregion

    #region Result Composition

    private OnlineAnalysisResult ComposeResults(
        List<FragmentAnalysisResult> fragments,
        ParseResult parseResult,
        int position)
    {
        var methodResults = fragments
            .Select(f => new MethodComplexitySnapshot
            {
                MethodName = f.MethodName,
                Complexity = f.Snapshot.Complexity,
                Confidence = f.Snapshot.Confidence,
                IsComplete = f.Snapshot.IsComplete,
                IncompleteReason = f.Snapshot.IncompleteReason,
                Span = f.Span
            })
            .ToList();

        // Find the focused method (at caret position)
        MethodComplexitySnapshot? focusedMethod = null;
        if (position >= 0)
        {
            focusedMethod = methodResults
                .FirstOrDefault(m => m.Span.Contains(position));
        }

        // Calculate overall confidence
        var overallConfidence = methodResults.Count > 0
            ? methodResults.Average(m => m.Confidence)
            : 0.0;

        // Aggregate complexity for file-level summary
        var aggregateComplexity = AggregateComplexity(methodResults);

        return new OnlineAnalysisResult
        {
            Success = true,
            Methods = methodResults,
            FocusedMethod = focusedMethod,
            AggregateComplexity = aggregateComplexity,
            OverallConfidence = overallConfidence,
            ParseDiagnostics = parseResult.Diagnostics
                .Select(d => new ParseDiagnostic
                {
                    Message = d.GetMessage(),
                    Severity = d.Severity,
                    Location = d.Location.GetLineSpan()
                })
                .ToList(),
            IsCodeComplete = parseResult.IsComplete,
            IncompleteRegions = parseResult.IncompleteNodes
                .Select(n => new IncompleteRegion
                {
                    Span = n.Node?.Span ?? default,
                    Reason = n.Reason.ToString()
                })
                .ToList()
        };
    }

    private ComplexityExpression AggregateComplexity(List<MethodComplexitySnapshot> methods)
    {
        if (methods.Count == 0)
            return ConstantComplexity.One;

        // For file-level, report the maximum complexity
        ComplexityExpression? max = null;
        var maxDegree = -1.0;

        foreach (var method in methods.Where(m => m.Complexity is not null))
        {
            var degree = EstimateDegree(method.Complexity!);
            if (degree > maxDegree)
            {
                maxDegree = degree;
                max = method.Complexity;
            }
        }

        return max ?? ConstantComplexity.One;
    }

    private static double EstimateDegree(ComplexityExpression expr)
    {
        return expr switch
        {
            ConstantComplexity => 0,
            LogarithmicComplexity => 0.5,
            VariableComplexity => 1,
            LinearComplexity => 1,
            PolyLogComplexity pl => pl.PolyDegree + (pl.LogExponent > 0 ? 0.1 : 0),
            PolynomialComplexity p => p.Degree,
            ExponentialComplexity => 100,
            FactorialComplexity => 200,
            _ => 1
        };
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Options for online analysis.
/// </summary>
public record AnalysisOptions
{
    /// <summary>
    /// Maximum time to spend on analysis before returning partial results.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Whether to use cached results when available.
    /// </summary>
    public bool UseCache { get; init; } = true;

    /// <summary>
    /// Minimum confidence to report a result.
    /// </summary>
    public double MinConfidence { get; init; } = 0.3;

    /// <summary>
    /// Maximum number of methods to analyze in one pass.
    /// </summary>
    public int MaxMethodsPerPass { get; init; } = 50;

    public static AnalysisOptions Default { get; } = new();
}

/// <summary>
/// Phases of online analysis.
/// </summary>
public enum OnlineAnalysisPhase
{
    Parsing,
    ScopeDetection,
    FragmentAnalysis,
    Composition
}

/// <summary>
/// Types of analysis scope.
/// </summary>
public enum ScopeType
{
    File,
    Class,
    Method
}

/// <summary>
/// Reasons for incomplete code.
/// </summary>
public enum IncompleteReason
{
    None,
    MissingToken,
    MissingBody,
    SkippedTokens,
    IncompleteLoop,
    IncompleteExpression,
    ActivelyEditing
}

/// <summary>
/// Result of parsing with recovery.
/// </summary>
internal record ParseResult
{
    public required SyntaxTree SyntaxTree { get; init; }
    public required SyntaxNode Root { get; init; }
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }
    public required List<IncompleteNode> IncompleteNodes { get; init; }
    public bool HasErrors { get; init; }
    public bool IsComplete { get; init; }
}

/// <summary>
/// An incomplete node in the syntax tree.
/// </summary>
internal record IncompleteNode
{
    public SyntaxNode? Node { get; init; }
    public IncompleteReason Reason { get; init; }
    public string? MissingElement { get; init; }
}

/// <summary>
/// Analysis scope definition.
/// </summary>
internal record AnalysisScope
{
    public ScopeType ScopeType { get; init; }
    public SyntaxNode? FocusNode { get; init; }
    public int Position { get; init; }
}

/// <summary>
/// Result of analyzing a code fragment.
/// </summary>
internal record FragmentAnalysisResult
{
    public required string MethodName { get; init; }
    public required MethodAnalysisSnapshot Snapshot { get; init; }
    public TextSpan Span { get; init; }
}

/// <summary>
/// Snapshot of a method's complexity analysis.
/// </summary>
public record MethodAnalysisSnapshot
{
    public ComplexityExpression? Complexity { get; init; }
    public double Confidence { get; init; }
    public bool IsComplete { get; init; }
    public string? IncompleteReason { get; init; }
    public IReadOnlyList<LoopSnapshot>? Loops { get; init; }
    public IReadOnlyList<string>? RecursiveCalls { get; init; }
}

/// <summary>
/// Snapshot of a loop's analysis.
/// </summary>
public record LoopSnapshot
{
    public required string LoopType { get; init; }
    public ComplexityExpression? IterationCount { get; init; }
    public ComplexityExpression? BodyComplexity { get; init; }
    public bool IsComplete { get; init; }
}

/// <summary>
/// Per-method complexity snapshot in online results.
/// </summary>
public record MethodComplexitySnapshot
{
    public required string MethodName { get; init; }
    public ComplexityExpression? Complexity { get; init; }
    public double Confidence { get; init; }
    public bool IsComplete { get; init; }
    public string? IncompleteReason { get; init; }
    public TextSpan Span { get; init; }
}

/// <summary>
/// Parse diagnostic for reporting to UI.
/// </summary>
public record ParseDiagnostic
{
    public required string Message { get; init; }
    public DiagnosticSeverity Severity { get; init; }
    public FileLinePositionSpan Location { get; init; }
}

/// <summary>
/// Region of incomplete code.
/// </summary>
public record IncompleteRegion
{
    public TextSpan Span { get; init; }
    public required string Reason { get; init; }
}

/// <summary>
/// Cached analysis result.
/// </summary>
public record CachedAnalysis
{
    public required MethodAnalysisSnapshot Snapshot { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Overall result of online analysis.
/// </summary>
public record OnlineAnalysisResult
{
    public bool Success { get; init; }
    public bool IsCancelled { get; init; }
    public string? ErrorMessage { get; init; }

    public IReadOnlyList<MethodComplexitySnapshot> Methods { get; init; } = Array.Empty<MethodComplexitySnapshot>();
    public MethodComplexitySnapshot? FocusedMethod { get; init; }
    public ComplexityExpression? AggregateComplexity { get; init; }
    public double OverallConfidence { get; init; }

    public IReadOnlyList<ParseDiagnostic> ParseDiagnostics { get; init; } = Array.Empty<ParseDiagnostic>();
    public bool IsCodeComplete { get; init; }
    public IReadOnlyList<IncompleteRegion> IncompleteRegions { get; init; } = Array.Empty<IncompleteRegion>();

    public static OnlineAnalysisResult Cancelled() => new() { IsCancelled = true };
    public static OnlineAnalysisResult Error(string message) => new() { ErrorMessage = message };
}

#endregion
