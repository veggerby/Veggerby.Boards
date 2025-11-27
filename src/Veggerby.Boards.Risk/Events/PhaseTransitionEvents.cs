using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Risk.Events;

/// <summary>
/// Event representing the end of the attack phase (player chooses to stop attacking).
/// </summary>
public sealed class EndAttackPhaseGameEvent : IGameEvent
{
}

/// <summary>
/// Event representing the end of the fortify phase (completes the turn).
/// </summary>
public sealed class EndFortifyPhaseGameEvent : IGameEvent
{
}

/// <summary>
/// Event representing skipping the fortify phase.
/// </summary>
public sealed class SkipFortifyPhaseGameEvent : IGameEvent
{
}
