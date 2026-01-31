using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using ComplexityAnalysis.IDE.Cli.Commands;

namespace ComplexityAnalysis.IDE.Cli;

/// <summary>
/// CLI entry point for the Complexity Analysis IDE tooling.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Complexity Analysis CLI for IDE integration")
        {
            new AnalyzeCommand(),
            new VersionCommand(),
            new ProbeCommand()
        };

        // Default action shows help
        rootCommand.SetHandler(() =>
        {
            Console.Error.WriteLine("Use 'complexity-cli --help' for usage information.");
        });

        return await rootCommand.InvokeAsync(args);
    }
}
