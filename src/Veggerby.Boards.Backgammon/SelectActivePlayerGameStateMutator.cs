using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Applies the starting active player selection state.
/// </summary>
public class SelectActivePlayerGameStateMutator : IStateMutator<SelectActivePlayerGameEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, SelectActivePlayerGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);
        var white = engine.Game.GetPlayer("white");
        var black = engine.Game.GetPlayer("black");
        if (white is null || black is null)
        {
            return gameState; // players missing
        }
        var active = @event.ActivePlayerId == white.Id ? white : black;
        var inactive = ReferenceEquals(active, white) ? black : white;
        return gameState.Next([new ActivePlayerState(active, true), new ActivePlayerState(inactive, false)]);
    }
}