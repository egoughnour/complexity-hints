using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using ComplexityAnalysis.IDE.Cli.Models;

namespace ComplexityAnalysis.IDE.Cli.Commands;

/// <summary>
/// Command to probe environment for required tools.
/// </summary>
public sealed class ProbeCommand : Command
{
    public ProbeCommand() : base("probe", "Check environment for required tools (dotnet, python, uv)")
    {
        var jsonOption = new Option<bool>(
            aliases: new[] { "--json", "-j" },
            description: "Output probe results as JSON");

        AddOption(jsonOption);

        this.SetHandler(ExecuteAsync, jsonOption);
    }

    private async Task ExecuteAsync(bool outputJson)
    {
        var dotnetInfo = await ProbeToolAsync("dotnet", "--version");
        var pythonInfo = await ProbeToolAsync("python3", "--version");
        var uvInfo = await ProbeToolAsync("uv", "--version");

        // Fallback to 'python' if 'python3' not available
        if (!pythonInfo.Available)
        {
            pythonInfo = await ProbeToolAsync("python", "--version");
        }

        var output = new ProbeOutput
        {
            Dotnet = dotnetInfo,
            Python = pythonInfo,
            Uv = uvInfo
        };

        if (outputJson)
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
            Console.WriteLine("Environment Probe Results:");
            Console.WriteLine();
            PrintToolStatus("dotnet", dotnetInfo);
            PrintToolStatus("python", pythonInfo);
            PrintToolStatus("uv", uvInfo);
        }
    }

    private static async Task<ToolInfo> ProbeToolAsync(string tool, string versionArg)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = versionArg,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ToolInfo { Available = false };
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return new ToolInfo { Available = false };
            }

            // Parse version from output
            var versionLine = string.IsNullOrEmpty(output) ? errorOutput : output;
            var version = ParseVersion(versionLine.Trim());

            // Get tool path
            var pathInfo = await GetToolPathAsync(tool);

            return new ToolInfo
            {
                Available = true,
                Version = version,
                Path = pathInfo
            };
        }
        catch (Exception)
        {
            return new ToolInfo { Available = false };
        }
    }

    private static async Task<string?> GetToolPathAsync(string tool)
    {
        try
        {
            var whichCommand = OperatingSystem.IsWindows() ? "where" : "which";
            var startInfo = new ProcessStartInfo
            {
                FileName = whichCommand,
                Arguments = tool,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return null;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0 ? output.Trim().Split('\n')[0] : null;
        }
        catch
        {
            return null;
        }
    }

    private static string ParseVersion(string output)
    {
        // Handle various version output formats:
        // "8.0.100" (dotnet)
        // "Python 3.11.4" (python)
        // "uv 0.5.1" (uv)
        
        var parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // If it starts with a version-like string, return it
        if (parts.Length > 0 && char.IsDigit(parts[0][0]))
        {
            return parts[0];
        }
        
        // Otherwise, look for the version after the tool name
        if (parts.Length > 1)
        {
            return parts[1];
        }
        
        return output;
    }

    private static void PrintToolStatus(string name, ToolInfo info)
    {
        if (info.Available)
        {
            Console.WriteLine($"  ✓ {name}: {info.Version}");
            if (info.Path != null)
            {
                Console.WriteLine($"    Path: {info.Path}");
            }
        }
        else
        {
            Console.WriteLine($"  ✗ {name}: not found");
        }
    }
}
