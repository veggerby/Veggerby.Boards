using System;
using System.Collections.Generic;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Rules.Conditions;

/// <summary>
/// Invalidates moves to tiles containing a threshold number of pieces owned by selected players (self/opponent/any).
/// </summary>
public class TileBlockedGameEventCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TileBlockedGameEventCondition"/> class.
    /// </summary>
    /// <param name="numberOfPiecesToBlock">Minimum piece count to block.</param>
    /// <param name="occupiedBy">Which players' pieces are counted.</param>
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

    /// <summary>
    /// Gets the blocking threshold.
    /// </summary>
    public int NumberOfPiecesToBlock
    {
        get;
    }

    /// <summary>
    /// Gets the player selection included in the count.
    /// </summary>
    public PlayerOption OccupiedBy
    {
        get;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Group pieces by ownership and count
        var selfCount = 0;
        var opponentCount = 0;
        var piecesOnTile = state.GetPiecesOnTile(@event.To);

        foreach (var piece in piecesOnTile)
        {
            if (piece.Owner.Equals(@event.Piece.Owner))
            {
                selfCount++;
            }
            else
            {
                opponentCount++;
            }
        }

        if ((OccupiedBy & PlayerOption.Any) == PlayerOption.Any)
        {
            return selfCount + opponentCount >= NumberOfPiecesToBlock
                ? ConditionResponse.Invalid
                : ConditionResponse.Valid;
        }

        if ((OccupiedBy & PlayerOption.Self) != 0)
        {
            return selfCount >= NumberOfPiecesToBlock
                ? ConditionResponse.Invalid
                : ConditionResponse.Valid;
        }

        if ((OccupiedBy & PlayerOption.Opponent) != 0)
        {
            return opponentCount >= NumberOfPiecesToBlock
                ? ConditionResponse.Invalid
                : ConditionResponse.Valid;
        }

        return ConditionResponse.Valid;
    }
}