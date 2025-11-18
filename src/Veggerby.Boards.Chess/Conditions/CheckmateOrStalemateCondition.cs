using System;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess.Conditions;

/// <summary>
/// Checks if the current position is either checkmate or stalemate (endgame condition).
/// </summary>
public sealed class CheckmateOrStalemateCondition : IGameStateCondition
{
    private readonly ChessEndgameDetector _detector;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckmateOrStalemateCondition"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public CheckmateOrStalemateCondition(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _detector = new ChessEndgameDetector(game);
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Check if checkmate or stalemate
        if (_detector.IsCheckmate(state) || _detector.IsStalemate(state))
        {
            return ConditionResponse.Valid;
        }

        return ConditionResponse.Ignore("Not in endgame");
    }
}
