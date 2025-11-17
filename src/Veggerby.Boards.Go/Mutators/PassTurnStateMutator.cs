using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go.Mutators;

/// <summary>
/// Applies a <see cref="PassTurnGameEvent"/> by incrementing TurnState.PassStreak and rotating the active player.
/// When two consecutive passes occur, adds a <see cref="GameEndedState"/> to mark game termination.
/// </summary>
/// <remarks>
/// This mutator integrates with core turn sequencing: TurnState.PassStreak tracks consecutive passes,
/// enabling deterministic two-pass termination. The pass streak is automatically reset to zero when
/// PlaceStoneStateMutator is invoked.
/// </remarks>
public sealed class PassTurnStateMutator : IStateMutator<PassTurnGameEvent>
{
    /// <summary>
    /// Increments TurnState.PassStreak and rotates active player. On second consecutive pass, marks the game as ended.
    /// Clears ko restriction on any pass.
    /// </summary>
    public GameState MutateState(GameEngine engine, GameState gameState, PassTurnGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(@event);

        // Find current TurnState
        TurnState? currentTurnState = null;
        foreach (var ts in gameState.GetStates<TurnState>())
        {
            currentTurnState = ts;
            break;
        }

        // If no TurnState exists (sequencing disabled), fall back to simple pass handling
        if (currentTurnState is null)
        {
            // Clear ko on pass
            var extras = gameState.GetExtras<GoStateExtras>();
            if (extras != null && extras.KoTileId != null)
            {
                var clearedExtras = extras with { KoTileId = null };
                return gameState.ReplaceExtras(clearedExtras);
            }
            return gameState;
        }

        // Increment pass streak and advance turn
        var newPassStreak = currentTurnState.PassStreak + 1;
        var advancedTurnState = new TurnState(
            currentTurnState.Artifact,
            currentTurnState.TurnNumber + 1,
            TurnSegment.Start,
            newPassStreak);

        // Apply turn state and rotate active player
        var updatedState = ApplyTurnAndRotatePlayer(engine, gameState, advancedTurnState);

        // Clear ko on pass
        var currentExtras = updatedState.GetExtras<GoStateExtras>();
        if (currentExtras != null && currentExtras.KoTileId != null)
        {
            var clearedExtras = currentExtras with { KoTileId = null };
            updatedState = updatedState.ReplaceExtras(clearedExtras);
        }

        // If two consecutive passes, mark game as ended
        if (newPassStreak >= 2)
        {
            return updatedState.Next([new GameEndedState()]);
        }

        return updatedState;
    }

    /// <summary>
    /// Applies the new turn state and rotates the active player if applicable.
    /// </summary>
    /// <remarks>
    /// Replicates core turn sequencing rotation logic: finds current active player, determines next in rotation,
    /// and updates both projections. Returns input state with only turn state when rotation not applicable.
    /// </remarks>
    private static GameState ApplyTurnAndRotatePlayer(GameEngine engine, GameState prior, TurnState newTurnState)
    {
        ActivePlayerState? currentActive = null;
        foreach (var aps in prior.GetStates<ActivePlayerState>())
        {
            if (aps.IsActive)
            {
                currentActive = aps;
                break;
            }
        }

        if (currentActive is null)
        {
            return prior.Next([newTurnState]);
        }

        var playersEnumerable = engine.Game.Players;
        if (playersEnumerable is null)
        {
            return prior.Next([newTurnState]);
        }

        // Convert to array for indexing
        var playersList = new System.Collections.Generic.List<Artifacts.Player>();
        foreach (var p in playersEnumerable)
        {
            playersList.Add(p);
        }

        var players = playersList.ToArray();
        var total = players.Length;
        if (total <= 1)
        {
            return prior.Next([newTurnState]);
        }

        // Find current player index
        var idx = -1;
        for (var i = 0; i < total; i++)
        {
            if (players[i].Equals(currentActive.Artifact))
            {
                idx = i;
                break;
            }
        }

        if (idx == -1)
        {
            return prior.Next([newTurnState]);
        }

        // Calculate next player
        var nextIndex = (idx + 1) % total;
        var nextPlayer = players[nextIndex];

        if (nextPlayer.Equals(currentActive.Artifact))
        {
            return prior.Next([newTurnState]);
        }

        // Update both active player projections
        var previousProjection = new ActivePlayerState(currentActive.Artifact, false);
        var nextProjection = new ActivePlayerState(nextPlayer, true);

        return prior.Next([newTurnState, previousProjection, nextProjection]);
    }
}