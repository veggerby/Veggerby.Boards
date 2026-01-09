using System;

using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Examples.RockPaperScissors;

/// <summary>
/// Mutator handling <see cref="SelectChoiceEvent"/> by recording the player's choice.
/// </summary>
internal sealed class SelectChoiceStateMutator : IStateMutator<SelectChoiceEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, SelectChoiceEvent @event)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        var choiceState = new PlayerChoiceState(@event.Player, @event.Choice);
        return gameState.Next([choiceState]);
    }
}
