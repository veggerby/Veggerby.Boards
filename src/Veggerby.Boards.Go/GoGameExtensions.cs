using System;

using Veggerby.Boards.Flows.LegalMoveGeneration;
using Veggerby.Boards.Go.MoveGeneration;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Go;

/// <summary>
/// Go-specific <see cref="GameProgress"/> extension methods.
/// </summary>
public static class GoGameExtensions
{
    /// <summary>
    /// Gets the Go-optimized legal move generator for this game.
    /// </summary>
    /// <param name="progress">The game progress to generate moves for.</param>
    /// <returns>
    /// A <see cref="GoLegalMoveGenerator"/> that integrates Go-specific move generation
    /// including stone placement, ko rule, and suicide rule validation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This Go-specific extension provides optimized move generation by:
    /// <list type="bullet">
    /// <item><description>Enumerating all empty intersections as placement candidates</description></item>
    /// <item><description>Filtering ko-forbidden moves</description></item>
    /// <item><description>Filtering suicide moves (unless capturing)</description></item>
    /// <item><description>Always allowing pass moves</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var generator = progress.GetGoLegalMoveGenerator();
    /// var legalMoves = generator.GetLegalMoves(progress.State);
    /// foreach (var move in legalMoves)
    /// {
    ///     if (move is PlaceStoneGameEvent placement)
    ///     {
    ///         Console.WriteLine($"Can place at {placement.Target.Id}");
    ///     }
    ///     else if (move is PassTurnGameEvent)
    ///     {
    ///         Console.WriteLine("Can pass");
    ///     }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// For general-purpose move generation across all game types, use the core
    /// <see cref="GameProgressExtensions.GetLegalMoveGenerator"/> extension instead.
    /// </para>
    /// </remarks>
    public static ILegalMoveGenerator GetGoLegalMoveGenerator(this GameProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        return new GoLegalMoveGenerator(progress.Engine, progress.State);
    }
}
