using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComplexityAnalysis.Calibration;

/// <summary>
/// Persists and loads calibration data to/from disk.
/// Supports JSON format with optional compression.
/// </summary>
public sealed class CalibrationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _basePath;

    /// <summary>
    /// Creates a calibration store with the specified base path.
    /// </summary>
    /// <param name="basePath">Directory to store calibration data. Defaults to user's local app data.</param>
    public CalibrationStore(string? basePath = null)
    {
        _basePath = basePath ?? GetDefaultPath();
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Saves calibration data for the current machine.
    /// </summary>
    public async Task SaveAsync(CalibrationData data, CancellationToken cancellationToken = default)
    {
        var fileName = GetFileName(data.HardwareProfile.ProfileId);
        var filePath = Path.Combine(_basePath, fileName);

        var json = JsonSerializer.Serialize(data, JsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);

        // Also save as "latest" for quick access
        var latestPath = Path.Combine(_basePath, "latest.json");
        await File.WriteAllTextAsync(latestPath, json, cancellationToken);
    }

    /// <summary>
    /// Loads the most recent calibration data.
    /// </summary>
    public async Task<CalibrationData?> LoadLatestAsync(CancellationToken cancellationToken = default)
    {
        var latestPath = Path.Combine(_basePath, "latest.json");

        if (!File.Exists(latestPath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(latestPath, cancellationToken);
        return JsonSerializer.Deserialize<CalibrationData>(json, JsonOptions);
    }

    /// <summary>
    /// Loads calibration data for a specific hardware profile.
    /// </summary>
    public async Task<CalibrationData?> LoadAsync(string profileId, CancellationToken cancellationToken = default)
    {
        var fileName = GetFileName(profileId);
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return JsonSerializer.Deserialize<CalibrationData>(json, JsonOptions);
    }

    /// <summary>
    /// Lists all available calibration profiles.
    /// </summary>
    public IEnumerable<string> ListProfiles()
    {
        if (!Directory.Exists(_basePath))
        {
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(_basePath, "calibration-*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith("calibration-"))
            {
                yield return fileName["calibration-".Length..];
            }
        }
    }

    /// <summary>
    /// Checks if calibration data exists and is recent enough.
    /// </summary>
    public async Task<bool> IsCalibrationValidAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var data = await LoadLatestAsync(cancellationToken);

        if (data == null)
        {
            return false;
        }

        return DateTime.UtcNow - data.CompletedAt < maxAge;
    }

    /// <summary>
    /// Deletes calibration data for a profile.
    /// </summary>
    public void Delete(string profileId)
    {
        var fileName = GetFileName(profileId);
        var filePath = Path.Combine(_basePath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// Exports calibration data as a summary report.
    /// </summary>
    public string GenerateReport(CalibrationData data)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("# BCL Complexity Calibration Report");
        sb.AppendLine();
        sb.AppendLine("## Hardware Profile");
        sb.AppendLine($"- **Machine:** {data.HardwareProfile.MachineName}");
        sb.AppendLine($"- **OS:** {data.HardwareProfile.OSDescription}");
        sb.AppendLine($"- **Runtime:** {data.HardwareProfile.RuntimeVersion}");
        sb.AppendLine($"- **Processors:** {data.HardwareProfile.ProcessorCount}");
        sb.AppendLine($"- **Memory:** {data.HardwareProfile.PhysicalMemoryBytes / (1024 * 1024 * 1024.0):F1} GB");
        sb.AppendLine();
        sb.AppendLine("## Calibration Summary");
        sb.AppendLine($"- **Started:** {data.StartedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"- **Duration:** {data.Duration.TotalSeconds:F1} seconds");
        sb.AppendLine($"- **Successful:** {data.SuccessfulCalibrations}");
        sb.AppendLine($"- **Failed:** {data.FailedCalibrations}");
        sb.AppendLine();
        sb.AppendLine("## Method Calibrations");
        sb.AppendLine();
        sb.AppendLine("| Method | Complexity | Constant (ns) | R² | Status |");
        sb.AppendLine("|--------|------------|---------------|-----|--------|");

        foreach (var (key, result) in data.MethodCalibrations.OrderBy(x => x.Key))
        {
            var status = result.Success ? "✓" : "✗";
            var constant = result.ConstantFactorNs > 0 ? $"{result.ConstantFactorNs:F2}" : "-";
            var rSquared = result.RSquared > 0 ? $"{result.RSquared:F3}" : "-";

            sb.AppendLine($"| {key} | {result.Complexity} | {constant} | {rSquared} | {status} |");
        }

        if (data.FailedCalibrations > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Failures");
            sb.AppendLine();

            foreach (var (key, result) in data.MethodCalibrations.Where(x => !x.Value.Success))
            {
                sb.AppendLine($"- **{key}**: {result.ErrorMessage}");
            }
        }

        return sb.ToString();
    }

    private static string GetDefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "ComplexityAnalysis", "Calibration");
    }

    private static string GetFileName(string profileId)
    {
        // Sanitize profile ID for file name
        var sanitized = string.Join("_", profileId.Split(Path.GetInvalidFileNameChars()));
        return $"calibration-{sanitized}.json";
    }
}

/// <summary>
/// Provides lookup of calibrated constant factors for BCL methods.
/// </summary>
public sealed class CalibratedComplexityLookup
{
    private readonly CalibrationData? _calibrationData;
    private readonly Dictionary<string, double> _constantFactors = new();

    public CalibratedComplexityLookup(CalibrationData? calibrationData)
    {
        _calibrationData = calibrationData;

        if (calibrationData != null)
        {
            foreach (var (key, result) in calibrationData.MethodCalibrations)
            {
                if (result.Success && result.ConstantFactorNs > 0)
                {
                    _constantFactors[NormalizeKey(key)] = result.ConstantFactorNs;
                }
            }
        }
    }

    /// <summary>
    /// Whether calibration data is available.
    /// </summary>
    public bool HasCalibration => _calibrationData != null;

    /// <summary>
    /// Gets the calibrated constant factor for a method, if available.
    /// </summary>
    public double? GetConstantFactor(string typeName, string methodName)
    {
        var key = NormalizeKey($"{typeName}.{methodName}");
        return _constantFactors.TryGetValue(key, out var factor) ? factor : null;
    }

    /// <summary>
    /// Gets the estimated time for an operation with given complexity and input size.
    /// </summary>
    /// <param name="typeName">Type containing the method.</param>
    /// <param name="methodName">Method name.</param>
    /// <param name="complexity">Complexity class (e.g., "O(n)", "O(log n)").</param>
    /// <param name="inputSize">Input size N.</param>
    /// <returns>Estimated time in nanoseconds, or null if not calibrated.</returns>
    public double? EstimateTime(string typeName, string methodName, string complexity, int inputSize)
    {
        var constant = GetConstantFactor(typeName, methodName);

        if (constant == null)
        {
            return null;
        }

        var scalingFactor = GetScalingFactor(complexity, inputSize);
        return constant.Value * scalingFactor;
    }

    /// <summary>
    /// Gets all calibrated methods.
    /// </summary>
    public IEnumerable<(string TypeName, string MethodName, double ConstantFactorNs)> GetAllCalibrations()
    {
        if (_calibrationData == null)
        {
            yield break;
        }

        foreach (var (key, result) in _calibrationData.MethodCalibrations)
        {
            if (result.Success)
            {
                yield return (result.TypeName, result.MethodName, result.ConstantFactorNs);
            }
        }
    }

    private static string NormalizeKey(string key)
    {
        return key.ToLowerInvariant()
            .Replace("<", "")
            .Replace(">", "")
            .Replace(",", "")
            .Replace(" ", "");
    }

    private static double GetScalingFactor(string complexity, int n)
    {
        var normalized = complexity.ToLowerInvariant()
            .Replace(" ", "")
            .Replace("o(", "")
            .Replace(")", "");

        return normalized switch
        {
            "1" => 1.0,
            "logn" or "log(n)" => Math.Log(n),
            "n" => n,
            "nlogn" or "nlog(n)" or "n*logn" => n * Math.Log(n),
            "n^2" or "n²" or "n*n" => (double)n * n,
            "n^3" or "n³" => Math.Pow(n, 3),
            "2^n" => Math.Pow(2, n),
            _ => n // Default to linear
        };
    }
}
