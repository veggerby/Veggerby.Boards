using System;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Chess.MoveGeneration;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess.Conditions;

/// <summary>
/// Checks if the current position is checkmate (active player's king in check with no legal moves).
/// </summary>
public sealed class CheckmateCondition : IGameStateCondition
{
    private readonly ChessEndgameDetector _detector;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckmateCondition"/> class.
    /// </summary>
    /// <param name="game">The chess game definition.</param>
    public CheckmateCondition(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _detector = new ChessEndgameDetector(game);
    }

    /// <inheritdoc />
    public ConditionResponse Evaluate(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Don't activate if game already ended
        if (state.GetStates<GameEndedState>().Any())
        {
            return ConditionResponse.Ignore("Game already ended");
        }

        return _detector.IsCheckmate(state) 
            ? ConditionResponse.Valid 
            : ConditionResponse.Ignore("Not checkmate");
    }
}
