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
        AddGridTiles(8, 8, (x, y) => $"tile-{GetChar(x + 1)}{y + 1}", (tile, x, y) =>
        {
            // Cardinal directions
            if (x > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x)}{y + 1}").InDirection(Constants.Directions.West);
            }

            if (x < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y + 1}").InDirection(Constants.Directions.East);
            }

            // Canonical relation mapping: increasing rank = north
            if (y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 1)}{y + 2}").InDirection(Constants.Directions.North);
            }

            if (y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 1)}{y}").InDirection(Constants.Directions.South);
            }

            // Diagonals
            if (x < 7 && y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y + 2}").InDirection(Constants.Directions.NorthEast);
            }

            if (x > 0 && y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x)}{y + 2}").InDirection(Constants.Directions.NorthWest);
            }

            if (x < 7 && y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y}").InDirection(Constants.Directions.SouthEast);
            }

            if (x > 0 && y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x)}{y}").InDirection(Constants.Directions.SouthWest);
            }
        });

        // NOTE: Previous design used a hidden sink tile for captures; replaced by explicit CapturedPieceState
        // markers to avoid distorting board topology / bitboard density. Comment retained for historical context.

        AddPiece(ChessIds.Pieces.WhiteQueen)
            .WithOwner(ChessIds.Players.White)
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Queen, ChessPieceColor.White))
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
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.King, ChessPieceColor.White))
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
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Queen, ChessPieceColor.Black))
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
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.King, ChessPieceColor.Black))
            .HasDirection(Constants.Directions.North).Done()
            .HasDirection(Constants.Directions.East).Done()
            .HasDirection(Constants.Directions.West).Done()
            .HasDirection(Constants.Directions.South).Done()
            .HasDirection(Constants.Directions.NorthEast).Done()
            .HasDirection(Constants.Directions.NorthWest).Done()
            .HasDirection(Constants.Directions.SouthEast).Done()
            .HasDirection(Constants.Directions.SouthWest).Done();

        // White pawns
        AddMultiplePieces(8, i => $"white-pawn-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Pawn, ChessPieceColor.White))
                // White forward = north (towards rank 8). Patterns are structural; legality gated by rule chain.
                .HasDirection(Constants.Directions.North).Done()
                .HasPattern(Constants.Directions.North)
                .HasPattern(Constants.Directions.North, Constants.Directions.North)
                .HasDirection(Constants.Directions.NorthEast).Done()
                .HasDirection(Constants.Directions.NorthWest).Done();
        });

        // Black pawns
        AddMultiplePieces(8, i => $"black-pawn-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Pawn, ChessPieceColor.Black))
                // Black forward = south (towards rank 1).
                .HasDirection(Constants.Directions.South).Done()
                .HasPattern(Constants.Directions.South)
                .HasPattern(Constants.Directions.South, Constants.Directions.South)
                .HasDirection(Constants.Directions.SouthEast).Done()
                .HasDirection(Constants.Directions.SouthWest).Done();
        });

        // White rooks
        AddMultiplePieces(2, i => $"white-rook-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Rook, ChessPieceColor.White))
                .HasDirection(Constants.Directions.North).CanRepeat()
                .HasDirection(Constants.Directions.East).CanRepeat()
                .HasDirection(Constants.Directions.South).CanRepeat()
                .HasDirection(Constants.Directions.West).CanRepeat();
        });

        // White knights
        AddMultiplePieces(2, i => $"white-knight-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Knight, ChessPieceColor.White))
                .HasPattern(Constants.Directions.West, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.West, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.East, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.East, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.North, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.North, Constants.Directions.West, Constants.Directions.West)
                .HasPattern(Constants.Directions.South, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.South, Constants.Directions.West, Constants.Directions.West);
        });

        // White bishops
        AddMultiplePieces(2, i => $"white-bishop-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Bishop, ChessPieceColor.White))
                .HasDirection(Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Constants.Directions.SouthWest).CanRepeat();
        });

        // Black rooks
        AddMultiplePieces(2, i => $"black-rook-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Rook, ChessPieceColor.Black))
                .HasDirection(Constants.Directions.North).CanRepeat()
                .HasDirection(Constants.Directions.East).CanRepeat()
                .HasDirection(Constants.Directions.South).CanRepeat()
                .HasDirection(Constants.Directions.West).CanRepeat();
        });

        // Black knights
        AddMultiplePieces(2, i => $"black-knight-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Knight, ChessPieceColor.Black))
                .HasPattern(Constants.Directions.West, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.West, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.East, Constants.Directions.North, Constants.Directions.North)
                .HasPattern(Constants.Directions.East, Constants.Directions.South, Constants.Directions.South)
                .HasPattern(Constants.Directions.North, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.North, Constants.Directions.West, Constants.Directions.West)
                .HasPattern(Constants.Directions.South, Constants.Directions.East, Constants.Directions.East)
                .HasPattern(Constants.Directions.South, Constants.Directions.West, Constants.Directions.West);
        });

        // Black bishops
        AddMultiplePieces(2, i => $"black-bishop-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Bishop, ChessPieceColor.Black))
                .HasDirection(Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Constants.Directions.SouthWest).CanRepeat();
        });

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

        AddGamePhase("move pieces")
            .WithEndGameDetection(
                game => new CheckmateOrStalemateCondition(game),
                game => new ChessEndGameMutator(game))
            .If<GameNotEndedCondition>() // Only allow moves when game hasn't ended
                .Then()
                    // Castling (must appear before generic king non-pawn movement so two-square king move is intercepted)
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<CastlingGameEventCondition>()
                            .And<CastlingKingSafetyGameEventCondition>()
                    .Then()
                        .Do<CastlingMoveMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    // Generic non-pawn capture (must appear before pawn specific branches)
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<NonPawnGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    // Generic non-pawn normal move
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<NonPawnGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Capture: path unobstructed (intermediates) AND destination has opponent piece
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // En-passant capture: special pawn diagonal onto empty en-passant target
                            .And<EnPassantCaptureGameEventCondition>() // includes distance==1 + diagonal semantics internally, but keep explicit guards for clarity
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                    .Then()
                        .Do<EnPassantCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Double-step pawn advance (must precede normal move so destination empty branch does not swallow it)
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                            .And<DistanceTwoGameEventCondition>()
                            .And<PawnInitialDoubleStepGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            // Normal move: path unobstructed AND destination empty
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<ForwardPawnDirectionGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }
}