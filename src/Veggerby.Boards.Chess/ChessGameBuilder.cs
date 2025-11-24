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

        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White);
        AddPlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black);

        // Explicit active player projection: white begins.
        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.White, true);
        WithActivePlayer(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black, false);

        // Canonical orientation: white pieces move toward increasing rank numbers (north), black toward decreasing (south).
        // Directions are declared once; semantics in conditions/mutators use this canonical mapping.
        AddDirection(Veggerby.Boards.Constants.Directions.North);
        AddDirection(Veggerby.Boards.Constants.Directions.East);
        AddDirection(Veggerby.Boards.Constants.Directions.South);
        AddDirection(Veggerby.Boards.Constants.Directions.West);
        AddDirection(Veggerby.Boards.Constants.Directions.NorthEast);
        AddDirection(Veggerby.Boards.Constants.Directions.NorthWest);
        AddDirection(Veggerby.Boards.Constants.Directions.SouthEast);
        AddDirection(Veggerby.Boards.Constants.Directions.SouthWest);

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
                tile.WithRelationTo($"tile-{GetChar(x)}{y + 1}").InDirection(Veggerby.Boards.Constants.Directions.West);
            }

            if (x < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y + 1}").InDirection(Veggerby.Boards.Constants.Directions.East);
            }

            // Canonical relation mapping: increasing rank = north
            if (y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 1)}{y + 2}").InDirection(Veggerby.Boards.Constants.Directions.North);
            }

            if (y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 1)}{y}").InDirection(Veggerby.Boards.Constants.Directions.South);
            }

            // Diagonals
            if (x < 7 && y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y + 2}").InDirection(Veggerby.Boards.Constants.Directions.NorthEast);
            }

            if (x > 0 && y < 7)
            {
                tile.WithRelationTo($"tile-{GetChar(x)}{y + 2}").InDirection(Veggerby.Boards.Constants.Directions.NorthWest);
            }

            if (x < 7 && y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x + 2)}{y}").InDirection(Veggerby.Boards.Constants.Directions.SouthEast);
            }

            if (x > 0 && y > 0)
            {
                tile.WithRelationTo($"tile-{GetChar(x)}{y}").InDirection(Veggerby.Boards.Constants.Directions.SouthWest);
            }
        });

        // NOTE: Previous design used a hidden sink tile for captures; replaced by explicit CapturedPieceState
        // markers to avoid distorting board topology / bitboard density. Comment retained for historical context.

        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteQueen)
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Queen, ChessPieceColor.White))
            .HasDirection(Veggerby.Boards.Constants.Directions.North).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.East).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.West).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.South).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).CanRepeat();

        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteKing)
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.King, ChessPieceColor.White))
            .HasDirection(Veggerby.Boards.Constants.Directions.North).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.East).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.West).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.South).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).Done();

        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackQueen)
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Queen, ChessPieceColor.Black))
            .HasDirection(Veggerby.Boards.Constants.Directions.North).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.East).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.West).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.South).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).CanRepeat()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).CanRepeat();

        AddPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackKing)
            .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
            .WithMetadata(new ChessPieceMetadata(ChessPieceRole.King, ChessPieceColor.Black))
            .HasDirection(Veggerby.Boards.Constants.Directions.North).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.East).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.West).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.South).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).Done()
            .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).Done();

        // White pawns
        AddMultiplePieces(8, i => $"white-pawn-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Pawn, ChessPieceColor.White))
                // White forward = north (towards rank 8). Patterns are structural; legality gated by rule chain.
                .HasDirection(Veggerby.Boards.Constants.Directions.North).Done()
                .HasPattern(Veggerby.Boards.Constants.Directions.North)
                .HasPattern(Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North)
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).Done()
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).Done();
        });

        // Black pawns
        AddMultiplePieces(8, i => $"black-pawn-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Pawn, ChessPieceColor.Black))
                // Black forward = south (towards rank 1).
                .HasDirection(Veggerby.Boards.Constants.Directions.South).Done()
                .HasPattern(Veggerby.Boards.Constants.Directions.South)
                .HasPattern(Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South)
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).Done()
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).Done();
        });

        // White rooks
        AddMultiplePieces(2, i => $"white-rook-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Rook, ChessPieceColor.White))
                .HasDirection(Veggerby.Boards.Constants.Directions.North).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.East).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.South).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.West).CanRepeat();
        });

        // White knights
        AddMultiplePieces(2, i => $"white-knight-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Knight, ChessPieceColor.White))
                .HasPattern(Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North)
                .HasPattern(Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South)
                .HasPattern(Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North)
                .HasPattern(Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South)
                .HasPattern(Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East)
                .HasPattern(Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West)
                .HasPattern(Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East)
                .HasPattern(Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West);
        });

        // White bishops
        AddMultiplePieces(2, i => $"white-bishop-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.White)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Bishop, ChessPieceColor.White))
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).CanRepeat();
        });

        // Black rooks
        AddMultiplePieces(2, i => $"black-rook-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Rook, ChessPieceColor.Black))
                .HasDirection(Veggerby.Boards.Constants.Directions.North).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.East).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.South).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.West).CanRepeat();
        });

        // Black knights
        AddMultiplePieces(2, i => $"black-knight-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Knight, ChessPieceColor.Black))
                .HasPattern(Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North)
                .HasPattern(Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South)
                .HasPattern(Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.North)
                .HasPattern(Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.South)
                .HasPattern(Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East)
                .HasPattern(Veggerby.Boards.Constants.Directions.North, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West)
                .HasPattern(Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.East, Veggerby.Boards.Constants.Directions.East)
                .HasPattern(Veggerby.Boards.Constants.Directions.South, Veggerby.Boards.Constants.Directions.West, Veggerby.Boards.Constants.Directions.West);
        });

        // Black bishops
        AddMultiplePieces(2, i => $"black-bishop-{i + 1}", (piece, _) =>
        {
            piece
                .WithOwner(Veggerby.Boards.Chess.Constants.ChessIds.Players.Black)
                .WithMetadata(new ChessPieceMetadata(ChessPieceRole.Bishop, ChessPieceColor.Black))
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthEast).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.NorthWest).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthEast).CanRepeat()
                .HasDirection(Veggerby.Boards.Constants.Directions.SouthWest).CanRepeat();
        });

        // State
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteRook1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteKnight1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.B1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteBishop1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C1);
        // Standard placement: queen on d-file, king on e-file
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteQueen).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteKing).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteBishop2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.F1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteKnight2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.G1);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhiteRook2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.H1);

        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.B2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn3).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn4).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn5).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn6).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.F2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn7).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.G2);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.WhitePawn8).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.H2);

        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.B7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn3).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn4).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn5).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn6).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.F7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn7).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.G7);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackPawn8).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.H7);

        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackRook1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.A8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackKnight1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.B8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackBishop1).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.C8);
        // Standard placement: queen on d-file, king on e-file
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackQueen).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.D8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackKing).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.E8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackBishop2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.F8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackKnight2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.G8);
        WithPiece(Veggerby.Boards.Chess.Constants.ChessIds.Pieces.BlackRook2).OnTile(Veggerby.Boards.Chess.Constants.ChessIds.Tiles.H8);

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