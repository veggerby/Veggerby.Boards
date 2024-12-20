﻿
using System;
using System.Linq;


using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions;

public class TileExceptionGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    public TileExceptionGameEventCondition(params Tile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);

        if (!tiles.Any())
        {
            throw new ArgumentException("Must provide at least one Tile", nameof(tiles));
        }

        Tiles = tiles;
    }

    public Tile[] Tiles { get; }

    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        return Tiles.Contains(@event.To) ? ConditionResponse.Invalid : ConditionResponse.Valid;
    }
}