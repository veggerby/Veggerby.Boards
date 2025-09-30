using System.Collections.Generic;

using Veggerby.Boards.Artifacts;
using Veggerby.Boards.Artifacts.Patterns;
using Veggerby.Boards.Artifacts.Relations;
using Veggerby.Boards.Backgammon;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.States;

namespace Veggerby.Boards.Tests.Core.Nomenclature;

/// <summary>
/// Basic coverage for nomenclature mappings (chess, backgammon, generic).
/// </summary>
public class NomenclatureTests
{
    [Fact]
    public void GivenChessPiece_WhenNamed_ThenUsesRoleLetter()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var piece = new Piece("white-pawn-5", owner: null, patterns: []);

        // act
        var name = nomenclature.GetPieceName(piece);

        // assert
        name.Should().Be("P");
    }

    [Fact]
    public void GivenBackgammonTile_WhenNamed_ThenStripsPrefix()
    {
        // arrange
        var nomenclature = new BackgammonNomenclature();
        var tile = new Tile("tile-point-12");

        // act
        var name = nomenclature.GetTileName(tile);

        // assert
        name.Should().Be("point-12");
    }

    [Fact]
    public void GivenSimplePawnAdvance_WhenDescribed_ThenUsesDestinationOnly()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var pawn = new Piece("white-pawn-5", owner: null, patterns: []);
        var from = new Tile(ChessIds.Tiles.E2);
        var mid = new Tile(ChessIds.Tiles.E3);
        var to = new Tile(ChessIds.Tiles.E4);
        var rel1 = new TileRelation(from, mid, Direction.South);
        var rel2 = new TileRelation(mid, to, Direction.South);
        var path = new TilePath([rel1, rel2]);
        var move = new MovePieceGameEvent(pawn, path);

        // act
        var san = nomenclature.Describe(move);

        // assert
        san.Should().Be("e4");
    }

    [Fact]
    public void GivenPawnCapture_WhenDescribed_ThenIncludesFileAndX()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var black = new Player(ChessIds.Players.Black);
        var pawn = new Piece("white-pawn-5", owner: white, patterns: []);
        var victim = new Piece("black-pawn-4", owner: black, patterns: []);
        var from = new Tile(ChessIds.Tiles.E4);
        var to = new Tile(ChessIds.Tiles.D5);
        var rel = new TileRelation(from, to, Direction.NorthWest); // diagonal capture style (engine semantics simplified)
        var path = new TilePath([rel]);
        var move = new MovePieceGameEvent(pawn, path);

        // Construct a minimal pre-move state containing both pieces on their squares
        var state = GameState.New(new IArtifactState[]
        {
            new PieceState(pawn, from),
            new PieceState(victim, to)
        });

        // act
        var san = nomenclature.Describe(state, move);

        // assert
        san.Should().Be("exd5");
    }

    [Fact]
    public void GivenTwoKnightsCanReach_WhenDescribed_ThenFileDisambiguates()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var patterns = new List<IPattern>
        {
            new FixedPattern(new[]{Direction.West, Direction.North, Direction.North}),
            new FixedPattern(new[]{Direction.West, Direction.South, Direction.South}),
            new FixedPattern(new[]{Direction.East, Direction.North, Direction.North}),
            new FixedPattern(new[]{Direction.East, Direction.South, Direction.South}),
            new FixedPattern(new[]{Direction.North, Direction.East, Direction.East}),
            new FixedPattern(new[]{Direction.North, Direction.West, Direction.West}),
            new FixedPattern(new[]{Direction.South, Direction.East, Direction.East}),
            new FixedPattern(new[]{Direction.South, Direction.West, Direction.West})
        };
        var knightB1 = new Piece("white-knight-1", white, patterns);
        var knightD1 = new Piece("white-knight-2", white, patterns);
        var fromB1 = new Tile(ChessIds.Tiles.B1);
        var fromD1 = new Tile(ChessIds.Tiles.D1);
        var c1 = new Tile(ChessIds.Tiles.C1);
        var c2 = new Tile(ChessIds.Tiles.C2);
        var c3 = new Tile(ChessIds.Tiles.C3);

        // Minimal board connectivity for required paths
        var relB1C1 = new TileRelation(fromB1, c1, Direction.East);
        var relC1C2 = new TileRelation(c1, c2, Direction.North);
        var relC2C3 = new TileRelation(c2, c3, Direction.North);
        var relD1C1 = new TileRelation(fromD1, c1, Direction.West);

        // Build path for knight from b1 -> c3 via E,N,N pattern
        var pathB1C3 = new TilePath([relB1C1, relC1C2, relC2C3]);
        var move = new MovePieceGameEvent(knightB1, pathB1C3);

        var state = GameState.New(new IArtifactState[]
        {
            new PieceState(knightB1, fromB1),
            new PieceState(knightD1, fromD1)
        });

        // Minimal board + game (relations required for path resolution visitor)
        var board = new Board("test-board", new[] { relB1C1, relC1C2, relC2C3, relD1C1 });
        var game = new Game(board, new[] { white }, new Artifact[] { knightB1, knightD1 });

        // act
        var san = nomenclature.Describe(game, state, move);

        // assert
        san.Should().Be("Nbc3");
    }

    [Fact]
    public void GivenRookMoveGivingCheck_WhenDescribed_ThenEndsWithPlus()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var black = new Player(ChessIds.Players.Black);
        // Rook patterns (four repeatable directions simplified as separate fixed sequences of single steps repeated in visitor logic)
        var rookPatterns = new IPattern[]
        {
            new DirectionPattern(Direction.North, true),
            new DirectionPattern(Direction.South, true),
            new DirectionPattern(Direction.East, true),
            new DirectionPattern(Direction.West, true)
        };
        var rook = new Piece("white-rook-1", white, rookPatterns);
        var king = new Piece("black-king", black, []);
        // Tiles a1..a8
        var a1 = new Tile(ChessIds.Tiles.A1); var a2 = new Tile(ChessIds.Tiles.A2); var a3 = new Tile(ChessIds.Tiles.A3); var a4 = new Tile(ChessIds.Tiles.A4);
        var a5 = new Tile(ChessIds.Tiles.A5); var a6 = new Tile(ChessIds.Tiles.A6); var a7 = new Tile(ChessIds.Tiles.A7); var a8 = new Tile(ChessIds.Tiles.A8);
        // Relations up the file
        var r1 = new TileRelation(a1, a2, Direction.North); var r2 = new TileRelation(a2, a3, Direction.North);
        var r3 = new TileRelation(a3, a4, Direction.North); var r4 = new TileRelation(a4, a5, Direction.North);
        var r5 = new TileRelation(a5, a6, Direction.North); var r6 = new TileRelation(a6, a7, Direction.North);
        var r7 = new TileRelation(a7, a8, Direction.North);
        var board = new Board("test-board-check", new[] { r1, r2, r3, r4, r5, r6, r7 });
        var game = new Game(board, new[] { white, black }, new Artifact[] { rook, king });
        // Move rook from a1 to a7 delivering check on king a8
        var path = new TilePath(new[] { r1, r2, r3, r4, r5, r6 });
        var move = new MovePieceGameEvent(rook, path);
        var state = GameState.New(new IArtifactState[] { new PieceState(rook, a1), new PieceState(king, a8) });

        // act
        var san = nomenclature.Describe(game, state, move);

        // assert
        san.Should().Be("Ra7+");
    }

    [Fact]
    public void GivenKingSideCastlingMove_WhenDescribed_ThenOutputsOO()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var king = new Piece("white-king", white, new IPattern[]
        {
            new DirectionPattern(Direction.East, false),
            new DirectionPattern(Direction.West, false)
        });
        var from = new Tile(ChessIds.Tiles.E1);
        var f1 = new Tile(ChessIds.Tiles.F1);
        var g1 = new Tile(ChessIds.Tiles.G1);
        var r1 = new TileRelation(from, f1, Direction.East);
        var r2 = new TileRelation(f1, g1, Direction.East);
        var board = new Board("castle-board-k", new[] { r1, r2 });
        var game = new Game(board, new[] { white }, new Artifact[] { king });
        var path = new TilePath(new[] { r1, r2 });
        var move = new MovePieceGameEvent(king, path);
        var state = GameState.New(new IArtifactState[] { new PieceState(king, from) });

        // act
        var san = nomenclature.Describe(game, state, move);

        // assert
        san.Should().Be("O-O");
    }

    [Fact]
    public void GivenQueenSideCastlingMove_WhenDescribed_ThenOutputsOOO()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var king = new Piece("white-king", white, new IPattern[]
        {
            new DirectionPattern(Direction.East, false),
            new DirectionPattern(Direction.West, false)
        });
        var from = new Tile(ChessIds.Tiles.E1);
        var d1 = new Tile(ChessIds.Tiles.D1);
        var c1 = new Tile(ChessIds.Tiles.C1);
        var r1 = new TileRelation(from, d1, Direction.West);
        var r2 = new TileRelation(d1, c1, Direction.West);
        var board = new Board("castle-board-q", new[] { r1, r2 });
        var game = new Game(board, new[] { white }, new Artifact[] { king });
        var path = new TilePath(new[] { r1, r2 });
        var move = new MovePieceGameEvent(king, path);
        var state = GameState.New(new IArtifactState[] { new PieceState(king, from) });

        // act
        var san = nomenclature.Describe(game, state, move);

        // assert
        san.Should().Be("O-O-O");
    }

    [Fact]
    public void GivenPawnPromotes_WhenDescribed_ThenAppendsEqualsQ()
    {
        // arrange
        var nomenclature = new ChessNomenclature();
        var white = new Player(ChessIds.Players.White);
        var pawn = new Piece("white-pawn-1", white, new IPattern[] { new DirectionPattern(Direction.South, false) });
        var from = new Tile(ChessIds.Tiles.E7);
        var to = new Tile(ChessIds.Tiles.E8);
        var r = new TileRelation(from, to, Direction.South); // Using South (engine's orientation) consistent with earlier examples
        var board = new Board("promo-board", new[] { r });
        var game = new Game(board, new[] { white }, new Artifact[] { pawn });
        var path = new TilePath(new[] { r });
        var move = new MovePieceGameEvent(pawn, path);
        var state = GameState.New(new IArtifactState[] { new PieceState(pawn, from) });

        // act
        var san = nomenclature.Describe(game, state, move);

        // assert
        san.Should().Be("e8=Q");
    }

    // Move event description implicitly covered via engine integration tests; keeping nomenclature tests minimal.
}