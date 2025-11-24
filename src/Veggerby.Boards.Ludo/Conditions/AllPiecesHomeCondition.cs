using System;
using System.Linq;

using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Ludo.Conditions;

/// <summary>
/// Condition that checks if any player has all 4 pieces in their final home square (winning condition).
/// </summary>
public class AllPiecesHomeCondition : IGameStateCondition
{
    private readonly int _playerCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllPiecesHomeCondition"/> class.
    /// </summary>
    /// <param name="playerCount">Number of players in the game.</param>
    public AllPiecesHomeCondition(int playerCount)
    {
        _playerCount = playerCount;
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var playerColors = new[] { "red", "blue", "green", "yellow" };

        for (int p = 0; p < _playerCount; p++)
        {
            var playerColor = playerColors[p];
            var finalHomeTileId = $"home-{playerColor}-4";

            // Get pieces at final home square
            var piecesAtFinalHome = state.GetStates<PieceState>()
                .Count(ps => ps.CurrentTile is not null &&
                             string.Equals(ps.CurrentTile.Id, finalHomeTileId, StringComparison.Ordinal));

            // If any player has all 4 pieces home, game is won
            if (piecesAtFinalHome >= 4)
            {
                return ConditionResponse.Valid;
            }
        }

        return ConditionResponse.Ignore("No player has all pieces home");
    }
}
