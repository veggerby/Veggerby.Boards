using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Ludo.Mutators;

/// <summary>
/// Generic mutator that clears dice (sets them to null state) after any game event.
/// </summary>
public class GenericClearDiceStateMutator : IStateMutator<IGameEvent>
{
    private readonly IEnumerable<Dice> _dice;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericClearDiceStateMutator"/> class.
    /// </summary>
    /// <param name="dice">Dice to clear.</param>
    public GenericClearDiceStateMutator(IEnumerable<Dice> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);

        if (!dice.Any())
        {
            throw new ArgumentException("At least one dice must be provided", nameof(dice));
        }

        _dice = dice;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var updates = _dice.Select(d => (IArtifactState)new NullDiceState(d)).ToList();
        return gameState.Next(updates);
    }
}
