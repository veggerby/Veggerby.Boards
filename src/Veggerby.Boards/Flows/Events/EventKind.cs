namespace Veggerby.Boards.Flows.Events;

/// <summary>
/// Classification flags for event filtering (experimental). Used by future DecisionPlan event kind pre-filter.
/// </summary>
[System.Flags]
internal enum EventKind : ulong
{
    None = 0,
    Move = 1UL << 0,
    Roll = 1UL << 1,
    Custom1 = 1UL << 2,
    Any = ulong.MaxValue
}

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
        return EventKind.Any;
    }

    public static EventKind ClassifyRule(Veggerby.Boards.Flows.Rules.IGameEventRule rule)
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
            }
        }
        return EventKind.Any;
    }
}