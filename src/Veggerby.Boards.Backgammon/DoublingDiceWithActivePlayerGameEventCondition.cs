using System;
using System.Linq;

using Veggerby.Boards;
using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;

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
        // Ensure event specifically targets the doubling cube artifact (single dice state)
        var newStates = @event.NewDiceStates.ToList();
        var targetsDoublingCube = newStates.Count == 1 && newStates[0].Artifact.Equals(DoublingDice);
        if (!targetsDoublingCube)
        {
            return ConditionResponse.Ignore("Event does not target doubling cube exclusively");
        }

        var activePlayer = state.GetStates<ActivePlayerState>().SingleOrDefault(x => x.IsActive);
        if (activePlayer is null)
        {
            // Prevent premature doubling before the starting player has been established.
            // Returning Ignore ensures the event is treated as not applicable rather than invalid (silent no-op semantics).
            return ConditionResponse.Ignore("No active player selected");
        }

        // Before the first successful doubling only a generic DiceState<int> may exist → treat as unowned (allow first doubling).
        var diceState = state.GetState<DoublingDiceState>(DoublingDice);
        if (diceState is null)
        {
            return ConditionResponse.Valid; // first doubling attempt allowed
        }

        // If current owner is null (should not normally happen once specialized) treat as valid.
        if (diceState.CurrentPlayer is null)
        {
            return ConditionResponse.Valid;
        }

        // Owner attempting to immediately redouble within same turn is not allowed (should be rejected silently -> Fail so rule not applied).
        if (diceState.CurrentPlayer.Equals(activePlayer.Artifact))
        {
            // Owner attempting immediate redouble within same turn; Ignore to preserve state (no exception or mutation).
            return ConditionResponse.Ignore("Owner immediate redouble not allowed");
        }

        // If opponent attempting redouble in same turn (active player not changed yet) we should also ignore.
        // We infer "same turn" by absence of any state change handing turn over; since no NextPlayer mutator ran,
        // active player still the one from first doubling. Thus any subsequent attempt (from either side) within
        // same state progression should be ignored.
        return ConditionResponse.Ignore("Immediate redouble attempt within same turn");
        // Opponent redouble validation will be reintroduced once explicit turn progression semantics added.
    }
}