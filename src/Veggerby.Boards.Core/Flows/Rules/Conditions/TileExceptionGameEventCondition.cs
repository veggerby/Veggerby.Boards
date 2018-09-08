
using System;
using System.Collections.Generic;
using System.Linq;
using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Core.Flows.Rules.Conditions
{
    public class TileExceptionGameEventCondition : IGameEventCondition<MovePieceGameEvent>
    {
        public TileExceptionGameEventCondition(params Tile[] tiles)
        {
            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            if (!tiles.Any())
            {
                throw new ArgumentException("Must provide at least one Tile", nameof(tiles));
            }

            Tiles = tiles;
        }

        public Tile[] Tiles { get; }

        public ConditionResponse Evaluate(GameState state, MovePieceGameEvent @event)
        {
            return Tiles.Contains(@event.To) ? ConditionResponse.Invalid : ConditionResponse.Valid;
        }
    }
}