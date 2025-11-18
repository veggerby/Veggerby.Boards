using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Mutator that adds game ended and checkmate/stalemate outcome states when endgame is detected.
/// </summary>
/// <remarks>
/// This mutator is designed to be used with phase-level endgame detection via <c>.WithEndGameDetection()</c>.
/// It determines whether the game ended in checkmate or stalemate and adds the appropriate states.
/// </remarks>
public sealed class ChessEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChessEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public ChessEndGameMutator(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Determine if checkmate or stalemate
        var detector = new ChessEndgameDetector(_game);

        if (detector.IsCheckmate(state))
        {
            // Checkmate - the active player (who cannot move) loses
            var loser = state.GetActivePlayer();
            var winner = GetOpponent(loser);
            var outcomeState = new ChessOutcomeState(EndgameStatus.Checkmate, winner, loser);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }
        else if (detector.IsStalemate(state))
        {
            // Stalemate - draw
            var player1 = state.GetActivePlayer();
            var player2 = GetOpponent(player1);
            var outcomeState = new ChessOutcomeState(EndgameStatus.Stalemate, player1, player2);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

        // Shouldn't reach here if condition was properly evaluated, but return unchanged state as fallback
        return state;
    }

    private Player? GetOpponent(Player currentPlayer)
    {
        foreach (var player in _game.Players)
        {
            if (player.Id != currentPlayer.Id)
            {
                return player;
            }
        }

        return null;
    }
}
