namespace ComplexityAnalysis.Calibration;

/// <summary>
/// Result of a single micro-benchmark measurement.
/// </summary>
public sealed record BenchmarkResult
{
    /// <summary>
    /// The input size N used for this measurement.
    /// </summary>
    public required int InputSize { get; init; }

    /// <summary>
    /// Mean time per operation in nanoseconds.
    /// </summary>
    public required double MeanNanoseconds { get; init; }

    /// <summary>
    /// Standard deviation in nanoseconds.
    /// </summary>
    public required double StdDevNanoseconds { get; init; }

    /// <summary>
    /// Number of iterations performed.
    /// </summary>
    public required int Iterations { get; init; }

    /// <summary>
    /// Minimum time observed in nanoseconds.
    /// </summary>
    public double MinNanoseconds { get; init; }

    /// <summary>
    /// Maximum time observed in nanoseconds.
    /// </summary>
    public double MaxNanoseconds { get; init; }

    /// <summary>
    /// Memory allocated per operation in bytes (if tracked).
    /// </summary>
    public long? AllocatedBytes { get; init; }

    /// <summary>
    /// Coefficient of variation (StdDev / Mean).
    /// Lower values indicate more stable measurements.
    /// </summary>
    public double CoefficientOfVariation =>
        MeanNanoseconds > 0 ? StdDevNanoseconds / MeanNanoseconds : 0;

    /// <summary>
    /// Whether the measurement is considered stable (CV < 0.1).
    /// </summary>
    public bool IsStable => CoefficientOfVariation < 0.1;
}

/// <summary>
/// Result of complexity verification through runtime measurement.
/// </summary>
public sealed record ComplexityVerificationResult
{
    /// <summary>
    /// The claimed complexity class.
    /// </summary>
    public required string ClaimedComplexity { get; init; }

    /// <summary>
    /// The best-fit complexity class from measurement.
    /// </summary>
    public required string MeasuredComplexity { get; init; }

    /// <summary>
    /// Whether the measured complexity matches the claim.
    /// </summary>
    public required bool Verified { get; init; }

    /// <summary>
    /// Estimated constant factor (c in c*f(n)).
    /// </summary>
    public double ConstantFactor { get; init; }

    /// <summary>
    /// R-squared value for the complexity fit (0.0 to 1.0).
    /// Higher values indicate better fit.
    /// </summary>
    public double RSquared { get; init; }

    /// <summary>
    /// Individual benchmark results used for verification.
    /// </summary>
    public required IReadOnlyList<BenchmarkResult> BenchmarkResults { get; init; }

    /// <summary>
    /// Confidence level in the verification (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Any warnings or notes about the verification.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Result of calibrating a specific BCL method.
/// </summary>
public sealed record BCLCalibrationResult
{
    /// <summary>
    /// The fully qualified method name.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    /// The type containing the method.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// The verified/calibrated complexity.
    /// </summary>
    public required string Complexity { get; init; }

    /// <summary>
    /// Estimated constant factor in nanoseconds per base operation.
    /// </summary>
    public double ConstantFactorNs { get; init; }

    /// <summary>
    /// Standard error of the constant factor estimate.
    /// </summary>
    public double ConstantFactorError { get; init; }

    /// <summary>
    /// Whether the calibration was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of data points used for calibration.
    /// </summary>
    public int DataPoints { get; init; }

    /// <summary>
    /// R-squared value for the fit.
    /// </summary>
    public double RSquared { get; init; }

    /// <summary>
    /// Timestamp when calibration was performed.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Hardware profile at time of calibration.
    /// </summary>
    public HardwareProfile? HardwareProfile { get; init; }

    /// <summary>
    /// Any error message if calibration failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Hardware profile for a calibration environment.
/// </summary>
public sealed record HardwareProfile
{
    /// <summary>
    /// Unique identifier for this hardware profile.
    /// </summary>
    public required string ProfileId { get; init; }

    /// <summary>
    /// Machine name.
    /// </summary>
    public string? MachineName { get; init; }

    /// <summary>
    /// Processor description.
    /// </summary>
    public string? ProcessorDescription { get; init; }

    /// <summary>
    /// Number of logical processors.
    /// </summary>
    public int ProcessorCount { get; init; }

    /// <summary>
    /// Physical memory in bytes.
    /// </summary>
    public long PhysicalMemoryBytes { get; init; }

    /// <summary>
    /// Operating system description.
    /// </summary>
    public string? OSDescription { get; init; }

    /// <summary>
    /// .NET runtime version.
    /// </summary>
    public string? RuntimeVersion { get; init; }

    /// <summary>
    /// Whether the process is running in 64-bit mode.
    /// </summary>
    public bool Is64BitProcess { get; init; }

    /// <summary>
    /// Reference benchmark score for normalization across machines.
    /// Higher is faster.
    /// </summary>
    public double ReferenceBenchmarkScore { get; init; }

    /// <summary>
    /// Timestamp when this profile was captured.
    /// </summary>
    public DateTime CapturedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a hardware profile for the current machine.
    /// </summary>
    public static HardwareProfile Current()
    {
        var profileId = $"{Environment.MachineName}-{Environment.ProcessorCount}P-{DateTime.UtcNow:yyyyMMdd}";
        
        return new HardwareProfile
        {
            ProfileId = profileId,
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            PhysicalMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            OSDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            RuntimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Is64BitProcess = Environment.Is64BitProcess,
            CapturedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Complete calibration data for a machine, containing all BCL calibration results.
/// </summary>
public sealed record CalibrationData
{
    /// <summary>
    /// Version of the calibration data format.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Hardware profile for this calibration data.
    /// </summary>
    public required HardwareProfile HardwareProfile { get; init; }

    /// <summary>
    /// All BCL method calibration results.
    /// </summary>
    public required IReadOnlyDictionary<string, BCLCalibrationResult> MethodCalibrations { get; init; }

    /// <summary>
    /// When the calibration was started.
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// When the calibration was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; }

    /// <summary>
    /// Total duration of calibration.
    /// </summary>
    public TimeSpan Duration => CompletedAt - StartedAt;

    /// <summary>
    /// Number of methods successfully calibrated.
    /// </summary>
    public int SuccessfulCalibrations =>
        MethodCalibrations.Values.Count(r => r.Success);

    /// <summary>
    /// Number of methods that failed calibration.
    /// </summary>
    public int FailedCalibrations =>
        MethodCalibrations.Values.Count(r => !r.Success);
}

/// <summary>
/// Options for benchmark configuration.
/// </summary>
public sealed record BenchmarkOptions
{
    /// <summary>
    /// Default benchmark options for quick calibration.
    /// </summary>
    public static BenchmarkOptions Quick => new()
    {
        WarmupIterations = 3,
        MeasurementIterations = 10,
        MinIterationTimeMs = 50,
        MaxIterationTimeMs = 500,
        InputSizes = [100, 1000, 10000]
    };

    /// <summary>
    /// Standard benchmark options for typical calibration.
    /// </summary>
    public static BenchmarkOptions Standard => new()
    {
        WarmupIterations = 5,
        MeasurementIterations = 20,
        MinIterationTimeMs = 100,
        MaxIterationTimeMs = 1000,
        InputSizes = [100, 500, 1000, 5000, 10000, 50000]
    };

    /// <summary>
    /// Thorough benchmark options for high-precision calibration.
    /// </summary>
    public static BenchmarkOptions Thorough => new()
    {
        WarmupIterations = 10,
        MeasurementIterations = 50,
        MinIterationTimeMs = 200,
        MaxIterationTimeMs = 2000,
        InputSizes = [100, 250, 500, 1000, 2500, 5000, 10000, 25000, 50000, 100000]
    };

    /// <summary>
    /// Number of warmup iterations before measurement.
    /// </summary>
    public int WarmupIterations { get; init; } = 5;

    /// <summary>
    /// Number of measurement iterations.
    /// </summary>
    public int MeasurementIterations { get; init; } = 20;

    /// <summary>
    /// Minimum time per iteration in milliseconds.
    /// </summary>
    public int MinIterationTimeMs { get; init; } = 100;

    /// <summary>
    /// Maximum time per iteration in milliseconds.
    /// </summary>
    public int MaxIterationTimeMs { get; init; } = 1000;

    /// <summary>
    /// Input sizes to benchmark at.
    /// </summary>
    public int[] InputSizes { get; init; } = [100, 1000, 10000, 100000];

    /// <summary>
    /// Whether to track memory allocations.
    /// </summary>
    public bool TrackAllocations { get; init; } = true;

    /// <summary>
    /// Whether to force garbage collection between iterations.
    /// </summary>
    public bool ForceGC { get; init; } = true;
}
