using System;
using System.Collections.Generic;
using System.Linq;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Flows.Patterns;
using Veggerby.Boards.Internal;

using Xunit;

namespace Veggerby.Boards.Tests.Patterns;

/// <summary>
/// Randomized (deterministic seed) parity tests ensuring the compiled pattern resolver
/// produces identical path acceptance results compared to the legacy visitor for a large
/// sample set of (piece, from, to) queries. Guardrail for semantic drift when expanding
/// supported pattern kinds.
/// </summary>
public class RandomizedCompiledPatternParityTests
{
    private static Game BuildGame()
    {
        var north = new Direction("north"); var south = new Direction("south"); var east = new Direction("east"); var west = new Direction("west");
        var ne = new Direction("ne"); var nw = new Direction("nw"); var se = new Direction("se"); var sw = new Direction("sw");
        var dirs = new[] { north, south, east, west, ne, nw, se, sw };
        var tiles = new List<Tile>();
        char[] files = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
        for (int r = 1; r <= 8; r++) foreach (var f in files) tiles.Add(new Tile($"{f}{r}"));
        Tile T(char f, int r) => tiles[(r - 1) * 8 + (f - 'a')];
        var rels = new List<TileRelation>();
        foreach (var f in files)
        {
            for (int r = 1; r <= 8; r++)
            {
                if (r < 8) rels.Add(new TileRelation(T(f, r), T(f, r + 1), north));
                if (r > 1) rels.Add(new TileRelation(T(f, r), T(f, r - 1), south));
                if (f < 'h') rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r), east));
                if (f > 'a') rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r), west));
                if (f < 'h' && r < 8) rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r + 1), ne));
                if (f > 'a' && r < 8) rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r + 1), nw));
                if (f < 'h' && r > 1) rels.Add(new TileRelation(T(f, r), T((char)(f + 1), r - 1), se));
                if (f > 'a' && r > 1) rels.Add(new TileRelation(T(f, r), T((char)(f - 1), r - 1), sw));
            }
        }
        var board = new Board("parity-board", rels);
        var white = new Player("white");
        var rook = new Piece("rook", white, [new DirectionPattern(north, true), new DirectionPattern(east, true), new DirectionPattern(south, true), new DirectionPattern(west, true)]);
        var bishop = new Piece("bishop", white, [new DirectionPattern(ne, true), new DirectionPattern(nw, true), new DirectionPattern(se, true), new DirectionPattern(sw, true)]);
        var queen = new Piece("queen", white, [new MultiDirectionPattern(dirs, true)]);
        var knight = new Piece("knight", white, [new FixedPattern([east, east, north]), new FixedPattern([east, east, south]), new FixedPattern([west, west, north]), new FixedPattern([west, west, south]), new FixedPattern([north, north, east]), new FixedPattern([north, north, west]), new FixedPattern([south, south, east]), new FixedPattern([south, south, west])]);
        var pawn = new Piece("pawn", white, [new DirectionPattern(north, false)]);
        var king = new Piece("king", white, [new MultiDirectionPattern(dirs, false)]);
        var artifacts = new Artifact[] { rook, bishop, queen, knight, pawn, king, white, board };
        return new Game(board, [white], artifacts);
    }

    [Fact]
    public void GivenRandomQueries_WhenResolved_ThenCompiledMatchesLegacy()
    {
        // arrange
        var game = BuildGame();
        var pieces = game.Artifacts.OfType<Piece>().Where(p => p.Owner.Id == "white").ToArray(); // only white-owned pieces
        var tiles = game.Board.Tiles.ToArray();
        var random = new System.Random(424242); // deterministic seed
        var table = PatternCompiler.Compile(game);
        var shape = Boards.Internal.Layout.BoardShape.Build(game.Board);
        var compiled = new CompiledPatternResolver(table, game.Board, null, shape);
        const int samples = 2000;

        // act
        var mismatches = new List<string>();
        for (int i = 0; i < samples; i++)
        {
            var piece = pieces[random.Next(pieces.Length)];
            var from = tiles[random.Next(tiles.Length)];
            var to = tiles[random.Next(tiles.Length)];
            if (from == to) { i--; continue; }

            // legacy
            bool legacyHit = false;
            var visitor = new ResolveTilePathPatternVisitor(game.Board, from, to);
            foreach (var pattern in piece.Patterns)
            {
                pattern.Accept(visitor);
                if (visitor.ResultPath is not null && visitor.ResultPath.To.Equals(to)) { legacyHit = true; break; }
            }

            // compiled
            bool compiledHit = compiled.TryResolve(piece, from, to, out var compiledPath) && compiledPath is not null && compiledPath.To.Equals(to);

            if (legacyHit != compiledHit)
            {
                mismatches.Add($"Mismatch {piece.Id} {from}->{to} legacy={legacyHit} compiled={compiledHit}");
                if (mismatches.Count > 10) { break; } // fail fast with sample of differences
            }
        }

        // assert
        Assert.True(mismatches.Count == 0, "Found mismatches:\n" + string.Join('\n', mismatches));
    }
}