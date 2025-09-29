using Veggerby.Boards;
using Veggerby.Boards.Chess;
using Veggerby.Boards.Flows.Events;
using Veggerby.Boards.Flows.Mutators;
using Veggerby.Boards.Flows.Rules.Conditions;
using Veggerby.Boards.States;
using Veggerby.Boards.States.Conditions;

namespace Veggerby.Boards.Tests.Chess.Support;

/// <summary>
/// Minimal chess scenario builder for focused en-passant tests (black captures white double-step).
/// Provides full 8x8 board topology but only includes required pawns (optionally an auxiliary pawn for non-capture reply tests).
/// </summary>
internal sealed class EnPassantScenarioBuilder : GameBuilder
{
    private readonly bool _includeAuxiliaryBlackPawn;

    public EnPassantScenarioBuilder(bool includeAuxiliaryBlackPawn = false)
    {
        _includeAuxiliaryBlackPawn = includeAuxiliaryBlackPawn;
    }

    protected override void Build()
    {
        BoardId = "chess-en-passant-scenario";

        AddPlayer("white");
        AddPlayer("black");

        // Active player projections: white starts, black inactive.
        WithActivePlayer("white", true);
        WithActivePlayer("black", false);

        // Reuse canonical directions
        AddDirection("north");
        AddDirection("east");
        AddDirection("south");
        AddDirection("west");
        AddDirection("north-east");
        AddDirection("north-west");
        AddDirection("south-east");
        AddDirection("south-west");

        // Board tiles + relations (same orientation as ChessGameBuilder after flip: increasing rank = north)
        static string FileChar(int idx) => ((char)('a' + idx - 1)).ToString();
        for (var file = 1; file <= 8; file++)
        {
            for (var rank = 1; rank <= 8; rank++)
            {
                var id = $"tile-{FileChar(file)}{rank}";
                var tile = AddTile(id);
                if (file < 8) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank}").InDirection("east");
                if (file > 1) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank}").InDirection("west");
                if (rank < 8) tile.WithRelationTo($"tile-{FileChar(file)}{rank + 1}").InDirection("north");
                if (rank > 1) tile.WithRelationTo($"tile-{FileChar(file)}{rank - 1}").InDirection("south");
                if (file < 8 && rank < 8) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank + 1}").InDirection("north-east");
                if (file > 1 && rank < 8) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank + 1}").InDirection("north-west");
                if (file < 8 && rank > 1) tile.WithRelationTo($"tile-{FileChar(file + 1)}{rank - 1}").InDirection("south-east");
                if (file > 1 && rank > 1) tile.WithRelationTo($"tile-{FileChar(file - 1)}{rank - 1}").InDirection("south-west");
            }
        }

        // Structural pawn patterns (white north, black south)
        AddPiece("white-pawn-test")
            .WithOwner("white")
            .HasDirection("north").Done()
            .HasPattern("north")
            .HasPattern("north", "north")
            .HasDirection("north-east").Done()
            .HasDirection("north-west").Done();

        AddPiece("black-pawn-test")
            .WithOwner("black")
            .HasDirection("south").Done()
            .HasPattern("south")
            .HasPattern("south", "south")
            .HasDirection("south-east").Done()
            .HasDirection("south-west").Done();

        if (_includeAuxiliaryBlackPawn)
        {
            AddPiece("black-pawn-aux")
                .WithOwner("black")
                .HasDirection("south").Done()
                .HasPattern("south")
                .HasPattern("south", "south")
                .HasDirection("south-east").Done()
                .HasDirection("south-west").Done();
        }

        // Initial placement: white pawn on e2, black pawn on d4 (ready to capture e3 after double-step)
        WithPiece("white-pawn-test").OnTile("tile-e2");
        WithPiece("black-pawn-test").OnTile("tile-d4");
        if (_includeAuxiliaryBlackPawn)
        {
            WithPiece("black-pawn-aux").OnTile("tile-h7");
        }

        // Chess extras baseline
        WithState(new ChessStateExtras(true, true, true, true, null, 0, 1, System.Array.Empty<string>()));

        // Active player projection now explicit via WithActivePlayer declarations above.

        // Movement phase (mirrors ChessGameBuilder ordering)
        AddGamePhase("move pieces")
            .If<NullGameStateCondition>()
                .Then()
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                            .And<DestinationHasOpponentPieceGameEventCondition>()
                    .Then()
                        .Do<ChessCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<EnPassantCaptureGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<DiagonalPawnDirectionGameEventCondition>()
                    .Then()
                        .Do<EnPassantCapturePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                            .And<DistanceTwoGameEventCondition>()
                            .And<PawnInitialDoubleStepGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()))
                    .ForEvent<MovePieceGameEvent>()
                        .If<PieceIsActivePlayerGameEventCondition>()
                            .And<PathNotObstructedGameEventCondition>()
                            .And<DistanceOneGameEventCondition>()
                            .And<ForwardPawnDirectionGameEventCondition>()
                            .And<DestinationIsEmptyGameEventCondition>()
                    .Then()
                        .Do<ChessMovePieceStateMutator>()
                        .Do(game => new NextPlayerStateMutator(new SingleActivePlayerGameStateCondition()));
    }
}