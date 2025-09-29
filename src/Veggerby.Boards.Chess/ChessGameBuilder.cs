using System.Text;

using Veggerby.Boards;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
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
        AddDirection("north");
        AddDirection("east");
        AddDirection("south");
        AddDirection("west");
        AddDirection("north-east");
        AddDirection("north-west");
        AddDirection("south-east");
        AddDirection("south-west");

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
                        .InDirection("west");
                }

                if (x < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y}")
                        .InDirection("east");
                }

                // Canonical relation mapping: increasing rank = north
                if (y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x)}{y + 1}")
                        .InDirection("north");
                }

                if (y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x)}{y - 1}")
                        .InDirection("south");
                }

                // Diagonals
                if (x < 8 && y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y + 1}")
                        .InDirection("north-east");
                }

                if (x > 1 && y < 8)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x - 1)}{y + 1}")
                        .InDirection("north-west");
                }

                if (x < 8 && y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x + 1)}{y - 1}")
                        .InDirection("south-east");
                }

                if (x > 1 && y > 1)
                {
                    tile
                        .WithRelationTo($"tile-{GetChar(x - 1)}{y - 1}")
                        .InDirection("south-west");
                }

            }
        }

        // NOTE: Previous design used a hidden sink tile for captures; replaced by explicit CapturedPieceState
        // markers to avoid distorting board topology / bitboard density. Comment retained for historical context.

        AddPiece(ChessIds.Pieces.WhiteQueen)
            .WithOwner(ChessIds.Players.White)
            .HasDirection("north").CanRepeat()
            .HasDirection("east").CanRepeat()
            .HasDirection("west").CanRepeat()
            .HasDirection("south").CanRepeat()
            .HasDirection("north-east").CanRepeat()
            .HasDirection("north-west").CanRepeat()
            .HasDirection("south-east").CanRepeat()
            .HasDirection("south-west").CanRepeat();

        AddPiece(ChessIds.Pieces.WhiteKing)
            .WithOwner(ChessIds.Players.White)
            .HasDirection("north").Done()
            .HasDirection("east").Done()
            .HasDirection("west").Done()
            .HasDirection("south").Done()
            .HasDirection("north-east").Done()
            .HasDirection("north-west").Done()
            .HasDirection("south-east").Done()
            .HasDirection("south-west").Done();

        AddPiece(ChessIds.Pieces.BlackQueen)
            .WithOwner(ChessIds.Players.Black)
            .HasDirection("north").CanRepeat()
            .HasDirection("east").CanRepeat()
            .HasDirection("west").CanRepeat()
            .HasDirection("south").CanRepeat()
            .HasDirection("north-east").CanRepeat()
            .HasDirection("north-west").CanRepeat()
            .HasDirection("south-east").CanRepeat()
            .HasDirection("south-west").CanRepeat();

        AddPiece(ChessIds.Pieces.BlackKing)
            .WithOwner(ChessIds.Players.Black)
            .HasDirection("north").Done()
            .HasDirection("east").Done()
            .HasDirection("west").Done()
            .HasDirection("south").Done()
            .HasDirection("north-east").Done()
            .HasDirection("north-west").Done()
            .HasDirection("south-east").Done()
            .HasDirection("south-west").Done();

        for (int i = 1; i <= 8; i++)
        {
            AddPiece($"white-pawn-{i}")
                .WithOwner(ChessIds.Players.White)
                // White forward = north (towards rank 8). Patterns are structural; legality gated by rule chain.
                .HasDirection("north").Done()
                .HasPattern("north")
                .HasPattern("north", "north")
                .HasDirection("north-east").Done()
                .HasDirection("north-west").Done();

            AddPiece($"black-pawn-{i}")
                .WithOwner(ChessIds.Players.Black)
                // Black forward = south (towards rank 1).
                .HasDirection("south").Done()
                .HasPattern("south")
                .HasPattern("south", "south")
                .HasDirection("south-east").Done()
                .HasDirection("south-west").Done();
        }

        for (int i = 1; i <= 8; i++)
        {
            AddPiece($"white-rook-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasDirection("north").CanRepeat()
                .HasDirection("east").CanRepeat()
                .HasDirection("south").CanRepeat()
                .HasDirection("west").CanRepeat();

            AddPiece($"white-knight-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasPattern("west", "north", "north")
                .HasPattern("west", "south", "south")
                .HasPattern("east", "north", "north")
                .HasPattern("east", "south", "south")
                .HasPattern("north", "east", "east")
                .HasPattern("north", "west", "west")
                .HasPattern("south", "east", "east")
                .HasPattern("south", "west", "west");

            AddPiece($"white-bishop-{i}")
                .WithOwner(ChessIds.Players.White)
                .HasDirection("north-east").CanRepeat()
                .HasDirection("north-west").CanRepeat()
                .HasDirection("south-east").CanRepeat()
                .HasDirection("south-west").CanRepeat();

            AddPiece($"black-rook-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasDirection("north").CanRepeat()
                .HasDirection("east").CanRepeat()
                .HasDirection("south").CanRepeat()
                .HasDirection("west").CanRepeat();

            AddPiece($"black-knight-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasPattern("west", "north", "north")
                .HasPattern("west", "south", "south")
                .HasPattern("east", "north", "north")
                .HasPattern("east", "south", "south")
                .HasPattern("north", "east", "east")
                .HasPattern("north", "west", "west")
                .HasPattern("south", "east", "east")
                .HasPattern("south", "west", "west");

            AddPiece($"black-bishop-{i}")
                .WithOwner(ChessIds.Players.Black)
                .HasDirection("north-east").CanRepeat()
                .HasDirection("north-west").CanRepeat()
                .HasDirection("south-east").CanRepeat()
                .HasDirection("south-west").CanRepeat();
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
            MovedPieceIds: System.Array.Empty<string>()));


        AddGamePhase("move pieces")
            .If<NullGameStateCondition>()
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