using System.Text.RegularExpressions;

namespace ComplexityAnalysis.Calibration;

/// <summary>
/// Calibrates BCL method complexities through runtime verification.
/// Generates benchmark code, runs it, and analyzes results to determine
/// constant factors for complexity expressions.
/// </summary>
public sealed class BCLCalibrator
{
    private readonly ComplexityVerifier _verifier;
    private readonly BenchmarkOptions _options;

    public BCLCalibrator(BenchmarkOptions? options = null)
    {
        _options = options ?? BenchmarkOptions.Standard;
        _verifier = new ComplexityVerifier(_options);
    }

    /// <summary>
    /// Calibrates List&lt;T&gt;.Add method.
    /// Expected: O(1) amortized.
    /// </summary>
    public BCLCalibrationResult CalibrateListAdd()
    {
        return CalibrateMethod(
            "List<T>",
            "Add",
            "O(1)",
            n => new List<int>(n), // Pre-sized to avoid resizing during benchmark
            list => list.Add(42)
        );
    }

    /// <summary>
    /// Calibrates List&lt;T&gt;.Contains method.
    /// Expected: O(n).
    /// </summary>
    public BCLCalibrationResult CalibrateListContains()
    {
        return CalibrateMethod(
            "List<T>",
            "Contains",
            "O(n)",
            n =>
            {
                var list = new List<int>(n);
                for (int i = 0; i < n; i++) list.Add(i);
                return (list, -1); // Search for non-existent element (worst case)
            },
            data => data.list.Contains(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates List&lt;T&gt;.Sort method.
    /// Expected: O(n log n).
    /// </summary>
    public BCLCalibrationResult CalibrateListSort()
    {
        return CalibrateMethod(
            "List<T>",
            "Sort",
            "O(n log n)",
            n =>
            {
                var random = new Random(42);
                var list = new List<int>(n);
                for (int i = 0; i < n; i++) list.Add(random.Next());
                return list;
            },
            list =>
            {
                // Clone and sort to avoid measuring already-sorted list
                var copy = new List<int>(list);
                copy.Sort();
            }
        );
    }

    /// <summary>
    /// Calibrates List&lt;T&gt;.BinarySearch method.
    /// Expected: O(log n).
    /// </summary>
    public BCLCalibrationResult CalibrateListBinarySearch()
    {
        return CalibrateMethod(
            "List<T>",
            "BinarySearch",
            "O(log n)",
            n =>
            {
                var list = new List<int>(n);
                for (int i = 0; i < n; i++) list.Add(i);
                return (list, n / 2); // Search for middle element
            },
            data => data.list.BinarySearch(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates Dictionary&lt;K,V&gt;.Add method.
    /// Expected: O(1) amortized.
    /// </summary>
    public BCLCalibrationResult CalibrateDictionaryAdd()
    {
        var counter = 0;
        return CalibrateMethod(
            "Dictionary<K,V>",
            "Add",
            "O(1)",
            n => new Dictionary<int, int>(n), // Pre-sized
            dict =>
            {
                var key = Interlocked.Increment(ref counter);
                dict.TryAdd(key, key);
            }
        );
    }

    /// <summary>
    /// Calibrates Dictionary&lt;K,V&gt;.TryGetValue method.
    /// Expected: O(1).
    /// </summary>
    public BCLCalibrationResult CalibrateDictionaryTryGetValue()
    {
        return CalibrateMethod(
            "Dictionary<K,V>",
            "TryGetValue",
            "O(1)",
            n =>
            {
                var dict = new Dictionary<int, int>(n);
                for (int i = 0; i < n; i++) dict[i] = i;
                return (dict, n / 2);
            },
            data => data.dict.TryGetValue(data.Item2, out _)
        );
    }

    /// <summary>
    /// Calibrates Dictionary&lt;K,V&gt;.ContainsKey method.
    /// Expected: O(1).
    /// </summary>
    public BCLCalibrationResult CalibrateDictionaryContainsKey()
    {
        return CalibrateMethod(
            "Dictionary<K,V>",
            "ContainsKey",
            "O(1)",
            n =>
            {
                var dict = new Dictionary<int, int>(n);
                for (int i = 0; i < n; i++) dict[i] = i;
                return (dict, n / 2);
            },
            data => data.dict.ContainsKey(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates HashSet&lt;T&gt;.Add method.
    /// Expected: O(1) amortized.
    /// </summary>
    public BCLCalibrationResult CalibrateHashSetAdd()
    {
        var counter = 0;
        return CalibrateMethod(
            "HashSet<T>",
            "Add",
            "O(1)",
            n => new HashSet<int>(n), // Pre-sized
            set =>
            {
                var key = Interlocked.Increment(ref counter);
                set.Add(key);
            }
        );
    }

    /// <summary>
    /// Calibrates HashSet&lt;T&gt;.Contains method.
    /// Expected: O(1).
    /// </summary>
    public BCLCalibrationResult CalibrateHashSetContains()
    {
        return CalibrateMethod(
            "HashSet<T>",
            "Contains",
            "O(1)",
            n =>
            {
                var set = new HashSet<int>(n);
                for (int i = 0; i < n; i++) set.Add(i);
                return (set, n / 2);
            },
            data => data.set.Contains(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates SortedSet&lt;T&gt;.Add method.
    /// Expected: O(log n).
    /// </summary>
    public BCLCalibrationResult CalibrateSortedSetAdd()
    {
        return CalibrateMethod(
            "SortedSet<T>",
            "Add",
            "O(log n)",
            n =>
            {
                var set = new SortedSet<int>();
                for (int i = 0; i < n; i++) set.Add(i * 2); // Even numbers
                return (set, n);
            },
            data =>
            {
                // Add and remove to keep set size constant
                data.set.Add(data.Item2 * 2 + 1); // Odd number
                data.set.Remove(data.Item2 * 2 + 1);
            }
        );
    }

    /// <summary>
    /// Calibrates SortedDictionary&lt;K,V&gt;.Add method.
    /// Expected: O(log n).
    /// </summary>
    public BCLCalibrationResult CalibrateSortedDictionaryAdd()
    {
        var counter = 0;
        return CalibrateMethod(
            "SortedDictionary<K,V>",
            "Add",
            "O(log n)",
            n =>
            {
                var dict = new SortedDictionary<int, int>();
                for (int i = 0; i < n; i++) dict[i * 2] = i; // Even keys
                return (dict, n);
            },
            data =>
            {
                var key = Interlocked.Increment(ref counter);
                data.dict[key * 2 + 1] = key; // Odd key
                data.dict.Remove(key * 2 + 1);
            }
        );
    }

    /// <summary>
    /// Calibrates String.Contains method.
    /// Expected: O(n*m) worst case, often O(n) average.
    /// </summary>
    public BCLCalibrationResult CalibrateStringContains()
    {
        return CalibrateMethod(
            "String",
            "Contains",
            "O(n)",
            n => (new string('a', n), "xyz"), // Search for non-existent pattern
            data => data.Item1.Contains(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates String.IndexOf method.
    /// Expected: O(n*m) worst case.
    /// </summary>
    public BCLCalibrationResult CalibrateStringIndexOf()
    {
        return CalibrateMethod(
            "String",
            "IndexOf",
            "O(n)",
            n => (new string('a', n), "xyz"),
            data => data.Item1.IndexOf(data.Item2)
        );
    }

    /// <summary>
    /// Calibrates Array.Sort method.
    /// Expected: O(n log n).
    /// </summary>
    public BCLCalibrationResult CalibrateArraySort()
    {
        return CalibrateMethod(
            "Array",
            "Sort",
            "O(n log n)",
            n =>
            {
                var random = new Random(42);
                var arr = new int[n];
                for (int i = 0; i < n; i++) arr[i] = random.Next();
                return arr;
            },
            arr =>
            {
                var copy = new int[arr.Length];
                Array.Copy(arr, copy, arr.Length);
                Array.Sort(copy);
            }
        );
    }

    /// <summary>
    /// Calibrates Array.BinarySearch method.
    /// Expected: O(log n).
    /// </summary>
    public BCLCalibrationResult CalibrateArrayBinarySearch()
    {
        return CalibrateMethod(
            "Array",
            "BinarySearch",
            "O(log n)",
            n =>
            {
                var arr = new int[n];
                for (int i = 0; i < n; i++) arr[i] = i;
                return (arr, n / 2);
            },
            data => Array.BinarySearch(data.arr, data.Item2)
        );
    }

    /// <summary>
    /// Calibrates LINQ Enumerable.Where (iteration).
    /// Expected: O(n) for full iteration.
    /// </summary>
    public BCLCalibrationResult CalibrateLinqWhere()
    {
        return CalibrateMethod(
            "Enumerable",
            "Where",
            "O(n)",
            n =>
            {
                var list = new List<int>(n);
                for (int i = 0; i < n; i++) list.Add(i);
                return list;
            },
            list =>
            {
                foreach (var _ in list.Where(x => x % 2 == 0)) { }
            }
        );
    }

    /// <summary>
    /// Calibrates LINQ Enumerable.OrderBy (full sort).
    /// Expected: O(n log n).
    /// </summary>
    public BCLCalibrationResult CalibrateLinqOrderBy()
    {
        return CalibrateMethod(
            "Enumerable",
            "OrderBy",
            "O(n log n)",
            n =>
            {
                var random = new Random(42);
                var list = new List<int>(n);
                for (int i = 0; i < n; i++) list.Add(random.Next());
                return list;
            },
            list =>
            {
                foreach (var _ in list.OrderBy(x => x)) { }
            }
        );
    }

    /// <summary>
    /// Calibrates StringBuilder.Append method.
    /// Expected: O(1) amortized.
    /// </summary>
    public BCLCalibrationResult CalibrateStringBuilderAppend()
    {
        return CalibrateMethod(
            "StringBuilder",
            "Append",
            "O(1)",
            n => new System.Text.StringBuilder(n * 10), // Pre-sized
            sb => sb.Append('x')
        );
    }

    /// <summary>
    /// Calibrates Regex.IsMatch for simple pattern.
    /// Expected: O(n) for simple patterns.
    /// </summary>
    public BCLCalibrationResult CalibrateRegexIsMatch()
    {
        var regex = new Regex(@"\d+", RegexOptions.Compiled);
        return CalibrateMethod(
            "Regex",
            "IsMatch",
            "O(n)",
            n => new string('a', n) + "123",
            str => regex.IsMatch(str)
        );
    }

    /// <summary>
    /// Runs all standard BCL calibrations.
    /// </summary>
    /// <param name="progress">Optional progress callback.</param>
    /// <returns>Complete calibration data.</returns>
    public CalibrationData RunFullCalibration(IProgress<(string Method, int Progress, int Total)>? progress = null)
    {
        var startedAt = DateTime.UtcNow;
        var hardwareProfile = HardwareProfile.Current();

        var calibrations = new List<(string Key, Func<BCLCalibrationResult> Calibrate)>
        {
            ("List<T>.Add", CalibrateListAdd),
            ("List<T>.Contains", CalibrateListContains),
            ("List<T>.Sort", CalibrateListSort),
            ("List<T>.BinarySearch", CalibrateListBinarySearch),
            ("Dictionary<K,V>.Add", CalibrateDictionaryAdd),
            ("Dictionary<K,V>.TryGetValue", CalibrateDictionaryTryGetValue),
            ("Dictionary<K,V>.ContainsKey", CalibrateDictionaryContainsKey),
            ("HashSet<T>.Add", CalibrateHashSetAdd),
            ("HashSet<T>.Contains", CalibrateHashSetContains),
            ("SortedSet<T>.Add", CalibrateSortedSetAdd),
            ("SortedDictionary<K,V>.Add", CalibrateSortedDictionaryAdd),
            ("String.Contains", CalibrateStringContains),
            ("String.IndexOf", CalibrateStringIndexOf),
            ("Array.Sort", CalibrateArraySort),
            ("Array.BinarySearch", CalibrateArrayBinarySearch),
            ("Enumerable.Where", CalibrateLinqWhere),
            ("Enumerable.OrderBy", CalibrateLinqOrderBy),
            ("StringBuilder.Append", CalibrateStringBuilderAppend),
            ("Regex.IsMatch", CalibrateRegexIsMatch)
        };

        var results = new Dictionary<string, BCLCalibrationResult>();
        var total = calibrations.Count;
        var current = 0;

        foreach (var (key, calibrate) in calibrations)
        {
            current++;
            progress?.Report((key, current, total));

            try
            {
                var result = calibrate();
                results[key] = result with { HardwareProfile = hardwareProfile };
            }
            catch (Exception ex)
            {
                results[key] = new BCLCalibrationResult
                {
                    MethodName = key.Split('.').Last(),
                    TypeName = key.Split('.').First(),
                    Complexity = "Unknown",
                    Success = false,
                    ErrorMessage = ex.Message,
                    HardwareProfile = hardwareProfile
                };
            }
        }

        return new CalibrationData
        {
            HardwareProfile = hardwareProfile,
            MethodCalibrations = results,
            StartedAt = startedAt,
            CompletedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generic method calibration helper.
    /// </summary>
    private BCLCalibrationResult CalibrateMethod<T>(
        string typeName,
        string methodName,
        string expectedComplexity,
        Func<int, T> setup,
        Action<T> action)
    {
        try
        {
            var benchmarkRunner = new MicroBenchmarkRunner(_options);
            var results = benchmarkRunner.Run(setup, action);

            var verification = _verifier.AnalyzeResults(expectedComplexity, results);

            return new BCLCalibrationResult
            {
                TypeName = typeName,
                MethodName = methodName,
                Complexity = verification.Verified ? expectedComplexity : verification.MeasuredComplexity,
                ConstantFactorNs = verification.ConstantFactor,
                ConstantFactorError = EstimateError(results, verification.ConstantFactor),
                Success = verification.Verified,
                DataPoints = results.Count,
                RSquared = verification.RSquared,
                Timestamp = DateTime.UtcNow,
                ErrorMessage = verification.Verified ? null : verification.Notes
            };
        }
        catch (Exception ex)
        {
            return new BCLCalibrationResult
            {
                TypeName = typeName,
                MethodName = methodName,
                Complexity = expectedComplexity,
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Estimates error in constant factor from measurement variance.
    /// </summary>
    private static double EstimateError(IReadOnlyList<BenchmarkResult> results, double constant)
    {
        if (results.Count < 2 || constant == 0)
        {
            return 0;
        }

        // Use coefficient of variation as error estimate
        var avgCV = results.Average(r => r.CoefficientOfVariation);
        return constant * avgCV;
    }
}
