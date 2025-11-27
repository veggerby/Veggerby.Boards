using System;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Monopoly.Events;
using Veggerby.Boards.Monopoly.States;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Monopoly.Mutators;

/// <summary>
/// Mutator that handles declining a trade between players.
/// </summary>
public class DeclineTradeStateMutator : IStateMutator<DeclineTradeGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, DeclineTradeGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Simply clear the trade state
        var clearedTradeState = new TradeProposalState();

        return gameState.ReplaceExtras(clearedTradeState);
    }
}
