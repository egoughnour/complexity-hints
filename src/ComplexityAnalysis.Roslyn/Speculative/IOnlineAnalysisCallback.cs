namespace ComplexityAnalysis.Roslyn.Speculative;

/// <summary>
/// Callback interface for online/incremental analysis progress.
/// Implementations receive real-time updates during code analysis,
/// suitable for IDE integration and live feedback.
/// </summary>
public interface IOnlineAnalysisCallback
{
    /// <summary>
    /// Called when analysis begins.
    /// </summary>
    /// <param name="sourceLength">Length of source text being analyzed.</param>
    void OnAnalysisStarted(int sourceLength);

    /// <summary>
    /// Called when an analysis phase begins.
    /// </summary>
    void OnPhaseStarted(OnlineAnalysisPhase phase);

    /// <summary>
    /// Called when an analysis phase completes.
    /// </summary>
    void OnPhaseCompleted(OnlineAnalysisPhase phase);

    /// <summary>
    /// Called to report analysis progress.
    /// </summary>
    /// <param name="completed">Number of items completed.</param>
    /// <param name="total">Total number of items.</param>
    /// <param name="currentItem">Name of current item being processed.</param>
    void OnProgress(int completed, int total, string currentItem);

    /// <summary>
    /// Called when analysis completes successfully.
    /// </summary>
    void OnAnalysisCompleted(OnlineAnalysisResult result, TimeSpan elapsed);

    /// <summary>
    /// Called when an error occurs during analysis.
    /// </summary>
    void OnError(Exception exception);
}

/// <summary>
/// Null implementation that does nothing.
/// </summary>
public sealed class NullOnlineAnalysisCallback : IOnlineAnalysisCallback
{
    public static NullOnlineAnalysisCallback Instance { get; } = new();

    public void OnAnalysisStarted(int sourceLength) { }
    public void OnPhaseStarted(OnlineAnalysisPhase phase) { }
    public void OnPhaseCompleted(OnlineAnalysisPhase phase) { }
    public void OnProgress(int completed, int total, string currentItem) { }
    public void OnAnalysisCompleted(OnlineAnalysisResult result, TimeSpan elapsed) { }
    public void OnError(Exception exception) { }
}

/// <summary>
/// Console-based callback for debugging and testing.
/// </summary>
public sealed class ConsoleOnlineAnalysisCallback : IOnlineAnalysisCallback
{
    public void OnAnalysisStarted(int sourceLength)
    {
        Console.WriteLine($"[Online Analysis] Started - {sourceLength} chars");
    }

    public void OnPhaseStarted(OnlineAnalysisPhase phase)
    {
        Console.WriteLine($"[Online Analysis] Phase: {phase}...");
    }

    public void OnPhaseCompleted(OnlineAnalysisPhase phase)
    {
        Console.WriteLine($"[Online Analysis] Phase: {phase} completed");
    }

    public void OnProgress(int completed, int total, string currentItem)
    {
        Console.WriteLine($"[Online Analysis] Progress: {completed}/{total} - {currentItem}");
    }

    public void OnAnalysisCompleted(OnlineAnalysisResult result, TimeSpan elapsed)
    {
        Console.WriteLine($"[Online Analysis] Completed in {elapsed.TotalMilliseconds:F1}ms");
        Console.WriteLine($"[Online Analysis] Methods: {result.Methods.Count}, Confidence: {result.OverallConfidence:P1}");
    }

    public void OnError(Exception exception)
    {
        Console.WriteLine($"[Online Analysis] Error: {exception.Message}");
    }
}

/// <summary>
/// Callback that buffers events for later processing.
/// Useful for testing and batch processing.
/// </summary>
public sealed class BufferedOnlineAnalysisCallback : IOnlineAnalysisCallback
{
    private readonly List<AnalysisEvent> _events = new();
    private readonly object _lock = new();

    public IReadOnlyList<AnalysisEvent> Events
    {
        get
        {
            lock (_lock)
            {
                return _events.ToList();
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    public void OnAnalysisStarted(int sourceLength)
    {
        AddEvent(new AnalysisStartedEvent(sourceLength));
    }

    public void OnPhaseStarted(OnlineAnalysisPhase phase)
    {
        AddEvent(new PhaseStartedEvent(phase));
    }

    public void OnPhaseCompleted(OnlineAnalysisPhase phase)
    {
        AddEvent(new PhaseCompletedEvent(phase));
    }

    public void OnProgress(int completed, int total, string currentItem)
    {
        AddEvent(new ProgressEvent(completed, total, currentItem));
    }

    public void OnAnalysisCompleted(OnlineAnalysisResult result, TimeSpan elapsed)
    {
        AddEvent(new AnalysisCompletedEvent(result, elapsed));
    }

    public void OnError(Exception exception)
    {
        AddEvent(new ErrorEvent(exception));
    }

    private void AddEvent(AnalysisEvent evt)
    {
        lock (_lock)
        {
            _events.Add(evt);
        }
    }
}

/// <summary>
/// Base class for analysis events.
/// </summary>
public abstract record AnalysisEvent(DateTime Timestamp)
{
    protected AnalysisEvent() : this(DateTime.UtcNow) { }
}

public record AnalysisStartedEvent(int SourceLength) : AnalysisEvent;
public record PhaseStartedEvent(OnlineAnalysisPhase Phase) : AnalysisEvent;
public record PhaseCompletedEvent(OnlineAnalysisPhase Phase) : AnalysisEvent;
public record ProgressEvent(int Completed, int Total, string CurrentItem) : AnalysisEvent;
public record AnalysisCompletedEvent(OnlineAnalysisResult Result, TimeSpan Elapsed) : AnalysisEvent;
public record ErrorEvent(Exception Exception) : AnalysisEvent;

/// <summary>
/// Aggregates multiple callbacks into one.
/// </summary>
public sealed class CompositeOnlineAnalysisCallback : IOnlineAnalysisCallback
{
    private readonly IOnlineAnalysisCallback[] _callbacks;

    public CompositeOnlineAnalysisCallback(params IOnlineAnalysisCallback[] callbacks)
    {
        _callbacks = callbacks;
    }

    public void OnAnalysisStarted(int sourceLength)
    {
        foreach (var cb in _callbacks)
            cb.OnAnalysisStarted(sourceLength);
    }

    public void OnPhaseStarted(OnlineAnalysisPhase phase)
    {
        foreach (var cb in _callbacks)
            cb.OnPhaseStarted(phase);
    }

    public void OnPhaseCompleted(OnlineAnalysisPhase phase)
    {
        foreach (var cb in _callbacks)
            cb.OnPhaseCompleted(phase);
    }

    public void OnProgress(int completed, int total, string currentItem)
    {
        foreach (var cb in _callbacks)
            cb.OnProgress(completed, total, currentItem);
    }

    public void OnAnalysisCompleted(OnlineAnalysisResult result, TimeSpan elapsed)
    {
        foreach (var cb in _callbacks)
            cb.OnAnalysisCompleted(result, elapsed);
    }

    public void OnError(Exception exception)
    {
        foreach (var cb in _callbacks)
            cb.OnError(exception);
    }
}
