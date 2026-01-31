using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using ComplexityAnalysis.IDE.Cli.Models;
using ComplexityAnalysis.Roslyn.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ComplexityAnalysis.IDE.Cli.Commands;

/// <summary>
/// Command to analyze a C# document for complexity.
/// </summary>
public sealed class AnalyzeCommand : Command
{
    public AnalyzeCommand() : base("analyze", "Analyze a C# file or source code for complexity")
    {
        var documentOption = new Option<string?>(
            aliases: new[] { "--document", "-d" },
            description: "Path to the C# file to analyze");

        var stdinOption = new Option<bool>(
            aliases: new[] { "--stdin" },
            description: "Read source code from stdin instead of a file");

        var jsonOption = new Option<bool>(
            aliases: new[] { "--json", "-j" },
            description: "Output results as JSON");

        var methodOption = new Option<string?>(
            aliases: new[] { "--method", "-m" },
            description: "Analyze only a specific method (by name)");

        AddOption(documentOption);
        AddOption(stdinOption);
        AddOption(jsonOption);
        AddOption(methodOption);

        this.SetHandler(ExecuteAsync, documentOption, stdinOption, jsonOption, methodOption);
    }

    private async Task ExecuteAsync(string? documentPath, bool useStdin, bool outputJson, string? methodFilter)
    {
        var stopwatch = Stopwatch.StartNew();
        var output = new AnalysisOutput();

        try
        {
            string sourceCode;
            string documentName;

            if (useStdin)
            {
                sourceCode = await Console.In.ReadToEndAsync();
                documentName = "<stdin>";
            }
            else if (!string.IsNullOrEmpty(documentPath))
            {
                if (!File.Exists(documentPath))
                {
                    output = new AnalysisOutput
                    {
                        Success = false,
                        Error = $"File not found: {documentPath}"
                    };
                    OutputResult(output, outputJson);
                    return;
                }

                sourceCode = await File.ReadAllTextAsync(documentPath);
                documentName = documentPath;
            }
            else
            {
                output = new AnalysisOutput
                {
                    Success = false,
                    Error = "Either --document or --stdin must be specified"
                };
                OutputResult(output, outputJson);
                return;
            }

            // Parse and analyze
            var results = AnalyzeSourceCode(sourceCode, documentName, methodFilter);

            stopwatch.Stop();

            output = new AnalysisOutput
            {
                Success = true,
                Document = documentName,
                Methods = results,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            output = new AnalysisOutput
            {
                Success = false,
                Error = ex.Message,
                ElapsedMs = stopwatch.ElapsedMilliseconds
            };
        }

        OutputResult(output, outputJson);
    }

    private List<MethodHint> AnalyzeSourceCode(string sourceCode, string documentName, string? methodFilter)
    {
        var results = new List<MethodHint>();

        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode, path: documentName);
        var root = syntaxTree.GetRoot();

        // Create a basic compilation for semantic analysis
        var compilation = CSharpCompilation.Create(
            "ComplexityAnalysis",
            syntaxTrees: new[] { syntaxTree },
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Build call graph for better analysis
        var callGraphBuilder = new CallGraphBuilder(compilation);
        var callGraph = callGraphBuilder.Build();

        // Create extractor with call graph
        var extractor = new RoslynComplexityExtractor(semanticModel, callGraph);

        // Find all methods
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => string.IsNullOrEmpty(methodFilter) || 
                       m.Identifier.Text.Equals(methodFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var method in methods)
        {
            try
            {
                var complexity = extractor.AnalyzeMethod(method);
                var location = method.GetLocation().GetLineSpan();

                // Generate method ID matching the VS Code extension format
                var spanStart = method.SpanStart;
                var methodId = $"MethodDeclaration::{method.Identifier.Text}::{spanStart}";

                results.Add(new MethodHint
                {
                    MethodId = methodId,
                    MethodName = method.Identifier.Text,
                    Line = location.StartLinePosition.Line + 1, // 1-based
                    Character = location.StartLinePosition.Character,
                    TimeComplexity = complexity.ToBigONotation(),
                    SpaceComplexity = null, // TODO: Implement space complexity analysis
                    Confidence = ComputeConfidence(complexity),
                    RequiresReview = RequiresReview(complexity),
                    ReviewReason = GetReviewReason(complexity)
                });
            }
            catch (Exception ex)
            {
                // Log error but continue with other methods
                Console.Error.WriteLine($"Error analyzing method {method.Identifier.Text}: {ex.Message}");
            }
        }

        return results;
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        // Get basic .NET references
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        
        return new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")),
        };
    }

    private static double ComputeConfidence(ComplexityAnalysis.Core.Complexity.ComplexityExpression complexity)
    {
        // Higher confidence for simpler expressions
        return complexity switch
        {
            ComplexityAnalysis.Core.Complexity.ConstantComplexity => 1.0,
            ComplexityAnalysis.Core.Complexity.VariableComplexity => 0.95,
            ComplexityAnalysis.Core.Complexity.LinearComplexity => 0.9,
            ComplexityAnalysis.Core.Complexity.PolynomialComplexity => 0.85,
            ComplexityAnalysis.Core.Complexity.LogarithmicComplexity => 0.9,
            ComplexityAnalysis.Core.Complexity.PolyLogComplexity => 0.85,
            ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity => 0.7,
            _ => 0.6
        };
    }

    private static bool RequiresReview(ComplexityAnalysis.Core.Complexity.ComplexityExpression complexity)
    {
        return complexity switch
        {
            ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity => true,
            ComplexityAnalysis.Core.Complexity.ExponentialComplexity => true,
            ComplexityAnalysis.Core.Complexity.FactorialComplexity => true,
            ComplexityAnalysis.Core.Complexity.BinaryOperationComplexity bin => 
                RequiresReview(bin.Left) || RequiresReview(bin.Right),
            _ => false
        };
    }

    private static string? GetReviewReason(ComplexityAnalysis.Core.Complexity.ComplexityExpression complexity)
    {
        return complexity switch
        {
            ComplexityAnalysis.Core.Recurrence.RecurrenceComplexity => "Contains recurrence relation",
            ComplexityAnalysis.Core.Complexity.ExponentialComplexity => "Exponential complexity detected",
            ComplexityAnalysis.Core.Complexity.FactorialComplexity => "Factorial complexity detected",
            _ => null
        };
    }

    private static void OutputResult(AnalysisOutput output, bool asJson)
    {
        if (asJson)
        {
            var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine(json);
        }
        else
        {
            if (!output.Success)
            {
                Console.Error.WriteLine($"Error: {output.Error}");
                return;
            }

            Console.WriteLine($"Analyzed: {output.Document}");
            Console.WriteLine($"Methods found: {output.Methods.Count}");
            Console.WriteLine();

            foreach (var method in output.Methods)
            {
                var review = method.RequiresReview ? " [REVIEW]" : "";
                Console.WriteLine($"  {method.MethodName} (line {method.Line}): {method.TimeComplexity}{review}");
                Console.WriteLine($"    Confidence: {method.Confidence:P0}");
                if (method.ReviewReason != null)
                {
                    Console.WriteLine($"    Review reason: {method.ReviewReason}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Analysis completed in {output.ElapsedMs}ms");
        }
    }
}
