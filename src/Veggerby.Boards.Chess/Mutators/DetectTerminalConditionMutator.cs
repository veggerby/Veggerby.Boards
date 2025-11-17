using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Generic mutator that detects terminal conditions and adds terminal states after a move.
/// </summary>
/// <remarks>
/// This mutator runs after each move to check if a terminal condition has been reached.
/// If a terminal condition is detected, it adds both GameEndedState and the appropriate
/// outcome state. This pattern can be reused across different game types.
/// </remarks>
public sealed class DetectTerminalConditionMutator : IStateMutator<MovePieceGameEvent>
{
    private readonly IGameStateCondition _checkmateCondition;
    private readonly IGameStateCondition _stalemateCondition;
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetectTerminalConditionMutator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public DetectTerminalConditionMutator(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _game = game;
        _checkmateCondition = new Conditions.CheckmateCondition(game);
        _stalemateCondition = new Conditions.StalemateCondition(game);
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, MovePieceGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Check checkmate condition
        if (_checkmateCondition.Evaluate(state).Result == ConditionResult.Valid)
        {
            var loser = state.GetActivePlayer();
            var winner = GetOpponent(loser);
            var outcomeState = new ChessOutcomeState(MoveGeneration.EndgameStatus.Checkmate, winner);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

        // Check stalemate condition
        if (_stalemateCondition.Evaluate(state).Result == ConditionResult.Valid)
        {
            var outcomeState = new ChessOutcomeState(MoveGeneration.EndgameStatus.Stalemate, null);
            return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
        }

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
