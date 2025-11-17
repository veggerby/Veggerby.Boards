using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon.Mutators;

/// <summary>
/// Mutator that adds game ended and backgammon outcome states when endgame is detected.
/// </summary>
/// <remarks>
/// This mutator is designed to be used with phase-level endgame detection via <c>.WithEndGameDetection()</c>.
/// It determines the type of victory (Normal, Gammon, or Backgammon) and adds the appropriate states.
/// </remarks>
public sealed class BackgammonEndGameMutator : IStateMutator<IGameEvent>
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgammonEndGameMutator"/> class.
    /// </summary>
    /// <param name="game">The backgammon game definition.</param>
    public BackgammonEndGameMutator(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public GameState MutateState(GameEngine engine, GameState state, IGameEvent @event)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(@event);

        // Determine winner
        var winner = DetermineWinner(state);

        if (winner is null)
        {
            // Shouldn't reach here if condition was properly evaluated
            return state;
        }

        // Determine victory type
        var victoryType = DetermineVictoryType(state, winner);

        var outcomeState = new BackgammonOutcomeState(winner, victoryType);

        return state.Next(new IArtifactState[] { new GameEndedState(), outcomeState });
    }

    private Player? DetermineWinner(GameState state)
    {
        var whiteHome = _game.GetTile("home-white");
        var blackHome = _game.GetTile("home-black");

        if (whiteHome is null || blackHome is null)
        {
            return null;
        }

        // Check if white has all 15 pieces on home
        if (CountPiecesOnTile(state, whiteHome, "white") == 15)
        {
            return _game.GetPlayer("white");
        }

        // Check if black has all 15 pieces on home
        if (CountPiecesOnTile(state, blackHome, "black") == 15)
        {
            return _game.GetPlayer("black");
        }

        return null;
    }

    private BackgammonVictoryType DetermineVictoryType(GameState state, Player winner)
    {
        var opponentPrefix = winner.Id == "white" ? "black" : "white";
        var opponentHome = _game.GetTile($"home-{opponentPrefix}");
        var bar = _game.GetTile("bar");

        if (opponentHome is null)
        {
            return BackgammonVictoryType.Normal;
        }

        var opponentBorneOff = CountPiecesOnTile(state, opponentHome, opponentPrefix);

        if (opponentBorneOff > 0)
        {
            // Opponent has borne off at least one checker - normal win
            return BackgammonVictoryType.Normal;
        }

        // Opponent hasn't borne off any checkers - at least a gammon
        // Check if backgammon: opponent has checkers on bar or in winner's home board

        // Check bar
        if (bar is not null && CountPiecesOnTile(state, bar, opponentPrefix) > 0)
        {
            return BackgammonVictoryType.Backgammon;
        }

        // Check if opponent has pieces in winner's home board
        // White home board: points 19-24
        // Black home board: points 1-6
        if (HasPiecesInWinnerHomeBoard(state, winner.Id, opponentPrefix))
        {
            return BackgammonVictoryType.Backgammon;
        }

        // No pieces on bar or in winner's home, but none borne off - gammon
        return BackgammonVictoryType.Gammon;
    }

    private bool HasPiecesInWinnerHomeBoard(GameState state, string winnerId, string opponentPrefix)
    {
        int startPoint;
        int endPoint;

        if (winnerId == "white")
        {
            // White's home board is points 19-24
            startPoint = 19;
            endPoint = 24;
        }
        else
        {
            // Black's home board is points 1-6
            startPoint = 1;
            endPoint = 6;
        }

        for (var point = startPoint; point <= endPoint; point++)
        {
            var tile = _game.GetTile($"point-{point}");

            if (tile is null)
            {
                continue;
            }

            if (CountPiecesOnTile(state, tile, opponentPrefix) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private int CountPiecesOnTile(GameState state, Tile tile, string playerPrefix)
    {
        var count = 0;

        for (var i = 1; i <= 15; i++)
        {
            var piece = _game.GetPiece($"{playerPrefix}-{i}");

            if (piece is null)
            {
                continue;
            }

            var pieceState = state.GetState<PieceState>(piece);

            if (pieceState is null)
            {
                continue;
            }

            if (pieceState.CurrentTile.Id == tile.Id)
            {
                count++;
            }
        }

        return count;
    }
}
