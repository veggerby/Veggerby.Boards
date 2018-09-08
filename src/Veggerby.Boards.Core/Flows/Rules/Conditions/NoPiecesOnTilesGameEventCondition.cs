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
    public class NoPiecesOnTilesGameEventCondition<T> : IGameEventCondition<T> where T : IGameEvent
    {
        public NoPiecesOnTilesGameEventCondition(params Tile[] tiles)
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

        public ConditionResponse Evaluate(GameState state, T @event)
        {
            var player = state.GetActivePlayer();
            return Tiles.All(tile => !state.GetPiecesOnTile(tile, player).Any())
                ? ConditionResponse.Valid
                : ConditionResponse.Invalid;
        }
    }
}