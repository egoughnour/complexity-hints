using System.CommandLine;
using System.Reflection;
using System.Text.Json;
using ComplexityAnalysis.IDE.Cli.Models;

namespace ComplexityAnalysis.IDE.Cli.Commands;

/// <summary>
/// Command to display version information.
/// </summary>
public sealed class VersionCommand : Command
{
    public VersionCommand() : base("version", "Display version information")
    {
        var jsonOption = new Option<bool>(
            aliases: new[] { "--json", "-j" },
            description: "Output version as JSON");

        AddOption(jsonOption);

        this.SetHandler(Execute, jsonOption);
    }

    private void Execute(bool outputJson)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        
        // Get Roslyn version from loaded assembly
        var roslynAssembly = typeof(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree).Assembly;
        var roslynVersion = roslynAssembly.GetName().Version?.ToString() ?? "unknown";

        var runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

        if (outputJson)
        {
            var output = new VersionOutput
            {
                Version = version,
                RoslynVersion = roslynVersion,
                Runtime = runtime
            };

            var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            Console.WriteLine(json);
        }
        else
        {
            Console.WriteLine($"complexity-cli v{version}");
            Console.WriteLine($"Roslyn: {roslynVersion}");
            Console.WriteLine($"Runtime: {runtime}");
        }
    }
}
