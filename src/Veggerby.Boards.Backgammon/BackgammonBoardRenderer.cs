using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Backgammon;

/// <summary>
/// Renders a simplified ASCII representation of a Backgammon board to a <see cref="TextWriter"/>.
/// </summary>
/// <remarks>
/// Shows points 24..13 (outer board top) and 12..1 (outer board bottom) with piece counts per point.
/// White points are traversed clockwise (increasing point number); black counterclockwise (decreasing).
/// Representation example segment: "24: b2 | 23: . | 22: . | ...". Bar / homes are summarized below.
/// </remarks>
public static class BackgammonBoardRenderer
{
    /// <summary>
    /// Writes the current Backgammon board state as ASCII to the provided writer.
    /// </summary>
    /// <param name="game">The compiled backgammon game definition.</param>
    /// <param name="state">The current immutable game state snapshot.</param>
    /// <param name="writer">Destination text writer.</param>
    public static void Write(Game game, GameState state, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(writer);

        // Aggregate piece counts by point
        var pointCounts = new Dictionary<string, (int white, int black)>();
        foreach (var ps in state.GetStates<PieceState>())
        {
            var tileId = ps.CurrentTile.Id; // builder uses plain point-N ids
            if (!tileId.StartsWith("point-")) { continue; }
            if (!pointCounts.TryGetValue(tileId, out var acc)) { acc = (0, 0); }
            if (ps.Artifact.Owner?.Id == "white") { acc.white++; } else if (ps.Artifact.Owner?.Id == "black") { acc.black++; }
            pointCounts[tileId] = acc;
        }

        writer.WriteLine("+---------------- BACKGAMMON ----------------+");
        writer.WriteLine("Top (24 -> 13)");
        for (int p = 24; p >= 13; p--)
        {
            var key = $"point-{p}";
            pointCounts.TryGetValue(key, out var acc);
            writer.Write($"{p,2}:{FormatPoint(acc)} ");
        }
        writer.WriteLine();
        writer.WriteLine("Bottom (12 -> 1)");
        for (int p = 12; p >= 1; p--)
        {
            var key = $"point-{p}";
            pointCounts.TryGetValue(key, out var acc);
            writer.Write($"{p,2}:{FormatPoint(acc)} ");
        }
        writer.WriteLine();

        // Bar & homes summary
        var bar = game.GetTile("bar");
        var homeWhite = game.GetTile("home-white");
        var homeBlack = game.GetTile("home-black");
        int barWhite = 0, barBlack = 0, homeW = 0, homeB = 0;
        foreach (var ps in state.GetStates<PieceState>())
        {
            if (ps.CurrentTile.Equals(bar)) { if (ps.Artifact.Owner?.Id == "white") barWhite++; else if (ps.Artifact.Owner?.Id == "black") barBlack++; }
            if (ps.CurrentTile.Equals(homeWhite)) { if (ps.Artifact.Owner?.Id == "white") homeW++; }
            if (ps.CurrentTile.Equals(homeBlack)) { if (ps.Artifact.Owner?.Id == "black") homeB++; }
        }
        writer.WriteLine($"Bar: W{barWhite} B{barBlack} | Home: W{homeW} B{homeB}");
        writer.WriteLine();
    }

    private static string FormatPoint((int white, int black) acc)
    {
        if (acc.white == 0 && acc.black == 0) return ".";
        var parts = new List<string>();
        if (acc.white > 0) parts.Add($"w{acc.white}");
        if (acc.black > 0) parts.Add($"b{acc.black}");
        return string.Join('/', parts);
    }
}