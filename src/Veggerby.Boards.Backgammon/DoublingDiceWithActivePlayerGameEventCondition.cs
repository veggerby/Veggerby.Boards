using System;
using System.Linq;

using Veggerby.Boards.Core;
using Veggerby.Boards.Core.Artifacts;
using Veggerby.Boards.Core.Flows.Events;
using Veggerby.Boards.Core.Flows.Rules.Conditions;
using Veggerby.Boards.Core.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Ensures doubling cube may only be rolled by its current owner or when unowned.
/// </summary>
public class DoublingDiceWithActivePlayerGameEventCondition : IGameEventCondition<RollDiceGameEvent<int>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DoublingDiceWithActivePlayerGameEventCondition"/> class.
    /// </summary>
    public DoublingDiceWithActivePlayerGameEventCondition(Dice doublingDice)
    {
        ArgumentNullException.ThrowIfNull(doublingDice);

        DoublingDice = doublingDice;
    }

    /// <summary>
    /// Gets the doubling dice artifact.
    /// </summary>
    public Dice DoublingDice { get; }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameEngine engine, GameState state, RollDiceGameEvent<int> @event)
    {
        var diceState = state.GetState<DoublingDiceState>(DoublingDice);
        var activePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);

        return diceState.CurrentPlayer is null || diceState.CurrentPlayer.Equals(activePlayer.Artifact)
            ? ConditionResponse.Valid
            : ConditionResponse.Fail("Doubling dice with opponent");
    }
}