using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Result of uncertainty tracking.
/// </summary>
public sealed record UncertaintyResult
{
    public bool HasUncertainty { get; init; }
    public ComplexityExpression? KnownComplexity { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<CodePattern> Patterns { get; init; } = Array.Empty<CodePattern>();
    public string? Explanation { get; init; }
}

/// <summary>
/// Tracks uncertainty from abstract, virtual, and interface method calls.
/// When complexity depends on runtime polymorphism, we track the dependency
/// rather than making potentially incorrect assumptions.
/// </summary>
public sealed class UncertaintyTracker
{
    private readonly SemanticModel _semanticModel;

    public UncertaintyTracker(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Analyzes a method for uncertainty from polymorphic calls.
    /// </summary>
    public UncertaintyResult Analyze(MethodDeclarationSyntax method)
    {
        var patterns = new List<CodePattern>();
        var dependencies = new List<string>();
        bool hasUncertainty = false;

        // Find all method invocations
        var invocations = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .ToList();

        foreach (var invocation in invocations)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                continue;

            // Check if it's a polymorphic call
            if (IsPolymorphicCall(methodSymbol, out var pattern, out var dependency))
            {
                hasUncertainty = true;
                patterns.Add(pattern);
                dependencies.Add(dependency);
            }
        }

        // Find member accesses that might be polymorphic (property getters, etc.)
        var memberAccesses = method.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .ToList();

        foreach (var access in memberAccesses)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(access);
            if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
            {
                if (IsPolymorphicProperty(propertySymbol, out var pattern, out var dependency))
                {
                    hasUncertainty = true;
                    patterns.Add(pattern);
                    dependencies.Add(dependency);
                }
            }
        }

        return new UncertaintyResult
        {
            HasUncertainty = hasUncertainty,
            KnownComplexity = hasUncertainty ? null : ConstantComplexity.One,
            Dependencies = dependencies.Distinct().ToList(),
            Patterns = patterns.Distinct().ToList(),
            Explanation = hasUncertainty
                ? $"Complexity depends on: {string.Join(", ", dependencies.Distinct().Take(3))}"
                : null
        };
    }

    private bool IsPolymorphicCall(IMethodSymbol method, out CodePattern pattern, out string dependency)
    {
        pattern = CodePattern.CallsVirtual;
        dependency = $"{method.ContainingType.Name}.{method.Name}";

        // Interface method
        if (method.ContainingType.TypeKind == TypeKind.Interface)
        {
            pattern = CodePattern.CallsInterface;
            return true;
        }

        // Abstract method
        if (method.IsAbstract)
        {
            pattern = CodePattern.CallsAbstract;
            return true;
        }

        // Virtual method that isn't sealed
        if (method.IsVirtual && !method.IsSealed)
        {
            pattern = CodePattern.CallsVirtual;
            return true;
        }

        // Override that isn't sealed
        if (method.IsOverride && !method.IsSealed)
        {
            pattern = CodePattern.CallsVirtual;
            return true;
        }

        return false;
    }

    private bool IsPolymorphicProperty(IPropertySymbol property, out CodePattern pattern, out string dependency)
    {
        pattern = CodePattern.CallsVirtual;
        dependency = $"{property.ContainingType.Name}.{property.Name}";

        // Interface property
        if (property.ContainingType.TypeKind == TypeKind.Interface)
        {
            pattern = CodePattern.CallsInterface;
            return true;
        }

        // Abstract property
        if (property.IsAbstract)
        {
            pattern = CodePattern.CallsAbstract;
            return true;
        }

        // Virtual property that isn't sealed
        if (property.IsVirtual && !property.IsSealed)
        {
            pattern = CodePattern.CallsVirtual;
            return true;
        }

        return false;
    }
}
