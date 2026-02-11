# ComplexityAnalysis.Calibration #

## Type BCLCalibrator

 Calibrates BCL method complexities through runtime verification. Generates benchmark code, runs it, and analyzes results to determine constant factors for complexity expressions. 



---
#### Method BCLCalibrator.CalibrateListAdd

 Calibrates List<T>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateListContains

 Calibrates List<T>.Contains method. Expected: O(n). 



---
#### Method BCLCalibrator.CalibrateListSort

 Calibrates List<T>.Sort method. Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateListBinarySearch

 Calibrates List<T>.BinarySearch method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateDictionaryAdd

 Calibrates Dictionary<K,V>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateDictionaryTryGetValue

 Calibrates Dictionary<K,V>.TryGetValue method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateDictionaryContainsKey

 Calibrates Dictionary<K,V>.ContainsKey method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateHashSetAdd

 Calibrates HashSet<T>.Add method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateHashSetContains

 Calibrates HashSet<T>.Contains method. Expected: O(1). 



---
#### Method BCLCalibrator.CalibrateSortedSetAdd

 Calibrates SortedSet<T>.Add method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateSortedDictionaryAdd

 Calibrates SortedDictionary<K,V>.Add method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateStringContains

 Calibrates String.Contains method. Expected: O(n*m) worst case, often O(n) average. 



---
#### Method BCLCalibrator.CalibrateStringIndexOf

 Calibrates String.IndexOf method. Expected: O(n*m) worst case. 



---
#### Method BCLCalibrator.CalibrateArraySort

 Calibrates Array.Sort method. Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateArrayBinarySearch

 Calibrates Array.BinarySearch method. Expected: O(log n). 



---
#### Method BCLCalibrator.CalibrateLinqWhere

 Calibrates LINQ Enumerable.Where (iteration). Expected: O(n) for full iteration. 



---
#### Method BCLCalibrator.CalibrateLinqOrderBy

 Calibrates LINQ Enumerable.OrderBy (full sort). Expected: O(n log n). 



---
#### Method BCLCalibrator.CalibrateStringBuilderAppend

 Calibrates StringBuilder.Append method. Expected: O(1) amortized. 



---
#### Method BCLCalibrator.CalibrateRegexIsMatch

 Calibrates Regex.IsMatch for simple pattern. Expected: O(n) for simple patterns. 



---
#### Method BCLCalibrator.RunFullCalibration(System.IProgress{System.ValueTuple{System.String,System.Int32,System.Int32}})

 Runs all standard BCL calibrations. 

|Name | Description |
|-----|------|
|progress: |Optional progress callback.|
**Returns**: Complete calibration data.



---
#### Method BCLCalibrator.CalibrateMethod``1(System.String,System.String,System.String,System.Func{System.Int32,``0},System.Action{``0})

 Generic method calibration helper. 



---
#### Method BCLCalibrator.EstimateError(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult},System.Double)

 Estimates error in constant factor from measurement variance. 



---
## Type BenchmarkResult

 Result of a single micro-benchmark measurement. 



---
#### Property BenchmarkResult.InputSize

 The input size N used for this measurement. 



---
#### Property BenchmarkResult.MeanNanoseconds

 Mean time per operation in nanoseconds. 



---
#### Property BenchmarkResult.StdDevNanoseconds

 Standard deviation in nanoseconds. 



---
#### Property BenchmarkResult.Iterations

 Number of iterations performed. 



---
#### Property BenchmarkResult.MinNanoseconds

 Minimum time observed in nanoseconds. 



---
#### Property BenchmarkResult.MaxNanoseconds

 Maximum time observed in nanoseconds. 



---
#### Property BenchmarkResult.AllocatedBytes

 Memory allocated per operation in bytes (if tracked). 



---
#### Property BenchmarkResult.CoefficientOfVariation

 Coefficient of variation (StdDev / Mean). Lower values indicate more stable measurements. 



---
## Type ComplexityVerificationResult

 Result of complexity verification through runtime measurement. 



---
#### Property ComplexityVerificationResult.ClaimedComplexity

 The claimed complexity class. 



---
#### Property ComplexityVerificationResult.MeasuredComplexity

 The best-fit complexity class from measurement. 



---
#### Property ComplexityVerificationResult.Verified

 Whether the measured complexity matches the claim. 



---
#### Property ComplexityVerificationResult.ConstantFactor

 Estimated constant factor (c in c*f(n)). 



---
#### Property ComplexityVerificationResult.RSquared

 R-squared value for the complexity fit (0.0 to 1.0). Higher values indicate better fit. 



---
#### Property ComplexityVerificationResult.BenchmarkResults

 Individual benchmark results used for verification. 



---
#### Property ComplexityVerificationResult.Confidence

 Confidence level in the verification (0.0 to 1.0). 



---
#### Property ComplexityVerificationResult.Notes

 Any warnings or notes about the verification. 



---
## Type BCLCalibrationResult

 Result of calibrating a specific BCL method. 



---
#### Property BCLCalibrationResult.MethodName

 The fully qualified method name. 



---
#### Property BCLCalibrationResult.TypeName

 The type containing the method. 



---
#### Property BCLCalibrationResult.Complexity

 The verified/calibrated complexity. 



---
#### Property BCLCalibrationResult.ConstantFactorNs

 Estimated constant factor in nanoseconds per base operation. 



---
#### Property BCLCalibrationResult.ConstantFactorError

 Standard error of the constant factor estimate. 



---
#### Property BCLCalibrationResult.Success

 Whether the calibration was successful. 



---
#### Property BCLCalibrationResult.DataPoints

 Number of data points used for calibration. 



---
#### Property BCLCalibrationResult.RSquared

 R-squared value for the fit. 



---
#### Property BCLCalibrationResult.Timestamp

 Timestamp when calibration was performed. 



---
#### Property BCLCalibrationResult.HardwareProfile

 Hardware profile at time of calibration. 



---
#### Property BCLCalibrationResult.ErrorMessage

 Any error message if calibration failed. 



---
## Type HardwareProfile

 Hardware profile for a calibration environment. 



---
#### Property HardwareProfile.ProfileId

 Unique identifier for this hardware profile. 



---
#### Property HardwareProfile.MachineName

 Machine name. 



---
#### Property HardwareProfile.ProcessorDescription

 Processor description. 



---
#### Property HardwareProfile.ProcessorCount

 Number of logical processors. 



---
#### Property HardwareProfile.PhysicalMemoryBytes

 Physical memory in bytes. 



---
#### Property HardwareProfile.OSDescription

 Operating system description. 



---
#### Property HardwareProfile.RuntimeVersion

 .NET runtime version. 



---
#### Property HardwareProfile.Is64BitProcess

 Whether the process is running in 64-bit mode. 



---
#### Property HardwareProfile.ReferenceBenchmarkScore

 Reference benchmark score for normalization across machines. Higher is faster. 



---
#### Property HardwareProfile.CapturedAt

 Timestamp when this profile was captured. 



---
#### Method HardwareProfile.Current

 Creates a hardware profile for the current machine. 



---
## Type CalibrationData

 Complete calibration data for a machine, containing all BCL calibration results. 



---
#### Property CalibrationData.Version

 Version of the calibration data format. 



---
#### Property CalibrationData.HardwareProfile

 Hardware profile for this calibration data. 



---
#### Property CalibrationData.MethodCalibrations

 All BCL method calibration results. 



---
#### Property CalibrationData.StartedAt

 When the calibration was started. 



---
#### Property CalibrationData.CompletedAt

 When the calibration was completed. 



---
#### Property CalibrationData.Duration

 Total duration of calibration. 



---
#### Property CalibrationData.SuccessfulCalibrations

 Number of methods successfully calibrated. 



---
#### Property CalibrationData.FailedCalibrations

 Number of methods that failed calibration. 



---
## Type BenchmarkOptions

 Options for benchmark configuration. 



---
#### Property BenchmarkOptions.Quick

 Default benchmark options for quick calibration. 



---
#### Property BenchmarkOptions.Standard

 Standard benchmark options for typical calibration. 



---
#### Property BenchmarkOptions.Thorough

 Thorough benchmark options for high-precision calibration. 



---
#### Property BenchmarkOptions.WarmupIterations

 Number of warmup iterations before measurement. 



---
#### Property BenchmarkOptions.MeasurementIterations

 Number of measurement iterations. 



---
#### Property BenchmarkOptions.MinIterationTimeMs

 Minimum time per iteration in milliseconds. 



---
#### Property BenchmarkOptions.MaxIterationTimeMs

 Maximum time per iteration in milliseconds. 



---
#### Property BenchmarkOptions.InputSizes

 Input sizes to benchmark at. 



---
#### Property BenchmarkOptions.TrackAllocations

 Whether to track memory allocations. 



---
#### Property BenchmarkOptions.ForceGC

 Whether to force garbage collection between iterations. 



---
## Type CalibrationStore

 Persists and loads calibration data to/from disk. Supports JSON format with optional compression. 



---
#### Method CalibrationStore.#ctor(System.String)

 Creates a calibration store with the specified base path. 

|Name | Description |
|-----|------|
|basePath: |Directory to store calibration data. Defaults to user's local app data.|


---
#### Method CalibrationStore.SaveAsync(ComplexityAnalysis.Calibration.CalibrationData,System.Threading.CancellationToken)

 Saves calibration data for the current machine. 



---
#### Method CalibrationStore.LoadLatestAsync(System.Threading.CancellationToken)

 Loads the most recent calibration data. 



---
#### Method CalibrationStore.LoadAsync(System.String,System.Threading.CancellationToken)

 Loads calibration data for a specific hardware profile. 



---
#### Method CalibrationStore.ListProfiles

 Lists all available calibration profiles. 



---
#### Method CalibrationStore.IsCalibrationValidAsync(System.TimeSpan,System.Threading.CancellationToken)

 Checks if calibration data exists and is recent enough. 



---
#### Method CalibrationStore.Delete(System.String)

 Deletes calibration data for a profile. 



---
#### Method CalibrationStore.GenerateReport(ComplexityAnalysis.Calibration.CalibrationData)

 Exports calibration data as a summary report. 



---
## Type CalibratedComplexityLookup

 Provides lookup of calibrated constant factors for BCL methods. 



---
#### Property CalibratedComplexityLookup.HasCalibration

 Whether calibration data is available. 



---
#### Method CalibratedComplexityLookup.GetConstantFactor(System.String,System.String)

 Gets the calibrated constant factor for a method, if available. 



---
#### Method CalibratedComplexityLookup.EstimateTime(System.String,System.String,System.String,System.Int32)

 Gets the estimated time for an operation with given complexity and input size. 

|Name | Description |
|-----|------|
|typeName: |Type containing the method.|
|methodName: |Method name.|
|complexity: |Complexity class (e.g., "O(n)", "O(log n)").|
|inputSize: |Input size N.|
**Returns**: Estimated time in nanoseconds, or null if not calibrated.



---
#### Method CalibratedComplexityLookup.GetAllCalibrations

 Gets all calibrated methods. 



---
## Type ComplexityVerifier

 Verifies claimed complexity classes against runtime measurements. Uses curve fitting to determine the best-fit complexity class. 



---
#### Method ComplexityVerifier.Verify``1(System.String,System.Func{System.Int32,``0},System.Action{``0})

 Verifies that an operation matches its claimed complexity. 

|Name | Description |
|-----|------|
|T: |Type of setup data.|
|Name | Description |
|-----|------|
|claimedComplexity: |The claimed complexity (e.g., "O(n)", "O(log n)").|
|setup: |Setup function that creates data for input size N.|
|action: |The action to verify.|
**Returns**: Verification result with measured complexity and constant factor.



---
#### Method ComplexityVerifier.AnalyzeResults(System.String,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Analyzes benchmark results to determine complexity class. 



---
#### Method ComplexityVerifier.DetectComplexity(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Detects the complexity class from benchmark results without a prior claim. 



---
#### Method ComplexityVerifier.EstimateConstantFactor(System.String,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Estimates the constant factor for a known complexity class. 



---
#### Method ComplexityVerifier.FitConstant(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(1) - constant time. 



---
#### Method ComplexityVerifier.FitLogarithmic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(log n) - logarithmic time. 



---
#### Method ComplexityVerifier.FitLinear(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n) - linear time. 



---
#### Method ComplexityVerifier.FitLinearithmic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n log n) - linearithmic time. 



---
#### Method ComplexityVerifier.FitQuadratic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n²) - quadratic time. 



---
#### Method ComplexityVerifier.FitCubic(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(n³) - cubic time. 



---
#### Method ComplexityVerifier.FitExponential(System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Fits data to O(2^n) - exponential time. 



---
#### Method ComplexityVerifier.LinearRegression(System.Collections.Generic.IReadOnlyList{System.Double},System.Collections.Generic.IReadOnlyList{System.Double})

 Performs linear regression without intercept: y = c*x Returns R² and coefficient c. 



---
#### Method ComplexityVerifier.NormalizeComplexity(System.String)

 Normalizes complexity notation to a standard form. 



---
#### Method ComplexityVerifier.ComputeConfidence(System.Double,System.Collections.Generic.IReadOnlyList{ComplexityAnalysis.Calibration.BenchmarkResult})

 Computes confidence based on R² and measurement stability. 



---
## Type MicroBenchmarkRunner

 Runs micro-benchmarks to measure operation timing with high precision. Uses careful warmup, statistical analysis, and outlier removal. 



---
#### Method MicroBenchmarkRunner.Run``1(System.Func{System.Int32,``0},System.Action{``0})

 Runs a benchmark on an action that operates on input of size N. 

|Name | Description |
|-----|------|
|setup: |Setup function that returns data for input size N.|
|action: |The action to benchmark, receives setup data.|
|T: |Type of setup data.|
**Returns**: List of benchmark results for each input size.



---
#### Method MicroBenchmarkRunner.MeasureAtSize``1(System.Func{System.Int32,``0},System.Action{``0},System.Int32)

 Runs a benchmark at a specific input size. 



---
#### Method MicroBenchmarkRunner.CalibrateOperationsPerIteration``1(System.Action{``0},``0)

 Calibrates the number of operations to run per iteration to meet timing requirements. 



---
#### Method MicroBenchmarkRunner.MeasureOperations``1(System.Action{``0},``0,System.Int32)

 Measures the time to execute a specific number of operations. 



---
#### Method MicroBenchmarkRunner.RemoveOutliers(System.Collections.Generic.List{System.Double})

 Removes outliers using the IQR method. 



---
#### Method MicroBenchmarkRunner.ComputeStdDev(System.Collections.Generic.List{System.Double},System.Double)

 Computes standard deviation. 



---


