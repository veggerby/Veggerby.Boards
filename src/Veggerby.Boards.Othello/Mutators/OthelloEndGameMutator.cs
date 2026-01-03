using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Othello.Mutators;

/// <summary>
/// Mutator that finalizes the Othello game by counting discs and determining the winner.
/// </summary>
public sealed class OthelloEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="OthelloEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The game instance.</param>
    public OthelloEndGameMutator(Game game)
    {
        _game = game;
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState gameState, IGameEvent? @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(gameState);

        // Count discs for each player
        var blackCount = 0;
        var whiteCount = 0;

        // Get all discs - both those with PieceState and those that have been flipped (FlippedDiscState)
        var piecesWithPieceState = gameState.GetStates<PieceState>().Select(ps => ps.Artifact);
        var piecesWithFlipState = gameState.GetStates<FlippedDiscState>().Select(fs => fs.Artifact);
        var allDiscs = piecesWithPieceState.Concat(piecesWithFlipState).Distinct();

        foreach (var piece in allDiscs)
        {
            var currentColor = OthelloHelper.GetCurrentDiscColor(piece, gameState);

            if (currentColor == OthelloDiscColor.Black)
            {
                blackCount++;
            }
            else if (currentColor == OthelloDiscColor.White)
            {
                whiteCount++;
            }
        }

        var blackPlayer = _game.GetPlayer(OthelloIds.Players.Black);
        var whitePlayer = _game.GetPlayer(OthelloIds.Players.White);

        if (blackPlayer == null || whitePlayer == null)
        {
            return gameState; // Should not happen in valid game
        }

        var players = new[] { blackPlayer, whitePlayer };

        Player? winner = null;
        if (blackCount > whiteCount)
        {
            winner = blackPlayer;
        }
        else if (whiteCount > blackCount)
        {
            winner = whitePlayer;
        }
        // else draw (winner remains null)

        var outcomeState = new OthelloOutcomeState(winner, blackCount, whiteCount, players);
        var endedState = new GameEndedState();

        return gameState.Next(new IArtifactState[] { outcomeState, endedState });
    }
}
