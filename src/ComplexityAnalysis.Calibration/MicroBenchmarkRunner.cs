using System.Diagnostics;

namespace ComplexityAnalysis.Calibration;

/// <summary>
/// Runs micro-benchmarks to measure operation timing with high precision.
/// Uses careful warmup, statistical analysis, and outlier removal.
/// </summary>
public sealed class MicroBenchmarkRunner
{
    private readonly BenchmarkOptions _options;

    public MicroBenchmarkRunner(BenchmarkOptions? options = null)
    {
        _options = options ?? BenchmarkOptions.Standard;
    }

    /// <summary>
    /// Runs a benchmark on an action that operates on input of size N.
    /// </summary>
    /// <param name="setup">Setup function that returns data for input size N.</param>
    /// <param name="action">The action to benchmark, receives setup data.</param>
    /// <typeparam name="T">Type of setup data.</typeparam>
    /// <returns>List of benchmark results for each input size.</returns>
    public List<BenchmarkResult> Run<T>(Func<int, T> setup, Action<T> action)
    {
        var results = new List<BenchmarkResult>();

        foreach (var inputSize in _options.InputSizes)
        {
            var result = MeasureAtSize(setup, action, inputSize);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Runs a benchmark at a specific input size.
    /// </summary>
    public BenchmarkResult MeasureAtSize<T>(Func<int, T> setup, Action<T> action, int inputSize)
    {
        // Setup data
        var data = setup(inputSize);

        // Warmup phase - JIT compilation and CPU cache warming
        for (int i = 0; i < _options.WarmupIterations; i++)
        {
            action(data);
        }

        // Determine number of operations per iteration
        var opsPerIteration = CalibrateOperationsPerIteration(action, data);

        // Measurement phase
        var measurements = new List<double>(_options.MeasurementIterations);
        long totalAllocated = 0;

        for (int i = 0; i < _options.MeasurementIterations; i++)
        {
            if (_options.ForceGC)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            long allocBefore = _options.TrackAllocations ? GC.GetAllocatedBytesForCurrentThread() : 0;

            var elapsed = MeasureOperations(action, data, opsPerIteration);

            if (_options.TrackAllocations)
            {
                totalAllocated += GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            }

            // Convert to nanoseconds per operation
            var nsPerOp = elapsed.TotalNanoseconds / opsPerIteration;
            measurements.Add(nsPerOp);
        }

        // Remove outliers using IQR method
        measurements = RemoveOutliers(measurements);

        if (measurements.Count == 0)
        {
            measurements = [0];
        }

        // Compute statistics
        var mean = measurements.Average();
        var stdDev = ComputeStdDev(measurements, mean);
        var min = measurements.Min();
        var max = measurements.Max();
        var allocPerOp = _options.TrackAllocations
            ? totalAllocated / (_options.MeasurementIterations * opsPerIteration)
            : (long?)null;

        return new BenchmarkResult
        {
            InputSize = inputSize,
            MeanNanoseconds = mean,
            StdDevNanoseconds = stdDev,
            MinNanoseconds = min,
            MaxNanoseconds = max,
            Iterations = measurements.Count,
            AllocatedBytes = allocPerOp
        };
    }

    /// <summary>
    /// Calibrates the number of operations to run per iteration to meet timing requirements.
    /// </summary>
    private int CalibrateOperationsPerIteration<T>(Action<T> action, T data)
    {
        int ops = 1;
        var sw = Stopwatch.StartNew();

        // Start with a single operation to estimate time
        action(data);
        sw.Stop();

        var singleOpMs = sw.Elapsed.TotalMilliseconds;

        if (singleOpMs >= _options.MinIterationTimeMs)
        {
            return 1;
        }

        // Calculate ops needed for minimum time
        ops = Math.Max(1, (int)(_options.MinIterationTimeMs / Math.Max(singleOpMs, 0.001)));
        ops = Math.Min(ops, 10_000_000); // Cap at 10M ops

        // Verify timing
        sw.Restart();
        for (int i = 0; i < Math.Min(ops, 1000); i++)
        {
            action(data);
        }
        sw.Stop();

        var verifyMs = sw.Elapsed.TotalMilliseconds;
        var adjustedOps = (int)(ops * (_options.MinIterationTimeMs / Math.Max(verifyMs, 0.001)));

        return Math.Max(1, Math.Min(adjustedOps, 10_000_000));
    }

    /// <summary>
    /// Measures the time to execute a specific number of operations.
    /// </summary>
    private static TimeSpan MeasureOperations<T>(Action<T> action, T data, int operations)
    {
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < operations; i++)
        {
            action(data);
        }

        sw.Stop();
        return sw.Elapsed;
    }

    /// <summary>
    /// Removes outliers using the IQR method.
    /// </summary>
    private static List<double> RemoveOutliers(List<double> data)
    {
        if (data.Count < 4)
        {
            return data;
        }

        var sorted = data.OrderBy(x => x).ToList();
        var q1Index = sorted.Count / 4;
        var q3Index = sorted.Count * 3 / 4;

        var q1 = sorted[q1Index];
        var q3 = sorted[q3Index];
        var iqr = q3 - q1;

        var lowerBound = q1 - 1.5 * iqr;
        var upperBound = q3 + 1.5 * iqr;

        return data.Where(x => x >= lowerBound && x <= upperBound).ToList();
    }

    /// <summary>
    /// Computes standard deviation.
    /// </summary>
    private static double ComputeStdDev(List<double> values, double mean)
    {
        if (values.Count < 2)
        {
            return 0;
        }

        var sumSquares = values.Sum(v => (v - mean) * (v - mean));
        return Math.Sqrt(sumSquares / (values.Count - 1));
    }
}
