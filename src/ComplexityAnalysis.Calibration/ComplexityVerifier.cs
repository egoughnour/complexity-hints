namespace ComplexityAnalysis.Calibration;

/// <summary>
/// Verifies claimed complexity classes against runtime measurements.
/// Uses curve fitting to determine the best-fit complexity class.
/// </summary>
public sealed class ComplexityVerifier
{
    private readonly MicroBenchmarkRunner _benchmarkRunner;

    public ComplexityVerifier(BenchmarkOptions? options = null)
    {
        _benchmarkRunner = new MicroBenchmarkRunner(options);
    }

    /// <summary>
    /// Verifies that an operation matches its claimed complexity.
    /// </summary>
    /// <typeparam name="T">Type of setup data.</typeparam>
    /// <param name="claimedComplexity">The claimed complexity (e.g., "O(n)", "O(log n)").</param>
    /// <param name="setup">Setup function that creates data for input size N.</param>
    /// <param name="action">The action to verify.</param>
    /// <returns>Verification result with measured complexity and constant factor.</returns>
    public ComplexityVerificationResult Verify<T>(
        string claimedComplexity,
        Func<int, T> setup,
        Action<T> action)
    {
        var benchmarkResults = _benchmarkRunner.Run(setup, action);
        return AnalyzeResults(claimedComplexity, benchmarkResults);
    }

    /// <summary>
    /// Analyzes benchmark results to determine complexity class.
    /// </summary>
    public ComplexityVerificationResult AnalyzeResults(
        string claimedComplexity,
        IReadOnlyList<BenchmarkResult> results)
    {
        if (results.Count < 2)
        {
            return new ComplexityVerificationResult
            {
                ClaimedComplexity = claimedComplexity,
                MeasuredComplexity = "Unknown",
                Verified = false,
                BenchmarkResults = results.ToList(),
                Notes = "Insufficient data points for complexity analysis"
            };
        }

        // Try fitting each complexity class
        var fits = new Dictionary<string, (double RSquared, double Constant)>
        {
            ["O(1)"] = FitConstant(results),
            ["O(log n)"] = FitLogarithmic(results),
            ["O(n)"] = FitLinear(results),
            ["O(n log n)"] = FitLinearithmic(results),
            ["O(n²)"] = FitQuadratic(results),
            ["O(n³)"] = FitCubic(results),
            ["O(2^n)"] = FitExponential(results)
        };

        // Find best fit
        var bestFit = fits
            .Where(f => !double.IsNaN(f.Value.RSquared) && f.Value.RSquared >= 0)
            .OrderByDescending(f => f.Value.RSquared)
            .FirstOrDefault();

        var measuredComplexity = bestFit.Key ?? "Unknown";
        var rSquared = bestFit.Value.RSquared;
        var constantFactor = bestFit.Value.Constant;

        // Normalize claimed complexity
        var normalizedClaim = NormalizeComplexity(claimedComplexity);
        var verified = normalizedClaim == measuredComplexity && rSquared >= 0.9;

        // Compute confidence based on R² and consistency
        var confidence = ComputeConfidence(rSquared, results);

        string? notes = null;
        if (!verified)
        {
            if (rSquared < 0.9)
            {
                notes = $"Low R² ({rSquared:F3}) - data doesn't fit any complexity class well";
            }
            else if (normalizedClaim != measuredComplexity)
            {
                notes = $"Claimed {claimedComplexity} but measured as {measuredComplexity}";
            }
        }

        return new ComplexityVerificationResult
        {
            ClaimedComplexity = claimedComplexity,
            MeasuredComplexity = measuredComplexity,
            Verified = verified,
            ConstantFactor = constantFactor,
            RSquared = rSquared,
            BenchmarkResults = results.ToList(),
            Confidence = confidence,
            Notes = notes
        };
    }

    /// <summary>
    /// Detects the complexity class from benchmark results without a prior claim.
    /// </summary>
    public string DetectComplexity(IReadOnlyList<BenchmarkResult> results)
    {
        var analysis = AnalyzeResults("Unknown", results);
        return analysis.MeasuredComplexity;
    }

    /// <summary>
    /// Estimates the constant factor for a known complexity class.
    /// </summary>
    public double EstimateConstantFactor(
        string complexityClass,
        IReadOnlyList<BenchmarkResult> results)
    {
        var normalized = NormalizeComplexity(complexityClass);

        return normalized switch
        {
            "O(1)" => FitConstant(results).Constant,
            "O(log n)" => FitLogarithmic(results).Constant,
            "O(n)" => FitLinear(results).Constant,
            "O(n log n)" => FitLinearithmic(results).Constant,
            "O(n²)" => FitQuadratic(results).Constant,
            "O(n³)" => FitCubic(results).Constant,
            "O(2^n)" => FitExponential(results).Constant,
            _ => FitLinear(results).Constant
        };
    }

    #region Curve Fitting Methods

    /// <summary>
    /// Fits data to O(1) - constant time.
    /// </summary>
    private static (double RSquared, double Constant) FitConstant(IReadOnlyList<BenchmarkResult> results)
    {
        var times = results.Select(r => r.MeanNanoseconds).ToList();
        var mean = times.Average();

        // R² is based on how much of variance is explained
        var ss_tot = times.Sum(t => (t - mean) * (t - mean));
        var ss_res = times.Sum(t => (t - mean) * (t - mean)); // residuals from mean

        // For constant, we need to check if times don't vary much with input size
        var maxTime = times.Max();
        var minTime = times.Min();
        var range = maxTime - minTime;
        var relativeRange = mean > 0 ? range / mean : double.MaxValue;

        // If times vary by less than 50%, consider it constant
        var rSquared = relativeRange < 0.5 ? 1.0 - relativeRange : 0.0;

        return (rSquared, mean);
    }

    /// <summary>
    /// Fits data to O(log n) - logarithmic time.
    /// </summary>
    private static (double RSquared, double Constant) FitLogarithmic(IReadOnlyList<BenchmarkResult> results)
    {
        // Transform: time = c * log(n)
        // Linear regression: time vs log(n)
        var logN = results.Select(r => Math.Log(r.InputSize)).ToList();
        var times = results.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(logN, times);
    }

    /// <summary>
    /// Fits data to O(n) - linear time.
    /// </summary>
    private static (double RSquared, double Constant) FitLinear(IReadOnlyList<BenchmarkResult> results)
    {
        var n = results.Select(r => (double)r.InputSize).ToList();
        var times = results.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(n, times);
    }

    /// <summary>
    /// Fits data to O(n log n) - linearithmic time.
    /// </summary>
    private static (double RSquared, double Constant) FitLinearithmic(IReadOnlyList<BenchmarkResult> results)
    {
        var nLogN = results.Select(r => r.InputSize * Math.Log(r.InputSize)).ToList();
        var times = results.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(nLogN, times);
    }

    /// <summary>
    /// Fits data to O(n²) - quadratic time.
    /// </summary>
    private static (double RSquared, double Constant) FitQuadratic(IReadOnlyList<BenchmarkResult> results)
    {
        var nSquared = results.Select(r => (double)r.InputSize * r.InputSize).ToList();
        var times = results.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(nSquared, times);
    }

    /// <summary>
    /// Fits data to O(n³) - cubic time.
    /// </summary>
    private static (double RSquared, double Constant) FitCubic(IReadOnlyList<BenchmarkResult> results)
    {
        var nCubed = results.Select(r => Math.Pow(r.InputSize, 3)).ToList();
        var times = results.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(nCubed, times);
    }

    /// <summary>
    /// Fits data to O(2^n) - exponential time.
    /// </summary>
    private static (double RSquared, double Constant) FitExponential(IReadOnlyList<BenchmarkResult> results)
    {
        // For exponential, we need to be careful about overflow
        // Only use small input sizes
        var validResults = results.Where(r => r.InputSize <= 30).ToList();

        if (validResults.Count < 2)
        {
            return (0, 0);
        }

        var exp = validResults.Select(r => Math.Pow(2, r.InputSize)).ToList();
        var times = validResults.Select(r => r.MeanNanoseconds).ToList();

        return LinearRegression(exp, times);
    }

    /// <summary>
    /// Performs linear regression without intercept: y = c*x
    /// Returns R² and coefficient c.
    /// </summary>
    private static (double RSquared, double Constant) LinearRegression(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y)
    {
        if (x.Count < 2 || x.Count != y.Count)
        {
            return (0, 0);
        }

        // Simple linear regression through origin: c = sum(x*y) / sum(x²)
        var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
        var sumXX = x.Sum(xi => xi * xi);

        if (sumXX < 1e-10)
        {
            return (0, 0);
        }

        var c = sumXY / sumXX;

        // Calculate R²
        var yMean = y.Average();
        var ss_tot = y.Sum(yi => (yi - yMean) * (yi - yMean));
        var ss_res = x.Zip(y, (xi, yi) => {
            var predicted = c * xi;
            return (yi - predicted) * (yi - predicted);
        }).Sum();

        var rSquared = ss_tot > 0 ? 1.0 - (ss_res / ss_tot) : 0.0;

        return (Math.Max(0, rSquared), c);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Normalizes complexity notation to a standard form.
    /// </summary>
    private static string NormalizeComplexity(string complexity)
    {
        var lower = complexity.ToLowerInvariant()
            .Replace(" ", "")
            .Replace("o(", "O(")
            .Replace("θ(", "O(")
            .Replace("Θ(", "O(");

        // Common normalizations
        return lower switch
        {
            "O(1)" or "constant" => "O(1)",
            "O(logn)" or "O(log(n))" or "logarithmic" => "O(log n)",
            "O(n)" or "linear" => "O(n)",
            "O(nlogn)" or "O(nlog(n))" or "O(n*logn)" or "linearithmic" => "O(n log n)",
            "O(n^2)" or "O(n*n)" or "quadratic" => "O(n²)",
            "O(n^3)" or "O(n*n*n)" or "cubic" => "O(n³)",
            "O(2^n)" or "exponential" => "O(2^n)",
            _ => complexity
        };
    }

    /// <summary>
    /// Computes confidence based on R² and measurement stability.
    /// </summary>
    private static double ComputeConfidence(double rSquared, IReadOnlyList<BenchmarkResult> results)
    {
        // Base confidence from R²
        var baseConfidence = Math.Max(0, rSquared);

        // Adjust for measurement stability
        var avgCV = results.Average(r => r.CoefficientOfVariation);
        var stabilityFactor = Math.Max(0, 1.0 - avgCV);

        // Adjust for number of data points
        var dataPointFactor = Math.Min(1.0, results.Count / 5.0);

        return baseConfidence * stabilityFactor * dataPointFactor;
    }

    #endregion
}
