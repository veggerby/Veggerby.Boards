namespace Veggerby.Boards.Flows.Events;

/// <summary>
/// Classification flags for event filtering (experimental). Used by future DecisionPlan event kind pre-filter.
/// </summary>
[System.Flags]
internal enum EventKind : ulong
{
    None = 0,
    // Piece movement events (e.g., MovePieceGameEvent)
    Move = 1UL << 0,
    // Dice roll events (e.g., RollDiceGameEvent<T>)
    Roll = 1UL << 1,
    // Generic state mutation events (e.g., selecting active player, doubling cube adjustments)
    State = 1UL << 2,
    // Phase control / structural flow events (future expansion; placeholder)
    Phase = 1UL << 3,
    // Reserved custom buckets for module-specific tagging (unassigned)
    Custom1 = 1UL << 10,
    Custom2 = 1UL << 11,
    Any = ulong.MaxValue
}

/// <summary>
/// Marker interface for non-movement, non-dice state mutation events.
/// </summary>
public interface IStateMutationGameEvent : IGameEvent { }

/// <summary>
/// Marker interface for events that influence phase/control flow.
/// </summary>
public interface IPhaseControlGameEvent : IGameEvent { }

/// <summary>
/// Utility responsible for classifying events and rules into <see cref="EventKind"/> buckets (experimental).
/// Reflection is used conservatively; future versions may embed explicit metadata.
/// </summary>
internal static class EventKindClassifier
{
    public static EventKind Classify(IGameEvent @event)
    {
        if (@event is MovePieceGameEvent)
        {
            return EventKind.Move;
        }
        var t = @event.GetType();
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RollDiceGameEvent<>))
        {
            return EventKind.Roll;
        }
        if (@event is IStateMutationGameEvent)
        {
            return EventKind.State;
        }
        if (@event is IPhaseControlGameEvent)
        {
            return EventKind.Phase;
        }
        // Placeholder: detect known state/phase events when introduced (e.g., SelectActivePlayerGameEvent)
        // if (t == typeof(SelectActivePlayerGameEvent)) return EventKind.State;
        return EventKind.Any; // fallback retains current broad classification
    }

    public static EventKind ClassifyRule(Rules.IGameEventRule rule)
    {
        if (rule is null)
        {
            return EventKind.None;
        }
        var type = rule.GetType();
        var handle = type.GetMethod("HandleEvent");
        if (handle is not null)
        {
            var ps = handle.GetParameters();
            if (ps.Length == 3)
            {
                var evtType = ps[2].ParameterType;
                if (evtType == typeof(MovePieceGameEvent))
                {
                    return EventKind.Move;
                }
                if (evtType.IsGenericType && evtType.GetGenericTypeDefinition() == typeof(RollDiceGameEvent<>))
                {
                    return EventKind.Roll;
                }
                if (typeof(IStateMutationGameEvent).IsAssignableFrom(evtType))
                {
                    return EventKind.State;
                }
                if (typeof(IPhaseControlGameEvent).IsAssignableFrom(evtType))
                {
                    return EventKind.Phase;
                }
                // Future: explicit matches for state / phase event types
                // if (evtType == typeof(SelectActivePlayerGameEvent)) return EventKind.State;
            }
        }
        return EventKind.Any;
    }
}