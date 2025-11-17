using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Combined mutator that detects terminal conditions and either adds terminal states OR rotates to the next player.
/// </summary>
/// <remarks>
/// This mutator runs after each move and:
/// 1. Checks for checkmate/stalemate
/// 2. If found, adds GameEndedState + outcome state (game ends)
/// 3. If not found, rotates to next player (game continues)
/// This ensures the game state correctly reflects termination without rotating past the end.
/// </remarks>
public sealed class EndgameCheckThenNextPlayerMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly IGameStateCondition _checkmateCondition;
    private readonly IGameStateCondition _stalemateCondition;
    private readonly Game _game;
    private readonly NextPlayerStateMutator _nextPlayerMutator;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndgameCheckThenNextPlayerMutator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public EndgameCheckThenNextPlayerMutator(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _game = game;
        _checkmateCondition = new Conditions.CheckmateCondition(game);
        _stalemateCondition = new Conditions.StalemateCondition(game);
        _nextPlayerMutator = new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition());
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Create detector to check terminal conditions
        var detector = new MoveGeneration.ChessEndgameDetector(_game);

        // Check checkmate condition
        if (detector.IsCheckmate(state))
        {
            var loser = state.GetActivePlayer();
            var winner = GetOpponent(loser);
            var outcomeState = new ChessOutcomeState(MoveGeneration.EndgameStatus.Checkmate, winner);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

        // Check stalemate condition
        if (detector.IsStalemate(state))
        {
            var outcomeState = new ChessOutcomeState(MoveGeneration.EndgameStatus.Stalemate, null);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

        // Game continues - rotate to next player
        return _nextPlayerMutator.MutateState(engine, state, @event);
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
