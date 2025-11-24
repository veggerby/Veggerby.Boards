using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.Ludo.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Conditions;

/// <summary>
/// Condition that validates a piece can enter the board (requires dice value of 6 and piece must be in base).
/// </summary>
public class EnterPieceCondition : IGameEventCondition<EnterPieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, EnterPieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var dice = engine.Game.GetArtifact<Dice>("dice");
        if (dice is null)
        {
            return ConditionResponse.Fail("Dice not found");
        }

        var diceState = state.GetState<DiceState<int>>(dice);
        if (diceState is null || diceState.CurrentValue != 6)
        {
            return ConditionResponse.Fail("Must roll 6 to enter");
        }

        var pieceState = state.GetState<PieceState>(@event.Piece);
        if (pieceState?.CurrentTile is null)
        {
            return ConditionResponse.Fail("Piece not on board");
        }

        // Verify piece is in base
        var owner = @event.Piece.Owner;
        if (owner is null)
        {
            return ConditionResponse.Fail("Piece has no owner");
        }

        var baseTileId = $"base-{owner.Id}";
        if (!string.Equals(pieceState.CurrentTile.Id, baseTileId, StringComparison.Ordinal))
        {
            return ConditionResponse.Fail("Piece not in base");
        }

        return ConditionResponse.Valid;
    }
}
