using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Result of speculative analysis for incomplete or partial code.
/// </summary>
public sealed record SpeculativeAnalysisResult
{
    /// <summary>Best-effort complexity estimate.</summary>
    public ComplexityExpression? Complexity { get; init; }

    /// <summary>Lower bound complexity (what we know for certain).</summary>
    public ComplexityExpression? LowerBound { get; init; }

    /// <summary>Upper bound complexity (conservative estimate).</summary>
    public ComplexityExpression? UpperBound { get; init; }

    /// <summary>Confidence in the result (0.0 to 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Whether the code appears incomplete (NIE, TODO, etc.).</summary>
    public bool IsIncomplete { get; init; }

    /// <summary>Whether the code appears to be a stub.</summary>
    public bool IsStub { get; init; }

    /// <summary>Whether the code contains TODO/FIXME markers.</summary>
    public bool HasTodoMarker { get; init; }

    /// <summary>Whether there's unresolved uncertainty from abstract/interface calls.</summary>
    public bool HasUncertainty { get; init; }

    /// <summary>Whether a complexity contract was used.</summary>
    public bool UsedContract { get; init; }

    /// <summary>Source of uncertainty (e.g., "IProcessor.Process").</summary>
    public string? UncertaintySource { get; init; }

    /// <summary>Methods this analysis depends on (for uncertainty tracking).</summary>
    public IReadOnlyList<string>? DependsOn { get; init; }

    /// <summary>Detected code patterns that inform the analysis.</summary>
    public IReadOnlyList<CodePattern> DetectedPatterns { get; init; } = Array.Empty<CodePattern>();

    /// <summary>Explanation of the analysis.</summary>
    public string? Explanation { get; init; }

    public override string ToString() =>
        $"Complexity: {Complexity?.ToBigONotation() ?? "?"}, Confidence: {Confidence:P0}, " +
        $"Incomplete: {IsIncomplete}, Stub: {IsStub}, HasUncertainty: {HasUncertainty}";

    public static SpeculativeAnalysisResult Incomplete(string explanation) =>
        new()
        {
            IsIncomplete = true,
            Confidence = 0.0,
            Explanation = explanation
        };

    public static SpeculativeAnalysisResult Stub(ComplexityExpression? complexity = null) =>
        new()
        {
            IsStub = true,
            Complexity = complexity ?? ConstantComplexity.One,
            LowerBound = ConstantComplexity.One,
            Confidence = 0.3,
            Explanation = "Method appears to be a stub implementation"
        };

    public static SpeculativeAnalysisResult WithUncertainty(
        ComplexityExpression? lowerBound,
        string uncertaintySource,
        IReadOnlyList<string> dependsOn) =>
        new()
        {
            LowerBound = lowerBound,
            HasUncertainty = true,
            UncertaintySource = uncertaintySource,
            DependsOn = dependsOn,
            Confidence = 0.5,
            Explanation = $"Complexity depends on: {uncertaintySource}"
        };

    public static SpeculativeAnalysisResult FromContract(
        ComplexityExpression complexity,
        string contractSource) =>
        new()
        {
            Complexity = complexity,
            LowerBound = complexity,
            UpperBound = complexity,
            UsedContract = true,
            Confidence = 0.9,
            Explanation = $"Used complexity contract from {contractSource}"
        };
}

/// <summary>
/// Detected code pattern that informs speculative analysis.
/// </summary>
public enum CodePattern
{
    /// <summary>throw new NotImplementedException()</summary>
    ThrowsNotImplementedException,

    /// <summary>throw new NotSupportedException()</summary>
    ThrowsNotSupportedException,

    /// <summary>Contains TODO/FIXME/HACK comment</summary>
    HasTodoComment,

    /// <summary>Returns default/null/empty</summary>
    ReturnsDefault,

    /// <summary>Method body is empty or just returns</summary>
    EmptyBody,

    /// <summary>Only increments counter (mock pattern)</summary>
    CounterOnly,

    /// <summary>Returns constant value</summary>
    ReturnsConstant,

    /// <summary>Calls abstract method</summary>
    CallsAbstract,

    /// <summary>Calls interface method</summary>
    CallsInterface,

    /// <summary>Calls virtual method that may be overridden</summary>
    CallsVirtual,

    /// <summary>Has [Complexity] attribute</summary>
    HasComplexityAttribute,

    /// <summary>Has XML doc with complexity info</summary>
    HasComplexityXmlDoc
}

/// <summary>
/// Analyzes partial, incomplete, or abstract code to produce speculative complexity estimates.
/// 
/// This is Phase D of the analysis pipeline, handling:
/// - Incomplete implementations (NotImplementedException, TODO)
/// - Abstract method calls
/// - Interface method calls  
/// - Stub detection
/// - Complexity contracts (attributes, XML docs)
/// </summary>
public sealed class SpeculativeAnalyzer
{
    private readonly SemanticModel _semanticModel;
    private readonly IncompleteCodeDetector _incompleteDetector;
    private readonly StubDetector _stubDetector;
    private readonly UncertaintyTracker _uncertaintyTracker;
    private readonly ComplexityContractReader _contractReader;

    public SpeculativeAnalyzer(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
        _incompleteDetector = new IncompleteCodeDetector();
        _stubDetector = new StubDetector();
        _uncertaintyTracker = new UncertaintyTracker(semanticModel);
        _contractReader = new ComplexityContractReader(semanticModel);
    }

    /// <summary>
    /// Analyzes a method for speculative complexity, handling incomplete code.
    /// </summary>
    public SpeculativeAnalysisResult Analyze(MethodDeclarationSyntax method)
    {
        var patterns = new List<CodePattern>();
        var dependencies = new List<string>();
        string? uncertaintySource = null;

        // 1. Check for incomplete code patterns
        var incompleteResult = _incompleteDetector.Detect(method);
        patterns.AddRange(incompleteResult.Patterns);

        if (incompleteResult.IsDefinitelyIncomplete)
        {
            return new SpeculativeAnalysisResult
            {
                IsIncomplete = true,
                HasTodoMarker = incompleteResult.HasTodoMarker,
                Confidence = 0.1,
                DetectedPatterns = patterns,
                Explanation = incompleteResult.Explanation
            };
        }

        // 2. Check for stub patterns
        var stubResult = _stubDetector.Detect(method, _semanticModel);
        patterns.AddRange(stubResult.Patterns);

        if (stubResult.IsStub)
        {
            return new SpeculativeAnalysisResult
            {
                IsStub = true,
                IsIncomplete = incompleteResult.IsLikelyIncomplete,
                HasTodoMarker = incompleteResult.HasTodoMarker,
                Complexity = ConstantComplexity.One,
                LowerBound = ConstantComplexity.One,
                Confidence = 0.3,
                DetectedPatterns = patterns,
                Explanation = stubResult.Explanation
            };
        }

        // 3. Check for complexity contracts
        var methodSymbol = _semanticModel.GetDeclaredSymbol(method);
        if (methodSymbol is not null)
        {
            var contract = _contractReader.ReadContract(methodSymbol);
            if (contract is not null)
            {
                patterns.Add(contract.Source == "attribute"
                    ? CodePattern.HasComplexityAttribute
                    : CodePattern.HasComplexityXmlDoc);

                return new SpeculativeAnalysisResult
                {
                    Complexity = contract.Complexity,
                    LowerBound = contract.Complexity,
                    UpperBound = contract.Complexity,
                    UsedContract = true,
                    Confidence = 0.9,
                    DetectedPatterns = patterns,
                    Explanation = $"Used complexity contract: {contract.Complexity.ToBigONotation()}"
                };
            }
        }

        // 4. Track uncertainty from abstract/interface calls
        var uncertainty = _uncertaintyTracker.Analyze(method);
        patterns.AddRange(uncertainty.Patterns);
        dependencies.AddRange(uncertainty.Dependencies);

        if (uncertainty.HasUncertainty)
        {
            uncertaintySource = string.Join(", ", uncertainty.Dependencies.Take(3));

            return new SpeculativeAnalysisResult
            {
                LowerBound = uncertainty.KnownComplexity,
                HasUncertainty = true,
                UncertaintySource = uncertaintySource,
                DependsOn = dependencies,
                HasTodoMarker = incompleteResult.HasTodoMarker,
                Confidence = 0.5,
                DetectedPatterns = patterns,
                Explanation = $"Complexity depends on: {uncertaintySource}"
            };
        }

        // 5. If we have partial TODO markers but otherwise complete code
        if (incompleteResult.HasTodoMarker)
        {
            return new SpeculativeAnalysisResult
            {
                HasTodoMarker = true,
                LowerBound = uncertainty.KnownComplexity ?? ConstantComplexity.One,
                Confidence = 0.7,
                DetectedPatterns = patterns,
                Explanation = "Code has TODO markers but appears mostly complete"
            };
        }

        // 6. Code appears complete - return high confidence
        return new SpeculativeAnalysisResult
        {
            Complexity = uncertainty.KnownComplexity,
            LowerBound = uncertainty.KnownComplexity,
            UpperBound = uncertainty.KnownComplexity,
            Confidence = 0.95,
            DetectedPatterns = patterns,
            Explanation = "Code appears complete"
        };
    }

    /// <summary>
    /// Analyzes a method by name in the compilation.
    /// </summary>
    public SpeculativeAnalysisResult? AnalyzeMethod(SyntaxTree tree, string methodName)
    {
        var root = tree.GetRoot();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == methodName);

        return method is null ? null : Analyze(method);
    }
}
