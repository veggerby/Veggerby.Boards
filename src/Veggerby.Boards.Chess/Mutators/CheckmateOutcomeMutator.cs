using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Chess.Mutators;

/// <summary>
/// Marks the game as ended with checkmate outcome when activated.
/// </summary>
/// <remarks>
/// This mutator should be used in a terminal phase (e.g., checkmate phase) to add
/// both GameEndedState and ChessOutcomeState when the phase becomes active.
/// It works with any event type since it's triggered by phase activation, not event processing.
/// </remarks>
public sealed class CheckmateOutcomeMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckmateOutcomeMutator"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public CheckmateOutcomeMutator(Game game)
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

        // Get the player who just moved (the winner)
        // The active player at this point is the one who's in checkmate (the loser)
        var loser = state.GetActivePlayer();
        var winner = GetOpponent(loser);

        var outcomeState = new ChessOutcomeState(EndgameStatus.Checkmate, winner);
        return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
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
