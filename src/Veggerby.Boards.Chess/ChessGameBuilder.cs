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

        AddPlayer("white");
        AddPlayer("black");

        // Explicit active player projection: white begins.
        WithActivePlayer("white", true);
        WithActivePlayer("black", false);

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

        AddPiece("white-queen")
            .WithOwner("white")
            .HasDirection("north").CanRepeat()
            .HasDirection("east").CanRepeat()
            .HasDirection("west").CanRepeat()
            .HasDirection("south").CanRepeat()
            .HasDirection("north-east").CanRepeat()
            .HasDirection("north-west").CanRepeat()
            .HasDirection("south-east").CanRepeat()
            .HasDirection("south-west").CanRepeat();

        AddPiece("white-king")
            .WithOwner("white")
            .HasDirection("north").Done()
            .HasDirection("east").Done()
            .HasDirection("west").Done()
            .HasDirection("south").Done()
            .HasDirection("north-east").Done()
            .HasDirection("north-west").Done()
            .HasDirection("south-east").Done()
            .HasDirection("south-west").Done();

        AddPiece("black-queen")
            .WithOwner("black")
            .HasDirection("north").CanRepeat()
            .HasDirection("east").CanRepeat()
            .HasDirection("west").CanRepeat()
            .HasDirection("south").CanRepeat()
            .HasDirection("north-east").CanRepeat()
            .HasDirection("north-west").CanRepeat()
            .HasDirection("south-east").CanRepeat()
            .HasDirection("south-west").CanRepeat();

        AddPiece("black-king")
            .WithOwner("black")
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
                .WithOwner("white")
                // White forward = north (towards rank 8). Patterns are structural; legality gated by rule chain.
                .HasDirection("north").Done()
                .HasPattern("north")
                .HasPattern("north", "north")
                .HasDirection("north-east").Done()
                .HasDirection("north-west").Done();

            AddPiece($"black-pawn-{i}")
                .WithOwner("black")
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
                .WithOwner("white")
                .HasDirection("north").CanRepeat()
                .HasDirection("east").CanRepeat()
                .HasDirection("south").CanRepeat()
                .HasDirection("west").CanRepeat();

            AddPiece($"white-knight-{i}")
                .WithOwner("white")
                .HasPattern("west", "north", "north")
                .HasPattern("west", "south", "south")
                .HasPattern("east", "north", "north")
                .HasPattern("east", "south", "south")
                .HasPattern("north", "east", "east")
                .HasPattern("north", "west", "west")
                .HasPattern("south", "east", "east")
                .HasPattern("south", "west", "west");

            AddPiece($"white-bishop-{i}")
                .WithOwner("white")
                .HasDirection("north-east").CanRepeat()
                .HasDirection("north-west").CanRepeat()
                .HasDirection("south-east").CanRepeat()
                .HasDirection("south-west").CanRepeat();

            AddPiece($"black-rook-{i}")
                .WithOwner("black")
                .HasDirection("north").CanRepeat()
                .HasDirection("east").CanRepeat()
                .HasDirection("south").CanRepeat()
                .HasDirection("west").CanRepeat();

            AddPiece($"black-knight-{i}")
                .WithOwner("black")
                .HasPattern("west", "north", "north")
                .HasPattern("west", "south", "south")
                .HasPattern("east", "north", "north")
                .HasPattern("east", "south", "south")
                .HasPattern("north", "east", "east")
                .HasPattern("north", "west", "west")
                .HasPattern("south", "east", "east")
                .HasPattern("south", "west", "west");

            AddPiece($"black-bishop-{i}")
                .WithOwner("black")
                .HasDirection("north-east").CanRepeat()
                .HasDirection("north-west").CanRepeat()
                .HasDirection("south-east").CanRepeat()
                .HasDirection("south-west").CanRepeat();
        }

        // State
        WithPiece("white-rook-1").OnTile("tile-a1");
        WithPiece("white-knight-1").OnTile("tile-b1");
        WithPiece("white-bishop-1").OnTile("tile-c1");
        // Standard placement: queen on d-file, king on e-file
        WithPiece("white-queen").OnTile("tile-d1");
        WithPiece("white-king").OnTile("tile-e1");
        WithPiece("white-bishop-2").OnTile("tile-f1");
        WithPiece("white-knight-2").OnTile("tile-g1");
        WithPiece("white-rook-2").OnTile("tile-h1");

        WithPiece("white-pawn-1").OnTile("tile-a2");
        WithPiece("white-pawn-2").OnTile("tile-b2");
        WithPiece("white-pawn-3").OnTile("tile-c2");
        WithPiece("white-pawn-4").OnTile("tile-d2");
        WithPiece("white-pawn-5").OnTile("tile-e2");
        WithPiece("white-pawn-6").OnTile("tile-f2");
        WithPiece("white-pawn-7").OnTile("tile-g2");
        WithPiece("white-pawn-8").OnTile("tile-h2");

        WithPiece("black-pawn-1").OnTile("tile-a7");
        WithPiece("black-pawn-2").OnTile("tile-b7");
        WithPiece("black-pawn-3").OnTile("tile-c7");
        WithPiece("black-pawn-4").OnTile("tile-d7");
        WithPiece("black-pawn-5").OnTile("tile-e7");
        WithPiece("black-pawn-6").OnTile("tile-f7");
        WithPiece("black-pawn-7").OnTile("tile-g7");
        WithPiece("black-pawn-8").OnTile("tile-h7");

        WithPiece("black-rook-1").OnTile("tile-a8");
        WithPiece("black-knight-1").OnTile("tile-b8");
        WithPiece("black-bishop-1").OnTile("tile-c8");
        // Standard placement: queen on d-file, king on e-file
        WithPiece("black-queen").OnTile("tile-d8");
        WithPiece("black-king").OnTile("tile-e8");
        WithPiece("black-bishop-2").OnTile("tile-f8");
        WithPiece("black-knight-2").OnTile("tile-g8");
        WithPiece("black-rook-2").OnTile("tile-h8");

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