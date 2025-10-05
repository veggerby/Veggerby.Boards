using System;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Ensures none of the specified tiles contain pieces owned by the active player.
/// </summary>
public class NoPiecesOnTilesGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoPiecesOnTilesGameEventCondition{T}"/> class.
    /// </summary>
    /// <param name="tiles">Tiles which must be empty of the active player's pieces.</param>
    public NoPiecesOnTilesGameEventCondition(params Tile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);

        if (!tiles.Any())
        {
            throw new ArgumentException("Must provide at least one Tile", nameof(tiles));
        }

        Tiles = tiles;
    }

    /// <summary>
    /// Gets monitored tiles.
    /// </summary>
    public Tile[] Tiles { get; }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, T @event)
    {
        if (!state.TryGetActivePlayer(out var player))
        {
            return ConditionResponse.Ignore("No active player");
        }
        return Tiles.All(tile => !state.GetPiecesOnTile(tile, player).Any())
            ? ConditionResponse.Valid
            : ConditionResponse.Invalid;
    }
}