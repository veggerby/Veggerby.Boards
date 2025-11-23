using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Mutators;

/// <summary>
/// Mutator that grants a bonus turn if a 6 was rolled, otherwise advances to next player.
/// </summary>
public class ConditionalBonusTurnStateMutator : IStateMutator<IGameEvent>
{
    private readonly Dice _dice;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalBonusTurnStateMutator"/> class.
    /// </summary>
    /// <param name="dice">The dice to check for rolling a 6.</param>
    public ConditionalBonusTurnStateMutator(Dice dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        _dice = dice;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var diceState = gameState.GetState<DiceState<int>>(_dice);
        if (diceState is not null && diceState.CurrentValue == 6)
        {
            // Bonus turn: use TurnReplayEvent to grant extra turn with same player
            var replayMutator = new TurnReplayStateMutator();
            return replayMutator.MutateState(engine, gameState, new TurnReplayEvent());
        }

        // Normal turn advancement: use NextPlayerStateMutator to rotate to next player
        // The NextPlayerStateMutator will handle turn advancement internally
        var nextPlayerMutator = new NextPlayerStateMutator(new States.Conditions.NullGameStateCondition());
        return nextPlayerMutator.MutateState(engine, gameState, @event);
    }
}
