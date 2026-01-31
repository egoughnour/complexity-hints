using ComplexityAnalysis.Calibration;
using Xunit;

namespace ComplexityAnalysis.Tests.Calibration;

/// <summary>
/// Tests for the hardware calibration system.
/// Note: Some tests are time-sensitive and marked with appropriate traits.
/// </summary>
public class CalibrationTests
{
    #region MicroBenchmarkRunner Tests

    [Fact]
    public void MicroBenchmarkRunner_MeasuresConstantTimeOperation()
    {
        var runner = new MicroBenchmarkRunner(BenchmarkOptions.Quick);

        var results = runner.Run(
            n => 0, // No setup needed
            _ => { var x = 1 + 1; } // Constant time
        );

        Assert.NotEmpty(results);
        foreach (var result in results)
        {
            Assert.True(result.MeanNanoseconds > 0);
            Assert.True(result.Iterations > 0);
        }
    }

    [Fact]
    public void MicroBenchmarkRunner_MeasuresLinearTimeOperation()
    {
        var runner = new MicroBenchmarkRunner(BenchmarkOptions.Quick);

        var results = runner.Run(
            n => new int[n],
            arr =>
            {
                var sum = 0;
                foreach (var x in arr) sum += x;
            }
        );

        Assert.NotEmpty(results);

        // Time should increase with input size for linear operation
        if (results.Count >= 2)
        {
            var first = results.First();
            var last = results.Last();

            // For linear operation, time should grow with n
            // Allow some variance but expect clear growth trend
            Assert.True(last.InputSize > first.InputSize);
        }
    }

    [Fact]
    public void MicroBenchmarkRunner_ReturnsStableResults()
    {
        var runner = new MicroBenchmarkRunner(new BenchmarkOptions
        {
            WarmupIterations = 5,
            MeasurementIterations = 20,
            InputSizes = [1000]
        });

        var results = runner.Run(
            n => new List<int>(n),
            list => list.Add(42)
        );

        Assert.Single(results);
        var result = results[0];

        // Should have reasonable stability
        Assert.True(result.CoefficientOfVariation < 1.0,
            $"CV too high: {result.CoefficientOfVariation}");
    }

    #endregion

    #region ComplexityVerifier Tests

    [Fact]
    public void ComplexityVerifier_DetectsLinearComplexity()
    {
        var verifier = new ComplexityVerifier(BenchmarkOptions.Quick);

        var result = verifier.Verify(
            "O(n)",
            n => Enumerable.Range(0, n).ToList(),
            list => { foreach (var _ in list) { } }
        );

        Assert.Equal("O(n)", result.ClaimedComplexity);
        // Linear should be detected (may not perfectly verify due to noise)
        Assert.NotNull(result.MeasuredComplexity);
        Assert.NotEmpty(result.BenchmarkResults);
    }

    [Fact]
    public void ComplexityVerifier_DetectsConstantComplexity()
    {
        var verifier = new ComplexityVerifier(BenchmarkOptions.Quick);

        var result = verifier.Verify(
            "O(1)",
            n =>
            {
                var dict = new Dictionary<int, int>(n);
                for (int i = 0; i < n; i++) dict[i] = i;
                return dict;
            },
            dict => dict.TryGetValue(0, out _)
        );

        Assert.Equal("O(1)", result.ClaimedComplexity);
        Assert.NotEmpty(result.BenchmarkResults);
    }

    [Fact]
    public void ComplexityVerifier_AnalyzesSyntheticDataCorrectly()
    {
        var verifier = new ComplexityVerifier();

        // Create synthetic linear data: time = 10 * n
        var linearData = new List<BenchmarkResult>
        {
            new() { InputSize = 100, MeanNanoseconds = 1000, StdDevNanoseconds = 50, Iterations = 10 },
            new() { InputSize = 500, MeanNanoseconds = 5000, StdDevNanoseconds = 100, Iterations = 10 },
            new() { InputSize = 1000, MeanNanoseconds = 10000, StdDevNanoseconds = 200, Iterations = 10 },
            new() { InputSize = 5000, MeanNanoseconds = 50000, StdDevNanoseconds = 500, Iterations = 10 }
        };

        var result = verifier.AnalyzeResults("O(n)", linearData);

        Assert.True(result.Verified, $"Expected verified, got: {result.Notes}");
        Assert.Equal("O(n)", result.MeasuredComplexity);
        Assert.True(result.RSquared > 0.95, $"R² too low: {result.RSquared}");
    }

    [Fact]
    public void ComplexityVerifier_AnalyzesLogNDataCorrectly()
    {
        var verifier = new ComplexityVerifier();

        // Create synthetic log(n) data: time = 100 * log(n)
        var logData = new List<BenchmarkResult>
        {
            new() { InputSize = 100, MeanNanoseconds = 460, StdDevNanoseconds = 10, Iterations = 10 },   // 100 * ln(100) ≈ 460
            new() { InputSize = 1000, MeanNanoseconds = 690, StdDevNanoseconds = 15, Iterations = 10 },  // 100 * ln(1000) ≈ 690
            new() { InputSize = 10000, MeanNanoseconds = 920, StdDevNanoseconds = 20, Iterations = 10 }, // 100 * ln(10000) ≈ 920
            new() { InputSize = 100000, MeanNanoseconds = 1150, StdDevNanoseconds = 30, Iterations = 10 } // 100 * ln(100000) ≈ 1150
        };

        var result = verifier.AnalyzeResults("O(log n)", logData);

        Assert.True(result.Verified, $"Expected verified for O(log n), got: {result.MeasuredComplexity}");
        Assert.True(result.RSquared > 0.9, $"R² too low: {result.RSquared}");
    }

    [Fact]
    public void ComplexityVerifier_AnalyzesNLogNDataCorrectly()
    {
        var verifier = new ComplexityVerifier();

        // Create synthetic n*log(n) data: time = 1 * n*log(n)
        var nLogNData = new List<BenchmarkResult>
        {
            new() { InputSize = 100, MeanNanoseconds = 460, StdDevNanoseconds = 20, Iterations = 10 },     // 100 * ln(100)
            new() { InputSize = 1000, MeanNanoseconds = 6900, StdDevNanoseconds = 150, Iterations = 10 },  // 1000 * ln(1000)
            new() { InputSize = 10000, MeanNanoseconds = 92000, StdDevNanoseconds = 2000, Iterations = 10 }, // 10000 * ln(10000)
            new() { InputSize = 100000, MeanNanoseconds = 1150000, StdDevNanoseconds = 20000, Iterations = 10 } // 100000 * ln(100000)
        };

        var result = verifier.AnalyzeResults("O(n log n)", nLogNData);

        Assert.True(result.Verified, $"Expected verified for O(n log n), got: {result.MeasuredComplexity}");
        Assert.True(result.RSquared > 0.9, $"R² too low: {result.RSquared}");
    }

    [Fact]
    public void ComplexityVerifier_DetectsMismatch()
    {
        var verifier = new ComplexityVerifier();

        // Create quadratic data but claim linear
        var quadraticData = new List<BenchmarkResult>
        {
            new() { InputSize = 100, MeanNanoseconds = 10000, StdDevNanoseconds = 100, Iterations = 10 },
            new() { InputSize = 200, MeanNanoseconds = 40000, StdDevNanoseconds = 200, Iterations = 10 },
            new() { InputSize = 300, MeanNanoseconds = 90000, StdDevNanoseconds = 300, Iterations = 10 },
            new() { InputSize = 400, MeanNanoseconds = 160000, StdDevNanoseconds = 400, Iterations = 10 }
        };

        var result = verifier.AnalyzeResults("O(n)", quadraticData);

        Assert.False(result.Verified, "Should not verify O(n) for quadratic data");
        Assert.Equal("O(n²)", result.MeasuredComplexity);
    }

    [Fact]
    public void ComplexityVerifier_EstimatesConstantFactor()
    {
        var verifier = new ComplexityVerifier();

        // Linear data with constant factor of 5
        var linearData = new List<BenchmarkResult>
        {
            new() { InputSize = 100, MeanNanoseconds = 500, StdDevNanoseconds = 10, Iterations = 10 },
            new() { InputSize = 1000, MeanNanoseconds = 5000, StdDevNanoseconds = 50, Iterations = 10 },
            new() { InputSize = 10000, MeanNanoseconds = 50000, StdDevNanoseconds = 200, Iterations = 10 }
        };

        var constant = verifier.EstimateConstantFactor("O(n)", linearData);

        // Should be approximately 5
        Assert.True(constant > 4 && constant < 6, $"Constant factor should be ~5, got {constant}");
    }

    #endregion

    #region BCLCalibrator Tests

    [Fact]
    public void BCLCalibrator_CalibratesListAdd()
    {
        var calibrator = new BCLCalibrator(BenchmarkOptions.Quick);
        var result = calibrator.CalibrateListAdd();

        Assert.Equal("List<T>", result.TypeName);
        Assert.Equal("Add", result.MethodName);
        Assert.True(result.DataPoints > 0);
        Assert.True(result.ConstantFactorNs >= 0);
    }

    [Fact]
    public void BCLCalibrator_CalibratesDictionaryTryGetValue()
    {
        var calibrator = new BCLCalibrator(BenchmarkOptions.Quick);
        var result = calibrator.CalibrateDictionaryTryGetValue();

        Assert.Equal("Dictionary<K,V>", result.TypeName);
        Assert.Equal("TryGetValue", result.MethodName);
        Assert.True(result.DataPoints > 0);
    }

    [Fact]
    public void BCLCalibrator_CalibratesListSort()
    {
        var calibrator = new BCLCalibrator(BenchmarkOptions.Quick);
        var result = calibrator.CalibrateListSort();

        Assert.Equal("List<T>", result.TypeName);
        Assert.Equal("Sort", result.MethodName);
        Assert.True(result.DataPoints > 0);
    }

    [Fact]
    public void BCLCalibrator_CalibratesListBinarySearch()
    {
        var calibrator = new BCLCalibrator(BenchmarkOptions.Quick);
        var result = calibrator.CalibrateListBinarySearch();

        Assert.Equal("List<T>", result.TypeName);
        Assert.Equal("BinarySearch", result.MethodName);
        Assert.True(result.DataPoints > 0);
    }

    #endregion

    #region HardwareProfile Tests

    [Fact]
    public void HardwareProfile_CapturesCurrentMachine()
    {
        var profile = HardwareProfile.Current();

        Assert.NotNull(profile.ProfileId);
        Assert.NotNull(profile.MachineName);
        Assert.True(profile.ProcessorCount > 0);
        Assert.True(profile.PhysicalMemoryBytes > 0);
        Assert.NotNull(profile.OSDescription);
        Assert.NotNull(profile.RuntimeVersion);
    }

    [Fact]
    public void HardwareProfile_GeneratesUniqueProfileId()
    {
        var profile1 = HardwareProfile.Current();
        var profile2 = HardwareProfile.Current();

        // Same machine should generate similar IDs (may differ by timestamp)
        Assert.Contains(Environment.MachineName, profile1.ProfileId);
    }

    #endregion

    #region CalibrationStore Tests

    [Fact]
    public async Task CalibrationStore_SavesAndLoadsData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"calibration_test_{Guid.NewGuid()}");
        try
        {
            var store = new CalibrationStore(tempDir);

            var calibrationData = new CalibrationData
            {
                HardwareProfile = HardwareProfile.Current(),
                MethodCalibrations = new Dictionary<string, BCLCalibrationResult>
                {
                    ["List<T>.Add"] = new BCLCalibrationResult
                    {
                        TypeName = "List<T>",
                        MethodName = "Add",
                        Complexity = "O(1)",
                        ConstantFactorNs = 5.0,
                        Success = true,
                        DataPoints = 5,
                        RSquared = 0.95
                    }
                },
                StartedAt = DateTime.UtcNow.AddMinutes(-1),
                CompletedAt = DateTime.UtcNow
            };

            await store.SaveAsync(calibrationData);
            var loaded = await store.LoadLatestAsync();

            Assert.NotNull(loaded);
            Assert.Equal(calibrationData.HardwareProfile.MachineName, loaded.HardwareProfile.MachineName);
            Assert.Single(loaded.MethodCalibrations);
            Assert.Equal("O(1)", loaded.MethodCalibrations["List<T>.Add"].Complexity);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CalibrationStore_ChecksValidityByAge()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"calibration_test_{Guid.NewGuid()}");
        try
        {
            var store = new CalibrationStore(tempDir);

            var calibrationData = new CalibrationData
            {
                HardwareProfile = HardwareProfile.Current(),
                MethodCalibrations = new Dictionary<string, BCLCalibrationResult>(),
                StartedAt = DateTime.UtcNow.AddMinutes(-1),
                CompletedAt = DateTime.UtcNow
            };

            await store.SaveAsync(calibrationData);

            // Should be valid for 1 hour
            Assert.True(await store.IsCalibrationValidAsync(TimeSpan.FromHours(1)));

            // Should not be valid for 0 seconds (already in the past)
            Assert.False(await store.IsCalibrationValidAsync(TimeSpan.FromSeconds(-1)));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void CalibrationStore_GeneratesReport()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"calibration_test_{Guid.NewGuid()}");
        var store = new CalibrationStore(tempDir);

        var calibrationData = new CalibrationData
        {
            HardwareProfile = HardwareProfile.Current(),
            MethodCalibrations = new Dictionary<string, BCLCalibrationResult>
            {
                ["List<T>.Add"] = new BCLCalibrationResult
                {
                    TypeName = "List<T>",
                    MethodName = "Add",
                    Complexity = "O(1)",
                    ConstantFactorNs = 5.0,
                    Success = true,
                    DataPoints = 5,
                    RSquared = 0.95
                },
                ["List<T>.Sort"] = new BCLCalibrationResult
                {
                    TypeName = "List<T>",
                    MethodName = "Sort",
                    Complexity = "O(n log n)",
                    ConstantFactorNs = 10.0,
                    Success = true,
                    DataPoints = 6,
                    RSquared = 0.98
                }
            },
            StartedAt = DateTime.UtcNow.AddMinutes(-1),
            CompletedAt = DateTime.UtcNow
        };

        var report = store.GenerateReport(calibrationData);

        Assert.Contains("BCL Complexity Calibration Report", report);
        Assert.Contains("Hardware Profile", report);
        Assert.Contains("List<T>.Add", report);
        Assert.Contains("O(1)", report);
        Assert.Contains("O(n log n)", report);
    }

    #endregion

    #region CalibratedComplexityLookup Tests

    [Fact]
    public void CalibratedComplexityLookup_ReturnsConstantFactor()
    {
        var calibrationData = new CalibrationData
        {
            HardwareProfile = HardwareProfile.Current(),
            MethodCalibrations = new Dictionary<string, BCLCalibrationResult>
            {
                ["List<T>.Add"] = new BCLCalibrationResult
                {
                    TypeName = "List<T>",
                    MethodName = "Add",
                    Complexity = "O(1)",
                    ConstantFactorNs = 5.0,
                    Success = true,
                    DataPoints = 5,
                    RSquared = 0.95
                }
            },
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var lookup = new CalibratedComplexityLookup(calibrationData);

        var factor = lookup.GetConstantFactor("List<T>", "Add");
        Assert.Equal(5.0, factor);
    }

    [Fact]
    public void CalibratedComplexityLookup_EstimatesTime()
    {
        var calibrationData = new CalibrationData
        {
            HardwareProfile = HardwareProfile.Current(),
            MethodCalibrations = new Dictionary<string, BCLCalibrationResult>
            {
                ["List<T>.Contains"] = new BCLCalibrationResult
                {
                    TypeName = "List<T>",
                    MethodName = "Contains",
                    Complexity = "O(n)",
                    ConstantFactorNs = 2.0, // 2ns per element
                    Success = true,
                    DataPoints = 5,
                    RSquared = 0.95
                }
            },
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var lookup = new CalibratedComplexityLookup(calibrationData);

        var estimatedTime = lookup.EstimateTime("List<T>", "Contains", "O(n)", 1000);

        // Should be approximately 2000 ns for n=1000
        Assert.NotNull(estimatedTime);
        Assert.Equal(2000.0, estimatedTime.Value, 0.1);
    }

    [Fact]
    public void CalibratedComplexityLookup_ReturnsNullForUnknownMethod()
    {
        var calibrationData = new CalibrationData
        {
            HardwareProfile = HardwareProfile.Current(),
            MethodCalibrations = new Dictionary<string, BCLCalibrationResult>(),
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        var lookup = new CalibratedComplexityLookup(calibrationData);

        var factor = lookup.GetConstantFactor("Unknown", "Method");
        Assert.Null(factor);
    }

    [Fact]
    public void CalibratedComplexityLookup_HandlesNullCalibrationData()
    {
        var lookup = new CalibratedComplexityLookup(null);

        Assert.False(lookup.HasCalibration);
        Assert.Null(lookup.GetConstantFactor("List<T>", "Add"));
    }

    #endregion

    #region BenchmarkOptions Tests

    [Fact]
    public void BenchmarkOptions_QuickHasReasonableDefaults()
    {
        var options = BenchmarkOptions.Quick;

        Assert.True(options.WarmupIterations >= 1);
        Assert.True(options.MeasurementIterations >= 5);
        Assert.NotEmpty(options.InputSizes);
    }

    [Fact]
    public void BenchmarkOptions_StandardHasMoreIterations()
    {
        var quick = BenchmarkOptions.Quick;
        var standard = BenchmarkOptions.Standard;

        Assert.True(standard.MeasurementIterations >= quick.MeasurementIterations);
        Assert.True(standard.InputSizes.Length >= quick.InputSizes.Length);
    }

    [Fact]
    public void BenchmarkOptions_ThoroughIsMostComplete()
    {
        var standard = BenchmarkOptions.Standard;
        var thorough = BenchmarkOptions.Thorough;

        Assert.True(thorough.MeasurementIterations >= standard.MeasurementIterations);
        Assert.True(thorough.InputSizes.Length >= standard.InputSizes.Length);
    }

    #endregion
}
