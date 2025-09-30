using System;
using System.Linq;

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
            return ConditionResponse.Ignore("No active player selected");
        }

        // First doubling allowed when no specialized cube state exists.
        var specialized = state.GetState<DoublingDiceState>(DoublingDice);
        if (specialized is null)
        {
            return ConditionResponse.Valid;
        }

        // Turn sequencing required (presence of TurnState is sufficient signal in shadow mode).
        var turnState = state.GetStates<TurnState>().FirstOrDefault();
        if (turnState is null)
        {
            return ConditionResponse.Ignore("No turn state – redoubles inert");
        }

        // Same-turn attempt blocked.
        if (turnState.TurnNumber <= specialized.LastDoubledTurn)
        {
            return ConditionResponse.Ignore("Same turn redouble blocked");
        }

        // Owner must be the active player to redouble (offer); if not on roll yet, ignore.
        if (!specialized.CurrentPlayer?.Equals(activePlayer.Artifact) ?? true)
        {
            return ConditionResponse.Ignore("Owner not active – cannot redouble yet");
        }

        return ConditionResponse.Valid;
    }
}