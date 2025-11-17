using System.Text;

using Veggerby.Boards.Chess.Conditions;
using Veggerby.Boards.Chess.Mutators;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Chess;

/// <summary>
/// Concrete <see cref="GameBuilder"/> defining a standard chess initial position including all piece
/// movement patterns (directional + fixed sequences) and a single movement phase.
/// </summary>
/// <remarks>
/// This module focuses on demonstrating complex pattern registration (e.g., knight L-shapes via fixed patterns)
/// and reuse of directional repeatable patterns for sliding pieces (rook, bishop, queen).
/// </remarks>
public class ChessGameBuilder : GameBuilder
{
    private static string GetChar(int i)
    {
        var b = Encoding.UTF8.GetBytes(['a'])[0] + i - 1;
        return Encoding.UTF8.GetString(new[] { (byte)b });
    }

    /// <summary>
    /// Configures the chess board tiles, relations, pieces, movement patterns and phases.
    /// </summary>
    protected override void Build()
    {
        // Game
        BoardId = "chess";

        AddPlayer(ChessIds.Players.White);
        AddPlayer(ChessIds.Players.Black);

        // Explicit active player projection: white begins.
        WithActivePlayer(ChessIds.Players.White, true);
        WithActivePlayer(ChessIds.Players.Black, false);

        // Canonical orientation: white pieces move toward increasing rank numbers (north), black toward decreasing (south).
        // Directions are declared once; semantics in conditions/mutators use this canonical mapping.
        AddDirection(Constants.Directions.North);
        AddDirection(Constants.Directions.East);
        AddDirection(Constants.Directions.South);
        AddDirection(Constants.Directions.West);
        AddDirection(Constants.Directions.NorthEast);
        AddDirection(Constants.Directions.NorthWest);
        AddDirection(Constants.Directions.SouthEast);
        AddDirection(Constants.Directions.SouthWest);

        /*         N
         * (a,1) ----- (h,1)    WHITE
         *   |           |
         *   |           |
         * W |           | E
         *   |           |
         *   |           |
         * (a,8) ----- (h,8)    BLACK
         *         S
         */
        for (int x = 1; x <= 8; x++)
        {
            for (int y = 1; y <= 8; y++)
            {
                var tile = AddTile($"tile-{GetChar(x)}{y}");

                if (x > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x - 1)}{y}")
                        .InDirection(Constants.Directions.West);
                }

                if (x < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y}")
                        .InDirection(Constants.Directions.East);
                }

                // Canonical relation mapping: increasing rank = north
                if (y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x)}{y + 1}")
                        .InDirection(Constants.Directions.North);
                }

                if (y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x)}{y - 1}")
                        .InDirection(Constants.Directions.South);
                }

                // Diagonals
                if (x < 8 && y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y + 1}")
                        .InDirection(Constants.Directions.NorthEast);
                }

                if (x > 1 && y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x - 1)}{y + 1}")
                        .InDirection(Constants.Directions.NorthWest);
                }

                if (x < 8 && y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y - 1}")
                        .InDirection(Constants.Directions.SouthEast);
                }

                if (x > 1 && y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x - 1)}{y - 1}")
                        .InDirection(Constants.Directions.SouthWest);
                }

            }
        }

        // NOTE: Previous design used a hidden sink tile for captures; replaced by explicit CapturedPieceState
        // markers to avoid distorting board topology / bitboard density. Comment retained for historical context.

        AddPiece(ChessIds.Pieces.WhiteQueen)
            .WithOwner(ChessIds.Players.White)
            .HasDirection(Constants.Directions.North).CanRepeat()
            .HasDirection(Constants.Directions.East).CanRepeat()
            .HasDirection(Constants.Directions.West).CanRepeat()
            .HasDirection(Constants.Directions.South).CanRepeat()
            .HasDirection(Constants.Directions.NorthEast).CanRepeat()
            .HasDirection(Constants.Directions.NorthWest).CanRepeat()
            .HasDirection(Constants.Directions.SouthEast).CanRepeat()
            .HasDirection(Constants.Directions.SouthWest).CanRepeat();

        AddPiece(ChessIds.Pieces.WhiteKing)
            .WithOwner(ChessIds.Players.White)
            .HasDirection(Constants.Directions.North).Done()
            .HasDirection(Constants.Directions.East).Done()
            .HasDirection(Constants.Directions.West).Done()
            .HasDirection(Constants.Directions.South).Done()
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done()
            .HasDirection(Constants.Directions.SouthEast).Done()
            .HasDirection(Constants.Directions.SouthWest).Done();

        AddPiece(ChessIds.Pieces.BlackQueen)
            .WithOwner(ChessIds.Players.Black)
            .HasDirection(Constants.Directions.North).CanRepeat()
            .HasDirection(Constants.Directions.East).CanRepeat()
            .HasDirection(Constants.Directions.West).CanRepeat()
            .HasDirection(Constants.Directions.South).CanRepeat()
            .HasDirection(Constants.Directions.NorthEast).CanRepeat()
            .HasDirection(Constants.Directions.NorthWest).CanRepeat()
            .HasDirection(Constants.Directions.SouthEast).CanRepeat()
            .HasDirection(Constants.Directions.SouthWest).CanRepeat();

        AddPiece(ChessIds.Pieces.BlackKing)
            .WithOwner(ChessIds.Players.Black)
            .HasDirection(Constants.Directions.North).Done()
            .HasDirection(Constants.Directions.East).Done()
            .HasDirection(Constants.Directions.West).Done()
            .HasDirection(Constants.Directions.South).Done()
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done()
            .HasDirection(Constants.Directions.SouthEast).Done()
            .HasDirection(Constants.Directions.SouthWest).Done();

        for (int i = 1; i <= 8; i++)
        {
            AddPiece($"white-pawn-{i}")
                .WithOwner(ChessIds.Players.White)
                // White forward = north (towards rank 8). Patterns are structural; legality gated by rule chain.
                .HasDirection(Constants.Directions.North).Done()
                .HasPattern(Constants.Directions.North)
                .HasPattern(Constants.Directions.North, Constants.Directions.North)
                .HasDirection(Constants.Directions.NorthEast).Done()
                .HasDirection(Constants.Directions.NorthWest).Done();

            AddPiece($"black-pawn-{i}")
                .WithOwner(ChessIds.Players.Black)
                // Black forward = south (towards rank 1).
                .HasDirection(Constants.Directions.South).Done()
                .HasPattern(Constants.Directions.South)
                .HasPattern(Constants.Directions.South, Constants.Directions.South)
                .HasDirection(Constants.Directions.SouthEast).Done()
                .HasDirection(Constants.Directions.SouthWest).Done();
        }

        for (int i = 1; i <= 8; i++)
        {
            AddPiece($"white-rook-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasDirection(Constants.Directions.North).CanRepeat()
                .HasDirection(Constants.Directions.East).CanRepeat()
                .HasDirection(Constants.Directions.South).CanRepeat()
                .HasDirection(Constants.Directions.West).CanRepeat();

            AddPiece($"white-knight-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasPattern(Constants.Directions.West, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.West, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.East, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.East, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.North, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.North, Constants.Directions.West, Constants.Directions.West)
                .HasPattern(Constants.Directions.South, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.South, Constants.Directions.West, Constants.Directions.West);

            AddPiece($"white-bishop-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasDirection(Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Constants.Directions.SouthWest).CanRepeat();

            AddPiece($"black-rook-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasDirection(Constants.Directions.North).CanRepeat()
                .HasDirection(Constants.Directions.East).CanRepeat()
                .HasDirection(Constants.Directions.South).CanRepeat()
                .HasDirection(Constants.Directions.West).CanRepeat();

            AddPiece($"black-knight-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasPattern(Constants.Directions.West, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.West, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.East, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.East, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.North, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.North, Constants.Directions.West, Constants.Directions.West)
                .HasPattern(Constants.Directions.South, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.South, Constants.Directions.West, Constants.Directions.West);

            AddPiece($"black-bishop-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasDirection(Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Constants.Directions.SouthWest).CanRepeat();
        }

        // State
        WithPiece(ChessIds.Pieces.WhiteRook1).OnTile(ChessIds.Tiles.A1);
        WithPiece(ChessIds.Pieces.WhiteKnight1).OnTile(ChessIds.Tiles.B1);
        WithPiece(ChessIds.Pieces.WhiteBishop1).OnTile(ChessIds.Tiles.C1);
        // Standard placement: queen on d-file, king on e-file
        WithPiece(ChessIds.Pieces.WhiteQueen).OnTile(ChessIds.Tiles.D1);
        WithPiece(ChessIds.Pieces.WhiteKing).OnTile(ChessIds.Tiles.E1);
        WithPiece(ChessIds.Pieces.WhiteBishop2).OnTile(ChessIds.Tiles.F1);
        WithPiece(ChessIds.Pieces.WhiteKnight2).OnTile(ChessIds.Tiles.G1);
        WithPiece(ChessIds.Pieces.WhiteRook2).OnTile(ChessIds.Tiles.H1);

        WithPiece(ChessIds.Pieces.WhitePawn1).OnTile(ChessIds.Tiles.A2);
        WithPiece(ChessIds.Pieces.WhitePawn2).OnTile(ChessIds.Tiles.B2);
        WithPiece(ChessIds.Pieces.WhitePawn3).OnTile(ChessIds.Tiles.C2);
        WithPiece(ChessIds.Pieces.WhitePawn4).OnTile(ChessIds.Tiles.D2);
        WithPiece(ChessIds.Pieces.WhitePawn5).OnTile(ChessIds.Tiles.E2);
        WithPiece(ChessIds.Pieces.WhitePawn6).OnTile(ChessIds.Tiles.F2);
        WithPiece(ChessIds.Pieces.WhitePawn7).OnTile(ChessIds.Tiles.G2);
        WithPiece(ChessIds.Pieces.WhitePawn8).OnTile(ChessIds.Tiles.H2);

        WithPiece(ChessIds.Pieces.BlackPawn1).OnTile(ChessIds.Tiles.A7);
        WithPiece(ChessIds.Pieces.BlackPawn2).OnTile(ChessIds.Tiles.B7);
        WithPiece(ChessIds.Pieces.BlackPawn3).OnTile(ChessIds.Tiles.C7);
        WithPiece(ChessIds.Pieces.BlackPawn4).OnTile(ChessIds.Tiles.D7);
        WithPiece(ChessIds.Pieces.BlackPawn5).OnTile(ChessIds.Tiles.E7);
        WithPiece(ChessIds.Pieces.BlackPawn6).OnTile(ChessIds.Tiles.F7);
        WithPiece(ChessIds.Pieces.BlackPawn7).OnTile(ChessIds.Tiles.G7);
        WithPiece(ChessIds.Pieces.BlackPawn8).OnTile(ChessIds.Tiles.H7);

        WithPiece(ChessIds.Pieces.BlackRook1).OnTile(ChessIds.Tiles.A8);
        WithPiece(ChessIds.Pieces.BlackKnight1).OnTile(ChessIds.Tiles.B8);
        WithPiece(ChessIds.Pieces.BlackBishop1).OnTile(ChessIds.Tiles.C8);
        // Standard placement: queen on d-file, king on e-file
        WithPiece(ChessIds.Pieces.BlackQueen).OnTile(ChessIds.Tiles.D8);
        WithPiece(ChessIds.Pieces.BlackKing).OnTile(ChessIds.Tiles.E8);
        WithPiece(ChessIds.Pieces.BlackBishop2).OnTile(ChessIds.Tiles.F8);
        WithPiece(ChessIds.Pieces.BlackKnight2).OnTile(ChessIds.Tiles.G8);
        WithPiece(ChessIds.Pieces.BlackRook2).OnTile(ChessIds.Tiles.H8);

        // Chess specific extras (initial castling rights all true, no en-passant target, clocks reset)
        WithState(new ChessStateExtras(
            WhiteCanCastleKingSide: true,
            WhiteCanCastleQueenSide: true,
            BlackCanCastleKingSide: true,
            BlackCanCastleQueenSide: true,
            EnPassantTargetTileId: null,
            HalfmoveClock: 0,
            FullmoveNumber: 1,
            MovedPieceIds: Array.Empty<string>()));

        // Piece role metadata map (piece id -> role) used by conditions instead of string heuristics
        // Immutable and color/index agnostic; populated explicitly for clarity.
        var roleMap = new System.Collections.Generic.Dictionary<string, ChessPieceRole>(56)
        {
            // White major pieces
            { ChessIds.Pieces.WhiteKing, ChessPieceRole.King },
            { ChessIds.Pieces.WhiteQueen, ChessPieceRole.Queen },
            { ChessIds.Pieces.WhiteRook1, ChessPieceRole.Rook },
            { ChessIds.Pieces.WhiteRook2, ChessPieceRole.Rook },
            { ChessIds.Pieces.WhiteBishop1, ChessPieceRole.Bishop },
            { ChessIds.Pieces.WhiteBishop2, ChessPieceRole.Bishop },
            { ChessIds.Pieces.WhiteKnight1, ChessPieceRole.Knight },
            { ChessIds.Pieces.WhiteKnight2, ChessPieceRole.Knight },
            // Black major pieces
            { ChessIds.Pieces.BlackKing, ChessPieceRole.King },
            { ChessIds.Pieces.BlackQueen, ChessPieceRole.Queen },
            { ChessIds.Pieces.BlackRook1, ChessPieceRole.Rook },
            { ChessIds.Pieces.BlackRook2, ChessPieceRole.Rook },
            { ChessIds.Pieces.BlackBishop1, ChessPieceRole.Bishop },
            { ChessIds.Pieces.BlackBishop2, ChessPieceRole.Bishop },
            { ChessIds.Pieces.BlackKnight1, ChessPieceRole.Knight },
            { ChessIds.Pieces.BlackKnight2, ChessPieceRole.Knight },
            // White pawns
            { ChessIds.Pieces.WhitePawn1, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn2, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn3, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn4, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn5, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn6, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn7, ChessPieceRole.Pawn },
            { ChessIds.Pieces.WhitePawn8, ChessPieceRole.Pawn },
            // Black pawns
            { ChessIds.Pieces.BlackPawn1, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn2, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn3, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn4, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn5, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn6, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn7, ChessPieceRole.Pawn },
            { ChessIds.Pieces.BlackPawn8, ChessPieceRole.Pawn },
        };
        WithState(new ChessPieceRolesExtras(roleMap));

        var colorMap = new System.Collections.Generic.Dictionary<string, ChessPieceColor>(56)
        {
            // White pieces
            { ChessIds.Pieces.WhiteKing, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteQueen, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteRook1, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteRook2, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteBishop1, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteBishop2, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteKnight1, ChessPieceColor.White },
            { ChessIds.Pieces.WhiteKnight2, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn1, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn2, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn3, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn4, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn5, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn6, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn7, ChessPieceColor.White },
            { ChessIds.Pieces.WhitePawn8, ChessPieceColor.White },
            // Black pieces
            { ChessIds.Pieces.BlackKing, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackQueen, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackRook1, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackRook2, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackBishop1, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackBishop2, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackKnight1, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackKnight2, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn1, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn2, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn3, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn4, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn5, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn6, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn7, ChessPieceColor.Black },
            { ChessIds.Pieces.BlackPawn8, ChessPieceColor.Black },
        };
        WithState(new ChessPieceColorsExtras(colorMap));

        // Build-time (compile-time equivalent) validation ensuring every canonical piece id has role and color entries.
        // Fast-fail to prevent silent drift if ChessIds.Pieces is modified without updating metadata.
        var expectedPieceIds = new string[]
        {
            ChessIds.Pieces.WhiteKing, ChessIds.Pieces.WhiteQueen, ChessIds.Pieces.WhiteRook1, ChessIds.Pieces.WhiteRook2,
            ChessIds.Pieces.WhiteBishop1, ChessIds.Pieces.WhiteBishop2, ChessIds.Pieces.WhiteKnight1, ChessIds.Pieces.WhiteKnight2,
            ChessIds.Pieces.WhitePawn1, ChessIds.Pieces.WhitePawn2, ChessIds.Pieces.WhitePawn3, ChessIds.Pieces.WhitePawn4,
            ChessIds.Pieces.WhitePawn5, ChessIds.Pieces.WhitePawn6, ChessIds.Pieces.WhitePawn7, ChessIds.Pieces.WhitePawn8,
            ChessIds.Pieces.BlackKing, ChessIds.Pieces.BlackQueen, ChessIds.Pieces.BlackRook1, ChessIds.Pieces.BlackRook2,
            ChessIds.Pieces.BlackBishop1, ChessIds.Pieces.BlackBishop2, ChessIds.Pieces.BlackKnight1, ChessIds.Pieces.BlackKnight2,
            ChessIds.Pieces.BlackPawn1, ChessIds.Pieces.BlackPawn2, ChessIds.Pieces.BlackPawn3, ChessIds.Pieces.BlackPawn4,
            ChessIds.Pieces.BlackPawn5, ChessIds.Pieces.BlackPawn6, ChessIds.Pieces.BlackPawn7, ChessIds.Pieces.BlackPawn8,
        };
        foreach (var pid in expectedPieceIds)
        {
            if (!roleMap.ContainsKey(pid) || !colorMap.ContainsKey(pid))
            {
                throw new System.InvalidOperationException($"Piece metadata drift: missing role/color for '{pid}'");
            }
        }


        AddGamePhase("move pieces")
            .If<GameNotEndedCondition>() // Only allow moves when game hasn't ended
                .Then()
                    // Castling (must appear before generic king non-pawn movement so two-square king move is intercepted)
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<CastlingGameEventCondition>()
                            .And<CastlingKingSafetyGameEventCondition>()
                    .Then()
                        .Do<CastlingMoveMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    // Generic non-pawn capture (must appear before pawn specific branches)
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<NonPawnGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    // Generic non-pawn normal move
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<NonPawnGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Capture: path unobstructed (intermediates) AND destination has opponent piece
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // En-passant capture: special pawn diagonal onto empty en-passant target
                            .And<EnPassantCaptureGameEventCondition>() // includes distance==1 + diagonal semantics internally, but keep explicit guards for clarity
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                    .Then()
                        .Do<EnPassantCapturePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Double-step pawn advance (must precede normal move so destination empty branch does not swallow it)
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                            .And<DistanceTwoGameEventCondition>()
                            .And<PawnInitialDoubleStepGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Normal move: path unobstructed AND destination empty
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<ForwardPawnDirectionGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new EndgameCheckThenNextPlayerMutator(game));
    }
}