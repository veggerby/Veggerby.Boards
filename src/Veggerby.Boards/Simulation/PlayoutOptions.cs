using System;

namespace Veggerby.Boards.Simulation;

/// <summary>
/// Options controlling simulation playout execution.
/// </summary>
public sealed class PlayoutOptions
{
    /// <summary>Maximum number of events to apply in a single playout (safety cap). Null = unlimited.</summary>
    public int? MaxEvents
    {
        get; init;
    }

    /// <summary>Optional limit on wall-clock duration per playout. Null = no time limit.</summary>
    public TimeSpan? TimeLimit
    {
        get; init;
    }

    /// <summary>When true, capture intermediate states (costly). Default false.</summary>
    public bool CaptureTrace
    {
        get; init;
    }

    /// <summary>Creates a new options instance with optional overrides.</summary>
    public PlayoutOptions With(int? maxEvents = null, TimeSpan? timeLimit = null, bool? captureTrace = null)
        => new()
        {
            MaxEvents = maxEvents ?? MaxEvents,
            TimeLimit = timeLimit ?? TimeLimit,
            CaptureTrace = captureTrace ?? CaptureTrace
        };

    internal bool ShouldStop(int applied, DateTime startedUtc)
    {
        if (MaxEvents.HasValue && applied >= MaxEvents.Value)
        {
            return true;
        }

        if (TimeLimit.HasValue && (DateTime.UtcNow - startedUtc) >= TimeLimit.Value)
        {
            return true;
        }

        return false;
    }
}