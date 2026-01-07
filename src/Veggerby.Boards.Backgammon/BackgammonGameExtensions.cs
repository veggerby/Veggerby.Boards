using System;

using Veggerby.Boards.Backgammon.MoveGeneration;
using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Backgammon-specific <see cref="GameProgress"/> extension methods.
/// </summary>
public static class BackgammonGameExtensions
{
    /// <summary>
    /// Gets the Backgammon-optimized legal move generator for this game.
    /// </summary>
    /// <param name="progress">The game progress to generate moves for.</param>
    /// <returns>
    /// A <see cref="BackgammonLegalMoveGenerator"/> that integrates Backgammon-specific move generation
    /// including dice-driven moves, bar re-entry, and bearing off.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This Backgammon-specific extension provides optimized move generation by:
    /// <list type="bullet">
    /// <item><description>Rolling dice when no dice values are available</description></item>
    /// <item><description>Enumerating piece moves constrained by dice values</description></item>
    /// <item><description>Prioritizing bar re-entry when pieces are on the bar</description></item>
    /// <item><description>Supporting bearing off when all pieces are in home board</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var generator = progress.GetBackgammonLegalMoveGenerator();
    /// var legalMoves = generator.GetLegalMoves(progress.State);
    /// foreach (var move in legalMoves)
    /// {
    ///     if (move is RollDiceGameEvent&lt;int&gt; roll)
    ///     {
    ///         Console.WriteLine("Can roll dice");
    ///     }
    ///     else if (move is MovePieceGameEvent pieceMove)
    ///     {
    ///         Console.WriteLine($"Can move {pieceMove.Piece.Id}");
    ///     }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// For general-purpose move generation across all game types, use the core
    /// <see cref="GameProgressExtensions.GetLegalMoveGenerator"/> extension instead.
    /// </para>
    /// </remarks>
    public static ILegalMoveGenerator GetBackgammonLegalMoveGenerator(this GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return new BackgammonLegalMoveGenerator(progress.Engine, progress.State);
    }
}
