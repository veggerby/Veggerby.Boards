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
    public class TileBlockedGameEventCondition : IGameEventCondition<MovePieceGameEvent>
    {
        public TileBlockedGameEventCondition(int numberOfPiecesToBlock = 2, PlayerOption occupiedBy = PlayerOption.Opponent)
        {
            if (numberOfPiecesToBlock < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfPiecesToBlock));
            }

            if ((occupiedBy & PlayerOption.Self) == 0 && (occupiedBy & PlayerOption.Opponent) == 0)
            {
                throw new ArgumentException("Must specify either Self or Opponent", nameof(occupiedBy));
            }

            NumberOfPiecesToBlock = numberOfPiecesToBlock;
            OccupiedBy = occupiedBy;
        }

        public int NumberOfPiecesToBlock { get; }
        public PlayerOption OccupiedBy { get; }

        public ConditionResponse Evaluate(GameState state, MovePieceGameEvent @event)
        {
            var pieceStates = state
                .GetPiecesOnTile(@event.To)
                .GroupBy(x => x.Owner.Equals(@event.Piece.Owner) ? PlayerOption.Self : PlayerOption.Opponent)
                .ToDictionary(x => x.Key, x => x.ToList());

            if ((OccupiedBy & PlayerOption.Any) != 0)
            {
                return pieceStates[PlayerOption.Self].Count() + pieceStates[PlayerOption.Opponent].Count() >= NumberOfPiecesToBlock
                    ? ConditionResponse.Invalid
                    : ConditionResponse.Valid;
            }

            if ((OccupiedBy & PlayerOption.Self) != 0)
            {
                return pieceStates[PlayerOption.Self].Count() >= NumberOfPiecesToBlock
                    ? ConditionResponse.Invalid
                    : ConditionResponse.Valid;
            }

            if ((OccupiedBy & PlayerOption.Opponent) != 0)
            {
                return pieceStates[PlayerOption.Opponent].Count() >= NumberOfPiecesToBlock
                    ? ConditionResponse.Invalid
                    : ConditionResponse.Valid;
            }

            return ConditionResponse.Valid;
        }
    }
}