using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ComplexityAnalysis.Core.Complexity;

namespace ComplexityAnalysis.Solver;

/// <summary>
/// Solves recurrence relations using SymPy via a Python subprocess.
/// Uses 'uv run' for zero-config isolated execution.
/// </summary>
public sealed class SymPyRecurrenceSolver
{
    private readonly string _scriptPath;
    private readonly TimeSpan _timeout;

    public SymPyRecurrenceSolver(string? scriptPath = null, TimeSpan? timeout = null)
    {
        _scriptPath = scriptPath ?? FindScriptPath();
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Solves a linear recurrence: T(n) = sum(coeffs[i] * T(n-1-i)) + f(n)
    /// </summary>
    public async Task<RecurrenceSolution> SolveLinearAsync(
        double[] coefficients,
        Dictionary<int, double> baseCases,
        string? fOfN = null,
        CancellationToken ct = default)
    {
        var request = new SolverRequest
        {
            Type = "linear",
            Coefficients = coefficients,
            BaseCases = baseCases,
            FOfN = fOfN ?? "0"
        };

        return await ExecuteAsync(request, ct);
    }

    /// <summary>
    /// Solves a divide-and-conquer recurrence: T(n) = a*T(n/b) + f(n)
    /// </summary>
    public async Task<RecurrenceSolution> SolveDivideAndConquerAsync(
        double a,
        double b,
        string fOfN,
        CancellationToken ct = default)
    {
        var request = new SolverRequest
        {
            Type = "divide_conquer",
            A = a,
            B = b,
            FOfN = fOfN
        };

        return await ExecuteAsync(request, ct);
    }

    /// <summary>
    /// Verifies that a proposed solution satisfies a recurrence.
    /// </summary>
    public async Task<RecurrenceSolution> VerifyAsync(
        string recurrence,
        string solution,
        Dictionary<int, double> baseCases,
        CancellationToken ct = default)
    {
        var request = new SolverRequest
        {
            Type = "verify",
            Recurrence = recurrence,
            Solution = solution,
            BaseCases = baseCases
        };

        return await ExecuteAsync(request, ct);
    }

    /// <summary>
    /// Compares asymptotic growth of two expressions using limits.
    /// Uses L'Hôpital's rule via SymPy for proper handling of indeterminate forms.
    /// </summary>
    /// <param name="f">First expression (e.g., "n**2")</param>
    /// <param name="g">Second expression (e.g., "n * log(n)")</param>
    /// <param name="boundType">Type of bound to verify: "O", "Omega", or "Theta"</param>
    public async Task<AsymptoticComparisonResult> CompareAsync(
        string f,
        string g,
        string boundType = "Theta",
        CancellationToken ct = default)
    {
        var request = new SolverRequest
        {
            Type = "compare",
            F = f,
            G = g,
            BoundType = boundType
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);

        try
        {
            var result = await RunPythonAsync(json, ct);
            var response = JsonSerializer.Deserialize<CompareResponse>(result, JsonOptions);

            if (response is null || !response.Success)
            {
                return new AsymptoticComparisonResult
                {
                    Success = false,
                    ErrorMessage = response?.Error ?? "Failed to parse response"
                };
            }

            return new AsymptoticComparisonResult
            {
                Success = true,
                BoundType = response.BoundType,
                Holds = response.Holds,
                Constant = response.Constant,
                Constants = response.Constants,
                Comparison = response.Comparison,
                LimitRatio = response.LimitRatio
            };
        }
        catch (Exception ex)
        {
            return new AsymptoticComparisonResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<RecurrenceSolution> ExecuteAsync(SolverRequest request, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);

        try
        {
            var result = await RunPythonAsync(json, ct);
            var response = JsonSerializer.Deserialize<SolverResponse>(result, JsonOptions);

            if (response is null)
            {
                return RecurrenceSolution.Failure("Failed to parse Python response");
            }

            if (!response.Success)
            {
                return RecurrenceSolution.Failure(response.Error ?? "Unknown error");
            }

            return new RecurrenceSolution
            {
                Success = true,
                ClosedForm = response.ClosedForm,
                Complexity = ParseComplexity(response.Complexity),
                ComplexityString = response.Complexity,
                Latex = response.Latex,
                Verified = response.Verified,
                MasterTheoremCase = response.MasterTheoremCase,
                CriticalExponent = response.CriticalExponent
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return RecurrenceSolution.Failure($"Python execution failed: {ex.Message}");
        }
    }

    private async Task<string> RunPythonAsync(string inputJson, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "uv",
            Arguments = $"run --script \"{_scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        // Write input
        await process.StandardInput.WriteAsync(inputJson);
        process.StandardInput.Close();

        // Read output with timeout
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_timeout);

        var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
        var errorTask = process.StandardError.ReadToEndAsync(cts.Token);

        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"Python solver timed out after {_timeout.TotalSeconds}s");
        }

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Python solver failed (exit {process.ExitCode}): {error}");
        }

        return output;
    }

    private static ComplexityExpression? ParseComplexity(string? complexityStr)
    {
        if (string.IsNullOrWhiteSpace(complexityStr))
            return null;

        // Parse common patterns: O(1), O(n), O(n^2), O(n*log(n)), O(2^n), O(phi^n)
        var str = complexityStr.Trim();

        if (str.StartsWith("O(") && str.EndsWith(")"))
        {
            str = str[2..^1]; // Remove O( and )
        }

        // Constant
        if (str == "1")
            return ConstantComplexity.One;

        // Linear
        if (str == "n")
            return new VariableComplexity(Variable.N);

        // Polynomial: n^k
        if (str.StartsWith("n^"))
        {
            if (double.TryParse(str[2..], out var degree))
                return PolynomialComplexity.OfDegree((int)degree, Variable.N);
        }

        // n*log(n)
        if (str.Contains("log"))
        {
            if (str == "n*log(n)" || str == "n * log(n)")
                return PolyLogComplexity.NLogN();
        }

        // Exponential: 2^n, phi^n
        if (str.EndsWith("^n"))
        {
            var baseStr = str[..^2];
            if (baseStr == "2")
                return new ExponentialComplexity(2.0, Variable.N);
            if (baseStr == "phi" || baseStr.Contains("sqrt(5)"))
                return new ExponentialComplexity((1 + Math.Sqrt(5)) / 2, Variable.N);
        }

        // Couldn't parse - return null
        return null;
    }

    private static string FindScriptPath()
    {
        // Look for the script relative to common locations
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "tools", "recurrence_solver.py"),
            Path.Combine(AppContext.BaseDirectory, "tools", "recurrence_solver.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "tools", "recurrence_solver.py"),
            // For tests running from project directory
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tools", "recurrence_solver.py")),
        };

        foreach (var candidate in candidates)
        {
            var fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
                return fullPath;
        }

        throw new FileNotFoundException(
            "Could not find recurrence_solver.py. Searched: " + string.Join(", ", candidates));
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private sealed class SolverRequest
    {
        public string Type { get; init; } = "linear";

        [JsonPropertyName("coeffs")]
        public double[]? Coefficients { get; init; }

        [JsonPropertyName("base")]
        public Dictionary<int, double>? BaseCases { get; init; }

        [JsonPropertyName("f_n")]
        public string? FOfN { get; init; }

        public double? A { get; init; }
        public double? B { get; init; }

        public string? Recurrence { get; init; }
        public string? Solution { get; init; }

        // For compare
        public string? F { get; init; }
        public string? G { get; init; }

        [JsonPropertyName("bound_type")]
        public string? BoundType { get; init; }
    }

    private sealed class SolverResponse
    {
        public bool Success { get; init; }

        [JsonPropertyName("closed_form")]
        public string? ClosedForm { get; init; }

        public string? Complexity { get; init; }
        public string? Latex { get; init; }
        public bool Verified { get; init; }
        public string? Error { get; init; }

        [JsonPropertyName("master_theorem_case")]
        public int? MasterTheoremCase { get; init; }

        [JsonPropertyName("critical_exponent")]
        public string? CriticalExponent { get; init; }
    }

    private sealed class CompareResponse
    {
        public bool Success { get; init; }

        [JsonPropertyName("bound_type")]
        public string? BoundType { get; init; }

        public bool Holds { get; init; }
        public double? Constant { get; init; }
        public double[]? Constants { get; init; }
        public string? Comparison { get; init; }

        [JsonPropertyName("limit_ratio")]
        public string? LimitRatio { get; init; }

        public string? Error { get; init; }
    }
}

/// <summary>
/// Result of asymptotic comparison between two expressions.
/// </summary>
public sealed record AsymptoticComparisonResult
{
    public bool Success { get; init; }

    /// <summary>Type of bound verified: "O", "Omega", or "Theta".</summary>
    public string? BoundType { get; init; }

    /// <summary>Whether the bound holds.</summary>
    public bool Holds { get; init; }

    /// <summary>The constant c for O or Ω bounds.</summary>
    public double? Constant { get; init; }

    /// <summary>The constants (c1, c2) for Θ bounds.</summary>
    public double[]? Constants { get; init; }

    /// <summary>
    /// Comparison result: "f &lt; g" (f = o(g)), "f ~ g" (f = Θ(g)), or "f &gt; g" (f = ω(g)).
    /// </summary>
    public string? Comparison { get; init; }

    /// <summary>The limit of f/g as n → ∞.</summary>
    public string? LimitRatio { get; init; }

    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Result of solving a recurrence relation.
/// </summary>
public sealed record RecurrenceSolution
{
    public bool Success { get; init; }
    public string? ClosedForm { get; init; }
    public ComplexityExpression? Complexity { get; init; }
    public string? ComplexityString { get; init; }
    public string? Latex { get; init; }
    public bool Verified { get; init; }
    public int? MasterTheoremCase { get; init; }
    public string? CriticalExponent { get; init; }
    public string? ErrorMessage { get; init; }

    public static RecurrenceSolution Failure(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
