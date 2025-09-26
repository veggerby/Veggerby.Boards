using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Internal;

/// <summary>
/// Centralized mapping from execution outcomes / exceptions to <see cref="EventResult"/>.
/// </summary>
internal static class EventRejectionClassifier
{
    public static EventResult Classify(GameProgress progress, IGameEvent @event)
    {
        var before = progress.State;
        try
        {
            var after = progress.HandleEvent(@event); // may throw
            if (!ReferenceEquals(before, after.State) && !before.Equals(after.State))
            {
                return EventResult.Accepted(after.State);
            }
            return EventResult.Rejected(before, EventRejectionReason.NotApplicable, "No rule produced a state change.");
        }
        catch (InvalidGameEventException ex)
        {
            return EventResult.Rejected(before, EventRejectionReason.RuleRejected, ex.ConditionResponse?.Reason);
        }
        catch (BoardException ex)
        {
            var msg = ex.Message ?? string.Empty;
            var reason = EventRejectionReason.InvalidEvent;
            if (msg.Contains("No valid dice state for path", StringComparison.OrdinalIgnoreCase))
            {
                reason = EventRejectionReason.PathNotFound;
            }
            else if (msg.Contains("Invalid from tile", StringComparison.OrdinalIgnoreCase))
            {
                reason = EventRejectionReason.InvalidOwnership;
            }
            return EventResult.Rejected(before, reason, ex.Message);
        }
        catch (Exception ex)
        {
            return EventResult.Rejected(before, EventRejectionReason.EngineInvariant, ex.Message);
        }
    }
}