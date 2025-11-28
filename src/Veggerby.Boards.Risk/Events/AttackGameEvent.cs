using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;

namespace Veggerby.Boards.Risk.Events;

/// <summary>
/// Event representing an attack from one territory to an adjacent enemy territory.
/// </summary>
public sealed class AttackGameEvent : IGameEvent
{
    private readonly Tile _fromTerritory;
    private readonly Tile _toTerritory;
    private readonly int _attackerDiceCount;

    /// <summary>
    /// Gets the attacking territory (source).
    /// </summary>
    public Tile FromTerritory => _fromTerritory;

    /// <summary>
    /// Gets the defending territory (target).
    /// </summary>
    public Tile ToTerritory => _toTerritory;

    /// <summary>
    /// Gets the number of dice the attacker will roll (1-3).
    /// </summary>
    public int AttackerDiceCount => _attackerDiceCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttackGameEvent"/> class.
    /// </summary>
    /// <param name="fromTerritory">The attacking territory.</param>
    /// <param name="toTerritory">The defending territory.</param>
    /// <param name="attackerDiceCount">Number of dice for attacker (1-3).</param>
    public AttackGameEvent(Tile fromTerritory, Tile toTerritory, int attackerDiceCount)
    {
        ArgumentNullException.ThrowIfNull(fromTerritory, nameof(fromTerritory));
        ArgumentNullException.ThrowIfNull(toTerritory, nameof(toTerritory));

        if (attackerDiceCount < 1 || attackerDiceCount > 3)
        {
            throw new ArgumentOutOfRangeException(nameof(attackerDiceCount), "Attacker must roll 1-3 dice.");
        }

        _fromTerritory = fromTerritory;
        _toTerritory = toTerritory;
        _attackerDiceCount = attackerDiceCount;
    }
}
