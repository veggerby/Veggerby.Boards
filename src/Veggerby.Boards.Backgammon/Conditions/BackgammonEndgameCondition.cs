using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Backgammon.Conditions;

/// <summary>
/// Checks if a backgammon game has ended (any player has borne off all checkers).
/// </summary>
/// <remarks>
/// This condition evaluates to Valid when any player has successfully moved all 15 of their
/// checkers to their home tile, indicating the game should end.
/// </remarks>
public sealed class BackgammonEndgameCondition : IGameStateCondition
{
    private readonly Game _game;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgammonEndgameCondition"/> class.
    /// </summary>
    /// <param name="game">The backgammon game definition.</param>
    public BackgammonEndgameCondition(Game game)
    {
        _game = game ?? throw new ArgumentNullException(nameof(game));
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var whiteHome = _game.GetTile("home-white");
        var blackHome = _game.GetTile("home-black");

        if (whiteHome is null || blackHome is null)
        {
            return ConditionResponse.Ignore("Home tiles not found");
        }

        // Check if white has won (all 15 pieces on home-white)
        if (HasWon(state, whiteHome, "white"))
        {
            return ConditionResponse.Valid;
        }

        // Check if black has won (all 15 pieces on home-black)
        if (HasWon(state, blackHome, "black"))
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore("Game not ended");
    }

    private bool HasWon(GameState state, Tile homeTile, string playerPrefix)
    {
        var piecesOnHome = 0;

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

            if (pieceState.CurrentTile.Id == homeTile.Id)
            {
                piecesOnHome++;
            }
        }

        return piecesOnHome == 15;
    }
}
