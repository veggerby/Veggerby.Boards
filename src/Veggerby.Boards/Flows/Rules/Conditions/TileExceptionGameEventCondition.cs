
using System;
using System.Linq;


using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Invalidates move events that target any of the specified forbidden tiles.
/// </summary>
public class TileExceptionGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TileExceptionGameEventCondition"/> class.
    /// </summary>
    /// <param name="tiles">Tiles that cannot be used as a destination.</param>
    public TileExceptionGameEventCondition(params Tile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);

        if (!tiles.Any())
        {
            throw new ArgumentException("Must provide at least one Tile", nameof(tiles));
        }

        Tiles = tiles;
    }

    /// <summary>
    /// Gets the forbidden tiles.
    /// </summary>
    public Tile[] Tiles
    {
        get;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);
        return Tiles.Contains(@event.To) ? ConditionResponse.Invalid : ConditionResponse.Valid;
    }
}