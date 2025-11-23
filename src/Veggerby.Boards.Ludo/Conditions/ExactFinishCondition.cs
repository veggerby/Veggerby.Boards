using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Conditions;

/// <summary>
/// Condition that validates exact count movement when entering final home square.
/// </summary>
public class ExactFinishCondition : IGameEventCondition<MovePieceGameEvent>
{
    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        var owner = @event.Piece.Owner;
        if (owner is null)
        {
            return ConditionResponse.Fail("Piece has no owner");
        }

        // Check if destination is in home stretch
        var finalHomeTileId = $"home-{owner.Id}-4";
        var destinationTileId = @event.To.Id;

        // If not moving to or within home stretch, no exact count needed
        if (!destinationTileId.StartsWith($"home-{owner.Id}-", StringComparison.Ordinal))
        {
            return ConditionResponse.Valid;
        }

        var dice = engine.Game.GetArtifact<Dice>("dice");
        if (dice is null)
        {
            return ConditionResponse.Fail("Dice not found");
        }

        var diceState = state.GetState<DiceState<int>>(dice);
        if (diceState is null)
        {
            return ConditionResponse.Fail("No dice value available");
        }

        var diceValue = diceState.CurrentValue;

        // If trying to go beyond the final square, invalid
        if (string.Equals(destinationTileId, finalHomeTileId, StringComparison.Ordinal))
        {
            // Moving to final square is OK
            return ConditionResponse.Valid;
        }

        // Check if this would overshoot
        var currentTileId = @event.From.Id;
        if (currentTileId.StartsWith($"home-{owner.Id}-", StringComparison.Ordinal))
        {
            // Extract current position in home stretch
            var currentPos = int.Parse(currentTileId.AsSpan(currentTileId.LastIndexOf('-') + 1));

            // If would overshoot final square (position 4)
            if (currentPos + diceValue > 4)
            {
                return ConditionResponse.Fail($"Cannot overshoot final square: need exact count");
            }
        }

        return ConditionResponse.Valid;
    }
}
