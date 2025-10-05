using System.Linq;

using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Post-dice-roll mutator that, during the opening phase, assigns the starting active player
/// when exactly two starting dice have been rolled with distinct values and no <see cref="ActivePlayerState"/> exists yet.
/// </summary>
/// <remarks>
/// This replaces the previously unused <c>SelectActivePlayerRule</c> wrapper approach by performing the selection
/// after the <see cref="DiceStateMutator{T}"/> has applied values, ensuring deterministic ordering without wrapping
/// rule composition. It preserves immutability: if preconditions are not satisfied, the original state is returned.
/// </remarks>
internal sealed class SelectStartingPlayerStateMutator : IStateMutator<RollDiceGameEvent<int>>
{
    public GameState MutateState(GameEngine engine, GameState gameState, RollDiceGameEvent<int> @event)
    {
        // Guard: only proceed if no active player assigned yet
        if (gameState.GetStates<ActivePlayerState>().Any())
        {
            return gameState; // already selected
        }

        // Collect the two opening dice (if they exist and were part of this roll)
        var diceIds = new[] { "dice-1", "dice-2" };
        var rolled = @event.NewDiceStates.Where(d => diceIds.Contains(d.Artifact.Id)).ToList();
        if (rolled.Count != 2)
        {
            return gameState; // not both starting dice in this event
        }

        var d1 = rolled[0];
        var d2 = rolled[1];
        if (d1.CurrentValue == d2.CurrentValue)
        {
            return gameState; // tie -> starter not chosen yet (players must re-roll)
        }

        // Determine starter (higher die value) – white vs black mapping via player ids
        var white = engine.Game.GetPlayer("white");
        var black = engine.Game.GetPlayer("black");
        var active = d1.CurrentValue > d2.CurrentValue ? white : black;
        var inactive = ReferenceEquals(active, white) ? black : white;
        return gameState.Next([
            new ActivePlayerState(active, true),
            new ActivePlayerState(inactive, false)
        ]);
    }
}