using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Flows.Mutators;

/// <summary>
/// Mutator that advances directly to the next turn (increment TurnNumber, reset Segment to Start,
/// and rotate active player) when a <see cref="TurnPassEvent"/> is handled under turn sequencing.
/// </summary>
/// <remarks>
/// This is a shortcut equivalent to ending all remaining segments in the current turn. It is inert
/// when turn sequencing is disabled or no current <see cref="TurnState"/> exists.
/// </remarks>
internal sealed class TurnPassStateMutator : IStateMutator<TurnPassEvent>
{
    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, TurnPassEvent @event)
    {
        if (!Internal.FeatureFlags.EnableTurnSequencing)
        {
            return gameState;
        }

        var currentTurnState = gameState.GetStates<TurnState>().FirstOrDefault();
        if (currentTurnState is null)
        {
            return gameState;
        }

        var advancedTurnState = new TurnState(currentTurnState.Artifact, currentTurnState.TurnNumber + 1, TurnSegment.Start);

        var activePlayerStates = gameState.GetStates<ActivePlayerState>();
        var currentActive = activePlayerStates.FirstOrDefault(x => x.IsActive);
        if (currentActive is null || !activePlayerStates.Any())
        {
            return gameState.Next([advancedTurnState]);
        }

        var players = engine.Game.Players;
        if (players is null || !players.Any())
        {
            return gameState.Next([advancedTurnState]);
        }

        var nextPlayer = players
            .Concat(players)
            .SkipWhile(p => !p.Equals(currentActive.Artifact))
            .Skip(1)
            .First();

        if (nextPlayer.Equals(currentActive.Artifact))
        {
            return gameState.Next([advancedTurnState]);
        }

        var previousPlayerProjection = new ActivePlayerState(currentActive.Artifact, false);
        var nextPlayerProjection = new ActivePlayerState(nextPlayer, true);
        return gameState.Next([advancedTurnState, previousPlayerProjection, nextPlayerProjection]);
    }
}