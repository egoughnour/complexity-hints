using System.Collections.Immutable;
using System.Xml.Linq;
using ComplexityAnalysis.Core.Complexity;
using Microsoft.CodeAnalysis;

namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Complexity contract information from attributes or XML docs.
/// </summary>
public sealed record ComplexityContract
{
    public required ComplexityExpression Complexity { get; init; }
    public required string Source { get; init; } // "attribute" or "xmldoc"
    public string? RawText { get; init; }
}

/// <summary>
/// Reads complexity contracts from:
/// - [Complexity("O(n)")] attributes
/// - XML documentation with complexity info
/// </summary>
public sealed class ComplexityContractReader
{
    private readonly SemanticModel _semanticModel;

    public ComplexityContractReader(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    /// Reads complexity contract from a method symbol.
    /// </summary>
    public ComplexityContract? ReadContract(IMethodSymbol method)
    {
        // 1. Check for [Complexity] attribute
        var attrContract = ReadFromAttribute(method);
        if (attrContract is not null)
            return attrContract;

        // 2. Check XML documentation
        var xmlContract = ReadFromXmlDoc(method);
        if (xmlContract is not null)
            return xmlContract;

        return null;
    }

    private ComplexityContract? ReadFromAttribute(IMethodSymbol method)
    {
        // Look for [Complexity("O(n)")] or [TimeComplexity("O(n log n)")] etc.
        foreach (var attr in method.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;
            if (attrName is "ComplexityAttribute" or "Complexity" or 
                "TimeComplexityAttribute" or "TimeComplexity" or
                "BigOAttribute" or "BigO")
            {
                // Get the first constructor argument
                if (attr.ConstructorArguments.Length > 0)
                {
                    var arg = attr.ConstructorArguments[0];
                    if (arg.Value is string complexityStr)
                    {
                        var complexity = ParseComplexityString(complexityStr);
                        if (complexity is not null)
                        {
                            return new ComplexityContract
                            {
                                Complexity = complexity,
                                Source = "attribute",
                                RawText = complexityStr
                            };
                        }
                    }
                }
            }
        }

        return null;
    }

    private ComplexityContract? ReadFromXmlDoc(IMethodSymbol method)
    {
        var xml = method.GetDocumentationCommentXml();
        if (string.IsNullOrWhiteSpace(xml))
            return null;

        try
        {
            var doc = XDocument.Parse(xml);

            // Look for <complexity> element
            var complexityElement = doc.Descendants("complexity").FirstOrDefault();
            if (complexityElement is not null)
            {
                var complexityStr = complexityElement.Value.Trim();
                var complexity = ParseComplexityString(complexityStr);
                if (complexity is not null)
                {
                    return new ComplexityContract
                    {
                        Complexity = complexity,
                        Source = "xmldoc",
                        RawText = complexityStr
                    };
                }
            }

            // Look for <remarks> containing "O(" notation
            var remarksElements = doc.Descendants("remarks");
            foreach (var remarks in remarksElements)
            {
                var text = remarks.Value;
                var complexity = ExtractComplexityFromText(text);
                if (complexity is not null)
                {
                    return new ComplexityContract
                    {
                        Complexity = complexity,
                        Source = "xmldoc",
                        RawText = text
                    };
                }
            }

            // Look in summary for complexity mentions
            var summaryElement = doc.Descendants("summary").FirstOrDefault();
            if (summaryElement is not null)
            {
                var text = summaryElement.Value;
                var complexity = ExtractComplexityFromText(text);
                if (complexity is not null)
                {
                    return new ComplexityContract
                    {
                        Complexity = complexity,
                        Source = "xmldoc",
                        RawText = text
                    };
                }
            }
        }
        catch
        {
            // XML parsing failed, skip
        }

        return null;
    }

    private static ComplexityExpression? ExtractComplexityFromText(string text)
    {
        // Look for patterns like "O(n)", "O(n log n)", "O(n^2)", etc.
        var patterns = new[]
        {
            @"O\s*\(\s*1\s*\)",
            @"O\s*\(\s*log\s*n\s*\)",
            @"O\s*\(\s*n\s*\)",
            @"O\s*\(\s*n\s*log\s*n\s*\)",
            @"O\s*\(\s*n\^2\s*\)",
            @"O\s*\(\s*n\s*\*\s*n\s*\)",
            @"O\s*\(\s*n\^3\s*\)",
            @"O\s*\(\s*2\^n\s*\)",
            @"O\s*\(\s*n!\s*\)"
        };

        var complexities = new ComplexityExpression[]
        {
            ConstantComplexity.One,
            new LogarithmicComplexity(1.0, Variable.N),
            new VariableComplexity(Variable.N),
            PolyLogComplexity.NLogN(),
            PolynomialComplexity.OfDegree(2, Variable.N),
            PolynomialComplexity.OfDegree(2, Variable.N),
            PolynomialComplexity.OfDegree(3, Variable.N),
            new ExponentialComplexity(2, Variable.N),
            new FactorialComplexity(Variable.N)
        };

        for (int i = 0; i < patterns.Length; i++)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(text, patterns[i], 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return complexities[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Parses a complexity string like "O(n)", "O(n log n)", "O(n^2)".
    /// </summary>
    public static ComplexityExpression? ParseComplexityString(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        str = str.Trim().ToUpperInvariant();

        // Remove "O(" prefix and ")" suffix
        if (str.StartsWith("O(") && str.EndsWith(")"))
        {
            str = str.Substring(2, str.Length - 3).Trim();
        }
        else if (str.StartsWith("O (") && str.EndsWith(")"))
        {
            str = str.Substring(3, str.Length - 4).Trim();
        }

        // Normalize
        str = str.Replace(" ", "").ToUpperInvariant();

        return str switch
        {
            "1" => ConstantComplexity.One,
            "LOGN" or "LOG(N)" => new LogarithmicComplexity(1.0, Variable.N),
            "N" => new VariableComplexity(Variable.N),
            "NLOGN" or "N*LOGN" or "NLOG(N)" => PolyLogComplexity.NLogN(),
            "N^2" or "N*N" or "N²" => PolynomialComplexity.OfDegree(2, Variable.N),
            "N^3" or "N³" => PolynomialComplexity.OfDegree(3, Variable.N),
            "N^4" => PolynomialComplexity.OfDegree(4, Variable.N),
            "2^N" or "2ⁿ" => new ExponentialComplexity(2, Variable.N),
            "N!" => new FactorialComplexity(Variable.N),
            _ => TryParsePolynomial(str)
        };
    }

    private static ComplexityExpression? TryParsePolynomial(string str)
    {
        // Try to parse n^k
        if (str.StartsWith("N^"))
        {
            var expStr = str.Substring(2);
            if (int.TryParse(expStr, out var exp))
            {
                return PolynomialComplexity.OfDegree(exp, Variable.N);
            }
        }

        // Try to parse k^n
        if (str.EndsWith("^N"))
        {
            var baseStr = str.Substring(0, str.Length - 2);
            if (int.TryParse(baseStr, out var baseVal))
            {
                return new ExponentialComplexity(baseVal, Variable.N);
            }
        }

        return null;
    }
}
